namespace datinate.app
{
    partial class LandingView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                HandleDisposing();
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            label2 = new Label();
            datGrouperBtn = new DatActionButton();
            saveCfgBtn = new LandingAccentButton();
            loadDatManagerBtn = new LandingAccentButton();
            addPathBtn = new LandingAccentButton();
            pathsContainer = new Panel();
            folderBrowser = new FolderBrowserDialog();
            datManagerBtn = new DatActionButton();
            tableLayoutPanel2 = new TableLayoutPanel();
            datRootsPanel = new Panel();
            panel1 = new Panel();
            setMameHashBtn = new LandingAccentButton();
            label3 = new Label();
            mameHashTextBox = new TextBox();
            panel2 = new Panel();
            loadDatGrouperProjectBtn = new LandingAccentButton();
            newDatGrouperProjectBtn = new LandingAccentButton();
            datGrouperContainer = new Panel();
            wallpaperPanel = new WallpaperPanel();
            mainLayoutPanel = new TableLayoutPanel();
            tableLayoutPanel2.SuspendLayout();
            datRootsPanel.SuspendLayout();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            wallpaperPanel.SuspendLayout();
            mainLayoutPanel.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label1.ForeColor = Color.FromArgb(72, 72, 72);
            label1.Location = new Point(-1, 54);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(899, 15);
            label1.TabIndex = 3;
            label1.Text = "Set the Root DAT Directories";
            label1.TextAlign = ContentAlignment.TopCenter;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label2.ForeColor = Color.FromArgb(72, 72, 72);
            label2.Location = new Point(0, 54);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(899, 15);
            label2.TabIndex = 4;
            label2.Text = "Load a DAT Grouper Project";
            label2.TextAlign = ContentAlignment.TopCenter;
            // 
            // datGrouperBtn
            // 
            datGrouperBtn.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            datGrouperBtn.BackColor = Color.FromArgb(35, 152, 220);
            datGrouperBtn.BannerMode = true;
            datGrouperBtn.ButtonImage = Datinate.Properties.Resources.datAction_grouper;
            datGrouperBtn.CornerRadius = 0;
            datGrouperBtn.CornerRadiusBottomLeft = 0;
            datGrouperBtn.CornerRadiusBottomRight = 0;
            datGrouperBtn.CornerRadiusTopLeft = 0;
            datGrouperBtn.CornerRadiusTopRight = 0;
            datGrouperBtn.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            datGrouperBtn.ForeColor = Color.White;
            datGrouperBtn.Location = new Point(0, 0);
            datGrouperBtn.Margin = new Padding(12, 0, 0, 0);
            datGrouperBtn.MaximumFontSize = 16;
            datGrouperBtn.MinimumSize = new Size(48, 28);
            datGrouperBtn.Name = "datGrouperBtn";
            datGrouperBtn.Padding = new Padding(0, 0, 0, 6);
            datGrouperBtn.ShowFocusCue = true;
            datGrouperBtn.Size = new Size(899, 54);
            datGrouperBtn.TabIndex = 3;
            datGrouperBtn.TabStop = false;
            datGrouperBtn.Text = "DAT Grouper";
            // 
            // saveCfgBtn
            // 
            saveCfgBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            saveCfgBtn.BackColor = Color.FromArgb(250, 250, 250);
            saveCfgBtn.BorderColor = Color.FromArgb(188, 188, 188);
            saveCfgBtn.Cursor = Cursors.Hand;
            saveCfgBtn.DisabledBackColor = Color.FromArgb(238, 238, 238);
            saveCfgBtn.DisabledBorderColor = Color.FromArgb(208, 208, 208);
            saveCfgBtn.DisabledForeColor = Color.FromArgb(145, 145, 145);
            saveCfgBtn.FlatStyle = FlatStyle.Flat;
            saveCfgBtn.ForeColor = Color.FromArgb(36, 36, 36);
            saveCfgBtn.Location = new Point(806, 105);
            saveCfgBtn.Margin = new Padding(4, 3, 4, 3);
            saveCfgBtn.Name = "saveCfgBtn";
            saveCfgBtn.Size = new Size(96, 27);
            saveCfgBtn.TabIndex = 3;
            saveCfgBtn.Text = "Save Paths";
            saveCfgBtn.UseVisualStyleBackColor = false;
            saveCfgBtn.Click += onSaveDatRootsClick;
            // 
            // loadDatManagerBtn
            // 
            loadDatManagerBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            loadDatManagerBtn.BackColor = Color.FromArgb(192, 0, 0);
            loadDatManagerBtn.BorderColor = Color.FromArgb(124, 0, 0);
            loadDatManagerBtn.Cursor = Cursors.Hand;
            loadDatManagerBtn.DisabledBackColor = Color.FromArgb(130, 84, 84);
            loadDatManagerBtn.DisabledBorderColor = Color.FromArgb(95, 61, 61);
            loadDatManagerBtn.DisabledForeColor = Color.FromArgb(248, 241, 241);
            loadDatManagerBtn.Enabled = false;
            loadDatManagerBtn.FlatStyle = FlatStyle.Flat;
            loadDatManagerBtn.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            loadDatManagerBtn.ForeColor = Color.White;
            loadDatManagerBtn.Location = new Point(806, 169);
            loadDatManagerBtn.Margin = new Padding(4, 3, 4, 3);
            loadDatManagerBtn.Name = "loadDatManagerBtn";
            loadDatManagerBtn.Size = new Size(96, 54);
            loadDatManagerBtn.TabIndex = 2;
            loadDatManagerBtn.Text = "Load";
            loadDatManagerBtn.UseVisualStyleBackColor = false;
            loadDatManagerBtn.Click += onLoadDatManagerClick;
            // 
            // addPathBtn
            // 
            addPathBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            addPathBtn.BackColor = Color.FromArgb(250, 250, 250);
            addPathBtn.BorderColor = Color.FromArgb(188, 188, 188);
            addPathBtn.Cursor = Cursors.Hand;
            addPathBtn.DisabledBackColor = Color.FromArgb(238, 238, 238);
            addPathBtn.DisabledBorderColor = Color.FromArgb(208, 208, 208);
            addPathBtn.DisabledForeColor = Color.FromArgb(145, 145, 145);
            addPathBtn.FlatStyle = FlatStyle.Flat;
            addPathBtn.ForeColor = Color.FromArgb(36, 36, 36);
            addPathBtn.Location = new Point(806, 72);
            addPathBtn.Margin = new Padding(4, 3, 4, 3);
            addPathBtn.Name = "addPathBtn";
            addPathBtn.Size = new Size(96, 27);
            addPathBtn.TabIndex = 1;
            addPathBtn.Text = "Add Root";
            addPathBtn.UseVisualStyleBackColor = false;
            addPathBtn.Click += onAddPath;
            // 
            // pathsContainer
            // 
            pathsContainer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pathsContainer.AutoScroll = true;
            pathsContainer.BackColor = Color.FromArgb(246, 241, 241);
            pathsContainer.BorderStyle = BorderStyle.FixedSingle;
            pathsContainer.Location = new Point(4, 72);
            pathsContainer.Margin = new Padding(4, 3, 4, 3);
            pathsContainer.Name = "pathsContainer";
            pathsContainer.Size = new Size(794, 119);
            pathsContainer.TabIndex = 6;
            // 
            // folderBrowser
            // 
            folderBrowser.Description = "Select DAT Root Directory";
            folderBrowser.ShowNewFolderButton = false;
            // 
            // datManagerBtn
            // 
            datManagerBtn.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            datManagerBtn.BackColor = Color.FromArgb(192, 0, 0);
            datManagerBtn.BannerMode = true;
            datManagerBtn.ButtonImage = Datinate.Properties.Resources.datAction_main;
            datManagerBtn.CornerRadius = 0;
            datManagerBtn.CornerRadiusBottomLeft = 0;
            datManagerBtn.CornerRadiusBottomRight = 0;
            datManagerBtn.CornerRadiusTopLeft = 0;
            datManagerBtn.CornerRadiusTopRight = 0;
            datManagerBtn.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            datManagerBtn.ForeColor = Color.White;
            datManagerBtn.Location = new Point(0, 0);
            datManagerBtn.Margin = new Padding(12, 0, 0, 0);
            datManagerBtn.MaximumFontSize = 16;
            datManagerBtn.MinimumSize = new Size(48, 28);
            datManagerBtn.Name = "datManagerBtn";
            datManagerBtn.Padding = new Padding(0, 0, 0, 6);
            datManagerBtn.ShowFocusCue = true;
            datManagerBtn.Size = new Size(899, 54);
            datManagerBtn.TabIndex = 3;
            datManagerBtn.TabStop = false;
            datManagerBtn.Text = "DAT Manager";
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel2.BackColor = Color.Transparent;
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(datRootsPanel, 0, 0);
            tableLayoutPanel2.Controls.Add(panel2, 0, 2);
            tableLayoutPanel2.Location = new Point(-1, 0);
            tableLayoutPanel2.Margin = new Padding(0);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 3;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 4F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.Size = new Size(900, 461);
            tableLayoutPanel2.TabIndex = 1;
            // 
            // datRootsPanel
            // 
            datRootsPanel.BackColor = Color.FromArgb(246, 246, 246);
            datRootsPanel.BorderStyle = BorderStyle.FixedSingle;
            datRootsPanel.Controls.Add(panel1);
            datRootsPanel.Controls.Add(loadDatManagerBtn);
            datRootsPanel.Controls.Add(saveCfgBtn);
            datRootsPanel.Controls.Add(addPathBtn);
            datRootsPanel.Controls.Add(pathsContainer);
            datRootsPanel.Controls.Add(datManagerBtn);
            datRootsPanel.Controls.Add(label1);
            datRootsPanel.Dock = DockStyle.Fill;
            datRootsPanel.Location = new Point(0, 0);
            datRootsPanel.Margin = new Padding(0);
            datRootsPanel.Name = "datRootsPanel";
            datRootsPanel.Size = new Size(900, 228);
            datRootsPanel.TabIndex = 2;
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            panel1.Controls.Add(setMameHashBtn);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(mameHashTextBox);
            panel1.Location = new Point(4, 194);
            panel1.Name = "panel1";
            panel1.Size = new Size(795, 29);
            panel1.TabIndex = 7;
            // 
            // setMameHashBtn
            // 
            setMameHashBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            setMameHashBtn.BackColor = Color.FromArgb(250, 250, 250);
            setMameHashBtn.BorderColor = Color.FromArgb(188, 188, 188);
            setMameHashBtn.Cursor = Cursors.Hand;
            setMameHashBtn.DisabledBackColor = Color.FromArgb(238, 238, 238);
            setMameHashBtn.DisabledBorderColor = Color.FromArgb(208, 208, 208);
            setMameHashBtn.DisabledForeColor = Color.FromArgb(145, 145, 145);
            setMameHashBtn.FlatStyle = FlatStyle.Flat;
            setMameHashBtn.ForeColor = Color.FromArgb(36, 36, 36);
            setMameHashBtn.Location = new Point(111, 1);
            setMameHashBtn.Margin = new Padding(4, 3, 4, 3);
            setMameHashBtn.Name = "setMameHashBtn";
            setMameHashBtn.Size = new Size(29, 27);
            setMameHashBtn.TabIndex = 8;
            setMameHashBtn.Text = "...";
            setMameHashBtn.UseVisualStyleBackColor = false;
            setMameHashBtn.Click += setMameHashBtn_Click;
            // 
            // label3
            // 
            label3.ForeColor = Color.FromArgb(72, 72, 72);
            label3.Location = new Point(4, 7);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(107, 15);
            label3.TabIndex = 8;
            label3.Text = "MAME hash folder";
            // 
            // mameHashTextBox
            // 
            mameHashTextBox.ForeColor = SystemColors.ControlDarkDark;
            mameHashTextBox.Location = new Point(147, 3);
            mameHashTextBox.Name = "mameHashTextBox";
            mameHashTextBox.PlaceholderText = "Set the MAME Software List DAT folder for accurate content descriptions...";
            mameHashTextBox.ReadOnly = true;
            mameHashTextBox.Size = new Size(648, 23);
            mameHashTextBox.TabIndex = 0;
            // 
            // panel2
            // 
            panel2.BackColor = Color.FromArgb(246, 246, 246);
            panel2.BorderStyle = BorderStyle.FixedSingle;
            panel2.Controls.Add(loadDatGrouperProjectBtn);
            panel2.Controls.Add(newDatGrouperProjectBtn);
            panel2.Controls.Add(datGrouperContainer);
            panel2.Controls.Add(datGrouperBtn);
            panel2.Controls.Add(label2);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(0, 232);
            panel2.Margin = new Padding(0);
            panel2.Name = "panel2";
            panel2.Size = new Size(900, 229);
            panel2.TabIndex = 1;
            // 
            // loadDatGrouperProjectBtn
            // 
            loadDatGrouperProjectBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            loadDatGrouperProjectBtn.BackColor = Color.FromArgb(35, 152, 220);
            loadDatGrouperProjectBtn.BorderColor = Color.FromArgb(24, 105, 153);
            loadDatGrouperProjectBtn.Cursor = Cursors.Hand;
            loadDatGrouperProjectBtn.DisabledBackColor = Color.FromArgb(90, 118, 138);
            loadDatGrouperProjectBtn.DisabledBorderColor = Color.FromArgb(66, 87, 103);
            loadDatGrouperProjectBtn.DisabledForeColor = Color.FromArgb(244, 247, 249);
            loadDatGrouperProjectBtn.Enabled = false;
            loadDatGrouperProjectBtn.FlatStyle = FlatStyle.Flat;
            loadDatGrouperProjectBtn.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            loadDatGrouperProjectBtn.ForeColor = Color.White;
            loadDatGrouperProjectBtn.Location = new Point(806, 170);
            loadDatGrouperProjectBtn.Margin = new Padding(4, 3, 4, 3);
            loadDatGrouperProjectBtn.Name = "loadDatGrouperProjectBtn";
            loadDatGrouperProjectBtn.Size = new Size(96, 54);
            loadDatGrouperProjectBtn.TabIndex = 8;
            loadDatGrouperProjectBtn.Text = "Load";
            loadDatGrouperProjectBtn.UseVisualStyleBackColor = false;
            // 
            // newDatGrouperProjectBtn
            // 
            newDatGrouperProjectBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            newDatGrouperProjectBtn.BackColor = Color.FromArgb(250, 250, 250);
            newDatGrouperProjectBtn.BorderColor = Color.FromArgb(188, 188, 188);
            newDatGrouperProjectBtn.Cursor = Cursors.Hand;
            newDatGrouperProjectBtn.DisabledBackColor = Color.FromArgb(238, 238, 238);
            newDatGrouperProjectBtn.DisabledBorderColor = Color.FromArgb(208, 208, 208);
            newDatGrouperProjectBtn.DisabledForeColor = Color.FromArgb(145, 145, 145);
            newDatGrouperProjectBtn.FlatStyle = FlatStyle.Flat;
            newDatGrouperProjectBtn.ForeColor = Color.FromArgb(36, 36, 36);
            newDatGrouperProjectBtn.Location = new Point(806, 72);
            newDatGrouperProjectBtn.Margin = new Padding(4, 3, 4, 3);
            newDatGrouperProjectBtn.Name = "newDatGrouperProjectBtn";
            newDatGrouperProjectBtn.Size = new Size(96, 27);
            newDatGrouperProjectBtn.TabIndex = 7;
            newDatGrouperProjectBtn.Text = "Create New";
            newDatGrouperProjectBtn.UseVisualStyleBackColor = false;
            // 
            // datGrouperContainer
            // 
            datGrouperContainer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            datGrouperContainer.AutoScroll = true;
            datGrouperContainer.BackColor = Color.FromArgb(241, 246, 249);
            datGrouperContainer.BorderStyle = BorderStyle.FixedSingle;
            datGrouperContainer.Location = new Point(4, 72);
            datGrouperContainer.Margin = new Padding(4, 3, 4, 3);
            datGrouperContainer.Name = "datGrouperContainer";
            datGrouperContainer.Size = new Size(794, 151);
            datGrouperContainer.TabIndex = 9;
            // 
            // wallpaperPanel
            // 
            wallpaperPanel.BackgroundImageLayout = ImageLayout.Stretch;
            wallpaperPanel.Controls.Add(mainLayoutPanel);
            wallpaperPanel.Dock = DockStyle.Fill;
            wallpaperPanel.Location = new Point(0, 0);
            wallpaperPanel.Margin = new Padding(0);
            wallpaperPanel.Name = "wallpaperPanel";
            wallpaperPanel.Padding = new Padding(0, 4, 0, 4);
            wallpaperPanel.Size = new Size(898, 469);
            wallpaperPanel.TabIndex = 2;
            wallpaperPanel.Wallpaper = Datinate.Properties.Resources.background_bw;
            // 
            // mainLayoutPanel
            // 
            mainLayoutPanel.BackColor = Color.Transparent;
            mainLayoutPanel.ColumnCount = 3;
            mainLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            mainLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 900F));
            mainLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            mainLayoutPanel.Controls.Add(tableLayoutPanel2, 1, 0);
            mainLayoutPanel.Dock = DockStyle.Fill;
            mainLayoutPanel.Location = new Point(0, 4);
            mainLayoutPanel.Margin = new Padding(0);
            mainLayoutPanel.Name = "mainLayoutPanel";
            mainLayoutPanel.RowCount = 1;
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayoutPanel.Size = new Size(898, 461);
            mainLayoutPanel.TabIndex = 2;
            mainLayoutPanel.Visible = false;
            // 
            // LandingView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            BackgroundImageLayout = ImageLayout.Stretch;
            Controls.Add(wallpaperPanel);
            Margin = new Padding(4, 3, 4, 3);
            Name = "LandingView";
            Size = new Size(898, 469);
            tableLayoutPanel2.ResumeLayout(false);
            datRootsPanel.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel2.ResumeLayout(false);
            wallpaperPanel.ResumeLayout(false);
            mainLayoutPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Label label1;
        private LandingAccentButton addPathBtn;
        private System.Windows.Forms.FolderBrowserDialog folderBrowser;
        private LandingAccentButton loadDatManagerBtn;
        private System.Windows.Forms.Panel pathsContainer;
        private LandingAccentButton saveCfgBtn;
        private DatActionButton datManagerBtn;
        private TableLayoutPanel tableLayoutPanel2;
        private Panel panel2;
        private DatActionButton datGrouperBtn;
        private Panel datRootsPanel;
        private Label label2;
        private WallpaperPanel wallpaperPanel;
        private TableLayoutPanel mainLayoutPanel;
        private LandingAccentButton loadDatGrouperProjectBtn;
        private LandingAccentButton newDatGrouperProjectBtn;
        private Panel datGrouperContainer;
        private Panel panel1;
        private TextBox mameHashTextBox;
        private Label label3;
        private LandingAccentButton setMameHashBtn;
    }
}