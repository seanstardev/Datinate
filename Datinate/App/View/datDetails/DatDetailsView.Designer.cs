namespace datinate.app {
    partial class DatDetailsView {
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
            tableLayoutPanel1 = new TableLayoutPanel();
            panel3 = new Panel();
            filterUI = new FilterUI();
            datDetailsLabel = new Label();
            gameList = new CustomDetailsListView();
            panel1 = new Panel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            compareRightBtn = new DatActionButton();
            datActionButton1 = new DatActionButton();
            compareLeftBtn = new DatActionButton();
            customiseBtn = new DatActionButton();
            sendToDatGrouperBtn = new DatActionButton();
            openDatBtn = new Button();
            splitContainer = new SplitContainer();
            tabControl = new BorderlessTabControl();
            romSummaryPage = new TabPage();
            tableLayoutPanel3 = new TableLayoutPanel();
            romSummaryList = new CustomDetailsListView();
            panel2 = new Panel();
            romSummaryLabel = new Label();
            gameBreakdownPage = new TabPage();
            tableLayoutPanel2 = new TableLayoutPanel();
            gameDetailsLabel = new Label();
            datTreeView = new TreeView();
            tableLayoutPanel1.SuspendLayout();
            panel3.SuspendLayout();
            panel1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            tabControl.SuspendLayout();
            romSummaryPage.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            panel2.SuspendLayout();
            gameBreakdownPage.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(panel3, 0, 0);
            tableLayoutPanel1.Controls.Add(gameList, 0, 1);
            tableLayoutPanel1.Controls.Add(panel1, 0, 2);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(4, 3, 4, 3);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel1.Size = new Size(919, 266);
            tableLayoutPanel1.TabIndex = 14;
            // 
            // panel3
            // 
            panel3.Controls.Add(filterUI);
            panel3.Controls.Add(datDetailsLabel);
            panel3.Dock = DockStyle.Fill;
            panel3.Location = new Point(5, 4);
            panel3.Margin = new Padding(4, 3, 4, 3);
            panel3.Name = "panel3";
            panel3.Size = new Size(909, 26);
            panel3.TabIndex = 15;
            // 
            // filterUI
            // 
            filterUI.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            filterUI.Location = new Point(651, 1);
            filterUI.Margin = new Padding(5, 3, 5, 3);
            filterUI.Name = "filterUI";
            filterUI.Size = new Size(261, 24);
            filterUI.TabIndex = 0;
            // 
            // datDetailsLabel
            // 
            datDetailsLabel.Anchor = AnchorStyles.Left;
            datDetailsLabel.AutoSize = true;
            datDetailsLabel.Location = new Point(4, 4);
            datDetailsLabel.Margin = new Padding(4, 0, 4, 0);
            datDetailsLabel.Name = "datDetailsLabel";
            datDetailsLabel.Size = new Size(113, 15);
            datDetailsLabel.TabIndex = 1;
            datDetailsLabel.Text = "Selected DAT Details";
            // 
            // gameList
            // 
            gameList.AllowColumnReorder = true;
            gameList.Dock = DockStyle.Fill;
            gameList.FullRowSelect = true;
            gameList.GridLines = true;
            gameList.Location = new Point(5, 37);
            gameList.Margin = new Padding(4, 3, 4, 3);
            gameList.Name = "gameList";
            gameList.Size = new Size(909, 184);
            gameList.TabIndex = 13;
            gameList.UseCompatibleStateImageBehavior = false;
            gameList.View = View.Details;
            gameList.ColumnClick += OnColumnClick;
            gameList.ItemSelectionChanged += OnGameSelectionChange;
            // 
            // panel1
            // 
            panel1.Controls.Add(flowLayoutPanel1);
            panel1.Controls.Add(openDatBtn);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(5, 228);
            panel1.Margin = new Padding(4, 3, 4, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(909, 34);
            panel1.TabIndex = 14;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            flowLayoutPanel1.Controls.Add(compareRightBtn);
            flowLayoutPanel1.Controls.Add(datActionButton1);
            flowLayoutPanel1.Controls.Add(compareLeftBtn);
            flowLayoutPanel1.Controls.Add(customiseBtn);
            flowLayoutPanel1.Controls.Add(sendToDatGrouperBtn);
            flowLayoutPanel1.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanel1.Location = new Point(438, 0);
            flowLayoutPanel1.Margin = new Padding(0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(471, 34);
            flowLayoutPanel1.TabIndex = 12;
            flowLayoutPanel1.WrapContents = false;
            // 
            // compareRightBtn
            // 
            compareRightBtn.BackColor = Color.FromArgb(192, 0, 192);
            compareRightBtn.CornerRadius = 0;
            compareRightBtn.CornerRadiusBottomLeft = 0;
            compareRightBtn.CornerRadiusBottomRight = 0;
            compareRightBtn.CornerRadiusTopLeft = 0;
            compareRightBtn.CornerRadiusTopRight = 0;
            compareRightBtn.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            compareRightBtn.ForeColor = Color.White;
            compareRightBtn.Location = new Point(439, 0);
            compareRightBtn.Margin = new Padding(0);
            compareRightBtn.MinimumSize = new Size(30, 28);
            compareRightBtn.Name = "compareRightBtn";
            compareRightBtn.Size = new Size(32, 38);
            compareRightBtn.TabIndex = 11;
            compareRightBtn.Text = "🡲";
            compareRightBtn.Click += OnCompareClick;
            // 
            // datActionButton1
            // 
            datActionButton1.BackColor = Color.FromArgb(192, 0, 192);
            datActionButton1.CornerRadius = 0;
            datActionButton1.CornerRadiusBottomLeft = 0;
            datActionButton1.CornerRadiusBottomRight = 0;
            datActionButton1.CornerRadiusTopLeft = 0;
            datActionButton1.CornerRadiusTopRight = 0;
            datActionButton1.Enabled = false;
            datActionButton1.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            datActionButton1.ForeColor = Color.White;
            datActionButton1.Location = new Point(373, 0);
            datActionButton1.Margin = new Padding(0);
            datActionButton1.MinimumSize = new Size(48, 30);
            datActionButton1.Name = "datActionButton1";
            datActionButton1.Size = new Size(66, 38);
            datActionButton1.TabIndex = 9;
            datActionButton1.Text = "Compare";
            // 
            // compareLeftBtn
            // 
            compareLeftBtn.BackColor = Color.FromArgb(192, 0, 192);
            compareLeftBtn.CornerRadius = 0;
            compareLeftBtn.CornerRadiusBottomLeft = 0;
            compareLeftBtn.CornerRadiusBottomRight = 0;
            compareLeftBtn.CornerRadiusTopLeft = 0;
            compareLeftBtn.CornerRadiusTopRight = 0;
            compareLeftBtn.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            compareLeftBtn.ForeColor = Color.White;
            compareLeftBtn.Location = new Point(341, 0);
            compareLeftBtn.Margin = new Padding(0);
            compareLeftBtn.MinimumSize = new Size(30, 28);
            compareLeftBtn.Name = "compareLeftBtn";
            compareLeftBtn.Size = new Size(32, 38);
            compareLeftBtn.TabIndex = 10;
            compareLeftBtn.Text = "🡰";
            compareLeftBtn.Click += OnCompareClick;
            // 
            // customiseBtn
            // 
            customiseBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            customiseBtn.BackColor = Color.FromArgb(0, 192, 0);
            customiseBtn.ButtonImage = Datinate.Properties.Resources.datAction_customise;
            customiseBtn.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            customiseBtn.ForeColor = Color.White;
            customiseBtn.Location = new Point(201, 0);
            customiseBtn.Margin = new Padding(0, 0, 10, 0);
            customiseBtn.MinimumSize = new Size(48, 30);
            customiseBtn.Name = "customiseBtn";
            customiseBtn.Size = new Size(130, 38);
            customiseBtn.TabIndex = 8;
            customiseBtn.Text = "Customise";
            customiseBtn.Click += OnCustomiseClick;
            // 
            // sendToDatGrouperBtn
            // 
            sendToDatGrouperBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            sendToDatGrouperBtn.BackColor = Color.FromArgb(35, 152, 220);
            sendToDatGrouperBtn.ButtonImage = Datinate.Properties.Resources.datAction_grouper;
            sendToDatGrouperBtn.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            sendToDatGrouperBtn.ForeColor = Color.White;
            sendToDatGrouperBtn.Location = new Point(11, 0);
            sendToDatGrouperBtn.Margin = new Padding(0, 0, 10, 0);
            sendToDatGrouperBtn.MinimumSize = new Size(48, 28);
            sendToDatGrouperBtn.Name = "sendToDatGrouperBtn";
            sendToDatGrouperBtn.Size = new Size(180, 38);
            sendToDatGrouperBtn.TabIndex = 7;
            sendToDatGrouperBtn.Text = "Send to Grouper";
            sendToDatGrouperBtn.Click += AddToProjectBtn_Click;
            // 
            // openDatBtn
            // 
            openDatBtn.BackColor = Color.White;
            openDatBtn.Cursor = Cursors.Hand;
            openDatBtn.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            openDatBtn.ImageAlign = ContentAlignment.MiddleRight;
            openDatBtn.Location = new Point(0, 0);
            openDatBtn.Margin = new Padding(0);
            openDatBtn.Name = "openDatBtn";
            openDatBtn.Size = new Size(30, 34);
            openDatBtn.TabIndex = 12;
            openDatBtn.Text = "↗";
            openDatBtn.UseVisualStyleBackColor = false;
            openDatBtn.Click += OpenDatBtn_Click;
            // 
            // splitContainer
            // 
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Location = new Point(0, 0);
            splitContainer.Margin = new Padding(4, 3, 4, 3);
            splitContainer.Name = "splitContainer";
            splitContainer.Orientation = Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(tableLayoutPanel1);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(tabControl);
            splitContainer.Size = new Size(919, 533);
            splitContainer.SplitterDistance = 266;
            splitContainer.SplitterWidth = 5;
            splitContainer.TabIndex = 15;
            // 
            // tabControl
            // 
            tabControl.Controls.Add(romSummaryPage);
            tabControl.Controls.Add(gameBreakdownPage);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 0);
            tabControl.Margin = new Padding(4, 3, 4, 3);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(919, 262);
            tabControl.TabIndex = 18;
            // 
            // romSummaryPage
            // 
            romSummaryPage.Controls.Add(tableLayoutPanel3);
            romSummaryPage.Location = new Point(0, 20);
            romSummaryPage.Margin = new Padding(4, 3, 4, 3);
            romSummaryPage.Name = "romSummaryPage";
            romSummaryPage.Padding = new Padding(4, 3, 4, 3);
            romSummaryPage.Size = new Size(919, 242);
            romSummaryPage.TabIndex = 0;
            romSummaryPage.Text = "ROM Summary";
            romSummaryPage.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 1;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Controls.Add(romSummaryList, 0, 0);
            tableLayoutPanel3.Controls.Add(panel2, 0, 1);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(4, 3);
            tableLayoutPanel3.Margin = new Padding(4, 3, 4, 3);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 2;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
            tableLayoutPanel3.Size = new Size(911, 236);
            tableLayoutPanel3.TabIndex = 1;
            // 
            // romSummaryList
            // 
            romSummaryList.Dock = DockStyle.Fill;
            romSummaryList.GridLines = true;
            romSummaryList.Location = new Point(4, 3);
            romSummaryList.Margin = new Padding(4, 3, 4, 3);
            romSummaryList.Name = "romSummaryList";
            romSummaryList.Size = new Size(903, 199);
            romSummaryList.TabIndex = 0;
            romSummaryList.UseCompatibleStateImageBehavior = false;
            romSummaryList.View = View.Details;
            romSummaryList.ColumnClick += OnColumnClick;
            // 
            // panel2
            // 
            panel2.Controls.Add(romSummaryLabel);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(4, 208);
            panel2.Margin = new Padding(4, 3, 4, 3);
            panel2.Name = "panel2";
            panel2.Size = new Size(903, 25);
            panel2.TabIndex = 1;
            // 
            // romSummaryLabel
            // 
            romSummaryLabel.AutoSize = true;
            romSummaryLabel.Location = new Point(0, 5);
            romSummaryLabel.Margin = new Padding(4, 0, 4, 0);
            romSummaryLabel.Name = "romSummaryLabel";
            romSummaryLabel.Size = new Size(0, 15);
            romSummaryLabel.TabIndex = 0;
            // 
            // gameBreakdownPage
            // 
            gameBreakdownPage.Controls.Add(tableLayoutPanel2);
            gameBreakdownPage.Location = new Point(0, 20);
            gameBreakdownPage.Margin = new Padding(4, 3, 4, 3);
            gameBreakdownPage.Name = "gameBreakdownPage";
            gameBreakdownPage.Padding = new Padding(4, 3, 4, 3);
            gameBreakdownPage.Size = new Size(919, 242);
            gameBreakdownPage.TabIndex = 1;
            gameBreakdownPage.Text = "Entry Breakdown";
            gameBreakdownPage.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(gameDetailsLabel, 0, 0);
            tableLayoutPanel2.Controls.Add(datTreeView, 0, 1);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(4, 3);
            tableLayoutPanel2.Margin = new Padding(4, 3, 4, 3);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 3;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 23F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
            tableLayoutPanel2.Size = new Size(911, 236);
            tableLayoutPanel2.TabIndex = 17;
            // 
            // gameDetailsLabel
            // 
            gameDetailsLabel.Anchor = AnchorStyles.Left;
            gameDetailsLabel.AutoSize = true;
            gameDetailsLabel.Location = new Point(5, 5);
            gameDetailsLabel.Margin = new Padding(4, 0, 4, 0);
            gameDetailsLabel.Name = "gameDetailsLabel";
            gameDetailsLabel.Size = new Size(123, 15);
            gameDetailsLabel.TabIndex = 0;
            gameDetailsLabel.Text = "Selected Game Details";
            // 
            // datTreeView
            // 
            datTreeView.Dock = DockStyle.Fill;
            datTreeView.Location = new Point(5, 28);
            datTreeView.Margin = new Padding(4, 3, 4, 3);
            datTreeView.Name = "datTreeView";
            datTreeView.Size = new Size(901, 172);
            datTreeView.TabIndex = 16;
            // 
            // DatDetailsView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(splitContainer);
            Margin = new Padding(4, 3, 4, 3);
            Name = "DatDetailsView";
            Size = new Size(919, 533);
            tableLayoutPanel1.ResumeLayout(false);
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            panel1.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            tabControl.ResumeLayout(false);
            romSummaryPage.ResumeLayout(false);
            tableLayoutPanel3.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            gameBreakdownPage.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            ResumeLayout(false);
        }



        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private CustomDetailsListView gameList;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label gameDetailsLabel;
        private System.Windows.Forms.TreeView datTreeView;
        private System.Windows.Forms.TabPage romSummaryPage;
        private System.Windows.Forms.TabPage gameBreakdownPage;
        private CustomDetailsListView romSummaryList;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label romSummaryLabel;
        private System.Windows.Forms.Panel panel3;
        private FilterUI filterUI;
        private System.Windows.Forms.Label datDetailsLabel;
        private BorderlessTabControl tabControl;
        private DatActionButton sendToDatGrouperBtn;
        private DatActionButton customiseBtn;
        private DatActionButton datActionButton1;
        private DatActionButton compareLeftBtn;
        private DatActionButton compareRightBtn;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button openDatBtn;
    }
}
