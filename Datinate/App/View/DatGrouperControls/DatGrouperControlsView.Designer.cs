namespace datinate.app
{
    partial class DatGrouperControlsView
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
            if (disposing) HandleDisposing();

            if (disposing && (components != null))
            {
                
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
            assignMediaBtn = new Button();
            partReportsBtn = new Button();
            curationTalliesLabel = new Label();
            swapPanelsRightBtn = new Button();
            mediaToggleBtn = new Button();
            entityIconPic = new PictureBox();
            searchNameBtn = new Button();
            swapPanelsLeftBtn = new Button();
            treeNodePic = new SmartCenterPictureBox();
            titleUI = new DatGrouperTitleUI();
            tableLayoutPanel1 = new TableLayoutPanel();
            hideExcludedBtn = new CheckBoxButton();
            hideAliasesBtn = new CheckBoxButton();
            datGrouperNodePreview = new DatGrouperNodePreviewUI();
            ((System.ComponentModel.ISupportInitialize)entityIconPic).BeginInit();
            ((System.ComponentModel.ISupportInitialize)treeNodePic).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // assignMediaBtn
            // 
            assignMediaBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            assignMediaBtn.Location = new Point(221, 158);
            assignMediaBtn.Name = "assignMediaBtn";
            assignMediaBtn.Size = new Size(315, 23);
            assignMediaBtn.TabIndex = 0;
            assignMediaBtn.Text = "Assign Media";
            assignMediaBtn.UseVisualStyleBackColor = true;
            assignMediaBtn.Visible = false;
            assignMediaBtn.Click += assignMediaBtn_Click;
            // 
            // partReportsBtn
            // 
            partReportsBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            partReportsBtn.Location = new Point(149, 158);
            partReportsBtn.Name = "partReportsBtn";
            partReportsBtn.Size = new Size(315, 23);
            partReportsBtn.TabIndex = 1;
            partReportsBtn.Text = "Part Reports";
            partReportsBtn.UseVisualStyleBackColor = true;
            partReportsBtn.Visible = false;
            partReportsBtn.Click += partReportsBtn_Click;
            // 
            // curationTalliesLabel
            // 
            curationTalliesLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            curationTalliesLabel.AutoEllipsis = true;
            curationTalliesLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            curationTalliesLabel.ForeColor = Color.Black;
            curationTalliesLabel.Location = new Point(3, 187);
            curationTalliesLabel.Name = "curationTalliesLabel";
            curationTalliesLabel.Size = new Size(607, 23);
            curationTalliesLabel.TabIndex = 2;
            curationTalliesLabel.Text = "Curated: 10,301 / 10,817 - 98%";
            curationTalliesLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // swapPanelsRightBtn
            // 
            swapPanelsRightBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            swapPanelsRightBtn.Cursor = Cursors.Hand;
            swapPanelsRightBtn.FlatAppearance.BorderSize = 0;
            swapPanelsRightBtn.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            swapPanelsRightBtn.ForeColor = SystemColors.ControlDarkDark;
            swapPanelsRightBtn.Location = new Point(570, 187);
            swapPanelsRightBtn.Name = "swapPanelsRightBtn";
            swapPanelsRightBtn.Size = new Size(40, 23);
            swapPanelsRightBtn.TabIndex = 4;
            swapPanelsRightBtn.Text = "■ ■";
            swapPanelsRightBtn.UseVisualStyleBackColor = true;
            swapPanelsRightBtn.Click += swapPanelsBtn_Click;
            // 
            // mediaToggleBtn
            // 
            mediaToggleBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            mediaToggleBtn.Location = new Point(3, 158);
            mediaToggleBtn.Name = "mediaToggleBtn";
            mediaToggleBtn.Size = new Size(140, 23);
            mediaToggleBtn.TabIndex = 5;
            mediaToggleBtn.Text = "Toggle Media Mode";
            mediaToggleBtn.UseVisualStyleBackColor = true;
            mediaToggleBtn.Visible = false;
            mediaToggleBtn.Click += previewMediaBtn_Click;
            // 
            // entityIconPic
            // 
            entityIconPic.BackColor = Color.White;
            entityIconPic.Location = new Point(0, 0);
            entityIconPic.Name = "entityIconPic";
            entityIconPic.Size = new Size(30, 30);
            entityIconPic.SizeMode = PictureBoxSizeMode.Zoom;
            entityIconPic.TabIndex = 6;
            entityIconPic.TabStop = false;
            // 
            // searchNameBtn
            // 
            searchNameBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            searchNameBtn.Location = new Point(542, 158);
            searchNameBtn.Name = "searchNameBtn";
            searchNameBtn.Size = new Size(68, 23);
            searchNameBtn.TabIndex = 7;
            searchNameBtn.Text = "Search";
            searchNameBtn.UseVisualStyleBackColor = true;
            searchNameBtn.Visible = false;
            searchNameBtn.Click += SearchNameBtn_Click;
            // 
            // swapPanelsLeftBtn
            // 
            swapPanelsLeftBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            swapPanelsLeftBtn.Cursor = Cursors.Hand;
            swapPanelsLeftBtn.FlatAppearance.BorderSize = 0;
            swapPanelsLeftBtn.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            swapPanelsLeftBtn.ForeColor = SystemColors.ControlDarkDark;
            swapPanelsLeftBtn.Location = new Point(3, 187);
            swapPanelsLeftBtn.Name = "swapPanelsLeftBtn";
            swapPanelsLeftBtn.Size = new Size(40, 23);
            swapPanelsLeftBtn.TabIndex = 11;
            swapPanelsLeftBtn.Text = "■ ■";
            swapPanelsLeftBtn.UseVisualStyleBackColor = true;
            swapPanelsLeftBtn.Click += swapPanelsBtn_Click;
            // 
            // treeNodePic
            // 
            treeNodePic.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            treeNodePic.BackColor = Color.White;
            treeNodePic.BackgroundImageLayout = ImageLayout.Center;
            treeNodePic.Location = new Point(-3, 86);
            treeNodePic.Name = "treeNodePic";
            treeNodePic.Size = new Size(613, 30);
            treeNodePic.SizeMode = PictureBoxSizeMode.CenterImage;
            treeNodePic.TabIndex = 12;
            treeNodePic.TabStop = false;
            treeNodePic.Visible = false;
            // 
            // titleUI
            // 
            titleUI.Anchor = AnchorStyles.None;
            titleUI.BackColor = Color.Transparent;
            titleUI.CornerRadius = 2;
            titleUI.Location = new Point(246, 5);
            titleUI.Name = "titleUI";
            titleUI.Size = new Size(120, 26);
            titleUI.TabIndex = 13;
            titleUI.TabStop = false;
            titleUI.Text = "datGrouperTitleui1";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.BackColor = Color.White;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Controls.Add(titleUI, 0, 0);
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(613, 36);
            tableLayoutPanel1.TabIndex = 14;
            // 
            // hideExcludedBtn
            // 
            hideExcludedBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            hideExcludedBtn.CheckedBackColor = SystemColors.GradientInactiveCaption;
            hideExcludedBtn.CheckedForeColor = SystemColors.ControlDarkDark;
            hideExcludedBtn.Cursor = Cursors.Hand;
            hideExcludedBtn.Location = new Point(49, 187);
            hideExcludedBtn.Name = "hideExcludedBtn";
            hideExcludedBtn.Padding = new Padding(16, 8, 16, 8);
            hideExcludedBtn.Size = new Size(100, 23);
            hideExcludedBtn.TabIndex = 15;
            hideExcludedBtn.Text = "Hide Excluded";
            hideExcludedBtn.Visible = false;
            hideExcludedBtn.CheckedChanged += hideExcludedBtn_CheckedChanged;
            // 
            // hideAliasesBtn
            // 
            hideAliasesBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            hideAliasesBtn.CheckedBackColor = SystemColors.GradientInactiveCaption;
            hideAliasesBtn.CheckedForeColor = SystemColors.ControlDarkDark;
            hideAliasesBtn.Cursor = Cursors.Hand;
            hideAliasesBtn.Location = new Point(464, 187);
            hideAliasesBtn.Name = "hideAliasesBtn";
            hideAliasesBtn.Padding = new Padding(16, 8, 16, 8);
            hideAliasesBtn.Size = new Size(100, 23);
            hideAliasesBtn.TabIndex = 16;
            hideAliasesBtn.Text = "Hide Aliases";
            hideAliasesBtn.Visible = false;
            hideAliasesBtn.CheckedChanged += hideAliasesBtn_CheckedChanged;
            // 
            // datGrouperNodePreview
            // 
            datGrouperNodePreview.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            datGrouperNodePreview.BackColor = SystemColors.Window;
            datGrouperNodePreview.Location = new Point(0, 36);
            datGrouperNodePreview.Name = "datGrouperNodePreview";
            datGrouperNodePreview.Size = new Size(613, 30);
            datGrouperNodePreview.TabIndex = 17;
            datGrouperNodePreview.TabStop = false;
            datGrouperNodePreview.Text = "datGrouperNodePreviewui1";
            // 
            // DatGrouperControlsView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(datGrouperNodePreview);
            Controls.Add(hideAliasesBtn);
            Controls.Add(hideExcludedBtn);
            Controls.Add(entityIconPic);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(treeNodePic);
            Controls.Add(swapPanelsLeftBtn);
            Controls.Add(searchNameBtn);
            Controls.Add(mediaToggleBtn);
            Controls.Add(swapPanelsRightBtn);
            Controls.Add(curationTalliesLabel);
            Controls.Add(partReportsBtn);
            Controls.Add(assignMediaBtn);
            Name = "DatGrouperControlsView";
            Size = new Size(613, 213);
            ((System.ComponentModel.ISupportInitialize)entityIconPic).EndInit();
            ((System.ComponentModel.ISupportInitialize)treeNodePic).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Button assignMediaBtn;
        private Button partReportsBtn;
        private Label curationTalliesLabel;
        private Button swapPanelsRightBtn;
        private Button mediaToggleBtn;
        private PictureBox entityIconPic;
        private Button searchNameBtn;
        private Button swapPanelsLeftBtn;
        private DatGrouperTitleUI titleUI;
        private TableLayoutPanel tableLayoutPanel1;
        private SmartCenterPictureBox treeNodePic;
        private CheckBoxButton hideExcludedBtn;
        private CheckBoxButton hideAliasesBtn;
        private DatGrouperNodePreviewUI datGrouperNodePreview;
    }
}
