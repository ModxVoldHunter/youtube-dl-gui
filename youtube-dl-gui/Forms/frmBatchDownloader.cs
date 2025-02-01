﻿#nullable enable
namespace youtube_dl_gui;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows.Forms;
public partial class frmBatchDownloader : LocalizedProcessingForm {
    public bool Debugging;

    private readonly List<int> DownloadTypes = [];      // List of types to download
    private readonly List<string> DownloadUrls = [];    // List of urls to download
    private readonly List<string> DownloadArgs = [];    // List of args to download
    private readonly List<int> DownloadQuality = [];    // List of the quality
    private readonly List<int> DownloadFormat = [];     // List of the formats
    private readonly List<bool> DownloadSoundVBR = [];  // List of if sound/vbr should be downloaded

    // Bool if the batch download is in progress
    [MemberNotNullWhen(true, nameof(DownloadThread), nameof(Downloader), nameof(NewInfo))]
    private bool InProgress { get; set; }

    private bool AbortDownload;
    private Thread? DownloadThread;                         // The thread for the batch downloader.
    private frmDownloader? Downloader;                      // The Downloader form that will be around. Will be disposed if aborted.
    private DownloadInfo? NewInfo;                          // The info of the download
    private bool ClipboardScannerActive;                    // Whether the clipboard scanner is active.
    string? ClipboardData;                                  // Clipboard data buffer.

    public frmBatchDownloader() {
        InitializeComponent();
        LoadLanguage();
        lvBatchDownloadQueue.SmallImageList = Program.BatchStatusImages;
    }

    [System.Diagnostics.DebuggerStepThrough]
    protected override void WndProc(ref Message m) {
        switch (m.Msg) {
            case NativeMethods.WM_CLIPBOARDUPDATE: {
                if (Clipboard.ContainsText()) {
                    ClipboardData = Clipboard.GetText();
                    if (!chkBatchDownloadClipboardScanVerifyLinks.Checked || DownloadHelper.SupportedDownloadLink(ClipboardData)) {
                        AddItemToList(ClipboardData);
                    }
                    ClipboardData = null;
                }
            } break;
        }
        base.WndProc(ref m);
    }

    protected internal void ApplicationExit(object sender, EventArgs e) {
        if (ClipboardScannerActive && NativeMethods.RemoveClipboardFormatListener(this.Handle))
            ClipboardScannerActive = false;
    }

    public override void LoadLanguage() {
        this.Text = Language.frmBatchDownload;
        lbBatchDownloadLink.Text = Language.lbBatchDownloadLink;
        lbBatchDownloadType.Text = Language.lbBatchDownloadType;
        lbBatchVideoSpecificArgument.Text = Language.lbBatchDownloadVideoSpecificArgument;
        btnBatchDownloadAdd.Text = Language.GenericAdd;
        sbBatchDownloadLoadArgs.Text = Language.sbBatchDownloadLoadArgs;
        mBatchDownloaderLoadArgsFromSettings.Text = Language.mBatchDownloaderLoadArgsFromSettings;
        mBatchDownloaderLoadArgsFromArgsTxt.Text = Language.mBatchDownloaderLoadArgsFromArgsTxt;
        mBatchDownloaderLoadArgsFromFile.Text = Language.mBatchDownloaderLoadArgsFromFile;
        mBatchDownloaderImportLinksFromFile.Text = Language.mBatchDownloaderImportLinksFromFile;
        mBatchDownloaderImportLinksFromClipboard.Text = Language.mBatchDownloaderImportLinksFromClipboard;
        btnBatchDownloadRemoveSelected.Text = Language.GenericRemoveSelected;
        btnBatchDownloadStartStopExit.Text = Language.GenericStart;
        lvBatchDownloadQueue.Columns[1].Text = Language.lbBatchDownloadType;
        lvBatchDownloadQueue.Columns[2].Text = Language.lbBatchDownloadVideoSpecificArgument;
        cbBatchDownloadType.Items.Add(Language.GenericVideo);
        cbBatchDownloadType.Items.Add(Language.GenericAudio);
        cbBatchDownloadType.Items.Add(Language.GenericCustom);
        sbBatchDownloaderImportLinks.Text = Language.sbBatchDownloaderImportLinks;
        chkBatchDownloadClipboardScanner.Text = Language.chkBatchDownloadClipboardScanner;
        chkBatchDownloadClipboardScanVerifyLinks.Text = Language.GenericVerifyLinks;
        if (AbortDownload) {
            sbBatchDownloader.Text = Language.sbBatchDownloaderAborted;
        }
        else {
            sbBatchDownloader.Text = InProgress ? Language.sbBatchDownloaderDownloading : Language.sbBatchDownloaderIdle;
        }
    }

