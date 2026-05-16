namespace datinate.app
{
    partial class ExportView
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
            autoSizeContainer = new TableLayoutPanel();
            leftFillerPanel = new Panel();
            panel1 = new Panel();
            rightFillerPanel = new Panel();
            panel2 = new Panel();
            rootLayout = new TableLayoutPanel();
            headerPanel = new Panel();
            panel4 = new Panel();
            titleLbl = new Label();
            bodyPanel = new Panel();
            tabControl = new TabControl();
            softwarePage = new TabPage();
            softwarePanel = new Panel();
            datStructureGroup = new GroupBox();
            label6 = new Label();
            exportAsM3uCB = new CheckBox();
            groupBox2 = new GroupBox();
            label2 = new Label();
            export1g1rCB = new CheckBox();
            skipExcludedGamesCB = new CheckBox();
            label1 = new Label();
            groupBox1 = new GroupBox();
            label4 = new Label();
            skipExcludedDescriptorFamiliesCB = new CheckBox();
            infoPanel = new Panel();
            label3 = new Label();
            exportSoftwareBtn = new Button();
            mediaPage = new TabPage();
            mediaPanel = new Panel();
            mediaGroupBox = new GroupBox();
            mediaContainer = new FlowLayoutPanel();
            infoPanelMedia = new Panel();
            label5 = new Label();
            exportMediaBtn = new Button();
            bottomBarPanel = new Panel();
            panel3 = new Panel();
            btnLayoutPanel = new FlowLayoutPanel();
            backBtn = new Button();
            saveSettingsBtn = new Button();
            autoSizeContainer.SuspendLayout();
            leftFillerPanel.SuspendLayout();
            rightFillerPanel.SuspendLayout();
            rootLayout.SuspendLayout();
            headerPanel.SuspendLayout();
            bodyPanel.SuspendLayout();
            tabControl.SuspendLayout();
            softwarePage.SuspendLayout();
            softwarePanel.SuspendLayout();
            datStructureGroup.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox1.SuspendLayout();
            infoPanel.SuspendLayout();
            mediaPage.SuspendLayout();
            mediaPanel.SuspendLayout();
            mediaGroupBox.SuspendLayout();
            infoPanelMedia.SuspendLayout();
            bottomBarPanel.SuspendLayout();
            btnLayoutPanel.SuspendLayout();
            SuspendLayout();
            // 
            // autoSizeContainer
            // 
            autoSizeContainer.ColumnCount = 3;
            autoSizeContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            autoSizeContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 800F));
            autoSizeContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            autoSizeContainer.Controls.Add(leftFillerPanel, 0, 0);
            autoSizeContainer.Controls.Add(rightFillerPanel, 2, 0);
            autoSizeContainer.Controls.Add(rootLayout, 1, 0);
            autoSizeContainer.Dock = DockStyle.Fill;
            autoSizeContainer.Location = new Point(0, 0);
            autoSizeContainer.Margin = new Padding(0);
            autoSizeContainer.Name = "autoSizeContainer";
            autoSizeContainer.RowCount = 1;
            autoSizeContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            autoSizeContainer.Size = new Size(879, 666);
            autoSizeContainer.TabIndex = 2;
            // 
            // leftFillerPanel
            // 
            leftFillerPanel.BackColor = SystemColors.ControlDarkDark;
            leftFillerPanel.Controls.Add(panel1);
            leftFillerPanel.Dock = DockStyle.Fill;
            leftFillerPanel.Location = new Point(0, 0);
            leftFillerPanel.Margin = new Padding(0);
            leftFillerPanel.Name = "leftFillerPanel";
            leftFillerPanel.Size = new Size(39, 666);
            leftFillerPanel.TabIndex = 2;
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel1.BackColor = SystemColors.Control;
            panel1.Location = new Point(0, 631);
            panel1.Margin = new Padding(0);
            panel1.Name = "panel1";
            panel1.Size = new Size(39, 35);
            panel1.TabIndex = 1;
            // 
            // rightFillerPanel
            // 
            rightFillerPanel.BackColor = SystemColors.ControlDarkDark;
            rightFillerPanel.Controls.Add(panel2);
            rightFillerPanel.Dock = DockStyle.Fill;
            rightFillerPanel.Location = new Point(839, 0);
            rightFillerPanel.Margin = new Padding(0);
            rightFillerPanel.Name = "rightFillerPanel";
            rightFillerPanel.Size = new Size(40, 666);
            rightFillerPanel.TabIndex = 1;
            // 
            // panel2
            // 
            panel2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel2.BackColor = SystemColors.Control;
            panel2.Location = new Point(0, 631);
            panel2.Margin = new Padding(0);
            panel2.Name = "panel2";
            panel2.Size = new Size(39, 35);
            panel2.TabIndex = 2;
            // 
            // rootLayout
            // 
            rootLayout.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rootLayout.ColumnCount = 1;
            rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            rootLayout.Controls.Add(headerPanel, 0, 0);
            rootLayout.Controls.Add(bodyPanel, 0, 1);
            rootLayout.Controls.Add(bottomBarPanel, 0, 2);
            rootLayout.Location = new Point(39, 0);
            rootLayout.Margin = new Padding(0);
            rootLayout.Name = "rootLayout";
            rootLayout.RowCount = 3;
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            rootLayout.Size = new Size(800, 666);
            rootLayout.TabIndex = 0;
            // 
            // headerPanel
            // 
            headerPanel.Controls.Add(panel4);
            headerPanel.Controls.Add(titleLbl);
            headerPanel.Dock = DockStyle.Fill;
            headerPanel.Location = new Point(0, 0);
            headerPanel.Margin = new Padding(0);
            headerPanel.Name = "headerPanel";
            headerPanel.Padding = new Padding(14, 10, 14, 8);
            headerPanel.Size = new Size(800, 34);
            headerPanel.TabIndex = 0;
            // 
            // panel4
            // 
            panel4.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel4.BackColor = SystemColors.ControlDarkDark;
            panel4.Location = new Point(0, 33);
            panel4.Margin = new Padding(0);
            panel4.Name = "panel4";
            panel4.Size = new Size(800, 1);
            panel4.TabIndex = 38;
            // 
            // titleLbl
            // 
            titleLbl.AutoSize = true;
            titleLbl.Dock = DockStyle.Fill;
            titleLbl.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
            titleLbl.ForeColor = Color.FromArgb(30, 32, 36);
            titleLbl.Location = new Point(14, 10);
            titleLbl.Margin = new Padding(0);
            titleLbl.Name = "titleLbl";
            titleLbl.Size = new Size(147, 20);
            titleLbl.TabIndex = 0;
            titleLbl.Text = "DAT Grouper Export";
            titleLbl.TextAlign = ContentAlignment.BottomLeft;
            // 
            // bodyPanel
            // 
            bodyPanel.AutoScroll = true;
            bodyPanel.BackColor = Color.White;
            bodyPanel.Controls.Add(tabControl);
            bodyPanel.Dock = DockStyle.Fill;
            bodyPanel.Location = new Point(0, 34);
            bodyPanel.Margin = new Padding(0);
            bodyPanel.Name = "bodyPanel";
            bodyPanel.Padding = new Padding(14, 10, 14, 10);
            bodyPanel.Size = new Size(800, 596);
            bodyPanel.TabIndex = 1;
            // 
            // tabControl
            // 
            tabControl.Controls.Add(softwarePage);
            tabControl.Controls.Add(mediaPage);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(14, 10);
            tabControl.Margin = new Padding(0);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(772, 576);
            tabControl.TabIndex = 1;
            // 
            // softwarePage
            // 
            softwarePage.Controls.Add(softwarePanel);
            softwarePage.Location = new Point(4, 24);
            softwarePage.Name = "softwarePage";
            softwarePage.Padding = new Padding(3);
            softwarePage.Size = new Size(764, 548);
            softwarePage.TabIndex = 0;
            softwarePage.Text = "Software && Global Export";
            softwarePage.UseVisualStyleBackColor = true;
            // 
            // softwarePanel
            // 
            softwarePanel.Controls.Add(datStructureGroup);
            softwarePanel.Controls.Add(groupBox2);
            softwarePanel.Controls.Add(groupBox1);
            softwarePanel.Controls.Add(infoPanel);
            softwarePanel.Controls.Add(exportSoftwareBtn);
            softwarePanel.Dock = DockStyle.Fill;
            softwarePanel.Location = new Point(3, 3);
            softwarePanel.Name = "softwarePanel";
            softwarePanel.Size = new Size(758, 542);
            softwarePanel.TabIndex = 0;
            // 
            // datStructureGroup
            // 
            datStructureGroup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            datStructureGroup.Controls.Add(label6);
            datStructureGroup.Controls.Add(exportAsM3uCB);
            datStructureGroup.Location = new Point(1, 350);
            datStructureGroup.Name = "datStructureGroup";
            datStructureGroup.Size = new Size(754, 100);
            datStructureGroup.TabIndex = 5;
            datStructureGroup.TabStop = false;
            datStructureGroup.Text = "DAT Structure";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.ForeColor = SystemColors.ControlDarkDark;
            label6.Location = new Point(17, 53);
            label6.Name = "label6";
            label6.Size = new Size(529, 15);
            label6.TabIndex = 3;
            label6.Text = "Untick to create a 'flatter' DAT structure. This option is only available when every Game has exactly one Part.";
            // 
            // exportAsM3uCB
            // 
            exportAsM3uCB.AutoSize = true;
            exportAsM3uCB.Location = new Point(17, 31);
            exportAsM3uCB.Name = "exportAsM3uCB";
            exportAsM3uCB.Size = new Size(141, 19);
            exportAsM3uCB.TabIndex = 2;
            exportAsM3uCB.Text = "Export with M3U files.";
            exportAsM3uCB.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            groupBox2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox2.Controls.Add(label2);
            groupBox2.Controls.Add(export1g1rCB);
            groupBox2.Controls.Add(skipExcludedGamesCB);
            groupBox2.Controls.Add(label1);
            groupBox2.Location = new Point(2, 178);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(753, 165);
            groupBox2.TabIndex = 5;
            groupBox2.TabStop = false;
            groupBox2.Text = "Game Settings";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.ForeColor = SystemColors.ControlDarkDark;
            label2.Location = new Point(17, 111);
            label2.Name = "label2";
            label2.Size = new Size(277, 15);
            label2.TabIndex = 3;
            label2.Text = "Skip Games where all Parts are marked as Excluded.";
            // 
            // export1g1rCB
            // 
            export1g1rCB.AutoSize = true;
            export1g1rCB.Location = new Point(17, 31);
            export1g1rCB.Name = "export1g1rCB";
            export1g1rCB.Size = new Size(124, 19);
            export1g1rCB.TabIndex = 0;
            export1g1rCB.Text = "1 Game per Family";
            export1g1rCB.UseVisualStyleBackColor = true;
            // 
            // skipExcludedGamesCB
            // 
            skipExcludedGamesCB.AutoSize = true;
            skipExcludedGamesCB.Location = new Point(17, 89);
            skipExcludedGamesCB.Name = "skipExcludedGamesCB";
            skipExcludedGamesCB.Size = new Size(236, 19);
            skipExcludedGamesCB.TabIndex = 2;
            skipExcludedGamesCB.Text = "Skip Games where all Parts are Excluded";
            skipExcludedGamesCB.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.ForeColor = SystemColors.ControlDarkDark;
            label1.Location = new Point(17, 53);
            label1.Name = "label1";
            label1.Size = new Size(359, 15);
            label1.TabIndex = 1;
            label1.Text = "Tick to Export only the first (parent) Game assigned to each Family.";
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(skipExcludedDescriptorFamiliesCB);
            groupBox1.Location = new Point(1, 72);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(754, 100);
            groupBox1.TabIndex = 4;
            groupBox1.TabStop = false;
            groupBox1.Text = "Family Settings";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.ForeColor = SystemColors.ControlDarkDark;
            label4.Location = new Point(17, 53);
            label4.Name = "label4";
            label4.Size = new Size(544, 15);
            label4.TabIndex = 3;
            label4.Text = "Tick to Skip all Games assigned to Families with Exclude Descriptors. (a.k.a. 'Scoring Exempt' Families).";
            // 
            // skipExcludedDescriptorFamiliesCB
            // 
            skipExcludedDescriptorFamiliesCB.AutoSize = true;
            skipExcludedDescriptorFamiliesCB.Location = new Point(17, 31);
            skipExcludedDescriptorFamiliesCB.Name = "skipExcludedDescriptorFamiliesCB";
            skipExcludedDescriptorFamiliesCB.Size = new Size(226, 19);
            skipExcludedDescriptorFamiliesCB.TabIndex = 2;
            skipExcludedDescriptorFamiliesCB.Text = "Skip Families with Exclude Descriptors";
            skipExcludedDescriptorFamiliesCB.UseVisualStyleBackColor = true;
            // 
            // infoPanel
            // 
            infoPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            infoPanel.BackColor = SystemColors.Control;
            infoPanel.Controls.Add(label3);
            infoPanel.Location = new Point(2, 5);
            infoPanel.Name = "infoPanel";
            infoPanel.Size = new Size(753, 61);
            infoPanel.TabIndex = 3;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label3.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label3.Location = new Point(3, 3);
            label3.Name = "label3";
            label3.Size = new Size(744, 55);
            label3.TabIndex = 0;
            label3.Text = "Configuration Options for Exporting the Software DAT.";
            label3.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // exportSoftwareBtn
            // 
            exportSoftwareBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            exportSoftwareBtn.Location = new Point(595, 516);
            exportSoftwareBtn.Name = "exportSoftwareBtn";
            exportSoftwareBtn.Size = new Size(160, 23);
            exportSoftwareBtn.TabIndex = 1;
            exportSoftwareBtn.Text = "Export Software";
            exportSoftwareBtn.UseVisualStyleBackColor = true;
            exportSoftwareBtn.Click += exportSoftwareBtn_Click;
            // 
            // mediaPage
            // 
            mediaPage.Controls.Add(mediaPanel);
            mediaPage.Location = new Point(4, 24);
            mediaPage.Name = "mediaPage";
            mediaPage.Padding = new Padding(3);
            mediaPage.Size = new Size(764, 548);
            mediaPage.TabIndex = 1;
            mediaPage.Text = "Media Export";
            mediaPage.UseVisualStyleBackColor = true;
            // 
            // mediaPanel
            // 
            mediaPanel.Controls.Add(mediaGroupBox);
            mediaPanel.Controls.Add(infoPanelMedia);
            mediaPanel.Controls.Add(exportMediaBtn);
            mediaPanel.Dock = DockStyle.Fill;
            mediaPanel.Location = new Point(3, 3);
            mediaPanel.Name = "mediaPanel";
            mediaPanel.Size = new Size(758, 542);
            mediaPanel.TabIndex = 0;
            // 
            // mediaGroupBox
            // 
            mediaGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            mediaGroupBox.Controls.Add(mediaContainer);
            mediaGroupBox.Location = new Point(3, 72);
            mediaGroupBox.Name = "mediaGroupBox";
            mediaGroupBox.Size = new Size(752, 438);
            mediaGroupBox.TabIndex = 5;
            mediaGroupBox.TabStop = false;
            mediaGroupBox.Text = "Set Priority Sources";
            // 
            // mediaContainer
            // 
            mediaContainer.AutoScroll = true;
            mediaContainer.Dock = DockStyle.Fill;
            mediaContainer.FlowDirection = FlowDirection.TopDown;
            mediaContainer.Location = new Point(3, 19);
            mediaContainer.Name = "mediaContainer";
            mediaContainer.Size = new Size(746, 416);
            mediaContainer.TabIndex = 0;
            mediaContainer.WrapContents = false;
            // 
            // infoPanelMedia
            // 
            infoPanelMedia.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            infoPanelMedia.BackColor = SystemColors.Control;
            infoPanelMedia.Controls.Add(label5);
            infoPanelMedia.Location = new Point(2, 5);
            infoPanelMedia.Name = "infoPanelMedia";
            infoPanelMedia.Size = new Size(753, 61);
            infoPanelMedia.TabIndex = 4;
            // 
            // label5
            // 
            label5.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label5.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label5.Location = new Point(3, 3);
            label5.Name = "label5";
            label5.Size = new Size(747, 55);
            label5.TabIndex = 0;
            label5.Text = "Configuration Options for Exporting the Media DAT.";
            label5.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // exportMediaBtn
            // 
            exportMediaBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            exportMediaBtn.Location = new Point(595, 516);
            exportMediaBtn.Name = "exportMediaBtn";
            exportMediaBtn.Size = new Size(160, 23);
            exportMediaBtn.TabIndex = 0;
            exportMediaBtn.Text = "Export Media";
            exportMediaBtn.UseVisualStyleBackColor = true;
            exportMediaBtn.Click += exportMediaBtn_Click;
            // 
            // bottomBarPanel
            // 
            bottomBarPanel.Controls.Add(panel3);
            bottomBarPanel.Controls.Add(btnLayoutPanel);
            bottomBarPanel.Dock = DockStyle.Bottom;
            bottomBarPanel.Location = new Point(0, 630);
            bottomBarPanel.Margin = new Padding(0);
            bottomBarPanel.Name = "bottomBarPanel";
            bottomBarPanel.Padding = new Padding(14, 10, 14, 10);
            bottomBarPanel.Size = new Size(800, 36);
            bottomBarPanel.TabIndex = 2;
            // 
            // panel3
            // 
            panel3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel3.BackColor = SystemColors.ControlDarkDark;
            panel3.Location = new Point(0, 0);
            panel3.Margin = new Padding(0);
            panel3.Name = "panel3";
            panel3.Size = new Size(800, 1);
            panel3.TabIndex = 37;
            // 
            // btnLayoutPanel
            // 
            btnLayoutPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnLayoutPanel.Controls.Add(backBtn);
            btnLayoutPanel.Controls.Add(saveSettingsBtn);
            btnLayoutPanel.FlowDirection = FlowDirection.RightToLeft;
            btnLayoutPanel.Location = new Point(384, 1);
            btnLayoutPanel.Margin = new Padding(0);
            btnLayoutPanel.Name = "btnLayoutPanel";
            btnLayoutPanel.Size = new Size(412, 31);
            btnLayoutPanel.TabIndex = 36;
            // 
            // backBtn
            // 
            backBtn.AutoSize = true;
            backBtn.Cursor = Cursors.Hand;
            backBtn.Location = new Point(312, 3);
            backBtn.Margin = new Padding(0, 3, 0, 0);
            backBtn.Name = "backBtn";
            backBtn.Size = new Size(100, 27);
            backBtn.TabIndex = 1;
            backBtn.Text = "← Exit";
            backBtn.UseVisualStyleBackColor = true;
            backBtn.Click += BackBtn_Click;
            // 
            // saveSettingsBtn
            // 
            saveSettingsBtn.AutoSize = true;
            saveSettingsBtn.Cursor = Cursors.Hand;
            saveSettingsBtn.Location = new Point(209, 3);
            saveSettingsBtn.Margin = new Padding(0, 3, 0, 0);
            saveSettingsBtn.Name = "saveSettingsBtn";
            saveSettingsBtn.Size = new Size(103, 27);
            saveSettingsBtn.TabIndex = 2;
            saveSettingsBtn.Text = "Save All Settings";
            saveSettingsBtn.UseVisualStyleBackColor = true;
            saveSettingsBtn.Click += saveSettingsBtn_Click;
            // 
            // ExportView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(autoSizeContainer);
            Name = "ExportView";
            Size = new Size(879, 666);
            autoSizeContainer.ResumeLayout(false);
            leftFillerPanel.ResumeLayout(false);
            rightFillerPanel.ResumeLayout(false);
            rootLayout.ResumeLayout(false);
            headerPanel.ResumeLayout(false);
            headerPanel.PerformLayout();
            bodyPanel.ResumeLayout(false);
            tabControl.ResumeLayout(false);
            softwarePage.ResumeLayout(false);
            softwarePanel.ResumeLayout(false);
            datStructureGroup.ResumeLayout(false);
            datStructureGroup.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            infoPanel.ResumeLayout(false);
            mediaPage.ResumeLayout(false);
            mediaPanel.ResumeLayout(false);
            mediaGroupBox.ResumeLayout(false);
            infoPanelMedia.ResumeLayout(false);
            bottomBarPanel.ResumeLayout(false);
            btnLayoutPanel.ResumeLayout(false);
            btnLayoutPanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel autoSizeContainer;
        private Panel leftFillerPanel;
        private Panel panel1;
        private Panel rightFillerPanel;
        private Panel panel2;
        private TableLayoutPanel rootLayout;
        private Panel headerPanel;
        private Panel panel4;
        private Label titleLbl;
        private Panel bodyPanel;
        private Panel bottomBarPanel;
        private Panel panel3;
        private FlowLayoutPanel btnLayoutPanel;
        private Button backBtn;
        private TabControl tabControl;
        private TabPage softwarePage;
        private TabPage mediaPage;
        private Panel softwarePanel;
        private Panel mediaPanel;
        private Button exportSoftwareBtn;
        private Button exportMediaBtn;
        private Label label1;
        private CheckBox export1g1rCB;
        private CheckBox skipExcludedGamesCB;
        private Panel infoPanel;
        private Label label3;
        private Label label2;
        private GroupBox groupBox1;
        private Label label4;
        private CheckBox skipExcludedDescriptorFamiliesCB;
        private GroupBox groupBox2;
        private Panel infoPanelMedia;
        private Label label5;
        private GroupBox datStructureGroup;
        private Label label6;
        private CheckBox exportAsM3uCB;
        private GroupBox mediaGroupBox;
        private FlowLayoutPanel mediaContainer;
        private Button saveSettingsBtn;
    }
}
