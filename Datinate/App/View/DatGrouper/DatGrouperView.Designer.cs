namespace datinate.app
{
    partial class DatGrouperView
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
            splitContainerLeftRight = new SplitContainer();
            leftPanel = new Panel();
            autoGrouperUI = new AutomatedWrapperUI();
            splitContainerMiddleRight = new SplitContainer();
            middlePanel = new Panel();
            webAndMainControlsContainer = new TableLayoutPanel();
            webViewsContainer = new Panel();
            webMediaPanel = new TableLayoutPanel();
            mediaAssignmentView = new MediaAssignmentView();
            webMainPanel = new TableLayoutPanel();
            mainWebView = new MainWebView();
            filler = new Panel();
            webSearchView = new WebSearchView();
            fillerX = new Panel();
            controlsView = new DatGrouperControlsView();
            rightPanel = new Panel();
            curatedGrouperUI = new CuratedWrapperUI();
            mediaView = new Media2View();
            tableLayoutPanel = new TableLayoutPanel();
            controlsPanel = new Panel();
            btnLayoutPanel = new FlowLayoutPanel();
            backBtn = new Button();
            saveBtn = new Button();
            curateBtn = new Button();
            cfgBtn = new Button();
            exportBtn = new Button();
            datChipContainer = new FlowLayoutPanel();
            summaryLabel = new Label();
            verticalLineui1 = new MRB.View.UI.VerticalLineUI();
            progressBar = new SegmentedProgressBarUI();
            panel2 = new Panel();
            panel4 = new Panel();
            panel5 = new Panel();
            panel6 = new Panel();
            panel7 = new Panel();
            ((System.ComponentModel.ISupportInitialize)splitContainerLeftRight).BeginInit();
            splitContainerLeftRight.Panel1.SuspendLayout();
            splitContainerLeftRight.Panel2.SuspendLayout();
            splitContainerLeftRight.SuspendLayout();
            leftPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerMiddleRight).BeginInit();
            splitContainerMiddleRight.Panel1.SuspendLayout();
            splitContainerMiddleRight.Panel2.SuspendLayout();
            splitContainerMiddleRight.SuspendLayout();
            middlePanel.SuspendLayout();
            webAndMainControlsContainer.SuspendLayout();
            webViewsContainer.SuspendLayout();
            webMediaPanel.SuspendLayout();
            webMainPanel.SuspendLayout();
            rightPanel.SuspendLayout();
            tableLayoutPanel.SuspendLayout();
            controlsPanel.SuspendLayout();
            btnLayoutPanel.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainerLeftRight
            // 
            splitContainerLeftRight.Dock = DockStyle.Fill;
            splitContainerLeftRight.Location = new Point(1, 1);
            splitContainerLeftRight.Margin = new Padding(0);
            splitContainerLeftRight.Name = "splitContainerLeftRight";
            // 
            // splitContainerLeftRight.Panel1
            // 
            splitContainerLeftRight.Panel1.Controls.Add(leftPanel);
            // 
            // splitContainerLeftRight.Panel2
            // 
            splitContainerLeftRight.Panel2.Controls.Add(splitContainerMiddleRight);
            splitContainerLeftRight.Size = new Size(1011, 558);
            splitContainerLeftRight.SplitterDistance = 335;
            splitContainerLeftRight.TabIndex = 0;
            // 
            // leftPanel
            // 
            leftPanel.Controls.Add(autoGrouperUI);
            leftPanel.Dock = DockStyle.Fill;
            leftPanel.Location = new Point(0, 0);
            leftPanel.Margin = new Padding(0);
            leftPanel.Name = "leftPanel";
            leftPanel.Size = new Size(335, 558);
            leftPanel.TabIndex = 1;
            // 
            // autoGrouperUI
            // 
            autoGrouperUI.Dock = DockStyle.Fill;
            autoGrouperUI.Location = new Point(0, 0);
            autoGrouperUI.Margin = new Padding(0);
            autoGrouperUI.Name = "autoGrouperUI";
            autoGrouperUI.Size = new Size(335, 558);
            autoGrouperUI.TabIndex = 0;
            // 
            // splitContainerMiddleRight
            // 
            splitContainerMiddleRight.Dock = DockStyle.Fill;
            splitContainerMiddleRight.Location = new Point(0, 0);
            splitContainerMiddleRight.Margin = new Padding(0);
            splitContainerMiddleRight.Name = "splitContainerMiddleRight";
            // 
            // splitContainerMiddleRight.Panel1
            // 
            splitContainerMiddleRight.Panel1.Controls.Add(middlePanel);
            // 
            // splitContainerMiddleRight.Panel2
            // 
            splitContainerMiddleRight.Panel2.Controls.Add(rightPanel);
            splitContainerMiddleRight.Size = new Size(672, 558);
            splitContainerMiddleRight.SplitterDistance = 339;
            splitContainerMiddleRight.TabIndex = 0;
            // 
            // middlePanel
            // 
            middlePanel.Controls.Add(webAndMainControlsContainer);
            middlePanel.Dock = DockStyle.Fill;
            middlePanel.Location = new Point(0, 0);
            middlePanel.Margin = new Padding(0);
            middlePanel.Name = "middlePanel";
            middlePanel.Size = new Size(339, 558);
            middlePanel.TabIndex = 33;
            // 
            // webAndMainControlsContainer
            // 
            webAndMainControlsContainer.ColumnCount = 1;
            webAndMainControlsContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            webAndMainControlsContainer.Controls.Add(webViewsContainer, 0, 0);
            webAndMainControlsContainer.Controls.Add(fillerX, 0, 1);
            webAndMainControlsContainer.Controls.Add(controlsView, 0, 2);
            webAndMainControlsContainer.Dock = DockStyle.Fill;
            webAndMainControlsContainer.Location = new Point(0, 0);
            webAndMainControlsContainer.Margin = new Padding(0);
            webAndMainControlsContainer.Name = "webAndMainControlsContainer";
            webAndMainControlsContainer.RowCount = 3;
            webAndMainControlsContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            webAndMainControlsContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
            webAndMainControlsContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 96F));
            webAndMainControlsContainer.Size = new Size(339, 558);
            webAndMainControlsContainer.TabIndex = 33;
            // 
            // webViewsContainer
            // 
            webViewsContainer.Controls.Add(webMediaPanel);
            webViewsContainer.Controls.Add(webMainPanel);
            webViewsContainer.Dock = DockStyle.Fill;
            webViewsContainer.Location = new Point(0, 0);
            webViewsContainer.Margin = new Padding(0);
            webViewsContainer.Name = "webViewsContainer";
            webViewsContainer.Size = new Size(339, 461);
            webViewsContainer.TabIndex = 33;
            // 
            // webMediaPanel
            // 
            webMediaPanel.BackColor = Color.Black;
            webMediaPanel.ColumnCount = 1;
            webMediaPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            webMediaPanel.Controls.Add(mediaAssignmentView, 0, 0);
            webMediaPanel.Dock = DockStyle.Fill;
            webMediaPanel.Location = new Point(0, 0);
            webMediaPanel.Margin = new Padding(0);
            webMediaPanel.Name = "webMediaPanel";
            webMediaPanel.Padding = new Padding(5);
            webMediaPanel.RowCount = 1;
            webMediaPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            webMediaPanel.Size = new Size(339, 461);
            webMediaPanel.TabIndex = 0;
            // 
            // mediaAssignmentView
            // 
            mediaAssignmentView.BackColor = SystemColors.Control;
            mediaAssignmentView.Dock = DockStyle.Fill;
            mediaAssignmentView.Location = new Point(5, 5);
            mediaAssignmentView.Margin = new Padding(0);
            mediaAssignmentView.Name = "mediaAssignmentView";
            mediaAssignmentView.Size = new Size(329, 451);
            mediaAssignmentView.TabIndex = 2;
            // 
            // webMainPanel
            // 
            webMainPanel.ColumnCount = 1;
            webMainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            webMainPanel.Controls.Add(mainWebView, 0, 0);
            webMainPanel.Controls.Add(filler, 0, 1);
            webMainPanel.Controls.Add(webSearchView, 0, 2);
            webMainPanel.Dock = DockStyle.Fill;
            webMainPanel.Location = new Point(0, 0);
            webMainPanel.Margin = new Padding(0);
            webMainPanel.Name = "webMainPanel";
            webMainPanel.RowCount = 3;
            webMainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            webMainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
            webMainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            webMainPanel.Size = new Size(339, 461);
            webMainPanel.TabIndex = 1;
            // 
            // mainWebView
            // 
            mainWebView.Dock = DockStyle.Fill;
            mainWebView.Location = new Point(0, 0);
            mainWebView.Margin = new Padding(0);
            mainWebView.Name = "mainWebView";
            mainWebView.Size = new Size(339, 425);
            mainWebView.TabIndex = 0;
            // 
            // filler
            // 
            filler.BackColor = SystemColors.ControlDarkDark;
            filler.Dock = DockStyle.Fill;
            filler.Location = new Point(0, 425);
            filler.Margin = new Padding(0);
            filler.Name = "filler";
            filler.Size = new Size(339, 1);
            filler.TabIndex = 34;
            // 
            // webSearchView
            // 
            webSearchView.Dock = DockStyle.Fill;
            webSearchView.Location = new Point(3, 429);
            webSearchView.Name = "webSearchView";
            webSearchView.Size = new Size(333, 29);
            webSearchView.TabIndex = 2;
            // 
            // fillerX
            // 
            fillerX.BackColor = SystemColors.ControlDarkDark;
            fillerX.Dock = DockStyle.Fill;
            fillerX.Location = new Point(0, 461);
            fillerX.Margin = new Padding(0);
            fillerX.Name = "fillerX";
            fillerX.Size = new Size(339, 1);
            fillerX.TabIndex = 35;
            // 
            // controlsView
            // 
            controlsView.Dock = DockStyle.Fill;
            controlsView.Location = new Point(0, 462);
            controlsView.Margin = new Padding(0);
            controlsView.Name = "controlsView";
            controlsView.Size = new Size(339, 96);
            controlsView.TabIndex = 1;
            // 
            // rightPanel
            // 
            rightPanel.Controls.Add(curatedGrouperUI);
            rightPanel.Controls.Add(mediaView);
            rightPanel.Dock = DockStyle.Fill;
            rightPanel.Location = new Point(0, 0);
            rightPanel.Margin = new Padding(0);
            rightPanel.Name = "rightPanel";
            rightPanel.Size = new Size(329, 558);
            rightPanel.TabIndex = 1;
            // 
            // curatedGrouperUI
            // 
            curatedGrouperUI.Dock = DockStyle.Fill;
            curatedGrouperUI.Location = new Point(0, 0);
            curatedGrouperUI.Margin = new Padding(0);
            curatedGrouperUI.Name = "curatedGrouperUI";
            curatedGrouperUI.Size = new Size(329, 558);
            curatedGrouperUI.TabIndex = 0;
            // 
            // mediaView
            // 
            mediaView.Dock = DockStyle.Fill;
            mediaView.Location = new Point(0, 0);
            mediaView.Margin = new Padding(0, 0, 0, 3);
            mediaView.Name = "mediaView";
            mediaView.Size = new Size(329, 558);
            mediaView.TabIndex = 33;
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.ColumnCount = 3;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 1F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 1F));
            tableLayoutPanel.Controls.Add(controlsPanel, 1, 3);
            tableLayoutPanel.Controls.Add(splitContainerLeftRight, 1, 1);
            tableLayoutPanel.Controls.Add(panel2, 1, 2);
            tableLayoutPanel.Controls.Add(panel4, 1, 0);
            tableLayoutPanel.Controls.Add(panel5, 2, 1);
            tableLayoutPanel.Controls.Add(panel6, 0, 1);
            tableLayoutPanel.Controls.Add(panel7, 1, 4);
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.Location = new Point(0, 0);
            tableLayoutPanel.Margin = new Padding(0);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 5;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
            tableLayoutPanel.Size = new Size(1013, 591);
            tableLayoutPanel.TabIndex = 1;
            // 
            // controlsPanel
            // 
            controlsPanel.Controls.Add(btnLayoutPanel);
            controlsPanel.Controls.Add(datChipContainer);
            controlsPanel.Controls.Add(summaryLabel);
            controlsPanel.Controls.Add(verticalLineui1);
            controlsPanel.Controls.Add(progressBar);
            controlsPanel.Dock = DockStyle.Fill;
            controlsPanel.Location = new Point(1, 560);
            controlsPanel.Margin = new Padding(0);
            controlsPanel.Name = "controlsPanel";
            controlsPanel.Size = new Size(1011, 30);
            controlsPanel.TabIndex = 0;
            // 
            // btnLayoutPanel
            // 
            btnLayoutPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnLayoutPanel.Controls.Add(backBtn);
            btnLayoutPanel.Controls.Add(saveBtn);
            btnLayoutPanel.Controls.Add(curateBtn);
            btnLayoutPanel.Controls.Add(cfgBtn);
            btnLayoutPanel.Controls.Add(exportBtn);
            btnLayoutPanel.FlowDirection = FlowDirection.RightToLeft;
            btnLayoutPanel.Location = new Point(607, 0);
            btnLayoutPanel.Margin = new Padding(0);
            btnLayoutPanel.Name = "btnLayoutPanel";
            btnLayoutPanel.Size = new Size(404, 31);
            btnLayoutPanel.TabIndex = 35;
            // 
            // backBtn
            // 
            backBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            backBtn.Cursor = Cursors.Hand;
            backBtn.Location = new Point(304, 3);
            backBtn.Margin = new Padding(0, 3, 0, 0);
            backBtn.Name = "backBtn";
            backBtn.Size = new Size(100, 26);
            backBtn.TabIndex = 28;
            backBtn.Text = "← Exit";
            backBtn.UseVisualStyleBackColor = true;
            // 
            // saveBtn
            // 
            saveBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            saveBtn.Cursor = Cursors.Hand;
            saveBtn.Location = new Point(204, 3);
            saveBtn.Margin = new Padding(0, 3, 0, 0);
            saveBtn.Name = "saveBtn";
            saveBtn.Size = new Size(100, 26);
            saveBtn.TabIndex = 27;
            saveBtn.Text = "Save";
            saveBtn.UseVisualStyleBackColor = true;
            // 
            // curateBtn
            // 
            curateBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            curateBtn.Cursor = Cursors.Hand;
            curateBtn.Location = new Point(104, 3);
            curateBtn.Margin = new Padding(0, 3, 0, 0);
            curateBtn.Name = "curateBtn";
            curateBtn.Size = new Size(100, 26);
            curateBtn.TabIndex = 31;
            curateBtn.Text = "Curate";
            curateBtn.UseVisualStyleBackColor = true;
            // 
            // cfgBtn
            // 
            cfgBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cfgBtn.Cursor = Cursors.Hand;
            cfgBtn.Location = new Point(4, 3);
            cfgBtn.Margin = new Padding(0, 3, 0, 0);
            cfgBtn.Name = "cfgBtn";
            cfgBtn.Size = new Size(100, 26);
            cfgBtn.TabIndex = 33;
            cfgBtn.Text = "Project Settings";
            cfgBtn.UseVisualStyleBackColor = true;
            // 
            // exportBtn
            // 
            exportBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            exportBtn.Cursor = Cursors.Hand;
            exportBtn.Location = new Point(304, 32);
            exportBtn.Margin = new Padding(0, 3, 0, 0);
            exportBtn.Name = "exportBtn";
            exportBtn.Size = new Size(100, 26);
            exportBtn.TabIndex = 35;
            exportBtn.Text = "Export";
            exportBtn.UseVisualStyleBackColor = true;
            // 
            // datChipContainer
            // 
            datChipContainer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            datChipContainer.BackColor = Color.Transparent;
            datChipContainer.Location = new Point(134, 4);
            datChipContainer.Name = "datChipContainer";
            datChipContainer.Size = new Size(470, 23);
            datChipContainer.TabIndex = 26;
            datChipContainer.Click += datChipContainer_Click;
            // 
            // summaryLabel
            // 
            summaryLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            summaryLabel.BackColor = Color.Transparent;
            summaryLabel.Location = new Point(139, 8);
            summaryLabel.Margin = new Padding(4, 0, 4, 0);
            summaryLabel.Name = "summaryLabel";
            summaryLabel.Size = new Size(465, 15);
            summaryLabel.TabIndex = 0;
            summaryLabel.Text = "-";
            summaryLabel.Click += summaryLabel_Click;
            // 
            // verticalLineui1
            // 
            verticalLineui1.Location = new Point(-3, -1);
            verticalLineui1.Margin = new Padding(4, 3, 4, 3);
            verticalLineui1.Name = "verticalLineui1";
            verticalLineui1.Size = new Size(1010, 1);
            verticalLineui1.TabIndex = 32;
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            progressBar.BackColor = Color.Transparent;
            progressBar.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            progressBar.Location = new Point(607, 0);
            progressBar.Margin = new Padding(0);
            progressBar.MinBarHeightPx = 6;
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(405, 31);
            progressBar.TabIndex = 36;
            progressBar.Text = "Loading Media DATs";
            progressBar.Visible = false;
            // 
            // panel2
            // 
            panel2.BackColor = SystemColors.ControlDarkDark;
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(1, 559);
            panel2.Margin = new Padding(0);
            panel2.Name = "panel2";
            panel2.Size = new Size(1011, 1);
            panel2.TabIndex = 1;
            // 
            // panel4
            // 
            panel4.BackColor = SystemColors.ControlDarkDark;
            panel4.Dock = DockStyle.Fill;
            panel4.Location = new Point(1, 0);
            panel4.Margin = new Padding(0);
            panel4.Name = "panel4";
            panel4.Size = new Size(1011, 1);
            panel4.TabIndex = 2;
            // 
            // panel5
            // 
            panel5.BackColor = SystemColors.ControlDarkDark;
            panel5.Dock = DockStyle.Fill;
            panel5.Location = new Point(1012, 1);
            panel5.Margin = new Padding(0);
            panel5.Name = "panel5";
            tableLayoutPanel.SetRowSpan(panel5, 3);
            panel5.Size = new Size(1, 589);
            panel5.TabIndex = 3;
            // 
            // panel6
            // 
            panel6.BackColor = SystemColors.ControlDarkDark;
            panel6.Dock = DockStyle.Fill;
            panel6.Location = new Point(0, 1);
            panel6.Margin = new Padding(0);
            panel6.Name = "panel6";
            tableLayoutPanel.SetRowSpan(panel6, 3);
            panel6.Size = new Size(1, 589);
            panel6.TabIndex = 4;
            // 
            // panel7
            // 
            panel7.BackColor = SystemColors.ControlDarkDark;
            panel7.Dock = DockStyle.Fill;
            panel7.Location = new Point(1, 590);
            panel7.Margin = new Padding(0);
            panel7.Name = "panel7";
            panel7.Size = new Size(1011, 1);
            panel7.TabIndex = 5;
            // 
            // DatGrouperView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel);
            Name = "DatGrouperView";
            Size = new Size(1013, 591);
            splitContainerLeftRight.Panel1.ResumeLayout(false);
            splitContainerLeftRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerLeftRight).EndInit();
            splitContainerLeftRight.ResumeLayout(false);
            leftPanel.ResumeLayout(false);
            splitContainerMiddleRight.Panel1.ResumeLayout(false);
            splitContainerMiddleRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerMiddleRight).EndInit();
            splitContainerMiddleRight.ResumeLayout(false);
            middlePanel.ResumeLayout(false);
            webAndMainControlsContainer.ResumeLayout(false);
            webViewsContainer.ResumeLayout(false);
            webMediaPanel.ResumeLayout(false);
            webMainPanel.ResumeLayout(false);
            rightPanel.ResumeLayout(false);
            tableLayoutPanel.ResumeLayout(false);
            controlsPanel.ResumeLayout(false);
            btnLayoutPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private SplitContainer splitContainerLeftRight;
        private SplitContainer splitContainerMiddleRight;
        private Label romsLabel;
        private Label label1;
        private Label label9;
        private Label partsLabel;
        private Label entrisLabel;
        private Label label3;
        private Label label2;
        private TableLayoutPanel tableLayoutPanel;
        private MainWebView mainWebView;
        private AutomatedWrapperUI autoGrouperUI;
        private TableLayoutPanel webMainPanel;
        private DatGrouperControlsView controlsView;
        private Panel controlsPanel;
        private MRB.View.UI.VerticalLineUI verticalLineui1;
        private Button curateBtn;
        private Button backBtn;
        private Label summaryLabel;
        private Button saveBtn;
        private FlowLayoutPanel datChipContainer;
        private Panel rightPanel;
        private Panel leftPanel;
        private Panel middlePanel;
        private WebSearchView webSearchView;
        private Panel panel2;
        private Panel filler;
        private Panel panel4;
        private Panel panel5;
        private Panel panel6;
        private Panel panel7;
        private Media2View mediaView;
        private MediaAssignmentView mediaAssignmentView;
        private TableLayoutPanel webMediaPanel;
        private TableLayoutPanel webAndMainControlsContainer;
        private Panel webViewsContainer;
        private Panel fillerX;
        private Button cfgBtn;
        private FlowLayoutPanel btnLayoutPanel;
        private SegmentedProgressBarUI progressBar;
        private CuratedWrapperUI curatedGrouperUI;
        private Button exportBtn;
    }
}
