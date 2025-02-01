﻿#nullable enable
namespace youtube_dl_gui;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class frmMain : LocalizedForm {
    #region variables
    public bool ProtocolInput;

    private bool ClipboardScannerActive;
    private string? ClipboardData;
    #endregion

    #region form
    public frmMain() {
        InitializeComponent();
        if (Program.DebugMode) {
            trayIcon.Dispose();
        }
        else {
            mDownloadSubtitles.Enabled = mDownloadSubtitles.Visible = false;
            trayIcon.ContextMenu = cmTray;
            trayIcon.Visible = true;
            tcMain.TabPages.Remove(tabDebug);
            btnDebugExtendedConverter.Dispose();
        }

        //mDownloadSeparator.Enabled = mDownloadSeparator.Visible =
        //    mQuickDownloadForm.Enabled = mQuickDownloadForm.Visible =
        //    mQuickDownloadFormAuthentication.Enabled = mQuickDownloadFormAuthentication.Visible =
        //    mExtendedDownloadForm.Enabled = mExtendedDownloadForm.Visible = true;

        this.Shown += async (s, e) => {
            Log.Write("Startup finished.");
            if (General.CheckForUpdatesOnLaunch) {
                try {
                    switch (await Updater.CheckForUpdate(false)) {
                        case true: {
                            Updater.ShowUpdateForm(true);
                        } break;
                        case null: {
                            Log.Write("The initial update check returned null.");
                        } break;
                    }
                }
                catch (Exception ex) {
                    if (!(ex is ThreadAbortException or OperationCanceledException or TaskCanceledException))
                        Log.ReportException(ex);
                }
            }
            if (General.AutoUpdateYoutubeDl) {
                try {
                    if (await Updater.CheckForYoutubeDlUpdate())
                        Updater.UpdateYoutubeDl(Downloads.useYtdlUpdater, this.Location);
                }
                catch (Exception ex) {
                    if (!(ex is ThreadAbortException or OperationCanceledException or TaskCanceledException))
                        Log.ReportException(ex);
                }
            }
        };
    }

    [DebuggerStepThrough]
    protected override void WndProc(ref Message m) {
        switch (m.Msg) {
            case NativeMethods.WM_CLIPBOARDUPDATE: {
                if (Clipboard.ContainsText()) {
                    ClipboardData = Clipboard.GetText();
                    if (mClipboardAutoDownloadVerifyLinks.Checked && !DownloadHelper.SupportedDownloadLink(ClipboardData)) {
                        return;
                    }
                    txtUrl.Text = ClipboardData;
                    ClipboardData = null;
                    DownloadDefaults(false);
                }
                m.Result = IntPtr.Zero;
            } break;
            default: {
                base.WndProc(ref m);
            } break;
        }
    }

    private void frmMain_Load(object sender, EventArgs e) {
        if (Saved.MainFormLocation.Valid) {
            this.StartPosition = FormStartPosition.Manual;
            this.Location = Saved.MainFormLocation;
        }

        this.Size = Saved.MainFormSize.Valid ?
            Saved.MainFormSize : this.MinimumSize;

        LoadLanguage();

        //if (!Saved.DownloadCustomArguments.IsNullEmptyWhitespace()) {
        //    cbCustomArguments.Items.AddRange(Saved.DownloadCustomArguments.Split('|'));
        //    if (Saved.CustomArgumentsIndex > -1 && Saved.CustomArgumentsIndex <= cbCustomArguments.Items.Count - 1)
        //        cbCustomArguments.SelectedIndex = Saved.CustomArgumentsIndex;
        //}

        //switch (General.SaveCustomArgs) {
        //    case 1:
        //        if (System.IO.File.Exists(Environment.CurrentDirectory + "\\args.txt")) {
        //            cbCustomArguments.Items.AddRange(System.IO.File.ReadAllLines(Environment.CurrentDirectory + "\\args.txt"));
        //            if (Saved.CustomArgumentsIndex > -1 && Saved.CustomArgumentsIndex <= cbCustomArguments.Items.Count - 1) {
        //                cbCustomArguments.SelectedIndex = Saved.CustomArgumentsIndex;
        //            }
        //        }
        //        break;
        //    case 2:
        //        if (!string.IsNullOrWhiteSpace(Saved.DownloadCustomArguments)) {
        //            cbCustomArguments.Items.AddRange(Saved.DownloadCustomArguments.Split('|'));
        //            if (Saved.CustomArgumentsIndex > -1 && Saved.CustomArgumentsIndex <= cbCustomArguments.Items.Count - 1) {
        //                cbCustomArguments.SelectedIndex = Saved.CustomArgumentsIndex;
        //            }
        //        }
        //        break;
        //}

        switch (Saved.downloadType) {
            case 1:
                rbAudio.Checked = true;
                break;
            case 2:
                rbCustom.Checked = true;
                break;
            default:
                rbVideo.Checked = true;
                break;
        }

        if (CustomArguments.YtdlArguments.Count > 0) {
            CustomArguments.YtdlArguments.For((Arg) => cbCustomArguments.Items.Add(Arg));
            if (Saved.CustomArgumentsIndex < cbCustomArguments.Items.Count) {
                cbCustomArguments.SelectedIndex = rbCustom.Checked ? Saved.CustomArgumentsIndex : Saved.CustomArgumentsIndex + 1;
            }
        }

        mClipboardAutoDownloadVerifyLinks.Checked = cmTrayClipboardAutoDownloadVerifyLinks.Checked = General.ClipboardAutoDownloadVerifyLinks;

        if (!Saved.FileNameSchemaHistory.IsNullEmptyWhitespace()) {
            cbSchema.Items.AddRange(Saved.FileNameSchemaHistory.Split('|'));
        }
        int index = cbSchema.Items.IndexOf(Downloads.fileNameSchema);
        if (index > -1) {
            cbSchema.SelectedIndex = index;
        }
        else {
            cbSchema.Items.Add(Downloads.fileNameSchema);
            cbSchema.SelectedIndex = cbSchema.Items.Count - 1;
        }

        if (General.DeleteUpdaterOnStartup) {
            System.IO.File.Delete(Environment.CurrentDirectory + "\\youtube-dl-gui-updater.exe");
        }
        if (General.DeleteBackupOnStartup) {
            System.IO.File.Delete(Environment.CurrentDirectory + "\\youtube-dl-gui.old.exe");
        }
    }

    private void frmMain_FormClosing(object sender, FormClosingEventArgs e) {
        this.Opacity = 0;
        if (this.WindowState == FormWindowState.Minimized) {
            this.WindowState = FormWindowState.Normal;
        }

        chkUseSelection.Checked = false;
        Saved.MainFormSize = this.Size;
        Saved.CustomArgumentsIndex = rbCustom.Checked ? cbCustomArguments.SelectedIndex : cbCustomArguments.SelectedIndex - 1;

        if (rbVideo.Checked) {
            Saved.downloadType = 0;
        }
        else if (rbAudio.Checked) {
            Saved.downloadType = 1;
        }
        else if (rbCustom.Checked) {
            Saved.downloadType = 2;
        }
        else {
            Saved.downloadType = -1;
        }

        if (rbConvertVideo.Checked) {
            Saved.convertType = 0;
        }
        else if (rbConvertAudio.Checked) {
            Saved.convertType = 1;
        }
        else if (rbConvertCustom.Checked) {
            Saved.convertType = 2;
        }
        else if (rbConvertAutoFFmpeg.Checked) {
            Saved.convertType = 6;
        }
        else {
            Saved.convertType = -1;
        }

        Saved.MainFormLocation = this.Location;

        trayIcon.Visible = false;
    }

