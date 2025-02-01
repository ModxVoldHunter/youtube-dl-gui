﻿#nullable enable
namespace youtube_dl_gui;
using System.Drawing;
using System.Windows.Forms;
using murrty.updater;
public partial class frmDownloadLanguage : LocalizedForm {
    private readonly GithubRepoContent[] EnumeratedLanguages;

    public string? FileName { get; private set; }

    public frmDownloadLanguage() {
        InitializeComponent();

        try {
            EnumeratedLanguages = Updater.GetAvailableLanguages()
                .Where(x => !x.name.IsNullEmptyWhitespace() && !x.download_url.IsNullEmptyWhitespace())
                .ToArray();

            if (EnumeratedLanguages.Length > 0) {
                // Uncomment these out when the SHA calcuation gets fixed.
                for (int i = 0; i < EnumeratedLanguages.Length; i++) {
                    GithubRepoContent Content = EnumeratedLanguages[i];
                    ListViewItem NewItem = new($"Item {Content.name}");
                    NewItem.SubItems[0].Text = $"{i + 1}: {(Content.name!.EndsWith(".ini") ? Content.name[..^4] : Content.name)} ({Content.size.SizeToString()})";
                    NewItem.UseItemStyleForSubItems = false;
                    NewItem.SubItems.Add(new ListViewItem.ListViewSubItem());
                    NewItem.SubItems[1].Text = Content.download_url;
                    //NewItem.SubItems[1].Text = $"{Content.Sha}";
                    NewItem.SubItems[1].ForeColor = Color.FromKnownColor(KnownColor.ScrollBar);
                    NewItem.SubItems[1].Font = this.Font;
                    //NewItem.ToolTipText = Content.DownloadUrl;
                    lvAvailableLanguages.Items.Add(NewItem);
                }
            }
        }
        catch (Exception ex) {
            EnumeratedLanguages = [];
            Log.ReportException(ex);
        }
    }

    public override void LoadLanguage() {
        if (Initialization.firstTime) {
            btnCancel.Text = Language.InternalEnglish.GenericCancel;
            btnOk.Text = Language.InternalEnglish.GenericOk;
            btnDownloadSelected.Text = Language.InternalEnglish.sbDownload;
            this.Text = Language.InternalEnglish.frmDownloadLanguage;
        }
        else {
            btnCancel.Text = Language.GenericCancel;
            btnOk.Text = Language.GenericOk;
            btnDownloadSelected.Text = Language.sbDownload;
            this.Text = Language.frmDownloadLanguage;
        }
    }

    private void DownloadSelectedLanguageFile() {
        var lang = EnumeratedLanguages[lvAvailableLanguages.SelectedIndices[0]];

        Log.Write($"Downloading language file {lang.name}.");
        if (!System.IO.Directory.Exists(Environment.CurrentDirectory + "\\lang")) {
            System.IO.Directory.CreateDirectory(Environment.CurrentDirectory + "\\lang");
        }
        string URL = lang.download_url!;
        string Output = Environment.CurrentDirectory + "\\lang\\" + lang.name;
        using frmGenericDownloadProgress Downloader = new(URL, Output);
        if (Downloader.ShowDialog() == DialogResult.OK) {
            Log.Write($"Finished downloading language file {lang.name}");
            System.Media.SystemSounds.Asterisk.Play();
            btnOk_Click(this, EventArgs.Empty);
        }
        else {
            Log.Write($"Could not download language file {lang.name}.");
            System.Media.SystemSounds.Hand.Play();
        }

        // The SHA on github doesn't match what I can calculate here.
        //if (Program.CalculateSha1Hash(Output).ToLower() != EnumeratedLanguages[listView1.SelectedIndices[0]].Sha.ToLower()) {
        //    Log.MessageBox(Language.dlgLanguageHashNoMatch, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //}
    }

    private void btnDownloadSelected_Click(object sender, EventArgs e) {
        DownloadSelectedLanguageFile();
    }

    private void btnCancel_Click(object sender, EventArgs e) {
        this.DialogResult = DialogResult.Cancel;
    }

    private void btnOk_Click(object sender, EventArgs e) {
        if (lvAvailableLanguages.SelectedIndices.Count > 0) {
            DownloadSelectedLanguageFile();
            FileName = EnumeratedLanguages[lvAvailableLanguages.SelectedIndices[0]].name!;
            if (FileName.EndsWith(".ini")) {
                FileName = FileName[..^4];
            }
            this.DialogResult = DialogResult.OK;
            return;
        }
        FileName = null;
        this.DialogResult = DialogResult.Cancel;
    }

    private void lvAvailableLanguages_SelectedIndexChanged(object sender, EventArgs e) {
        btnOk.Enabled = lvAvailableLanguages.SelectedIndices.Count > 0;
        btnDownloadSelected.Enabled = lvAvailableLanguages.SelectedIndices.Count > 0;
    }
}