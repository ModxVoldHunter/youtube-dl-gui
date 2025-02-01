﻿namespace youtube_dl_gui {
    partial class frmMerger {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TreeNode treeNode10 = new System.Windows.Forms.TreeNode("<---");
            System.Windows.Forms.TreeNode treeNode11 = new System.Windows.Forms.TreeNode("nSelectedSourceVideo");
            System.Windows.Forms.TreeNode treeNode12 = new System.Windows.Forms.TreeNode("nSelectedSourceAudio");
            System.Windows.Forms.TreeNode treeNode13 = new System.Windows.Forms.TreeNode("nSelectedSourceSubtitle");
            System.Windows.Forms.TreeNode treeNode14 = new System.Windows.Forms.TreeNode("nSelectedSourceAttachment");
            System.Windows.Forms.TreeNode treeNode15 = new System.Windows.Forms.TreeNode("nSelectedStreamsVideo");
            System.Windows.Forms.TreeNode treeNode16 = new System.Windows.Forms.TreeNode("nSelectedStreamsAudio");
            System.Windows.Forms.TreeNode treeNode17 = new System.Windows.Forms.TreeNode("nSelectedStreamsSubtitle");
            System.Windows.Forms.TreeNode treeNode18 = new System.Windows.Forms.TreeNode("nSelectedStreamsAttachment");
            this.pnLower = new System.Windows.Forms.Panel();
            this.btnMergeFiles = new System.Windows.Forms.Button();
            this.pnLoadedFiles = new System.Windows.Forms.Panel();
            this.btnRemoveFiles = new System.Windows.Forms.Button();
            this.btnAddFiles = new System.Windows.Forms.Button();
            this.lbFileSources = new System.Windows.Forms.ListBox();
            this.pnStreams = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tvSelectedSources = new youtube_dl_gui.ExplorerTreeView();
            this.tvSelectedStreams = new youtube_dl_gui.ExplorerTreeView();
            this.ttFiles = new System.Windows.Forms.ToolTip(this.components);
            this.pnLower.SuspendLayout();
            this.pnLoadedFiles.SuspendLayout();
            this.pnStreams.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnLower
            // 
            this.pnLower.BackColor = System.Drawing.SystemColors.Menu;
            this.pnLower.Controls.Add(this.btnMergeFiles);
            this.pnLower.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnLower.Location = new System.Drawing.Point(0, 326);
            this.pnLower.Name = "pnLower";
            this.pnLower.Size = new System.Drawing.Size(581, 48);
            this.pnLower.TabIndex = 0;
            // 
            // btnMergeFiles
            // 
            this.btnMergeFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMergeFiles.Location = new System.Drawing.Point(460, 12);
            this.btnMergeFiles.Name = "btnMergeFiles";
            this.btnMergeFiles.Size = new System.Drawing.Size(109, 23);
            this.btnMergeFiles.TabIndex = 0;
            this.btnMergeFiles.Text = "btnMergeFiles";
            this.btnMergeFiles.UseVisualStyleBackColor = true;
            this.btnMergeFiles.Click += new System.EventHandler(this.btnMergeFiles_Click);
            // 
            // pnLoadedFiles
            // 
            this.pnLoadedFiles.Controls.Add(this.btnRemoveFiles);
            this.pnLoadedFiles.Controls.Add(this.btnAddFiles);
            this.pnLoadedFiles.Controls.Add(this.lbFileSources);
            this.pnLoadedFiles.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnLoadedFiles.Location = new System.Drawing.Point(0, 0);
            this.pnLoadedFiles.Name = "pnLoadedFiles";
            this.pnLoadedFiles.Size = new System.Drawing.Size(180, 326);
            this.pnLoadedFiles.TabIndex = 1;
            // 
            // btnRemoveFiles
            // 
            this.btnRemoveFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRemoveFiles.Location = new System.Drawing.Point(95, 297);
            this.btnRemoveFiles.Name = "btnRemoveFiles";
            this.btnRemoveFiles.Size = new System.Drawing.Size(75, 23);
            this.btnRemoveFiles.TabIndex = 2;
            this.btnRemoveFiles.Text = "btnRemoveFiles";
            this.btnRemoveFiles.UseVisualStyleBackColor = true;
            this.btnRemoveFiles.Click += new System.EventHandler(this.btnRemoveFiles_Click);
            // 
            // btnAddFiles
            // 
            this.btnAddFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAddFiles.Location = new System.Drawing.Point(7, 297);
            this.btnAddFiles.Name = "btnAddFiles";
            this.btnAddFiles.Size = new System.Drawing.Size(75, 23);
            this.btnAddFiles.TabIndex = 1;
            this.btnAddFiles.Text = "btnAddFiles";
            this.btnAddFiles.UseVisualStyleBackColor = true;
            this.btnAddFiles.Click += new System.EventHandler(this.btnAddFiles_Click);
            // 
            // lbFileSources
            // 
            this.lbFileSources.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lbFileSources.FormattingEnabled = true;
            this.lbFileSources.Location = new System.Drawing.Point(3, 3);
            this.lbFileSources.Name = "lbFileSources";
            this.lbFileSources.Size = new System.Drawing.Size(171, 290);
            this.lbFileSources.TabIndex = 0;
            this.lbFileSources.SelectedIndexChanged += new System.EventHandler(this.lbFileSources_SelectedIndexChanged);
            // 
            // pnStreams
            // 
            this.pnStreams.Controls.Add(this.splitContainer1);
            this.pnStreams.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnStreams.Location = new System.Drawing.Point(180, 0);
            this.pnStreams.Name = "pnStreams";
            this.pnStreams.Size = new System.Drawing.Size(401, 326);
            this.pnStreams.TabIndex = 2;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tvSelectedSources);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tvSelectedStreams);
            this.splitContainer1.Size = new System.Drawing.Size(401, 326);
            this.splitContainer1.SplitterDistance = 200;
            this.splitContainer1.TabIndex = 0;
            // 
            // tvSelectedSources
            // 
            this.tvSelectedSources.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tvSelectedSources.Location = new System.Drawing.Point(6, 3);
            this.tvSelectedSources.Name = "tvSelectedSources";
            treeNode10.Name = "nodeFile";
            treeNode10.Text = "<---";
            treeNode11.Name = "nSelectedSourceVideo";
            treeNode11.Text = "nSelectedSourceVideo";
            treeNode12.Name = "nSelectedSourceAudio";
            treeNode12.Text = "nSelectedSourceAudio";
            treeNode13.Name = "nSelectedSourceSubtitle";
            treeNode13.Text = "nSelectedSourceSubtitle";
            treeNode14.Name = "nSelectedSourceAttachment";
            treeNode14.Text = "nSelectedSourceAttachment";
            this.tvSelectedSources.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode10,
            treeNode11,
            treeNode12,
            treeNode13,
            treeNode14});
            this.tvSelectedSources.ShowLines = false;
            this.tvSelectedSources.ShowNodeToolTips = true;
            this.tvSelectedSources.Size = new System.Drawing.Size(191, 317);
            this.tvSelectedSources.TabIndex = 0;
            this.tvSelectedSources.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvSelectedSources_NodeMouseDoubleClick);
            // 
            // tvSelectedStreams
            // 
            this.tvSelectedStreams.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tvSelectedStreams.Location = new System.Drawing.Point(3, 3);
            this.tvSelectedStreams.Name = "tvSelectedStreams";
            treeNode15.Name = "nSelectedStreamsVideo";
            treeNode15.Text = "nSelectedStreamsVideo";
            treeNode16.Name = "nSelectedStreamsAudio";
            treeNode16.Text = "nSelectedStreamsAudio";
            treeNode17.Name = "nSelectedStreamsSubtitle";
            treeNode17.Text = "nSelectedStreamsSubtitle";
            treeNode18.Name = "nSelectedStreamsAttachment";
            treeNode18.Text = "nSelectedStreamsAttachment";
            this.tvSelectedStreams.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode15,
            treeNode16,
            treeNode17,
            treeNode18});
            this.tvSelectedStreams.ShowLines = false;
            this.tvSelectedStreams.Size = new System.Drawing.Size(191, 317);
            this.tvSelectedStreams.TabIndex = 1;
            this.tvSelectedStreams.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvSelectedStreams_NodeMouseDoubleClick);
            // 
            // frmMerger
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(581, 374);
            this.Controls.Add(this.pnStreams);
            this.Controls.Add(this.pnLoadedFiles);
            this.Controls.Add(this.pnLower);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = global::youtube_dl_gui.Properties.Resources.ProgramIcon;
            this.Name = "frmMerger";
            this.Text = "frmMerger";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.frmMerger_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.frmMerger_DragEnter);
            this.pnLower.ResumeLayout(false);
            this.pnLoadedFiles.ResumeLayout(false);
            this.pnStreams.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnLower;
        private System.Windows.Forms.Panel pnLoadedFiles;
        private System.Windows.Forms.Panel pnStreams;
        private System.Windows.Forms.Button btnRemoveFiles;
        private System.Windows.Forms.Button btnAddFiles;
        private System.Windows.Forms.ListBox lbFileSources;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnMergeFiles;
        private ExplorerTreeView tvSelectedSources;
        private ExplorerTreeView tvSelectedStreams;
        private System.Windows.Forms.ToolTip ttFiles;
    }
}