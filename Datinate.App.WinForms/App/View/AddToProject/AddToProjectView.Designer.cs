namespace com.RADIO.Datinate.App.View.datDetails.addToProject
{
    partial class AddToProjectView
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                HandleDisposing();
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            panelContainer = new Panel();
            datGroupGroupBox = new GroupBox();
            resourceGroupBox = new GroupBox();
            resourceContainer = new FlowLayoutPanel();
            mediaGroupBox = new GroupBox();
            mediaContainer = new FlowLayoutPanel();
            softwareGroupBox = new GroupBox();
            softwareContainer = new FlowLayoutPanel();
            contentTypeGroup = new GroupBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            contentTypeCombo = new ComboBox();
            addAllContentBtn = new Button();
            datNameGroup = new GroupBox();
            datNameLabel = new Label();
            datSubsetGroup = new GroupBox();
            subsetPanel = new Panel();
            radioDatContentUseSubset = new RadioButton();
            radioDatContentUseAll = new RadioButton();
            label1 = new Label();
            descriptionLabel = new Label();
            datSubsetFolderCombo = new ComboBox();
            datSubsetNameCombo = new ComboBox();
            controlsGroup = new GroupBox();
            mediaIncludeBtn = new Button();
            includeGamesBtn = new Button();
            panelContainer.SuspendLayout();
            datGroupGroupBox.SuspendLayout();
            resourceGroupBox.SuspendLayout();
            mediaGroupBox.SuspendLayout();
            softwareGroupBox.SuspendLayout();
            contentTypeGroup.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            datNameGroup.SuspendLayout();
            datSubsetGroup.SuspendLayout();
            subsetPanel.SuspendLayout();
            controlsGroup.SuspendLayout();
            SuspendLayout();
            // 
            // panelContainer
            // 
            panelContainer.Controls.Add(datGroupGroupBox);
            panelContainer.Controls.Add(contentTypeGroup);
            panelContainer.Controls.Add(datNameGroup);
            panelContainer.Controls.Add(datSubsetGroup);
            panelContainer.Controls.Add(controlsGroup);
            panelContainer.Dock = DockStyle.Fill;
            panelContainer.Location = new Point(0, 0);
            panelContainer.Margin = new Padding(4, 3, 4, 3);
            panelContainer.Name = "panelContainer";
            panelContainer.Size = new Size(527, 654);
            panelContainer.TabIndex = 14;
            // 
            // datGroupGroupBox
            // 
            datGroupGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            datGroupGroupBox.BackColor = Color.White;
            datGroupGroupBox.Controls.Add(resourceGroupBox);
            datGroupGroupBox.Controls.Add(mediaGroupBox);
            datGroupGroupBox.Controls.Add(softwareGroupBox);
            datGroupGroupBox.Location = new Point(4, 97);
            datGroupGroupBox.Name = "datGroupGroupBox";
            datGroupGroupBox.Size = new Size(519, 276);
            datGroupGroupBox.TabIndex = 13;
            datGroupGroupBox.TabStop = false;
            datGroupGroupBox.Text = "DAT Group";
            // 
            // resourceGroupBox
            // 
            resourceGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            resourceGroupBox.Controls.Add(resourceContainer);
            resourceGroupBox.Location = new Point(6, 209);
            resourceGroupBox.Name = "resourceGroupBox";
            resourceGroupBox.Size = new Size(507, 60);
            resourceGroupBox.TabIndex = 2;
            resourceGroupBox.TabStop = false;
            resourceGroupBox.Text = "Resource";
            // 
            // resourceContainer
            // 
            resourceContainer.Dock = DockStyle.Fill;
            resourceContainer.Location = new Point(3, 19);
            resourceContainer.Name = "resourceContainer";
            resourceContainer.Size = new Size(501, 38);
            resourceContainer.TabIndex = 0;
            // 
            // mediaGroupBox
            // 
            mediaGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            mediaGroupBox.Controls.Add(mediaContainer);
            mediaGroupBox.Location = new Point(6, 116);
            mediaGroupBox.Name = "mediaGroupBox";
            mediaGroupBox.Size = new Size(507, 100);
            mediaGroupBox.TabIndex = 1;
            mediaGroupBox.TabStop = false;
            mediaGroupBox.Text = "Media";
            // 
            // mediaContainer
            // 
            mediaContainer.Dock = DockStyle.Fill;
            mediaContainer.Location = new Point(3, 19);
            mediaContainer.Name = "mediaContainer";
            mediaContainer.Size = new Size(501, 78);
            mediaContainer.TabIndex = 0;
            // 
            // softwareGroupBox
            // 
            softwareGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            softwareGroupBox.Controls.Add(softwareContainer);
            softwareGroupBox.Location = new Point(6, 22);
            softwareGroupBox.Name = "softwareGroupBox";
            softwareGroupBox.Size = new Size(507, 100);
            softwareGroupBox.TabIndex = 0;
            softwareGroupBox.TabStop = false;
            softwareGroupBox.Text = "Software";
            // 
            // softwareContainer
            // 
            softwareContainer.Dock = DockStyle.Fill;
            softwareContainer.Location = new Point(3, 19);
            softwareContainer.Name = "softwareContainer";
            softwareContainer.Size = new Size(501, 78);
            softwareContainer.TabIndex = 0;
            // 
            // contentTypeGroup
            // 
            contentTypeGroup.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            contentTypeGroup.Controls.Add(tableLayoutPanel1);
            contentTypeGroup.Location = new Point(4, 392);
            contentTypeGroup.Margin = new Padding(4, 3, 4, 3);
            contentTypeGroup.Name = "contentTypeGroup";
            contentTypeGroup.Padding = new Padding(4, 3, 4, 3);
            contentTypeGroup.Size = new Size(519, 58);
            contentTypeGroup.TabIndex = 10;
            contentTypeGroup.TabStop = false;
            contentTypeGroup.Text = "Content Type";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(contentTypeCombo, 0, 0);
            tableLayoutPanel1.Controls.Add(addAllContentBtn, 1, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(4, 19);
            tableLayoutPanel1.Margin = new Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(511, 36);
            tableLayoutPanel1.TabIndex = 5;
            // 
            // contentTypeCombo
            // 
            contentTypeCombo.Dock = DockStyle.Fill;
            contentTypeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            contentTypeCombo.FormattingEnabled = true;
            contentTypeCombo.Location = new Point(4, 3);
            contentTypeCombo.Margin = new Padding(4, 3, 4, 3);
            contentTypeCombo.Name = "contentTypeCombo";
            contentTypeCombo.Size = new Size(440, 23);
            contentTypeCombo.TabIndex = 4;
            // 
            // addAllContentBtn
            // 
            addAllContentBtn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            addAllContentBtn.Location = new Point(452, 3);
            addAllContentBtn.Margin = new Padding(4, 3, 3, 3);
            addAllContentBtn.Name = "addAllContentBtn";
            addAllContentBtn.Size = new Size(56, 23);
            addAllContentBtn.TabIndex = 5;
            addAllContentBtn.Text = "Add All";
            addAllContentBtn.UseVisualStyleBackColor = true;
            addAllContentBtn.Click += addAllContentBtn_Click;
            // 
            // datNameGroup
            // 
            datNameGroup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            datNameGroup.BackColor = SystemColors.Control;
            datNameGroup.Controls.Add(datNameLabel);
            datNameGroup.Location = new Point(4, 3);
            datNameGroup.Margin = new Padding(4, 3, 4, 3);
            datNameGroup.Name = "datNameGroup";
            datNameGroup.Padding = new Padding(4, 3, 4, 3);
            datNameGroup.Size = new Size(519, 77);
            datNameGroup.TabIndex = 10;
            datNameGroup.TabStop = false;
            datNameGroup.Text = "Selected DAT";
            // 
            // datNameLabel
            // 
            datNameLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            datNameLabel.BackColor = Color.White;
            datNameLabel.Location = new Point(8, 18);
            datNameLabel.Margin = new Padding(4, 0, 4, 0);
            datNameLabel.Name = "datNameLabel";
            datNameLabel.Size = new Size(503, 51);
            datNameLabel.TabIndex = 7;
            datNameLabel.Text = "DAT Name";
            datNameLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // datSubsetGroup
            // 
            datSubsetGroup.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            datSubsetGroup.Controls.Add(subsetPanel);
            datSubsetGroup.Controls.Add(label1);
            datSubsetGroup.Controls.Add(descriptionLabel);
            datSubsetGroup.Controls.Add(datSubsetFolderCombo);
            datSubsetGroup.Controls.Add(datSubsetNameCombo);
            datSubsetGroup.Location = new Point(4, 456);
            datSubsetGroup.Margin = new Padding(4, 3, 4, 3);
            datSubsetGroup.Name = "datSubsetGroup";
            datSubsetGroup.Padding = new Padding(4, 3, 4, 3);
            datSubsetGroup.Size = new Size(519, 115);
            datSubsetGroup.TabIndex = 12;
            datSubsetGroup.TabStop = false;
            datSubsetGroup.Text = "DAT Entries Filter";
            // 
            // subsetPanel
            // 
            subsetPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            subsetPanel.Controls.Add(radioDatContentUseSubset);
            subsetPanel.Controls.Add(radioDatContentUseAll);
            subsetPanel.Location = new Point(9, 23);
            subsetPanel.Margin = new Padding(0);
            subsetPanel.Name = "subsetPanel";
            subsetPanel.Padding = new Padding(4, 3, 4, 3);
            subsetPanel.Size = new Size(506, 31);
            subsetPanel.TabIndex = 13;
            // 
            // radioDatContentUseSubset
            // 
            radioDatContentUseSubset.AutoSize = true;
            radioDatContentUseSubset.Location = new Point(89, 10);
            radioDatContentUseSubset.Margin = new Padding(4, 3, 4, 3);
            radioDatContentUseSubset.Name = "radioDatContentUseSubset";
            radioDatContentUseSubset.Size = new Size(82, 19);
            radioDatContentUseSubset.TabIndex = 1;
            radioDatContentUseSubset.Text = "Use Subset";
            radioDatContentUseSubset.UseVisualStyleBackColor = true;
            // 
            // radioDatContentUseAll
            // 
            radioDatContentUseAll.AutoSize = true;
            radioDatContentUseAll.Checked = true;
            radioDatContentUseAll.Location = new Point(11, 10);
            radioDatContentUseAll.Margin = new Padding(4, 3, 4, 3);
            radioDatContentUseAll.Name = "radioDatContentUseAll";
            radioDatContentUseAll.Size = new Size(70, 19);
            radioDatContentUseAll.TabIndex = 0;
            radioDatContentUseAll.TabStop = true;
            radioDatContentUseAll.Text = "No Filter";
            radioDatContentUseAll.UseVisualStyleBackColor = true;
            radioDatContentUseAll.CheckedChanged += radioDatContentUseAll_CheckedChanged;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Italic);
            label1.ForeColor = SystemColors.ControlDarkDark;
            label1.Location = new Point(278, 66);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(103, 13);
            label1.TabIndex = 14;
            label1.Text = "DAT Entry Path Part";
            // 
            // descriptionLabel
            // 
            descriptionLabel.AutoSize = true;
            descriptionLabel.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Italic);
            descriptionLabel.ForeColor = SystemColors.ControlDarkDark;
            descriptionLabel.Location = new Point(8, 66);
            descriptionLabel.Margin = new Padding(4, 0, 4, 0);
            descriptionLabel.Name = "descriptionLabel";
            descriptionLabel.Size = new Size(87, 13);
            descriptionLabel.TabIndex = 4;
            descriptionLabel.Text = "DAT Entry Name";
            // 
            // datSubsetFolderCombo
            // 
            datSubsetFolderCombo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            datSubsetFolderCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            datSubsetFolderCombo.FormattingEnabled = true;
            datSubsetFolderCombo.Location = new Point(278, 84);
            datSubsetFolderCombo.Margin = new Padding(4, 3, 4, 3);
            datSubsetFolderCombo.Name = "datSubsetFolderCombo";
            datSubsetFolderCombo.Size = new Size(233, 23);
            datSubsetFolderCombo.TabIndex = 12;
            // 
            // datSubsetNameCombo
            // 
            datSubsetNameCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            datSubsetNameCombo.FormattingEnabled = true;
            datSubsetNameCombo.Location = new Point(9, 84);
            datSubsetNameCombo.Margin = new Padding(4, 3, 4, 3);
            datSubsetNameCombo.Name = "datSubsetNameCombo";
            datSubsetNameCombo.Size = new Size(233, 23);
            datSubsetNameCombo.TabIndex = 5;
            datSubsetNameCombo.SelectedIndexChanged += datSubsetNameCombo_SelectedIndexChanged;
            // 
            // controlsGroup
            // 
            controlsGroup.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            controlsGroup.Controls.Add(mediaIncludeBtn);
            controlsGroup.Controls.Add(includeGamesBtn);
            controlsGroup.Location = new Point(4, 579);
            controlsGroup.Margin = new Padding(4, 3, 4, 3);
            controlsGroup.Name = "controlsGroup";
            controlsGroup.Padding = new Padding(4, 3, 4, 3);
            controlsGroup.Size = new Size(519, 55);
            controlsGroup.TabIndex = 6;
            controlsGroup.TabStop = false;
            controlsGroup.Text = "Add to Group";
            // 
            // mediaIncludeBtn
            // 
            mediaIncludeBtn.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            mediaIncludeBtn.Location = new Point(361, 22);
            mediaIncludeBtn.Margin = new Padding(4, 3, 4, 3);
            mediaIncludeBtn.Name = "mediaIncludeBtn";
            mediaIncludeBtn.Size = new Size(150, 27);
            mediaIncludeBtn.TabIndex = 13;
            mediaIncludeBtn.Text = "Media && Resource";
            mediaIncludeBtn.UseVisualStyleBackColor = true;
            mediaIncludeBtn.Click += mediaIncludeBtn_Click;
            // 
            // includeGamesBtn
            // 
            includeGamesBtn.Location = new Point(7, 22);
            includeGamesBtn.Margin = new Padding(4, 3, 4, 3);
            includeGamesBtn.Name = "includeGamesBtn";
            includeGamesBtn.Size = new Size(150, 27);
            includeGamesBtn.TabIndex = 1;
            includeGamesBtn.Text = "Software";
            includeGamesBtn.UseVisualStyleBackColor = true;
            includeGamesBtn.Click += includeBtn_Click;
            // 
            // AddToProjectView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(panelContainer);
            Name = "AddToProjectView";
            Size = new Size(527, 654);
            panelContainer.ResumeLayout(false);
            datGroupGroupBox.ResumeLayout(false);
            resourceGroupBox.ResumeLayout(false);
            mediaGroupBox.ResumeLayout(false);
            softwareGroupBox.ResumeLayout(false);
            contentTypeGroup.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            datNameGroup.ResumeLayout(false);
            datSubsetGroup.ResumeLayout(false);
            datSubsetGroup.PerformLayout();
            subsetPanel.ResumeLayout(false);
            subsetPanel.PerformLayout();
            controlsGroup.ResumeLayout(false);
            ResumeLayout(false);
        }

        private System.Windows.Forms.Panel panelContainer;
        private System.Windows.Forms.GroupBox datNameGroup;
        private System.Windows.Forms.Label datNameLabel;
        private System.Windows.Forms.GroupBox datSubsetGroup;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label descriptionLabel;
        private System.Windows.Forms.Panel subsetPanel;
        private System.Windows.Forms.RadioButton radioDatContentUseSubset;
        private System.Windows.Forms.RadioButton radioDatContentUseAll;
        private System.Windows.Forms.ComboBox datSubsetFolderCombo;
        private System.Windows.Forms.ComboBox datSubsetNameCombo;
        private System.Windows.Forms.GroupBox controlsGroup;
        private System.Windows.Forms.Button mediaIncludeBtn;
        private System.Windows.Forms.Button includeGamesBtn;
        private GroupBox contentTypeGroup;
        private ComboBox contentTypeCombo;
        private TableLayoutPanel tableLayoutPanel1;
        private Button addAllContentBtn;
        private GroupBox datGroupGroupBox;
        private GroupBox softwareGroupBox;
        private FlowLayoutPanel softwareContainer;
        private GroupBox resourceGroupBox;
        private FlowLayoutPanel resourceContainer;
        private GroupBox mediaGroupBox;
        private FlowLayoutPanel mediaContainer;
    }
}