    private void frmBatchDownloader_Load(object sender, EventArgs e) {
        cbBatchDownloadType.SelectedIndex = Batch.SelectedType;
        if (Batch.SelectedType == 0) {
            chkBatchDownloaderSoundVBR.Checked = Batch.DownloadVideoSound;
            cbBatchQuality.SelectedIndex = Batch.SelectedVideoQuality;
            cbBatchFormat.SelectedIndex = Batch.SelectedVideoFormat;
        }
        else if (Batch.SelectedType == 1) {
            if (Batch.DownloadAudioVBR) {
                chkBatchDownloaderSoundVBR.Checked = true;
                cbBatchQuality.SelectedIndex = Batch.SelectedAudioQualityVBR;
            }
            else {
                chkBatchDownloaderSoundVBR.Checked = false;
                cbBatchQuality.SelectedIndex = Batch.SelectedAudioQuality;
            }
            cbBatchFormat.SelectedIndex = Batch.SelectedAudioFormat;
        }

        if (Saved.BatchDownloaderLocation.Valid) {
            this.StartPosition = FormStartPosition.Manual;
            this.Location = Saved.BatchDownloaderLocation;
        }
        chkBatchDownloadClipboardScanVerifyLinks.Checked = Batch.ClipboardScannerVerifyLinks;
    }

    private void frmBatchDownloader_FormClosing(object sender, FormClosingEventArgs e) {
        if (this.WindowState == FormWindowState.Minimized) {
            this.Opacity = 0;
            this.WindowState = FormWindowState.Normal;
        }
        Batch.ClipboardScannerVerifyLinks = chkBatchDownloadClipboardScanVerifyLinks.Checked;
        Saved.BatchDownloaderLocation = this.Location;
        this.Dispose();
    }

    private void txtBatchDownloadLink_TextChanged(object sender, EventArgs e) {
        btnBatchDownloadAdd.Enabled = !string.IsNullOrWhiteSpace(txtBatchDownloadLink.Text) && cbBatchDownloadType.SelectedIndex > -1;
    }

    private void txtBatchDownloadLink_KeyPress(object sender, KeyPressEventArgs e) {
        if (e.KeyChar == 13) {
            AddItemToList(txtBatchDownloadLink.Text);
            txtBatchDownloadLink.Clear();
        }
    }

    private void btnBatchDownloadAdd_Click(object sender, EventArgs e) {
        AddItemToList(txtBatchDownloadLink.Text);
        txtBatchDownloadLink.Clear();
    }

    private void btnBatchDownloadRemoveSelected_Click(object sender, EventArgs e) {
        RemoveItemsFromList();
    }

    private void sbBatchDownloadLoadArgs_Click(object sender, EventArgs e) {
        if (CustomArguments.YtdlArguments.Count > 0) {
            CustomArguments.YtdlArguments.For((Arg) => cbArguments.Items.Add(Arg));
            cbArguments.SelectedIndex = Saved.CustomArgumentsIndex;
        }
    }

    private void mBatchDownloaderLoadArgsFromSettings_Click(object sender, EventArgs e) {
        if (CustomArguments.YtdlArguments.Count > 0) {
            CustomArguments.YtdlArguments.For((Arg) => cbArguments.Items.Add(Arg));
            cbArguments.SelectedIndex = Saved.CustomArgumentsIndex;
        }
    }