    public override void LoadLanguage() {
        mSettings.Text = Language.mSettings;
        mTools.Text = Language.mTools;
        mBatch.Text = Language.mBatch;
        mBatchDownload.Text = Language.mBatchDownload;
        mBatchExtendedDownload.Text = Language.mBatchExtendedDownload;
        mBatchConverter.Text = Language.mBatchConvert;
        mArchiveDownloader.Text = Language.mArchiveDownloader;
        mMerger.Text = Language.tabMerge;
        mDownloadSubtitles.Text = Language.mDownloadSubtitles;
        mMiscTools.Text = Language.mMiscTools;
        mClipboardAutoDownload.Text = Language.mClipboardAutoDownload;
        mClipboardAutoDownloadVerifyLinks.Text = Language.GenericVerifyLinks;
        mHelp.Text = Language.mHelp;
        mLanguage.Text = Language.mLanguage;
        mSupportedSites.Text = Language.mSupportedSites;
        mLog.Text = Language.frmLog;
        mAbout.Text = Language.mAbout;

        tabDownload.Text = Language.tabDownload;
        tabConvert.Text = Language.tabConvert;

        lbURL.Text = Language.lbURL;
        txtUrl.TextHint = Language.txtUrlHint;
        gbDownloadType.Text = Language.gbDownloadType;
        rbVideo.Text = Language.GenericVideo;
        rbAudio.Text = Language.GenericAudio;
        rbCustom.Text = Language.GenericCustom;
        lbQuality.Text = Language.lbQuality;
        lbFormat.Text = Language.lbFormat;
        chkDownloadSound.Text = Language.GenericSound;
        chkUseSelection.Text = Language.chkUseSelection;
        rbVideoSelectionPlaylistIndex.Text = Language.rbVideoSelectionPlaylistIndex;
        rbVideoSelectionPlaylistItems.Text = Language.rbVideoSelectionPlaylistItems;
        rbVideoSelectionBeforeDate.Text = Language.rbVideoSelectionBeforeDate;
        rbVideoSelectionOnDate.Text = Language.rbVideoSelectionOnDate;
        rbVideoSelectionAfterDate.Text = Language.rbVideoSelectionAfterDate;
        txtPlaylistStart.TextHint = Language.txtPlaylistStartHint;
        txtPlaylistEnd.TextHint = Language.txtPlaylistEndHint;
        txtPlaylistItems.TextHint = Language.txtPlaylistItemsHint;
        txtVideoDate.TextHint = Language.txtVideoDateHint;

        lbSchema.Text = Language.lbSettingsDownloadsFileNameSchema;
        lbCustomArguments.Text = Language.lbCustomArguments;
        sbDownload.Text = Language.sbDownload;
        mDownloadWithAuthentication.Text = Language.mDownloadWithAuthentication;
        mBatchDownloadFromFile.Text = Language.mBatchDownloadFromFile;
        mQuickDownloadForm.Text = Language.mQuickDownloadForm;
        mQuickDownloadFormAuthentication.Text = Language.mQuickDownloadFormAuthentication;
        mExtendedDownloadForm.Text = Language.mExtendedDownloadForm;
        mExtendedDownloadFormAuthentication.Text = Language.mExtendedDownloadFormAuthentication;

        lbConvertInput.Text = Language.lbConvertInput;
        lbConvertOutput.Text = Language.lbConvertOutput;
        rbConvertVideo.Text = Language.GenericVideo;
        rbConvertAudio.Text = Language.GenericAudio;
        rbConvertCustom.Text = Language.GenericCustom;
        rbConvertAuto.Text = Language.rbConvertAuto;
        rbConvertAutoFFmpeg.Text = Language.rbConvertAutoFFmpeg;
        btnConvert.Text = Language.btnConvert;

        cmTrayShowForm.Text = Language.cmTrayShowForm;
        cmTrayDownloader.Text = Language.cmTrayDownloader;
        cmTrayDownloadClipboard.Text = Language.cmTrayDownloadClipboard;
        cmTrayDownloadBestVideo.Text = Language.cmTrayDownloadBestVideo;
        cmTrayDownloadBestAudio.Text = Language.cmTrayDownloadBestAudio;
        cmTrayDownloadCustom.Text = Language.cmTrayDownloadCustom;
        cmTrayDownloadCustomTxtBox.Text = Language.cmTrayDownloadCustomTxtBox;
        cmTrayDownloadCustomTxt.Text = Language.cmTrayDownloadCustomTxt;
        cmTrayDownloadCustomSettings.Text = Language.cmTrayDownloadCustomSettings;
        cmTrayClipboardAutoDownload.Text = Language.mClipboardAutoDownload;
        cmTrayClipboardAutoDownloadVerifyLinks.Text = Language.GenericVerifyLinks;
        cmTrayConverter.Text = Language.cmTrayConverter;
        cmTrayConvertTo.Text = Language.cmTrayConvertTo;
        cmTrayConvertVideo.Text = Language.cmTrayConvertVideo;
        cmTrayConvertAudio.Text = Language.cmTrayConvertAudio;
        cmTrayConvertCustom.Text = Language.cmTrayConvertCustom;
        cmTrayConvertAutomatic.Text = Language.cmTrayConvertAutomatic;
        cmTrayConvertAutoFFmpeg.Text = Language.cmTrayConvertAutoFFmpeg;
        cmTrayExit.Text = Language.cmTrayExit;

        if (cbFormat.Items.Count > 0) {
            cbFormat.Items[0] = Language.GenericInputBest;
        }
        if (cbQuality.Items.Count > 0) {
            cbQuality.Items[0] = Language.GenericInputBest;
        }

        if (!rbCustom.Checked && cbCustomArguments.Items.Count > 0)
            cbCustomArguments.Items[0] = Language.GenericDoNotInclude;

        gbDownloadType.Size = new(
            rbVideo.Size.Width + 2 + rbAudio.Size.Width + (rbCustom.Size.Width - 2) + 12,
            gbDownloadType.Size.Height
        );
        gbDownloadType.Location = new(
            (tabDownload.Size.Width - gbDownloadType.Size.Width) / 2,
            gbDownloadType.Location.Y
        );

        rbVideo.Location = new(
            (gbDownloadType.Size.Width - (rbVideo.Size.Width + rbAudio.Size.Width + rbCustom.Size.Width)) / 2,
            rbVideo.Location.Y
        );
        rbAudio.Location = new(
            rbVideo.Location.X + rbVideo.Size.Width + 2,
            rbAudio.Location.Y
        );
        rbCustom.Location = new(
            rbAudio.Location.X + rbAudio.Size.Width + 2,
            rbCustom.Location.Y
        );

        gbSelection.Size = new(
            rbVideoSelectionBeforeDate.Size.Width + rbVideoSelectionOnDate.Size.Width + rbVideoSelectionAfterDate.Size.Width + 12,
            20
        );
        gbSelection.Location = new(
            (tabDownload.Size.Width - gbSelection.Size.Width) / 2,
            gbSelection.Location.Y
        );
        rbVideoSelectionPlaylistIndex.Location = new(
            (gbSelection.Size.Width - (rbVideoSelectionPlaylistIndex.Size.Width + rbVideoSelectionPlaylistItems.Size.Width)) / 2,
            rbVideoSelectionPlaylistIndex.Location.Y
        );
        rbVideoSelectionPlaylistItems.Location = new(
            rbVideoSelectionPlaylistIndex.Location.X + rbVideoSelectionPlaylistItems.Size.Width + 2,
            rbVideoSelectionPlaylistItems.Location.Y
        );
        rbVideoSelectionBeforeDate.Location = new(
            (gbSelection.Size.Width - (rbVideoSelectionBeforeDate.Size.Width + rbVideoSelectionOnDate.Size.Width + rbVideoSelectionAfterDate.Size.Width)) / 2,
            rbVideoSelectionBeforeDate.Location.Y
        );
        rbVideoSelectionOnDate.Location = new(
            rbVideoSelectionBeforeDate.Location.X + rbVideoSelectionBeforeDate.Size.Width + 2,
            rbVideoSelectionOnDate.Location.Y
        );
        rbVideoSelectionAfterDate.Location = new(
            rbVideoSelectionOnDate.Location.X + rbVideoSelectionOnDate.Width + 2,
            rbVideoSelectionAfterDate.Location.Y
        );

        rbConvertVideo.Location = new(
            (tabConvert.Size.Width - (rbConvertVideo.Size.Width + rbConvertAudio.Size.Width + rbConvertCustom.Size.Width + 2)) / 2,
            rbConvertVideo.Location.Y
        );
        rbConvertAudio.Location = new(
            rbConvertVideo.Location.X + rbConvertVideo.Width + 2,
            rbConvertVideo.Location.Y
        );
        rbConvertCustom.Location = new(
            rbConvertAudio.Location.X + rbConvertAudio.Size.Width + 2,
            rbConvertAudio.Location.Y
        );
        rbConvertAuto.Location = new(
            (tabConvert.Size.Width / 2) - ((rbConvertAuto.Width + rbConvertAutoFFmpeg.Width) / 2),
            rbConvertAuto.Location.Y
        );
        rbConvertAutoFFmpeg.Location = new(
            rbConvertAuto.Location.X + rbConvertAuto.Size.Width + 2,
            rbConvertAutoFFmpeg.Location.Y
        );
    }

