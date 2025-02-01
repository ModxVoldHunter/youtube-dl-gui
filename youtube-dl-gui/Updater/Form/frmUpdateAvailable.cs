﻿#nullable enable
namespace youtube_dl_gui;
using System.Windows.Forms;
using murrty.updater;
public partial class frmUpdateAvailable : Form {
    /// <summary>
    /// Whether the "Skip" button should be enabled.
    /// </summary>
    public bool BlockSkip { get; init; }

    /// <summary>
    /// The update that is available.
    /// </summary>
    internal GithubData UpdateData { get; }

    public frmUpdateAvailable(GithubData Update) {
        InitializeComponent();
        this.UpdateData = Update;
        this.Text = Language.frmUpdateAvailable;
        lbUpdateAvailableHeader.Text = Language.lbUpdateAvailableHeader;
        lbUpdateAvailableCurrentVersion.Text = $"{Language.lbUpdateAvailableCurrentVersion.Format(Program.CurrentVersion)}";
        lbUpdateAvailableChangelog.Text = Language.lbUpdateAvailableChangelog;
        btnUpdateAvailableUpdate.Text = Language.btnUpdateAvailableUpdate;
        btnUpdateAvailableSkip.Text = Language.btnUpdateAvailableSkipVersion;
        btnUpdateAvailableOk.Text = Language.GenericOk;
        this.Shown += (s, e) => lbUpdateAvailableHeader.Focus();
    }

    private void btnUpdateAvailableSkip_Click(object sender, EventArgs e) {
        this.DialogResult = DialogResult.Ignore;
    }

    private void btnUpdateAvailableUpdate_Click(object sender, EventArgs e) {
        this.DialogResult = DialogResult.Yes;
    }

    private void btnUpdateAvailableOk_Click(object sender, EventArgs e) {
        this.DialogResult = DialogResult.OK;
    }

    private void frmUpdateAvailable_Load(object sender, EventArgs e) {
        btnUpdateAvailableSkip.Enabled = !BlockSkip;
        lbUpdateAvailableUpdateVersion.Text = $"{Language.lbUpdateAvailableUpdateVersion.Format(UpdateData.Version)}";
        txtUpdateAvailableName.Text = UpdateData.VersionHeader;
        rtbUpdateAvailableChangelog.Text = UpdateData.VersionDescription;
        lbUpdateSize.Text = Language.lbUpdateSize.Format(UpdateData.ExecutableSize.SizeToString());
    }
}