    private void mBatchDownloaderLoadArgsFromArgsTxt_Click(object sender, EventArgs e) {
        if (System.IO.File.Exists(Environment.CurrentDirectory + "\\args.txt")) {
            cbArguments.Items.AddRange(System.IO.File.ReadAllLines(Environment.CurrentDirectory + "\\args.txt"));
            if (Saved.CustomArgumentsIndex > -1 && Saved.CustomArgumentsIndex <= cbArguments.Items.Count) {
                cbArguments.SelectedIndex = Saved.CustomArgumentsIndex;
            }
        }
    }

    private void mBatchDownloaderLoadArgsFromFile_Click(object sender, EventArgs e) {
        using OpenFileDialog ofd = new();
        ofd.Title = "Select a file to read as arguments";
        ofd.Filter = "All files (*.*)|*.*";
        if (ofd.ShowDialog() == DialogResult.OK) {
            if (System.IO.File.Exists(ofd.FileName)) {
                cbArguments.Text = System.IO.File.ReadAllText(ofd.FileName).Trim(' ').Replace('\n', ' ').Trim(' ');
            }
        }
    }

    private void lvBatchDownloadQueue_KeyDown(object sender, KeyEventArgs e) {
        if (e.Control && e.KeyCode == Keys.A) {
            for (int i = 0; i < lvBatchDownloadQueue.Items.Count; i++) {
                lvBatchDownloadQueue.Items[i].Selected = true;
            }
        }
    }

    private void lvBatchDownloadQueue_KeyUp(object sender, KeyEventArgs e) {
        if (e.KeyCode == Keys.Delete) {
            RemoveItemsFromList();
        }
    }

    private void lvBatchDownloadQueue_SelectedIndexChanged(object sender, EventArgs e) {
        //cbBatchDownloadType.SelectedIndexChanged -= cbBatchDownloadType_SelectedIndexChanged;
        if (lvBatchDownloadQueue.SelectedIndices.Count > 0) {
            //btnBatchDownloadRemoveSelected.Enabled = true;
            //if (lvBatchDownloadQueue.SelectedIndices.Count > 1) {
            //    cbBatchDownloadType.SelectedIndex = -1;
            //}
            //else {
            //    for (int i = lvBatchDownloadQueue.Items.Count - 1; i >= 0; i--) {
            //        if (lvBatchDownloadQueue.Items[i].Selected) {
            //            cbBatchDownloadType.SelectedIndex = cbBatchDownloadType.Items.IndexOf(lvBatchDownloadQueue.Items[i].SubItems[1].Text);
            //        }
            //    }
            //}
            btnBatchDownloadRemoveSelected.Enabled = !InProgress;
        }
        else {
            btnBatchDownloadRemoveSelected.Enabled = false;
        }
        //cbBatchDownloadType.SelectedIndexChanged += cbBatchDownloadType_SelectedIndexChanged;
    }