    private void ToggleClipboardScanning() {
        if (ClipboardScannerActive) {
            if (NativeMethods.RemoveClipboardFormatListener(this.Handle)) {
                Application.ApplicationExit -= ApplicationExit;
                mClipboardAutoDownload.Checked = mClipboardAutoDownloadVerifyLinks.Enabled = cmTrayClipboardAutoDownload.Checked = cmTrayClipboardAutoDownloadVerifyLinks.Enabled = false;
                ClipboardScannerActive = false;
                Log.Write("Clipboard auto-download scanning stopped.");
            }
        }
        else {
            if (!General.ClipboardAutoDownloadNoticeRead) {
                if (Log.MessageBox(Language.dlgClipboardAutoDownloadNotice, MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                    return;

                General.ClipboardAutoDownloadNoticeRead = true;
            }
            if (NativeMethods.AddClipboardFormatListener(this.Handle)) {
                Application.ApplicationExit += ApplicationExit;
                ClipboardScannerActive = true;
                mClipboardAutoDownload.Checked = mClipboardAutoDownloadVerifyLinks.Enabled = cmTrayClipboardAutoDownload.Checked = cmTrayClipboardAutoDownloadVerifyLinks.Enabled = true;
                Log.Write("Clipboard auto-download scanning started.");
            }
        }
    }

    private void ToggleClipboardVerifyLinks() {
        mClipboardAutoDownloadVerifyLinks.Checked ^= true;
        cmTrayClipboardAutoDownloadVerifyLinks.Checked ^= true;
    }

    internal void RemoveTrayIcon() {
        if (trayIcon != null) {
            trayIcon.Visible = false;
        }
    }

    internal void ApplicationExit(object sender, EventArgs e) {
        if (ClipboardScannerActive && NativeMethods.RemoveClipboardFormatListener(this.Handle))
            ClipboardScannerActive = false;
    }
    #endregion

    #region main menu
    private void mSettings_Click(object sender, EventArgs e) {
        using frmSettings settings = new();
        settings.ShowDialog();
        cbSchema.Text = Downloads.fileNameSchema;
        cbSchema.Items.Clear();
        if (!string.IsNullOrEmpty(Saved.FileNameSchemaHistory)) {
            cbSchema.Items.AddRange(Saved.FileNameSchemaHistory.Split('|'));
        }

        //mDownloadSeparator.Enabled = mDownloadSeparator.Visible =
        //    mQuickDownloadForm.Enabled = mQuickDownloadForm.Visible =
        //    mQuickDownloadFormAuthentication.Enabled = mQuickDownloadFormAuthentication.Visible =
        //    mExtendedDownloadForm.Enabled = mExtendedDownloadForm.Visible = true;
    }

    private void mBatchDownload_Click(object sender, EventArgs e) {
        frmBatchDownloader BatchDownload = new();
        BatchDownload.Show();
    }
    private void mBatchExtendedDownload_Click(object sender, EventArgs e) {
        frmExtendedDownloader Batch = new();
        Batch.Show();
    }
    private void mBatchConverter_Click(object sender, EventArgs e) {
        frmBatchConverter BatchConvert = new();
        BatchConvert.Show();
    }
    private void mArchiveDownloader_Click(object sender, EventArgs e) {
        using frmArchiveDownloader ArchiveDownloader = new();
        ArchiveDownloader.ShowDialog();
    }
    private void mMerger_Click(object sender, EventArgs e) {
        frmMerger MergeForm = new();
        MergeForm.Show();
    }
    private void mDownloadSubtitles_Click(object sender, EventArgs e) {
        frmSubtitles downloadSubtitles = new();
        downloadSubtitles.ShowDialog();
    }
    private void mMiscTools_Click(object sender, EventArgs e) {
        using frmMiscTools tools = new();
        tools.ShowDialog();
    }
    private void mClipboardAutoDownload_Click(object sender, EventArgs e) {
        ToggleClipboardScanning();
    }
    private void mClipboardAutoDownloadVerifyLinks_Click(object sender, EventArgs e) {
        ToggleClipboardVerifyLinks();
    }

    private void mLanguage_Click(object sender, EventArgs e) {
        using frmLanguage LangPicker = new();
        LangPicker.ShowDialog();
    }
    private void mSupportedSites_Click(object sender, EventArgs e) {
        switch (Verification.GetYoutubeDlType()) {
            case (int)GitID.YoutubeDl:
            case (int)GitID.YoutubeDlNightly: {
                Process.Start("https://github.com/ytdl-org/youtube-dl/blob/master/docs/supportedsites.md");
            } break;
            default: {
                Process.Start("https://github.com/yt-dlp/yt-dlp/blob/master/supportedsites.md");
            } break;
        }
    }
    private void mLog_Click(object sender, EventArgs e) {
        Log.ShowLog();
    }

    private void mAbout_Click(object sender, EventArgs e) {
        if (Initialization.ScreenshotMode) {
            frmAbout about = new();
            about.Show();
        }
        else {
            using frmAbout about = new();
            about.ShowDialog();
        }
    }
    #endregion

    #region tray menu
    private void cmTrayShowForm_Click(object sender, EventArgs e) {
        this.Show();
    }

    private void cmTrayDownloadBestVideo_Click(object sender, EventArgs e) {
        if (!Clipboard.ContainsText()) {
            return;
        }

        DownloadInfo NewInfo = new(Clipboard.GetText()) {
            VideoQuality = (VideoQualityType)Saved.videoQuality,
            Type = 0,
        };
        frmDownloader Downloader = new(NewInfo);
        Downloader.Show();
    }
    private void cmTrayDownloadBestAudio_Click(object sender, EventArgs e) {
        if (!Clipboard.ContainsText()) {
            return;
        }

        DownloadInfo NewInfo = new(Clipboard.GetText()) {
            AudioCBRQuality = AudioCBRQualityType.best,
            Type = DownloadType.Audio,
        };
        frmDownloader Downloader = new(NewInfo);
        Downloader.Show();
    }

    private void cmTrayDownloadCustomTxtBox_Click(object sender, EventArgs e) {
        if (!Clipboard.ContainsText()) {
            return;
        }

        if (string.IsNullOrEmpty(cbCustomArguments.Text)) {
            System.Media.SystemSounds.Asterisk.Play();
            cbCustomArguments.Focus();
            return;
        }

        DownloadInfo NewInfo = new(Clipboard.GetText()) {
            Arguments = cbCustomArguments.Text,
            Type = DownloadType.Custom,
        };
        frmDownloader Downloader = new(NewInfo);
        Downloader.Show();
    }
    private void cmTrayDownloadCustomTxt_Click(object sender, EventArgs e) {
        if (!Clipboard.ContainsText()) {
            return;
        }

        if (!System.IO.File.Exists(Environment.CurrentDirectory + "\\args.txt")) {
            Log.MessageBox(Language.dlgMainArgsTxtDoesntExist);
            return;
        }
        if (string.IsNullOrEmpty(System.IO.File.ReadAllText(Environment.CurrentDirectory + "\\args.txt"))) {
            Log.MessageBox(Language.dlgMainArgsTxtIsEmpty);
            return;
        }

        DownloadInfo NewInfo = new(Clipboard.GetText()) {
            Arguments = System.IO.File.ReadAllLines(Environment.CurrentDirectory + "\\args.txt")[0],
            Type = DownloadType.Custom,
        };
        frmDownloader Downloader = new(NewInfo);
        Downloader.Show();
    }
    private void cmTrayDownloadCustomSettings_Click(object sender, EventArgs e) {
        if (Clipboard.ContainsText() || Saved.CustomArgumentsIndex < 0) {
            return;
        }

        if (cbCustomArguments.Items.Count < (rbCustom.Checked ? 1 : 2)) {
            Log.MessageBox(Language.dlgMainArgsNoneSaved);
            return;
        }

        DownloadInfo NewInfo = new(Clipboard.GetText()) {
            Arguments = cbCustomArguments.Items[Saved.CustomArgumentsIndex] as string,
            Type = DownloadType.Custom,
        };
        frmDownloader Downloader = new(NewInfo);
        Downloader.Show();
    }
    private void cmTrayClipboardAutoDownload_Click(object sender, EventArgs e) {
        ToggleClipboardScanning();
    }
    private void cmTrayClipboardAutoDownloadVerifyLinks_Click(object sender, EventArgs e) {
        ToggleClipboardVerifyLinks();
    }

    private void cmTrayConvertVideo_Click(object sender, EventArgs e) {
        convertFromTray(ConversionType.Video);
    }
    private void cmTrayConvertAudio_Click(object sender, EventArgs e) {
        convertFromTray(ConversionType.Audio);
    }
    private void cmTrayConvertCustom_Click(object sender, EventArgs e) {
        convertFromTray(ConversionType.Custom);
    }
    private void cmTrayConvertAutomatic_Click(object sender, EventArgs e) {
        convertFromTray();
    }
    private void cmTrayConvertAutoFFmpeg_Click(object sender, EventArgs e) {
        convertFromTray(ConversionType.FfmpegDefault);
    }

    private void cmTrayExit_Click(object sender, EventArgs e) {
        trayIcon.Visible = false;
        Environment.Exit(0);
    }
    #endregion

    #region downloader
    private void rbVideo_CheckedChanged(object sender, EventArgs e) {
        if (!rbVideo.Checked)
            return;

        //cbCustomArguments.Enabled = false;
        cbQuality.SelectedIndex = -1;
        cbQuality.Items.Clear();
        cbQuality.Items.AddRange(Formats.VideoQualityArray);
        cbQuality.Items[0] = Language.GenericInputBest;
        cbQuality.Items[^1] = Language.GenericInputWorst;
        cbFormat.SelectedIndex = -1;
        cbFormat.Items.Clear();
        cbFormat.Items.AddRange(Formats.VideoFormatsNamesArray);
        cbFormat.Items[0] = Language.GenericInputBest;
        cbQuality.Enabled = true;
        cbFormat.Enabled = true;
        chkDownloadSound.Enabled = true;
        chkDownloadSound.Text = Language.GenericSound;
        if (Downloads.SaveFormatQuality) {
            cbQuality.SelectedIndex = Saved.videoQuality;
            cbFormat.SelectedIndex = Saved.VideoFormat;
            chkDownloadSound.Checked = Downloads.VideoDownloadSound;
        }
        else {
            cbQuality.SelectedIndex = 0;
            cbFormat.SelectedIndex = 0;
        }
        if (cbCustomArguments.Items.Count == 0 || (string)cbCustomArguments.Items[0] != Language.GenericDoNotInclude) {
            cbCustomArguments.Items.Insert(0, Language.GenericDoNotInclude);
        }
    }
    private void rbAudio_CheckedChanged(object sender, EventArgs e) {
        if (!rbAudio.Checked)
            return;

        cbQuality.SelectedIndex = -1;
        cbFormat.SelectedIndex = -1;
        cbQuality.Items.Clear();
        if (Downloads.AudioDownloadAsVBR) {
            cbQuality.Items.AddRange(new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" });
        }
        else {
            cbQuality.Items.AddRange(Formats.AudioQualityNamesArray);
            cbQuality.Items[0] = Language.GenericInputBest;
            cbQuality.Items[^1] = Language.GenericInputWorst;
        }
        cbFormat.Items.Clear();
        cbFormat.Items.AddRange(Formats.AudioFormatsArray);
        cbFormat.Items[0] = Language.GenericInputBest;
        cbQuality.Enabled = true;
        cbFormat.Enabled = true;
        chkDownloadSound.Enabled = true;
        chkDownloadSound.Checked = Downloads.AudioDownloadAsVBR;
        chkDownloadSound.Text = "VBR";
        if (Downloads.SaveFormatQuality) {
            cbQuality.SelectedIndex = Saved.audioQuality;
            cbFormat.SelectedIndex = Saved.AudioFormat;
        }
        else {
            cbQuality.SelectedIndex = 0;
            cbFormat.SelectedIndex = 0;
        }
        if (cbCustomArguments.Items.Count == 0 || (string)cbCustomArguments.Items[0] != Language.GenericDoNotInclude) {
            cbCustomArguments.Items.Insert(0, Language.GenericDoNotInclude);
        }
    }
    private void rbCustom_CheckedChanged(object sender, EventArgs e) {
        if (!rbCustom.Checked)
            return;

        cbQuality.SelectedIndex = -1;
        cbFormat.SelectedIndex = -1;
        cbQuality.Enabled = false;
        cbFormat.Enabled = false;
        chkDownloadSound.Checked = false;
        chkDownloadSound.Enabled = false;
        if ((string)cbCustomArguments.Items[0] == Language.GenericDoNotInclude)
            cbCustomArguments.Items.RemoveAt(0);
        if (Downloads.SaveFormatQuality)
            cbCustomArguments.SelectedIndex = Saved.CustomArgumentsIndex;
    }
    private void chkDownloadSound_CheckedChanged(object sender, EventArgs e) {
        if (rbAudio.Checked) {
            cbQuality.Items.Clear();

            if (chkDownloadSound.Checked) {
                cbQuality.SelectedIndex = -1;
                cbQuality.Items.AddRange(Formats.VbrQualities);
                cbQuality.SelectedIndex = Downloads.SaveFormatQuality ? Saved.AudioVBRQuality : 0;
            }
            else {
                cbQuality.Items.AddRange(Formats.AudioQualityNamesArray);
                cbQuality.Items[0] = Language.GenericInputBest;
                cbQuality.SelectedIndex = Downloads.SaveFormatQuality ? Saved.audioQuality : 0;
            }
        }
    }
    private void chkUseSelection_CheckedChanged(object sender, EventArgs e) {
        // 375 minimum height

        // 86 difference

        // 274 ?? 446

        if (chkUseSelection.Checked) {
            //if (this.Size.Height < 446) {
            //    AddedHeight = (446 - this.Size.Height);
            //    gbSelection.Size = new Size(gbSelection.Size.Width, 106);
            //    this.Size = new Size(this.Width, this.Height + AddedHeight);
            //}
            //else {
            //    gbSelection.Size = new Size(gbSelection.Size.Width, 106);
            //}

            gbSelection.Size = new(gbSelection.Size.Width, 106);
            this.Size = new(this.Width, this.Height + 86);
            this.MinimumSize = new(this.MinimumSize.Width, this.MinimumSize.Height + 86);
        }
        else {
            //gbSelection.Size = new Size(gbSelection.Size.Width, 20);
            //if (this.Size.Height > 446) {
            //    this.Size = new Size(this.Width, this.Height - 86);
            //}
            //else {
            //    this.Size = new Size(this.Width, this.Height - AddedHeight);
            //}

            gbSelection.Size = new(gbSelection.Size.Width, 20);
            this.MinimumSize = new(this.MinimumSize.Width, this.MinimumSize.Height - 86);
            this.Size = new(this.Width, this.Height - 86);
        }
    }
    private void txtPlaylistItems_KeyPress(object sender, KeyPressEventArgs e) {
        if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)8 && e.KeyChar != ',') {
            e.Handled = true;
        }
    }
    private void txtVideoDate_KeyPress(object sender, KeyPressEventArgs e) {
        if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)8) {
            e.Handled = true;
        }
    }
    private void rbVideoSelectionPlaylistIndex_CheckedChanged(object sender, EventArgs e) {
        if (rbVideoSelectionPlaylistIndex.Checked) {
            panelPlaylistStartEnd.Visible = true;
            panelPlaylistItems.Visible = false;
            panelDate.Visible = false;
            chkUseSelection.Checked = true;
        }
    }
    private void rbVideoSelectionPlaylistItems_CheckedChanged(object sender, EventArgs e) {
        if (rbVideoSelectionPlaylistItems.Checked) {
            panelPlaylistStartEnd.Visible = false;
            panelPlaylistItems.Visible = true;
            panelDate.Visible = false;
            chkUseSelection.Checked = true;
        }
    }
    private void rbVideoSelectionBeforeDate_CheckedChanged(object sender, EventArgs e) {
        if (rbVideoSelectionBeforeDate.Checked) {
            panelPlaylistStartEnd.Visible = false;
            panelPlaylistItems.Visible = false;
            panelDate.Visible = true;
            chkUseSelection.Checked = true;
        }
    }
    private void rbVideoSelectionOnDate_CheckedChanged(object sender, EventArgs e) {
        if (rbVideoSelectionOnDate.Checked) {
            panelPlaylistStartEnd.Visible = false;
            panelPlaylistItems.Visible = false;
            panelDate.Visible = true;
            chkUseSelection.Checked = true;
        }
    }
    private void rbVideoSelectionAfterDate_CheckedChanged(object sender, EventArgs e) {
        if (rbVideoSelectionAfterDate.Checked) {
            panelPlaylistStartEnd.Visible = false;
            panelPlaylistItems.Visible = false;
            panelDate.Visible = true;
            chkUseSelection.Checked = true;
        }
    }

    private void txtUrl_MouseEnter(object sender, EventArgs e) {
        if (General.HoverOverURLTextBoxToPaste && txtUrl.Text != Clipboard.GetText()) {
            txtUrl.Text = Clipboard.GetText();
        }
    }
    private void txtUrl_KeyDown(object sender, KeyEventArgs e) {
        if (e.KeyCode == Keys.Return)
                DownloadDefaults(false);
    }
    private void txtUrl_TextChanged(object sender, EventArgs e) {
        //btnMainYtdlpExtended.Enabled = txtUrl.Text.Length > 0;
    }
    private void cbSchema_KeyPress(object sender, KeyPressEventArgs e) {
        switch (e.KeyChar) {
            case ':': case '*': case '?':
            case '"': case '<': case '>':
            case '|': {
                System.Media.SystemSounds.Beep.Play();
                e.Handled = true;
            } break;
        }
    }
    private void cbCustomArguments_KeyDown(object sender, KeyEventArgs e) {
        switch (e.KeyCode) {
            case Keys.OemPipe when e.Shift: {
                e.Handled = e.SuppressKeyPress = true;
                System.Media.SystemSounds.Exclamation.Play();
            } break;
        }
    }
    private void sbDownload_Click(object sender, EventArgs e) {
        DownloadDefaults(false);
    }
    private void btnSaveTo_Click(object sender, EventArgs e) {
        using BetterFolderBrowserNS.BetterFolderBrowser fbd = new() {
            Multiselect = false,
            RootFolder = Downloads.downloadPath,
        };

        if (fbd.ShowDialog() != DialogResult.OK)
            return;

        Downloads.downloadPath = fbd.SelectedPath;
    }
    private void mDownloadWithAuthentication_Click(object sender, EventArgs e) {
        DownloadDefaults(true);
    }
    private void mBatchDownloadFromFile_Click(object sender, EventArgs e) {
        if (!Downloads.SkipBatchTip) {
            switch (Log.MessageBox(Language.msgBatchDownloadFromFile, MessageBoxButtons.YesNoCancel)) {
                case DialogResult.Cancel:
                    return;
                case DialogResult.Yes:
                    Downloads.SkipBatchTip = true;
                    break;
            }
        }

        string TextFile = string.Empty;
        using OpenFileDialog ofd = new();
        ofd.Filter = "Text Document (*.txt)|*.txt";
        ofd.Title = "Select a file with URLs...";
        ofd.Multiselect = false;
        ofd.CheckFileExists = true;
        ofd.CheckPathExists = true;
        if (ofd.ShowDialog() == DialogResult.OK) {
            TextFile = ofd.FileName;
        }
        else {
            return;
        }

        Thread BatchThread = new(() => {
            string videoArguments = string.Empty;
            DownloadType Type = DownloadType.None;
            int BatchQuality = 0;
            string schema = string.Empty;

            this.Invoke((Action)delegate {
                if (!chkDownloadSound.Checked) { videoArguments += "-nosound"; }
                BatchQuality = cbQuality.SelectedIndex;
                if (!string.IsNullOrWhiteSpace(cbSchema.Text)) {
                    schema = cbSchema.Text;
                    if (!Saved.FileNameSchemaHistory.Contains(cbSchema.Text)) {
                        cbSchema.Items.Add(cbSchema.Text);
                        if (Saved.FileNameSchemaHistory == null) {
                            Saved.FileNameSchemaHistory = cbSchema.Text;
                        }
                        else {
                            Saved.FileNameSchemaHistory += "|" + cbSchema.Text;
                        }
                    }
                }
                if (rbVideo.Checked) { Type = DownloadType.Video; }
                else if (rbAudio.Checked) { Type = DownloadType.Audio; }
                else if (rbCustom.Checked) { Type = DownloadType.Custom; }
                else { Type = DownloadType.Unknown; }
            });

            if (System.IO.File.Exists(TextFile)) {
                string[] ReadFile = System.IO.File.ReadAllLines(TextFile);
                if (ReadFile.Length == 0) {
                    return;
                }
                for (int i = 0; i < ReadFile.Length; i++) {
                    DownloadInfo NewInfo = new(ReadFile[i].Trim()) {
                        BatchDownload = true,
                        FileNameSchema = schema
                    };
                    switch (Type) {
                        case DownloadType.Video:
                            if (!chkDownloadSound.Checked) {
                                NewInfo.SkipAudioForVideos = true;
                            }
                            NewInfo.Arguments = videoArguments;
                            NewInfo.VideoQuality = (VideoQualityType)BatchQuality;
                            NewInfo.Type = DownloadType.Video;
                            break;
                        case DownloadType.Audio:
                            if (chkDownloadSound.Checked) {
                                NewInfo.AudioVBRQuality = (AudioVBRQualityType)BatchQuality;
                            }
                            else {
                                NewInfo.AudioCBRQuality = (AudioCBRQualityType)BatchQuality;
                            }
                            NewInfo.Type = DownloadType.Audio;
                            break;
                        case DownloadType.Custom:
                            NewInfo.Arguments = cbCustomArguments.Text;
                            NewInfo.Type = DownloadType.Custom;
                            break;
                        case DownloadType.Unknown:
                            NewInfo.VideoQuality = 0;
                            NewInfo.Type = DownloadType.Video;
                            break;
                    }
                    using frmDownloader Downloader = new(NewInfo);
                    Downloader.ShowDialog();
                    if (Downloader.DialogResult == DialogResult.Abort) {
                        break;
                    }
                }
            }
        }) {
            Name = "Batch download"
        };
        BatchThread.Start();
    }
    private void mQuickDownloadForm_Click(object sender, EventArgs e) {
        StartDownload(
            Extended: false,
            Authenticate: false);
    }
    private void mQuickDownloadFormAuthentication_Click(object sender, EventArgs e) {
        StartDownload(
            Extended: false,
            Authenticate: true);
    }
    private void mExtendedDownloadForm_Click(object sender, EventArgs e) {
        StartDownload(
            Extended: true,
            Authenticate: false);
    }
    private void mExtendedDownloadFormAuthentication_Click(object sender, EventArgs e) {
        StartDownload(
            Extended: true,
            Authenticate: true);
    }

    private void DownloadDefaults(bool auth) =>
        StartDownload(Downloads.ExtendedDownloaderPreferExtendedForm, auth);
    private void StartDownload(bool Extended, bool Authenticate) {
        if (txtUrl.Text.IsNullEmptyWhitespace()) {
            txtUrl.Focus();
            System.Media.SystemSounds.Exclamation.Play();
            return;
        }

        string URL = txtUrl.Text;//.Replace("\\", "-");

        AuthenticationDetails? Auth = null;
        if (Authenticate) {
            Auth = AuthenticationDetails.GetAuthentication();
            if (Auth is null) {
                return;
            }
        }

        Form Downloader;

        if (Extended) {
            string? Arguments = null;
            if (Downloads.ExtendedDownloaderIncludeCustomArguments && ((rbVideo.Checked || rbAudio.Checked) && cbCustomArguments.SelectedIndex > 0)) {
                Arguments = cbCustomArguments.Text.IsNullEmptyWhitespace() ? string.Empty : cbCustomArguments.Text;

                if (!cbCustomArguments.Items.Contains(cbCustomArguments.Text) && !cbCustomArguments.Text.IsNullEmptyWhitespace()) {
                    cbCustomArguments.SelectedIndex = cbCustomArguments.Items.Add(cbCustomArguments.Text);
                    CustomArguments.AddYtdlArgument(cbCustomArguments.Text, true);
                }
            }

            Downloader = new frmExtendedDownloader(
                URL,
                Arguments,
                false,
                Auth);
        }
        else {
            DownloadInfo NewInfo = new(URL);
            if (!rbCustom.Checked) {
                if (chkUseSelection.Checked) {
                    if (rbVideoSelectionPlaylistIndex.Checked && (txtPlaylistStart.Text.Length > 0 || txtPlaylistEnd.Text.Length > 0)) {
                        NewInfo.PlaylistSelection = PlaylistSelectionType.PlaylistStartPlaylistEnd;
                        if (int.TryParse(txtPlaylistStart.Text, out int PlaylistStart)) {
                            NewInfo.PlaylistSelectionIndexStart = PlaylistStart;
                        }
                        if (int.TryParse(txtPlaylistEnd.Text, out int PlaylistEnd)) {
                            NewInfo.PlaylistSelectionIndexEnd = PlaylistEnd;
                        }
                    }
                    else if (rbVideoSelectionPlaylistItems.Checked && txtPlaylistItems.Text.Length > 0) {
                        NewInfo.PlaylistSelection = PlaylistSelectionType.PlaylistItems;
                        NewInfo.PlaylistSelectionArg = txtPlaylistItems.Text;
                    }
                    else if (rbVideoSelectionBeforeDate.Checked && txtVideoDate.Text.Length > 0) {
                        NewInfo.PlaylistSelection = PlaylistSelectionType.DateBefore;
                        NewInfo.PlaylistSelectionArg = txtVideoDate.Text;
                    }
                    else if (rbVideoSelectionOnDate.Checked && txtVideoDate.Text.Length > 0) {
                        NewInfo.PlaylistSelection = PlaylistSelectionType.DateDuring;
                        NewInfo.PlaylistSelectionArg = txtVideoDate.Text;
                    }
                    else if (rbVideoSelectionAfterDate.Checked && txtVideoDate.Text.Length > 0) {
                        NewInfo.PlaylistSelection = PlaylistSelectionType.DateAfter;
                        NewInfo.PlaylistSelectionArg = txtVideoDate.Text;
                    }
                }

                if (rbVideo.Checked) {
                    NewInfo.VideoQuality = (VideoQualityType)cbQuality.SelectedIndex;
                    NewInfo.VideoFormat = (VideoFormatType)cbFormat.SelectedIndex;
                    NewInfo.Type = DownloadType.Video;
                    NewInfo.SkipAudioForVideos = !chkDownloadSound.Checked;

                    Saved.downloadType = (int)DownloadType.Video;
                    Saved.videoQuality = cbQuality.SelectedIndex;
                    Saved.VideoFormat = cbFormat.SelectedIndex;
                    Downloads.VideoDownloadSound = chkDownloadSound.Checked;
                }
                else if (rbAudio.Checked) {
                    NewInfo.Type = DownloadType.Audio;
                    if (chkDownloadSound.Checked) {
                        NewInfo.AudioVBRQuality = (AudioVBRQualityType)cbQuality.SelectedIndex;
                        Saved.AudioVBRQuality = cbQuality.SelectedIndex;
                    }
                    else {
                        NewInfo.AudioCBRQuality = (AudioCBRQualityType)cbQuality.SelectedIndex;
                        Saved.audioQuality = cbQuality.SelectedIndex;
                    }
                    NewInfo.UseVBR = chkDownloadSound.Checked;
                    NewInfo.AudioFormat = (AudioFormatType)cbFormat.SelectedIndex;

                    Saved.downloadType = (int)DownloadType.Audio;
                    Saved.audioQuality = cbQuality.SelectedIndex;
                    Saved.AudioFormat = cbFormat.SelectedIndex;
                    Downloads.AudioDownloadAsVBR = chkDownloadSound.Checked;
                }
                else {
                    throw new Exception("Video, Audio, or Custom was not selected in the form, please select an actual download option to proceed.");
                }
            }
            else {
                NewInfo.Type = DownloadType.Custom;
                NewInfo.Arguments = cbCustomArguments.Text;
                if (!cbCustomArguments.Text.IsNullEmptyWhitespace() && !cbCustomArguments.Items.Contains(cbCustomArguments.Text)) {
                    cbCustomArguments.SelectedIndex = cbCustomArguments.Items.Add(cbCustomArguments.Text);
                    CustomArguments.AddYtdlArgument(cbCustomArguments.Text, true);
                }

                Saved.downloadType = (int)DownloadType.Custom;
            }

            if ((rbVideo.Checked || rbAudio.Checked) && cbCustomArguments.SelectedIndex != 0) {
                if (!cbCustomArguments.Text.IsNullEmptyWhitespace()) {
                    NewInfo.Arguments = cbCustomArguments.Text;
                    if (!cbCustomArguments.Items.Contains(cbCustomArguments.Text)) {
                        cbCustomArguments.Items.Add(cbCustomArguments.Text);
                        CustomArguments.AddYtdlArgument(cbCustomArguments.Text, true);
                    }
                }
            }

            NewInfo.FileNameSchema = cbSchema.Text;
            if (!cbSchema.Items.Contains(cbSchema.Text))
                cbSchema.Items.Add(cbSchema.Text);

            Downloader = new frmDownloader(NewInfo);
        }

        Downloader.Show();

        if (General.ClearURLOnDownload) {
            txtUrl.Clear();
        }

        if (General.ClearClipboardOnDownload) {
            Clipboard.Clear();
        }
    }
    #endregion

    #region converter
    private void btnConvertInput_Click(object sender, EventArgs e) {
        using OpenFileDialog ofd = new();
        ofd.Title = Language.dlgConvertSelectFileToConvert;
        ofd.AutoUpgradeEnabled = true;
        ofd.Multiselect = false;
        string AllFormats = Formats.JoinFormats([
            Formats.AllFiles,
            Formats.VideoFormats,
            Formats.AudioFormats,
            !Formats.CustomFormats.IsNullEmptyWhitespace() ? Formats.CustomFormats : ""
        ]);

        ofd.Filter = AllFormats;

        if (ofd.ShowDialog() == DialogResult.OK) {
            if (!string.IsNullOrEmpty(txtConvertOutput.Text))
                btnConvert.Enabled = true;

            txtConvertInput.Text = ofd.FileName;
            btnConvertOutput.Enabled = true;

            string fileWithoutExt = System.IO.Path.GetFileNameWithoutExtension(ofd.FileName);
            using SaveFileDialog sfd = new();
            sfd.Title = Language.dlgSaveOutputFileAs;
            sfd.FileName = fileWithoutExt;
            if (rbConvertVideo.Checked) {
                sfd.Filter = Formats.JoinFormats([
                            Formats.VideoFormats,
                            !Formats.CustomFormats.IsNullEmptyWhitespace() ? Formats.CustomFormats : "",
                            Formats.AllFiles
                        ]);
                sfd.FilterIndex = Saved.convertSaveVideoIndex;
            }
            else if (rbConvertAudio.Checked) {
                sfd.Filter = Formats.JoinFormats([
                            Formats.AudioFormats,
                            !Formats.CustomFormats.IsNullEmptyWhitespace() ? Formats.CustomFormats : "",
                            Formats.AllFiles
                        ]);
                sfd.FilterIndex = Saved.convertSaveAudioIndex;
            }
            else {
                sfd.Filter = AllFormats;
                sfd.FilterIndex = Saved.convertSaveUnknownIndex;
            }
            if (sfd.ShowDialog() == DialogResult.OK) {
                txtConvertOutput.Text = sfd.FileName;
                btnConvert.Enabled = true;
                if (rbConvertVideo.Checked) {
                    Saved.convertSaveVideoIndex = sfd.FilterIndex;
                }
                else if (rbConvertAudio.Checked) {
                    Saved.convertSaveAudioIndex = sfd.FilterIndex;
                }
                else {
                    Saved.convertSaveUnknownIndex = sfd.FilterIndex;
                }
            }
        }
    }

    private void btnConvertOutput_Click(object sender, EventArgs e) {
        using SaveFileDialog sfd = new();
        sfd.Title = Language.dlgSaveOutputFileAs;
        sfd.FileName = System.IO.Path.GetFileNameWithoutExtension(txtConvertInput.Text);
        if (rbConvertVideo.Checked) {
            sfd.Filter = Formats.JoinFormats([
                    Formats.VideoFormats,
                    !Formats.CustomFormats.IsNullEmptyWhitespace() ? Formats.CustomFormats : ""
                ]);
            sfd.FilterIndex = Saved.convertSaveVideoIndex;
        }
        else if (rbConvertAudio.Checked) {
            sfd.Filter = Formats.JoinFormats([
                    Formats.AudioFormats,
                    !Formats.CustomFormats.IsNullEmptyWhitespace() ? Formats.CustomFormats : ""
                ]);
            sfd.FilterIndex = Saved.convertSaveAudioIndex;
        }
        else {
            sfd.Filter = Formats.JoinFormats([
                    Formats.AllFiles,
                    Formats.VideoFormats,
                    Formats.AudioFormats,
                    !Formats.CustomFormats.IsNullEmptyWhitespace() ? Formats.CustomFormats : ""
                ]);
            sfd.FilterIndex = Saved.convertSaveUnknownIndex;
        }

        if (sfd.ShowDialog() == DialogResult.OK) {
            txtConvertOutput.Text = sfd.FileName;
            btnConvert.Enabled = true;
            if (rbConvertVideo.Checked) {
                Saved.convertSaveVideoIndex = sfd.FilterIndex;
            }
            else if (rbConvertAudio.Checked) {
                Saved.convertSaveAudioIndex = sfd.FilterIndex;
            }
            else {
                Saved.convertSaveUnknownIndex = sfd.FilterIndex;
            }
        }
    }

    private void btnConvert_Click(object sender, EventArgs e) {
        btnConvert.Enabled = false;
        btnConvertInput.Enabled = false;
        btnConvertOutput.Enabled = false;

        ConvertInfo NewConversion = new(txtConvertInput.Text, txtConvertOutput.Text);

        if (rbConvertVideo.Checked) {
            NewConversion.Type = ConversionType.Video;
        }
        else if (rbConvertAudio.Checked) {
            NewConversion.Type = ConversionType.Audio;
        }
        else if (rbConvertCustom.Checked) {
            NewConversion.Type = ConversionType.Custom;
        }
        else if (rbConvertAuto.Checked) {
            NewConversion.Type = ConvertHelper.GetFiletype(txtConvertOutput.Text);
        }
        else {
            NewConversion.Type = ConversionType.FfmpegDefault;
        }

        btnConvert.Enabled = true;
        btnConvertInput.Enabled = true;
        btnConvertOutput.Enabled = true;

        frmConverter Converter = new(NewConversion);
        Converter.Show();
    }

    private void convertFromTray(ConversionType conversionType = ConversionType.Unspecified) {
        // -1 = automatic
        // 0 = video
        // 1 = audio
        // 2 = custom
        // 6 = ffmpeg auto

        using OpenFileDialog ofd = new();
        using SaveFileDialog sfd = new();
        ofd.Title = "Browse for file to convert";
        if (ofd.ShowDialog() == DialogResult.OK) {
            string fileWithoutExt = System.IO.Path.GetFileNameWithoutExtension(ofd.FileName);
            btnConvertOutput.Enabled = true;

            sfd.Title = "Save ouput to...";
            sfd.FileName = fileWithoutExt;
            sfd.Filter = conversionType switch {
                ConversionType.Video => Formats.VideoFormats,
                ConversionType.Audio => Formats.AudioFormats,
                _ => "All File Formats (*.*)|*.*"
            };

            if (sfd.ShowDialog() == DialogResult.OK) {
                frmConverter Converter = new(new(ofd.FileName, sfd.FileName) {
                    Type = conversionType switch {
                        ConversionType.Video => ConversionType.Video,
                        ConversionType.Audio => ConversionType.Audio,
                        ConversionType.Custom => ConversionType.Custom,
                        _ => ConversionType.FfmpegDefault,
                    },
                });
                Converter.Show();
                //Convert.convertFile(inputFile, outputFile, conversionType);
            }
        }
    }
    #endregion

    #region debug
    private async void btnDebugForceUpdateCheck_Click(object sender, EventArgs e) {
        await Updater.CheckForUpdate(true);
    }
    private void btnDebugForceAvailableUpdate_Click(object sender, EventArgs e) {
    }
    private void btnDebugDownloadArgs_Click(object sender, EventArgs e) {
        if (!Clipboard.ContainsText())
            return;

        //YoutubeDlData? testData = chkDebugPlaylistDownload.Checked ?
        //    YoutubeDlData.GeneratePlaylist(Clipboard.GetText(), out _) : YoutubeDlData.GenerateData(Clipboard.GetText(), out _);

        //frmDownloader Downloader = new();
        //Downloader.Show();
    }
    private void btnDebugRotateQualityFormat_Click(object sender, EventArgs e) {
        Point s = lbQuality.Location;
        Point t = lbFormat.Location;
        Point u = cbQuality.Location;
        Point v = cbFormat.Location;

        lbFormat.Location = s;
        lbQuality.Location = t;
        cbFormat.Location = u;
        cbQuality.Location = v;
    }
    private void btnDebugThrowException_Click(object sender, EventArgs e) {
        //try {
            throw new Exception("An exception has been thrown.");
        //}
        //catch (Exception ex) {
        //    Log.ReportException(ex, false);
        //}
    }
    private void btnYtdlVersion_Click(object sender, EventArgs e) {
        Log.MessageBox(Verification.YoutubeDlVersion ?? "No yt-dl version.");
    }
    private void btnDebugCheckVerification_Click(object sender, EventArgs e) {
        Log.MessageBox($$"""
            Youtube-DL Path: { {{Verification.YoutubeDlPath}} }
            Youtube-DL Version: { {{Verification.YoutubeDlVersion}} }

            FFmpeg Path: { {{Verification.FFmpegPath}} }
            """);
    }
    private void btnTestCopyData_Click(object sender, EventArgs e) {
#if DEBUG
        nint valPointer = 0;
        nint cdsPointer = 0;
        try {
            const string Argument = "Hello, world!";
            byte[] bytes = Encoding.Unicode.GetBytes(Argument);
            valPointer = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, valPointer, bytes.Length);

            CopyDataStruct copyData = new() {
                dwData = (int)DownloadType.Video,
                cbData = bytes.Length * sizeof(byte),
                lpData = valPointer
            };

            cdsPointer = CopyData.NintAlloc(copyData);
            CopyData.SendMessage(
                hWnd: this.Handle,
                Msg: CopyData.WM_COPYDATA,
                wParam: 0x1,
                lParam: cdsPointer);
            // wParam should be the handle to the Window that sent the message.
            // Since WM_COPYDATA is overridden, I can DO WHAT I WANT.
            // 0x1 = The data was sent from another instance.
        }
        finally {
            if (valPointer != 0)
                Marshal.FreeHGlobal(valPointer);
            if (cdsPointer != 0)
                Marshal.FreeHGlobal(cdsPointer);
        }
#endif
    }

    private void btnDebugExtendedConverter_Click(object sender, EventArgs e) {
        //using OpenFileDialog ofd = new();
        //if (ofd.ShowDialog() == DialogResult.OK) {
        //    MediaInfoData mdata = MediaInfoData.GenerateData(ofd.FileName, out _);
        //    FfprobeData fdata = FfprobeData.GenerateData(ofd.FileName, out _);
        //    Console.WriteLine();
        //}

        frmExtendedConverter cv = new();
        cv.Show();
    }
    #endregion
}