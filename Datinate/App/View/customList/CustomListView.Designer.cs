using datinate.app;
using Datinate.App.View.customList;

namespace com.RADIO.Datinate.App.View.customList {
    partial class CustomListView {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
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
            gamesIncludedGroup = new GroupBox();
            tableLayoutPanel2 = new TableLayoutPanel();
            included_lv = new FastListUI();
            tableLayoutPanel1 = new TableLayoutPanel();
            fileCountLabel = new Label();
            includeSizeLabel = new Label();
            splitContainer2 = new SplitContainer();
            gamesExcludedGroup = new GroupBox();
            tableLayoutPanel3 = new TableLayoutPanel();
            excluded_lv = new FastListUI();
            tableLayoutPanel4 = new TableLayoutPanel();
            excludeSizeLabel = new Label();
            excludedCount_lbl = new Label();
            datInfoGroup = new GroupBox();
            createDatBtn = new Button();
            datPathLabel = new Label();
            datTypeLabel = new Label();
            label9 = new Label();
            pathLabel = new Label();
            datNameLabel = new Label();
            label5 = new Label();
            splitContainer3 = new SplitContainer();
            splitContainer1 = new SplitContainer();
            flagsCategoriesTabControl = new BorderlessTabControl();
            flagsPage = new TabPage();
            flagsGroup = new GroupBox();
            CopyFlagListToClipboardBtn = new LinkLabel();
            flagSummaryLabel = new Label();
            flagListView_lv = new FlagListUI();
            categoriesPage = new TabPage();
            groupBox2 = new GroupBox();
            linkLabel1 = new LinkLabel();
            categorySummaryLabel = new Label();
            categoryListView_lv = new FlagListUI();
            expressionFiltersUI = new ExpressionFiltersUI();
            mainContainer = new TableLayoutPanel();
            tableLayoutPanel6 = new TableLayoutPanel();
            groupBox5 = new GroupBox();
            freezeExpressionsUpdateCheckbox = new CheckBox();
            l2 = new Label();
            regexNumberLabel = new Label();
            l3 = new Label();
            regexLetterLabel = new Label();
            conditionalBtn = new Button();
            clearBtn = new Button();
            l1 = new Label();
            regexAnythingLabel = new Label();
            goBtn = new Button();
            excludeBtn = new Button();
            searchTextinput = new TextBox();
            includeBtn = new Button();
            gamesIncludedGroup.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            gamesExcludedGroup.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            tableLayoutPanel4.SuspendLayout();
            datInfoGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer3).BeginInit();
            splitContainer3.Panel1.SuspendLayout();
            splitContainer3.Panel2.SuspendLayout();
            splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            flagsCategoriesTabControl.SuspendLayout();
            flagsPage.SuspendLayout();
            flagsGroup.SuspendLayout();
            categoriesPage.SuspendLayout();
            groupBox2.SuspendLayout();
            mainContainer.SuspendLayout();
            tableLayoutPanel6.SuspendLayout();
            groupBox5.SuspendLayout();
            SuspendLayout();
            // 
            // gamesIncludedGroup
            // 
            gamesIncludedGroup.Controls.Add(tableLayoutPanel2);
            gamesIncludedGroup.Dock = DockStyle.Fill;
            gamesIncludedGroup.Location = new Point(0, 0);
            gamesIncludedGroup.Margin = new Padding(4, 3, 4, 3);
            gamesIncludedGroup.Name = "gamesIncludedGroup";
            gamesIncludedGroup.Padding = new Padding(4, 3, 4, 3);
            gamesIncludedGroup.Size = new Size(323, 475);
            gamesIncludedGroup.TabIndex = 25;
            gamesIncludedGroup.TabStop = false;
            gamesIncludedGroup.Text = "Entries Included";
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(included_lv, 0, 0);
            tableLayoutPanel2.Controls.Add(tableLayoutPanel1, 0, 1);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(4, 19);
            tableLayoutPanel2.Margin = new Padding(4, 3, 4, 3);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
            tableLayoutPanel2.Size = new Size(315, 453);
            tableLayoutPanel2.TabIndex = 9;
            // 
            // included_lv
            // 
            included_lv.Dock = DockStyle.Fill;
            included_lv.FullRowSelect = true;
            included_lv.HeaderStyle = ColumnHeaderStyle.None;
            included_lv.Location = new Point(4, 3);
            included_lv.Margin = new Padding(4, 3, 4, 3);
            included_lv.MultiSelect = false;
            included_lv.Name = "included_lv";
            included_lv.Size = new Size(307, 416);
            included_lv.TabIndex = 6;
            included_lv.UseCompatibleStateImageBehavior = false;
            included_lv.View = System.Windows.Forms.View.Details;
            included_lv.VirtualMode = true;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(fileCountLabel, 0, 0);
            tableLayoutPanel1.Controls.Add(includeSizeLabel, 1, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(4, 425);
            tableLayoutPanel1.Margin = new Padding(4, 3, 4, 3);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(307, 25);
            tableLayoutPanel1.TabIndex = 8;
            // 
            // fileCountLabel
            // 
            fileCountLabel.Anchor = AnchorStyles.Left;
            fileCountLabel.AutoEllipsis = true;
            fileCountLabel.AutoSize = true;
            fileCountLabel.Font = new Font("Segoe UI", 9F);
            fileCountLabel.Location = new Point(4, 5);
            fileCountLabel.Margin = new Padding(4, 0, 4, 0);
            fileCountLabel.Name = "fileCountLabel";
            fileCountLabel.Size = new Size(22, 15);
            fileCountLabel.TabIndex = 2;
            fileCountLabel.Text = "---";
            // 
            // includeSizeLabel
            // 
            includeSizeLabel.Anchor = AnchorStyles.Right;
            includeSizeLabel.AutoEllipsis = true;
            includeSizeLabel.AutoSize = true;
            includeSizeLabel.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            includeSizeLabel.Location = new Point(272, 5);
            includeSizeLabel.Margin = new Padding(4, 3, 4, 3);
            includeSizeLabel.Name = "includeSizeLabel";
            includeSizeLabel.Size = new Size(31, 15);
            includeSizeLabel.TabIndex = 9;
            includeSizeLabel.Text = "Size";
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Margin = new Padding(4, 3, 4, 3);
            splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(gamesIncludedGroup);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(gamesExcludedGroup);
            splitContainer2.Size = new Size(663, 475);
            splitContainer2.SplitterDistance = 323;
            splitContainer2.SplitterWidth = 5;
            splitContainer2.TabIndex = 26;
            // 
            // gamesExcludedGroup
            // 
            gamesExcludedGroup.Controls.Add(tableLayoutPanel3);
            gamesExcludedGroup.Dock = DockStyle.Fill;
            gamesExcludedGroup.Location = new Point(0, 0);
            gamesExcludedGroup.Margin = new Padding(4, 3, 4, 3);
            gamesExcludedGroup.Name = "gamesExcludedGroup";
            gamesExcludedGroup.Padding = new Padding(4, 3, 4, 3);
            gamesExcludedGroup.Size = new Size(335, 475);
            gamesExcludedGroup.TabIndex = 19;
            gamesExcludedGroup.TabStop = false;
            gamesExcludedGroup.Text = "Entries Excluded";
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 1;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Controls.Add(excluded_lv, 0, 0);
            tableLayoutPanel3.Controls.Add(tableLayoutPanel4, 0, 1);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(4, 19);
            tableLayoutPanel3.Margin = new Padding(4, 3, 4, 3);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 2;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
            tableLayoutPanel3.Size = new Size(327, 453);
            tableLayoutPanel3.TabIndex = 10;
            // 
            // excluded_lv
            // 
            excluded_lv.Dock = DockStyle.Fill;
            excluded_lv.FullRowSelect = true;
            excluded_lv.HeaderStyle = ColumnHeaderStyle.None;
            excluded_lv.Location = new Point(4, 3);
            excluded_lv.Margin = new Padding(4, 3, 4, 3);
            excluded_lv.MultiSelect = false;
            excluded_lv.Name = "excluded_lv";
            excluded_lv.Size = new Size(319, 416);
            excluded_lv.TabIndex = 6;
            excluded_lv.UseCompatibleStateImageBehavior = false;
            excluded_lv.View = System.Windows.Forms.View.Details;
            excluded_lv.VirtualMode = true;
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.ColumnCount = 2;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel4.Controls.Add(excludeSizeLabel, 1, 0);
            tableLayoutPanel4.Controls.Add(excludedCount_lbl, 0, 0);
            tableLayoutPanel4.Dock = DockStyle.Fill;
            tableLayoutPanel4.Location = new Point(4, 425);
            tableLayoutPanel4.Margin = new Padding(4, 3, 4, 3);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 1;
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.Size = new Size(319, 25);
            tableLayoutPanel4.TabIndex = 11;
            // 
            // excludeSizeLabel
            // 
            excludeSizeLabel.Anchor = AnchorStyles.Right;
            excludeSizeLabel.AutoEllipsis = true;
            excludeSizeLabel.AutoSize = true;
            excludeSizeLabel.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            excludeSizeLabel.Location = new Point(284, 5);
            excludeSizeLabel.Margin = new Padding(4, 3, 4, 3);
            excludeSizeLabel.Name = "excludeSizeLabel";
            excludeSizeLabel.Size = new Size(31, 15);
            excludeSizeLabel.TabIndex = 10;
            excludeSizeLabel.Text = "Size";
            // 
            // excludedCount_lbl
            // 
            excludedCount_lbl.Anchor = AnchorStyles.Left;
            excludedCount_lbl.AutoEllipsis = true;
            excludedCount_lbl.AutoSize = true;
            excludedCount_lbl.Location = new Point(4, 5);
            excludedCount_lbl.Margin = new Padding(4, 3, 4, 3);
            excludedCount_lbl.Name = "excludedCount_lbl";
            excludedCount_lbl.Size = new Size(22, 15);
            excludedCount_lbl.TabIndex = 9;
            excludedCount_lbl.Text = "---";
            // 
            // datInfoGroup
            // 
            datInfoGroup.Controls.Add(createDatBtn);
            datInfoGroup.Controls.Add(datPathLabel);
            datInfoGroup.Controls.Add(datTypeLabel);
            datInfoGroup.Controls.Add(label9);
            datInfoGroup.Controls.Add(pathLabel);
            datInfoGroup.Controls.Add(datNameLabel);
            datInfoGroup.Controls.Add(label5);
            datInfoGroup.Dock = DockStyle.Fill;
            datInfoGroup.Location = new Point(475, 3);
            datInfoGroup.Margin = new Padding(4, 3, 4, 3);
            datInfoGroup.Name = "datInfoGroup";
            datInfoGroup.Padding = new Padding(4, 3, 4, 3);
            datInfoGroup.Size = new Size(567, 102);
            datInfoGroup.TabIndex = 28;
            datInfoGroup.TabStop = false;
            datInfoGroup.Text = "DAT Information";
            // 
            // createDatBtn
            // 
            createDatBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            createDatBtn.Location = new Point(399, 17);
            createDatBtn.Margin = new Padding(4, 3, 4, 3);
            createDatBtn.Name = "createDatBtn";
            createDatBtn.Size = new Size(160, 27);
            createDatBtn.TabIndex = 8;
            createDatBtn.Text = "Create DAT";
            createDatBtn.UseVisualStyleBackColor = true;
            createDatBtn.Click += createDatBtn_Click;
            // 
            // datPathLabel
            // 
            datPathLabel.AutoSize = true;
            datPathLabel.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            datPathLabel.Location = new Point(63, 81);
            datPathLabel.Margin = new Padding(4, 0, 4, 0);
            datPathLabel.Name = "datPathLabel";
            datPathLabel.Size = new Size(19, 15);
            datPathLabel.TabIndex = 6;
            datPathLabel.Text = "---";
            // 
            // datTypeLabel
            // 
            datTypeLabel.AutoSize = true;
            datTypeLabel.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            datTypeLabel.Location = new Point(63, 60);
            datTypeLabel.Margin = new Padding(4, 0, 4, 0);
            datTypeLabel.Name = "datTypeLabel";
            datTypeLabel.Size = new Size(19, 15);
            datTypeLabel.TabIndex = 5;
            datTypeLabel.Text = "---";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.ForeColor = SystemColors.ControlDarkDark;
            label9.Location = new Point(21, 60);
            label9.Margin = new Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new Size(34, 15);
            label9.TabIndex = 4;
            label9.Text = "Type:";
            // 
            // pathLabel
            // 
            pathLabel.AutoSize = true;
            pathLabel.ForeColor = SystemColors.ControlDarkDark;
            pathLabel.Location = new Point(21, 82);
            pathLabel.Margin = new Padding(4, 0, 4, 0);
            pathLabel.Name = "pathLabel";
            pathLabel.Size = new Size(34, 15);
            pathLabel.TabIndex = 2;
            pathLabel.Text = "Path:";
            // 
            // datNameLabel
            // 
            datNameLabel.AutoSize = true;
            datNameLabel.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            datNameLabel.Location = new Point(63, 40);
            datNameLabel.Margin = new Padding(4, 0, 4, 0);
            datNameLabel.Name = "datNameLabel";
            datNameLabel.Size = new Size(19, 15);
            datNameLabel.TabIndex = 1;
            datNameLabel.Text = "---";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.ForeColor = SystemColors.ControlDarkDark;
            label5.Location = new Point(24, 41);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(31, 15);
            label5.TabIndex = 0;
            label5.Text = "DAT:";
            // 
            // splitContainer3
            // 
            splitContainer3.Dock = DockStyle.Fill;
            splitContainer3.Location = new Point(4, 117);
            splitContainer3.Margin = new Padding(4, 3, 4, 3);
            splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            splitContainer3.Panel1.Controls.Add(splitContainer1);
            // 
            // splitContainer3.Panel2
            // 
            splitContainer3.Panel2.Controls.Add(splitContainer2);
            splitContainer3.Size = new Size(1046, 475);
            splitContainer3.SplitterDistance = 378;
            splitContainer3.SplitterWidth = 5;
            splitContainer3.TabIndex = 27;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Margin = new Padding(4, 3, 4, 3);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(flagsCategoriesTabControl);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(expressionFiltersUI);
            splitContainer1.Size = new Size(378, 475);
            splitContainer1.SplitterDistance = 220;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 21;
            // 
            // flagsCategoriesTabControl
            // 
            flagsCategoriesTabControl.Controls.Add(flagsPage);
            flagsCategoriesTabControl.Controls.Add(categoriesPage);
            flagsCategoriesTabControl.Dock = DockStyle.Fill;
            flagsCategoriesTabControl.Location = new Point(0, 0);
            flagsCategoriesTabControl.Name = "flagsCategoriesTabControl";
            flagsCategoriesTabControl.SelectedIndex = 0;
            flagsCategoriesTabControl.Size = new Size(378, 220);
            flagsCategoriesTabControl.TabIndex = 22;
            flagsCategoriesTabControl.SelectedIndexChanged += flagsCategoriesTabControl_SelectedIndexChanged;
            // 
            // flagsPage
            // 
            flagsPage.Controls.Add(flagsGroup);
            flagsPage.Location = new Point(0, 20);
            flagsPage.Name = "flagsPage";
            flagsPage.Padding = new Padding(3);
            flagsPage.Size = new Size(378, 200);
            flagsPage.TabIndex = 0;
            flagsPage.Text = "Flags";
            flagsPage.UseVisualStyleBackColor = true;
            // 
            // flagsGroup
            // 
            flagsGroup.Controls.Add(CopyFlagListToClipboardBtn);
            flagsGroup.Controls.Add(flagSummaryLabel);
            flagsGroup.Controls.Add(flagListView_lv);
            flagsGroup.Dock = DockStyle.Fill;
            flagsGroup.Location = new Point(3, 3);
            flagsGroup.Margin = new Padding(4, 3, 4, 3);
            flagsGroup.Name = "flagsGroup";
            flagsGroup.Padding = new Padding(4, 3, 4, 3);
            flagsGroup.Size = new Size(372, 194);
            flagsGroup.TabIndex = 17;
            flagsGroup.TabStop = false;
            // 
            // CopyFlagListToClipboardBtn
            // 
            CopyFlagListToClipboardBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            CopyFlagListToClipboardBtn.AutoSize = true;
            CopyFlagListToClipboardBtn.Location = new Point(349, 2);
            CopyFlagListToClipboardBtn.Name = "CopyFlagListToClipboardBtn";
            CopyFlagListToClipboardBtn.Size = new Size(15, 15);
            CopyFlagListToClipboardBtn.TabIndex = 21;
            CopyFlagListToClipboardBtn.TabStop = true;
            CopyFlagListToClipboardBtn.Text = "C";
            CopyFlagListToClipboardBtn.LinkClicked += CopyFlagListToClipboardBtn_LinkClicked;
            // 
            // flagSummaryLabel
            // 
            flagSummaryLabel.Location = new Point(1, 3);
            flagSummaryLabel.Margin = new Padding(4, 0, 4, 0);
            flagSummaryLabel.Name = "flagSummaryLabel";
            flagSummaryLabel.Size = new Size(148, 15);
            flagSummaryLabel.TabIndex = 20;
            flagSummaryLabel.Text = "---";
            flagSummaryLabel.TextAlign = ContentAlignment.BottomLeft;
            // 
            // flagListView_lv
            // 
            flagListView_lv.Dock = DockStyle.Fill;
            flagListView_lv.FullRowSelect = true;
            flagListView_lv.Location = new Point(4, 19);
            flagListView_lv.Margin = new Padding(4, 3, 4, 3);
            flagListView_lv.MultiSelect = false;
            flagListView_lv.Name = "flagListView_lv";
            flagListView_lv.Size = new Size(364, 172);
            flagListView_lv.TabIndex = 4;
            flagListView_lv.UseCompatibleStateImageBehavior = false;
            flagListView_lv.View = System.Windows.Forms.View.Details;
            flagListView_lv.SelectedIndexChanged += flagListView_lv_SelectedIndexChanged;
            // 
            // categoriesPage
            // 
            categoriesPage.Controls.Add(groupBox2);
            categoriesPage.Location = new Point(0, 20);
            categoriesPage.Name = "categoriesPage";
            categoriesPage.Padding = new Padding(3);
            categoriesPage.Size = new Size(378, 200);
            categoriesPage.TabIndex = 1;
            categoriesPage.Text = "Categories";
            categoriesPage.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(linkLabel1);
            groupBox2.Controls.Add(categorySummaryLabel);
            groupBox2.Controls.Add(categoryListView_lv);
            groupBox2.Dock = DockStyle.Fill;
            groupBox2.Location = new Point(3, 3);
            groupBox2.Margin = new Padding(4, 3, 4, 3);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(4, 3, 4, 3);
            groupBox2.Size = new Size(372, 194);
            groupBox2.TabIndex = 18;
            groupBox2.TabStop = false;
            // 
            // linkLabel1
            // 
            linkLabel1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            linkLabel1.AutoSize = true;
            linkLabel1.Location = new Point(512, 2);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(15, 15);
            linkLabel1.TabIndex = 21;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "C";
            // 
            // categorySummaryLabel
            // 
            categorySummaryLabel.Location = new Point(1, 3);
            categorySummaryLabel.Margin = new Padding(4, 0, 4, 0);
            categorySummaryLabel.Name = "categorySummaryLabel";
            categorySummaryLabel.Size = new Size(148, 15);
            categorySummaryLabel.TabIndex = 20;
            categorySummaryLabel.Text = "---";
            categorySummaryLabel.TextAlign = ContentAlignment.BottomLeft;
            // 
            // categoryListView_lv
            // 
            categoryListView_lv.Dock = DockStyle.Fill;
            categoryListView_lv.FullRowSelect = true;
            categoryListView_lv.Location = new Point(4, 19);
            categoryListView_lv.Margin = new Padding(4, 3, 4, 3);
            categoryListView_lv.MultiSelect = false;
            categoryListView_lv.Name = "categoryListView_lv";
            categoryListView_lv.Size = new Size(364, 172);
            categoryListView_lv.TabIndex = 4;
            categoryListView_lv.UseCompatibleStateImageBehavior = false;
            categoryListView_lv.View = System.Windows.Forms.View.Details;
            categoryListView_lv.SelectedIndexChanged += categoryListView_lv_SelectedIndexChanged;
            // 
            // expressionFiltersUI
            // 
            expressionFiltersUI.Dock = DockStyle.Fill;
            expressionFiltersUI.Location = new Point(0, 0);
            expressionFiltersUI.Name = "expressionFiltersUI";
            expressionFiltersUI.Size = new Size(378, 250);
            expressionFiltersUI.TabIndex = 32;
            // 
            // mainContainer
            // 
            mainContainer.ColumnCount = 1;
            mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainContainer.Controls.Add(tableLayoutPanel6, 0, 0);
            mainContainer.Controls.Add(splitContainer3, 0, 1);
            mainContainer.Dock = DockStyle.Fill;
            mainContainer.Location = new Point(0, 0);
            mainContainer.Margin = new Padding(4, 3, 4, 3);
            mainContainer.Name = "mainContainer";
            mainContainer.RowCount = 2;
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 114F));
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainContainer.Size = new Size(1054, 595);
            mainContainer.TabIndex = 31;
            // 
            // tableLayoutPanel6
            // 
            tableLayoutPanel6.ColumnCount = 2;
            tableLayoutPanel6.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 471F));
            tableLayoutPanel6.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel6.Controls.Add(groupBox5, 0, 0);
            tableLayoutPanel6.Controls.Add(datInfoGroup, 1, 0);
            tableLayoutPanel6.Dock = DockStyle.Fill;
            tableLayoutPanel6.Location = new Point(4, 3);
            tableLayoutPanel6.Margin = new Padding(4, 3, 4, 3);
            tableLayoutPanel6.Name = "tableLayoutPanel6";
            tableLayoutPanel6.RowCount = 1;
            tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel6.Size = new Size(1046, 108);
            tableLayoutPanel6.TabIndex = 29;
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(freezeExpressionsUpdateCheckbox);
            groupBox5.Controls.Add(l2);
            groupBox5.Controls.Add(regexNumberLabel);
            groupBox5.Controls.Add(l3);
            groupBox5.Controls.Add(regexLetterLabel);
            groupBox5.Controls.Add(conditionalBtn);
            groupBox5.Controls.Add(clearBtn);
            groupBox5.Controls.Add(l1);
            groupBox5.Controls.Add(regexAnythingLabel);
            groupBox5.Controls.Add(goBtn);
            groupBox5.Controls.Add(excludeBtn);
            groupBox5.Controls.Add(searchTextinput);
            groupBox5.Controls.Add(includeBtn);
            groupBox5.Dock = DockStyle.Fill;
            groupBox5.Location = new Point(4, 3);
            groupBox5.Margin = new Padding(4, 3, 4, 3);
            groupBox5.Name = "groupBox5";
            groupBox5.Padding = new Padding(4, 3, 4, 3);
            groupBox5.Size = new Size(463, 102);
            groupBox5.TabIndex = 20;
            groupBox5.TabStop = false;
            groupBox5.Text = "Filter";
            // 
            // freezeExpressionsUpdateCheckbox
            // 
            freezeExpressionsUpdateCheckbox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            freezeExpressionsUpdateCheckbox.AutoSize = true;
            freezeExpressionsUpdateCheckbox.CheckAlign = ContentAlignment.MiddleRight;
            freezeExpressionsUpdateCheckbox.Font = new Font("Microsoft Sans Serif", 6.75F);
            freezeExpressionsUpdateCheckbox.ForeColor = SystemColors.ControlDarkDark;
            freezeExpressionsUpdateCheckbox.Location = new Point(284, 48);
            freezeExpressionsUpdateCheckbox.Margin = new Padding(4, 3, 4, 3);
            freezeExpressionsUpdateCheckbox.Name = "freezeExpressionsUpdateCheckbox";
            freezeExpressionsUpdateCheckbox.Size = new Size(171, 16);
            freezeExpressionsUpdateCheckbox.TabIndex = 18;
            freezeExpressionsUpdateCheckbox.Text = "Pause Updates when Adding Filters";
            freezeExpressionsUpdateCheckbox.UseVisualStyleBackColor = true;
            // 
            // l2
            // 
            l2.AutoSize = true;
            l2.ForeColor = SystemColors.ControlDarkDark;
            l2.Location = new Point(181, 81);
            l2.Margin = new Padding(4, 0, 4, 0);
            l2.Name = "l2";
            l2.Size = new Size(124, 15);
            l2.TabIndex = 9;
            l2.Text = "One or more numbers";
            // 
            // regexNumberLabel
            // 
            regexNumberLabel.AutoSize = true;
            regexNumberLabel.Font = new Font("Microsoft Sans Serif", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            regexNumberLabel.ForeColor = SystemColors.WindowText;
            regexNumberLabel.Location = new Point(298, 70);
            regexNumberLabel.Margin = new Padding(4, 0, 4, 0);
            regexNumberLabel.Name = "regexNumberLabel";
            regexNumberLabel.Size = new Size(23, 31);
            regexNumberLabel.TabIndex = 12;
            regexNumberLabel.Text = ":";
            // 
            // l3
            // 
            l3.AutoSize = true;
            l3.ForeColor = SystemColors.ControlDarkDark;
            l3.Location = new Point(333, 81);
            l3.Margin = new Padding(4, 0, 4, 0);
            l3.Name = "l3";
            l3.Size = new Size(109, 15);
            l3.TabIndex = 10;
            l3.Text = "One or more letters";
            // 
            // regexLetterLabel
            // 
            regexLetterLabel.AutoSize = true;
            regexLetterLabel.Font = new Font("Microsoft Sans Serif", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            regexLetterLabel.ForeColor = SystemColors.WindowText;
            regexLetterLabel.Location = new Point(437, 78);
            regexLetterLabel.Margin = new Padding(4, 0, 4, 0);
            regexLetterLabel.Name = "regexLetterLabel";
            regexLetterLabel.Size = new Size(25, 31);
            regexLetterLabel.TabIndex = 11;
            regexLetterLabel.Text = "\"";
            // 
            // conditionalBtn
            // 
            conditionalBtn.Location = new Point(142, 48);
            conditionalBtn.Margin = new Padding(4, 3, 4, 3);
            conditionalBtn.Name = "conditionalBtn";
            conditionalBtn.Size = new Size(135, 25);
            conditionalBtn.TabIndex = 17;
            conditionalBtn.Text = "Conditional Exclude";
            conditionalBtn.UseVisualStyleBackColor = true;
            conditionalBtn.Click += OnConditionalExcludeBtn;
            // 
            // clearBtn
            // 
            clearBtn.Location = new Point(379, 20);
            clearBtn.Margin = new Padding(4, 3, 4, 3);
            clearBtn.Name = "clearBtn";
            clearBtn.Size = new Size(38, 25);
            clearBtn.TabIndex = 16;
            clearBtn.Text = "Clr";
            clearBtn.UseVisualStyleBackColor = true;
            clearBtn.Click += OnSearchClear;
            // 
            // l1
            // 
            l1.AutoSize = true;
            l1.ForeColor = SystemColors.ControlDarkDark;
            l1.Location = new Point(7, 82);
            l1.Margin = new Padding(4, 0, 4, 0);
            l1.Name = "l1";
            l1.Size = new Size(140, 15);
            l1.TabIndex = 8;
            l1.Text = "Zero or more of anything";
            // 
            // regexAnythingLabel
            // 
            regexAnythingLabel.AutoSize = true;
            regexAnythingLabel.Font = new Font("Microsoft Sans Serif", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            regexAnythingLabel.ForeColor = SystemColors.WindowText;
            regexAnythingLabel.Location = new Point(143, 79);
            regexAnythingLabel.Margin = new Padding(4, 0, 4, 0);
            regexAnythingLabel.Name = "regexAnythingLabel";
            regexAnythingLabel.Size = new Size(26, 31);
            regexAnythingLabel.TabIndex = 13;
            regexAnythingLabel.Text = "*";
            // 
            // goBtn
            // 
            goBtn.Location = new Point(417, 20);
            goBtn.Margin = new Padding(4, 3, 4, 3);
            goBtn.Name = "goBtn";
            goBtn.Size = new Size(38, 25);
            goBtn.TabIndex = 15;
            goBtn.Text = "Go";
            goBtn.UseVisualStyleBackColor = true;
            goBtn.Click += OnSearch;
            // 
            // excludeBtn
            // 
            excludeBtn.Location = new Point(75, 48);
            excludeBtn.Margin = new Padding(4, 3, 4, 3);
            excludeBtn.Name = "excludeBtn";
            excludeBtn.Size = new Size(68, 25);
            excludeBtn.TabIndex = 1;
            excludeBtn.Text = "Exclude";
            excludeBtn.UseVisualStyleBackColor = true;
            excludeBtn.Click += OnExcludeBtnClick;
            // 
            // searchTextinput
            // 
            searchTextinput.Location = new Point(8, 21);
            searchTextinput.Margin = new Padding(4, 3, 4, 3);
            searchTextinput.Name = "searchTextinput";
            searchTextinput.Size = new Size(366, 23);
            searchTextinput.TabIndex = 5;
            searchTextinput.TextChanged += OnSearchTextChange;
            searchTextinput.DoubleClick += search_tb_DoubleClick;
            searchTextinput.KeyUp += searchTextinput_KeyUp;
            // 
            // includeBtn
            // 
            includeBtn.Location = new Point(8, 48);
            includeBtn.Margin = new Padding(4, 3, 4, 3);
            includeBtn.Name = "includeBtn";
            includeBtn.Size = new Size(68, 25);
            includeBtn.TabIndex = 7;
            includeBtn.Text = "Include";
            includeBtn.UseVisualStyleBackColor = true;
            includeBtn.Click += OnIncludeBtnClick;
            // 
            // CustomListView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(mainContainer);
            Name = "CustomListView";
            Size = new Size(1054, 595);
            gamesIncludedGroup.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            gamesExcludedGroup.ResumeLayout(false);
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel4.ResumeLayout(false);
            tableLayoutPanel4.PerformLayout();
            datInfoGroup.ResumeLayout(false);
            datInfoGroup.PerformLayout();
            splitContainer3.Panel1.ResumeLayout(false);
            splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer3).EndInit();
            splitContainer3.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            flagsCategoriesTabControl.ResumeLayout(false);
            flagsPage.ResumeLayout(false);
            flagsGroup.ResumeLayout(false);
            flagsGroup.PerformLayout();
            categoriesPage.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            mainContainer.ResumeLayout(false);
            tableLayoutPanel6.ResumeLayout(false);
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.GroupBox gamesIncludedGroup;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label fileCountLabel;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.GroupBox gamesExcludedGroup;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label excludedCount_lbl;
        private System.Windows.Forms.GroupBox datInfoGroup;
        private System.Windows.Forms.Button createDatBtn;
        private System.Windows.Forms.Label datPathLabel;
        private System.Windows.Forms.Label datTypeLabel;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label pathLabel;
        private System.Windows.Forms.Label datNameLabel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox flagsGroup;
        private System.Windows.Forms.Label flagSummaryLabel;
        private FlagListUI flagListView_lv;
        private System.Windows.Forms.TableLayoutPanel mainContainer;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.CheckBox freezeExpressionsUpdateCheckbox;
        private System.Windows.Forms.Button conditionalBtn;
        private System.Windows.Forms.Button clearBtn;
        private System.Windows.Forms.Button goBtn;
        private System.Windows.Forms.Label l1;
        private System.Windows.Forms.Label regexAnythingLabel;
        private System.Windows.Forms.Label l2;
        private System.Windows.Forms.Label regexNumberLabel;
        private System.Windows.Forms.Label l3;
        private System.Windows.Forms.Label regexLetterLabel;
        private System.Windows.Forms.Button excludeBtn;
        private System.Windows.Forms.TextBox searchTextinput;
        private System.Windows.Forms.Button includeBtn;
        private LinkLabel CopyFlagListToClipboardBtn;
        private TabPage flagsPage;
        private TabPage categoriesPage;
        private FastListUI included_lv;
        private FastListUI excluded_lv;
        private GroupBox groupBox2;
        private LinkLabel linkLabel1;
        private Label categorySummaryLabel;
        private FlagListUI categoryListView_lv;
        private ExpressionFiltersUI expressionFiltersUI;
        private Label excludeSizeLabel;
        private Label includeSizeLabel;
        private BorderlessTabControl flagsCategoriesTabControl;
        private TableLayoutPanel tableLayoutPanel4;
    }
}