    private void chkBatchDownloaderSoundVBR_CheckedChanged(object sender, EventArgs e) {
        if (cbBatchDownloadType.SelectedIndex == 1) {
            cbBatchQuality.SelectedIndex = -1;
            cbBatchQuality.Items.Clear();
            if (chkBatchDownloaderSoundVBR.Checked) {
                cbBatchQuality.Items.AddRange(new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" });
                cbBatchQuality.SelectedIndex = Batch.SelectedAudioQualityVBR;
            }
            else {
                cbBatchQuality.Items.AddRange(Formats.AudioQualityNamesArray);
                cbBatchQuality.SelectedIndex = Batch.SelectedAudioQualityVBR;
            }
        }
    }

    private void cbBatchDownloadType_SelectedIndexChanged(object sender, EventArgs e) {
        btnBatchDownloadAdd.Enabled = !string.IsNullOrWhiteSpace(txtBatchDownloadLink.Text) && cbBatchDownloadType.SelectedIndex > -1;

        if (cbBatchDownloadType.SelectedIndex > -1) {
            sbBatchDownloaderImportLinks.Enabled = true;
            cbBatchQuality.SelectedIndex = -1;
            cbBatchFormat.SelectedIndex = -1;
            cbBatchQuality.Items.Clear();
            cbBatchFormat.Items.Clear();

            switch (cbBatchDownloadType.SelectedIndex) {
                case 0: {
                    cbBatchQuality.Items.AddRange(Formats.VideoQualityArray);
                    cbBatchFormat.Items.AddRange(Formats.VideoFormatsNamesArray);
                    cbBatchQuality.SelectedIndex = Batch.SelectedVideoQuality;
                    cbBatchFormat.SelectedIndex = Batch.SelectedVideoFormat;
                    chkBatchDownloaderSoundVBR.Text = Language.GenericSound;
                    chkBatchDownloaderSoundVBR.Checked = Batch.DownloadVideoSound;

                    SetControls(false);
                } break;

                case 1: {
                    if (Batch.DownloadAudioVBR) {
                        cbBatchQuality.Items.AddRange(new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" });
                        cbBatchQuality.SelectedIndex = Batch.SelectedAudioQualityVBR;
                    }
                    else {
                        cbBatchQuality.Items.AddRange(Formats.AudioQualityNamesArray);
                        cbBatchQuality.SelectedIndex = Batch.SelectedAudioQuality;
                    }
                    cbBatchFormat.Items.AddRange(Formats.AudioFormatsArray);
                    cbBatchFormat.SelectedIndex = Batch.SelectedAudioFormat;
                    chkBatchDownloaderSoundVBR.Text = "VBR";
                    chkBatchDownloaderSoundVBR.Checked = Batch.DownloadAudioVBR;

                    SetControls(false);
                } break;

                case 2: {
                    SetControls(true);
                } break;
            }

            //if (lvBatchDownloadQueue.SelectedItems.Count > 0) {
            //    for (int i = 0; i < lvBatchDownloadQueue.SelectedItems.Count; i++) {
            //        lvBatchDownloadQueue.SelectedItems[i].SubItems[1].Text = cbBatchDownloadType.GetItemText(cbBatchDownloadType.SelectedItem);
            //    }
            //}
        }
    }

    private void btnBatchDownloadStartStopExit_Click(object sender, EventArgs e) {
        if (InProgress) {
            Downloader.Invoke((Action)delegate {
                Downloader.RetryOrAbort();
            });
        }
        else if (DownloadUrls.Count > 0) {
            Log.Write($"Starting batch download with {DownloadUrls.Count} links to download.");
            InProgress = true;
            AbortDownload = false;
            btnBatchDownloadRemoveSelected.Enabled = false;
            btnBatchDownloadStartStopExit.Text = Language.GenericStop;
            string BatchTime = BatchHelper.CurrentTime;
            DownloadThread = new(() => {
                for (int i = 0; i < DownloadUrls.Count; i++) {
                    NewInfo = new DownloadInfo(DownloadUrls[i]) {
                        BatchDownload = true,
                        BatchTime = BatchTime,
                    };
                    switch (DownloadTypes[i]) {
                        case 0:
                            NewInfo.Type = DownloadType.Video;
                            NewInfo.VideoQuality = (VideoQualityType)DownloadQuality[i];
                            NewInfo.VideoFormat = (VideoFormatType)DownloadFormat[i];
                            NewInfo.SkipAudioForVideos = !DownloadSoundVBR[i];
                            break;
                        case 1:
                            NewInfo.Type = DownloadType.Audio;
                            if (DownloadSoundVBR[i]) {
                                NewInfo.UseVBR = true;
                                NewInfo.AudioVBRQuality = (AudioVBRQualityType)DownloadQuality[i];
                            }
                            else {
                                NewInfo.UseVBR = false;
                                NewInfo.AudioCBRQuality = (AudioCBRQualityType)DownloadQuality[i];
                            }
                            NewInfo.AudioFormat = (AudioFormatType)DownloadFormat[i];
                            break;
                        case 2:
                            NewInfo.Type = DownloadType.Custom;
                            NewInfo.CustomArguments = DownloadArgs[i];
                            break;
                        default:
                            continue;
                    }
                    this.Invoke((Action)delegate {
                        lvBatchDownloadQueue.Items[i].ImageIndex = (int)StatusIcon.Processing;
                        sbBatchDownloader.Text = Language.sbBatchDownloaderDownloading;
                    });
                    Downloader = new frmDownloader(NewInfo);
                    switch (Downloader.ShowDialog()) {
                        case DialogResult.Yes:
                            this.Invoke((Action)delegate {
                                lvBatchDownloadQueue.Items[i].ImageIndex = (int)StatusIcon.Finished;
                            });
                            break;
                        case DialogResult.No:
                            this.Invoke((Action)delegate {
                                lvBatchDownloadQueue.Items[i].ImageIndex = (int)StatusIcon.Errored;
                            });
                            break;
                        case DialogResult.Abort:
                            this.Invoke((Action)delegate {
                                lvBatchDownloadQueue.Items[i].ImageIndex = (int)StatusIcon.Waiting;
                            });
                            Log.Write($"Batch download aborted, {i} conversion finished.");
                            AbortDownload = true;
                            break;
                        case DialogResult.Ignore:
                            this.Invoke((Action)delegate {
                                lvBatchDownloadQueue.Items[i].ImageIndex = (int)StatusIcon.Waiting;
                            });
                            break;
                        default:
                            this.Invoke((Action)delegate {
                                lvBatchDownloadQueue.Items[i].ImageIndex = (int)StatusIcon.Finished;
                            });
                            break;
                    }
                    if (AbortDownload) { break; }
                }
                this.Invoke((Action)delegate {
                    if (AbortDownload) {
                        sbBatchDownloader.Text = Language.sbBatchDownloaderAborted;
                    }
                    else {
                        sbBatchDownloader.Text = Language.sbBatchDownloaderFinished;
                    }
                    btnBatchDownloadStartStopExit.Text = Language.GenericStart;
                });
                System.Media.SystemSounds.Exclamation.Play();
                InProgress = false;
                Log.Write("Batch download finished running.");
            }) {
                Name = $"Batch download {BatchTime}"
            };
            DownloadThread.Start();
        }
    }

    private void SetControls(bool Custom) {
        cbArguments.Visible = Custom;
        cbBatchQuality.Visible = !Custom;
        cbBatchQuality.Enabled = !Custom;
        cbBatchFormat.Visible = !Custom;
        cbBatchFormat.Enabled = !Custom;
        pnAudioVBR.Visible = !Custom;
        chkBatchDownloaderSoundVBR.Enabled = !Custom;
        chkBatchDownloaderSoundVBR.Visible = !Custom;
        sbBatchDownloadLoadArgs.Visible = Custom;
    }

    private void AddItemToList(string URL) {
        if (!string.IsNullOrEmpty(URL) && cbBatchDownloadType.SelectedIndex != -1) {
            for (int i = 0; i < lvBatchDownloadQueue.Items.Count; i++) {
                if (lvBatchDownloadQueue.Items[i].Text[1..] == URL) {
                    System.Media.SystemSounds.Asterisk.Play();
                    return;
                }
            }
            ListViewItem lvi = new() {
                Checked = false,
                Name = URL
            };

            lvi.SubItems[0].Text = $" {URL}";
            switch (cbBatchDownloadType.SelectedIndex) {
                case -1:
                    System.Media.SystemSounds.Asterisk.Play();
                    return;
                case 0:
                    lvi.SubItems.Add("Video");
                    DownloadTypes.Add(0);
                    break;
                case 1:
                    lvi.SubItems.Add("Audio");
                    DownloadTypes.Add(1);
                    break;
                case 2:
                    lvi.SubItems.Add("Custom");
                    DownloadTypes.Add(2);
                    break;
            }
            if (cbBatchDownloadType.SelectedIndex != 2) {
                if (cbBatchDownloadType.SelectedIndex == 0) {
                    lvi.SubItems.Add($"Q: {cbBatchQuality.GetItemText(cbBatchQuality.SelectedItem)}, F: {cbBatchFormat.GetItemText(cbBatchFormat.SelectedItem)}, {(chkBatchDownloaderSoundVBR.Checked ? "sound" : "no sound")}");
                }
                else if (cbBatchDownloadType.SelectedIndex == 1) {
                    lvi.SubItems.Add($"Q: {cbBatchQuality.GetItemText(cbBatchQuality.SelectedItem)}, F: {cbBatchFormat.GetItemText(cbBatchFormat.SelectedItem)}, {(chkBatchDownloaderSoundVBR.Checked ? "vbr" : "no vbr")}");
                }
            }
            else {
                lvi.SubItems.Add(cbArguments.Text);
            }
            lvi.ImageIndex = (int)StatusIcon.Waiting;
            DownloadArgs.Add(cbArguments.Text);
            DownloadUrls.Add(URL);
            DownloadQuality.Add(cbBatchQuality.SelectedIndex);
            DownloadFormat.Add(cbBatchFormat.SelectedIndex);
            DownloadSoundVBR.Add(chkBatchDownloaderSoundVBR.Checked);
            lvBatchDownloadQueue.Items.Add(lvi);

            btnBatchDownloadStartStopExit.Enabled = true;
        }
    }

    private void RemoveItemsFromList() {
        if (lvBatchDownloadQueue.SelectedIndices.Count > 0 && !InProgress) {
            for (int i = lvBatchDownloadQueue.Items.Count - 1; i >= 0; i--) {
                if (lvBatchDownloadQueue.Items[i].Selected) {
                    lvBatchDownloadQueue.Items[i].Remove();
                    DownloadUrls.RemoveAt(i);
                    DownloadTypes.RemoveAt(i);
                    DownloadArgs.RemoveAt(i);
                    DownloadQuality.RemoveAt(i);
                    DownloadFormat.RemoveAt(i);
                    DownloadSoundVBR.RemoveAt(i);
                }
            }

            if (lvBatchDownloadQueue.Items.Count == 0) {
                btnBatchDownloadStartStopExit.Enabled = false;
            }
        }
    }

    private void sbImportLinks_Click(object sender, EventArgs e) {
        sbBatchDownloaderImportLinks.ShowDropDownMenu();
    }

    private void mBatchDownloaderImportLinksFromFile_Click(object sender, EventArgs e) {
        using OpenFileDialog ofd = new();
        ofd.Title = "Select a text file to import...";
        ofd.Filter = "Text document (*.txt)|*.txt";
        if (ofd.ShowDialog() == DialogResult.OK) {
            System.IO.StreamReader reader = new(ofd.FileName);
            string CurrentLine;
            while ((CurrentLine = reader.ReadLine()) != null) {
                AddItemToList(CurrentLine);
            }
        }
    }

    private void mBatchDownloadImportLinksFromClipboard_Click(object sender, EventArgs e) {
        if (Clipboard.ContainsText()) {
            string[] Data = Clipboard.GetText().Replace("\r\n", "\n").Split('\n');
            for (int i = 0; i < Data.Length; i++) {
                AddItemToList(Data[i]);
            }
        }
        else {
            Log.MessageBox("The clipboard does not contain text that can be added.");
        }
    }

    private void chkBatchDownloadClipboardScanner_CheckedChanged(object sender, EventArgs e) {
        if (chkBatchDownloadClipboardScanner.Checked) {
            if (!Batch.ClipboardScannerNoticeViewed) {
                if (Log.MessageBox(Language.dlgBatchDownloadClipboardScannerNotice, MessageBoxButtons.OKCancel) == DialogResult.Cancel) {
                    chkBatchDownloadClipboardScanner.Checked = false;
                    return;
                }
                else {
                    Batch.ClipboardScannerNoticeViewed = true;
                }
            }
            if (NativeMethods.AddClipboardFormatListener(this.Handle)) {
                Application.ApplicationExit += ApplicationExit;
                chkBatchDownloadClipboardScanVerifyLinks.Enabled = true;
                ClipboardScannerActive = true;
                Log.Write("Clipboard scanning for batch download queueing stopped.");
            }
            else {
                chkBatchDownloadClipboardScanner.Checked = false;
            }
        }
        else {
            if (ClipboardScannerActive) {
                if (NativeMethods.RemoveClipboardFormatListener(this.Handle)) {
                    Application.ApplicationExit -= ApplicationExit;
                    chkBatchDownloadClipboardScanVerifyLinks.Enabled = false;
                    ClipboardScannerActive = false;
                    Log.Write("Clipboard scanning for batch download queueing started.");
                }
            }
        }
    }
}