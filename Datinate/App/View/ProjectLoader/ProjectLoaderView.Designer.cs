namespace datinate.app 
{
    partial class ProjectLoaderView 
    {
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            runDatGrouperBtn = new Button();
            gamesIncludeContainer = new FlowLayoutPanel();
            saveProjectBtn = new Button();
            selectProjectGroup = new GroupBox();
            newProjectBtn = new Button();
            projectListHostPanel = new Panel();
            projectListBox = new ProjectsListBoxUI();
            createProjectPanel = new Panel();
            saveNewProjectBtn = new Button();
            cancelNewProjectBtn = new Button();
            label1 = new Label();
            projectNameText = new TextBox();
            includeGroup = new GroupBox();
            includeMediaGroup = new GroupBox();
            mediaIncludeContainer = new FlowLayoutPanel();
            commentGroup = new GroupBox();
            commentText = new TextBox();
            applyCommentEditBtn = new Button();
            cancelCommentEditBtn = new Button();
            editCommentBtn = new Button();
            commentLabel = new Label();
            leftContainer = new Panel();
            currentProjectPanel = new Panel();
            currentProjectNameLabel = new Label();
            imageList1 = new ImageList(components);
            rightContainer = new Panel();
            tabControl = new InvisibleTabControl();
            tabPage1 = new TabPage();
            tableLayoutPanel1 = new TableLayoutPanel();
            tabPage2 = new TabPage();
            advancedSettingsView = new DatGrouperProjectSettingsView();
            advancedSettingsBtn = new Button();
            btnFlowPanel = new FlowLayoutPanel();
            cancelAdvancedSettingsBtn = new Button();
            selectProjectGroup.SuspendLayout();
            projectListHostPanel.SuspendLayout();
            createProjectPanel.SuspendLayout();
            includeGroup.SuspendLayout();
            includeMediaGroup.SuspendLayout();
            commentGroup.SuspendLayout();
            leftContainer.SuspendLayout();
            currentProjectPanel.SuspendLayout();
            rightContainer.SuspendLayout();
            tabControl.SuspendLayout();
            tabPage1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            tabPage2.SuspendLayout();
            btnFlowPanel.SuspendLayout();
            SuspendLayout();
            // 
            // runDatGrouperBtn
            // 
            runDatGrouperBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            runDatGrouperBtn.Cursor = Cursors.Hand;
            runDatGrouperBtn.Location = new Point(322, 0);
            runDatGrouperBtn.Margin = new Padding(4, 0, 4, 0);
            runDatGrouperBtn.Name = "runDatGrouperBtn";
            runDatGrouperBtn.Size = new Size(160, 27);
            runDatGrouperBtn.TabIndex = 0;
            runDatGrouperBtn.Text = "Run DAT Grouper";
            runDatGrouperBtn.UseVisualStyleBackColor = true;
            runDatGrouperBtn.Click += buildProjectBtn_Click;
            // 
            // gamesIncludeContainer
            // 
            gamesIncludeContainer.AutoScroll = true;
            gamesIncludeContainer.BackColor = SystemColors.Control;
            gamesIncludeContainer.Dock = DockStyle.Fill;
            gamesIncludeContainer.Location = new Point(0, 16);
            gamesIncludeContainer.Margin = new Padding(4, 3, 4, 3);
            gamesIncludeContainer.Name = "gamesIncludeContainer";
            gamesIncludeContainer.Size = new Size(742, 376);
            gamesIncludeContainer.TabIndex = 1;
            // 
            // saveProjectBtn
            // 
            saveProjectBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            saveProjectBtn.Cursor = Cursors.Hand;
            saveProjectBtn.Location = new Point(598, 0);
            saveProjectBtn.Margin = new Padding(4, 0, 4, 0);
            saveProjectBtn.Name = "saveProjectBtn";
            saveProjectBtn.Size = new Size(88, 27);
            saveProjectBtn.TabIndex = 2;
            saveProjectBtn.Text = "Save Project";
            saveProjectBtn.UseVisualStyleBackColor = true;
            saveProjectBtn.Click += saveBtn_Click;
            // 
            // selectProjectGroup
            // 
            selectProjectGroup.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            selectProjectGroup.Controls.Add(newProjectBtn);
            selectProjectGroup.Controls.Add(projectListHostPanel);
            selectProjectGroup.Location = new Point(0, 51);
            selectProjectGroup.Margin = new Padding(4, 3, 4, 3);
            selectProjectGroup.Name = "selectProjectGroup";
            selectProjectGroup.Padding = new Padding(4, 3, 4, 3);
            selectProjectGroup.Size = new Size(364, 568);
            selectProjectGroup.TabIndex = 3;
            selectProjectGroup.TabStop = false;
            selectProjectGroup.Text = "Projects";
            // 
            // newProjectBtn
            // 
            newProjectBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            newProjectBtn.Cursor = Cursors.Hand;
            newProjectBtn.Location = new Point(7, 533);
            newProjectBtn.Margin = new Padding(4, 3, 4, 3);
            newProjectBtn.Name = "newProjectBtn";
            newProjectBtn.Size = new Size(349, 27);
            newProjectBtn.TabIndex = 8;
            newProjectBtn.Text = "New Project";
            newProjectBtn.UseVisualStyleBackColor = true;
            newProjectBtn.Click += newProjectBtn_Click;
            // 
            // projectListHostPanel
            // 
            projectListHostPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            projectListHostPanel.BackColor = Color.FromArgb(205, 212, 221);
            projectListHostPanel.Controls.Add(projectListBox);
            projectListHostPanel.Controls.Add(createProjectPanel);
            projectListHostPanel.Location = new Point(7, 19);
            projectListHostPanel.Margin = new Padding(0);
            projectListHostPanel.Name = "projectListHostPanel";
            projectListHostPanel.Padding = new Padding(1);
            projectListHostPanel.Size = new Size(349, 512);
            projectListHostPanel.TabIndex = 0;
            // 
            // projectListBox
            // 
            projectListBox.BackColor = Color.White;
            projectListBox.BorderStyle = BorderStyle.None;
            projectListBox.Dock = DockStyle.Fill;
            projectListBox.DrawMode = DrawMode.OwnerDrawFixed;
            projectListBox.ForeColor = Color.FromArgb(25, 25, 25);
            projectListBox.FormattingEnabled = true;
            projectListBox.IntegralHeight = false;
            projectListBox.ItemHeight = 34;
            projectListBox.Location = new Point(1, 1);
            projectListBox.Margin = new Padding(0);
            projectListBox.Name = "projectListBox";
            projectListBox.Size = new Size(347, 510);
            projectListBox.TabIndex = 0;
            projectListBox.SelectedIndexChanged += projectListBox_SelectedIndexChanged;
            projectListBox.MouseDown += projectListBox_MouseDown;
            // 
            // createProjectPanel
            // 
            createProjectPanel.BackColor = Color.White;
            createProjectPanel.Controls.Add(saveNewProjectBtn);
            createProjectPanel.Controls.Add(cancelNewProjectBtn);
            createProjectPanel.Controls.Add(label1);
            createProjectPanel.Controls.Add(projectNameText);
            createProjectPanel.Dock = DockStyle.Fill;
            createProjectPanel.Location = new Point(1, 1);
            createProjectPanel.Margin = new Padding(0);
            createProjectPanel.Name = "createProjectPanel";
            createProjectPanel.Size = new Size(347, 510);
            createProjectPanel.TabIndex = 10;
            createProjectPanel.Visible = false;
            // 
            // saveNewProjectBtn
            // 
            saveNewProjectBtn.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            saveNewProjectBtn.Cursor = Cursors.Hand;
            saveNewProjectBtn.Location = new Point(0, 46);
            saveNewProjectBtn.Margin = new Padding(4, 3, 4, 3);
            saveNewProjectBtn.Name = "saveNewProjectBtn";
            saveNewProjectBtn.Size = new Size(347, 27);
            saveNewProjectBtn.TabIndex = 8;
            saveNewProjectBtn.Text = "Save New Project";
            saveNewProjectBtn.UseVisualStyleBackColor = true;
            saveNewProjectBtn.Click += saveBtn_Click;
            // 
            // cancelNewProjectBtn
            // 
            cancelNewProjectBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cancelNewProjectBtn.Cursor = Cursors.Hand;
            cancelNewProjectBtn.Location = new Point(266, 17);
            cancelNewProjectBtn.Margin = new Padding(4, 3, 4, 3);
            cancelNewProjectBtn.Name = "cancelNewProjectBtn";
            cancelNewProjectBtn.Size = new Size(82, 23);
            cancelNewProjectBtn.TabIndex = 9;
            cancelNewProjectBtn.Text = "Cancel";
            cancelNewProjectBtn.UseVisualStyleBackColor = true;
            cancelNewProjectBtn.Click += cancelNewProjectBtn_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(0, 1);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(106, 15);
            label1.TabIndex = 7;
            label1.Text = "New Project Name";
            // 
            // projectNameText
            // 
            projectNameText.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            projectNameText.Enabled = false;
            projectNameText.Location = new Point(0, 17);
            projectNameText.Margin = new Padding(4, 3, 4, 3);
            projectNameText.Name = "projectNameText";
            projectNameText.Size = new Size(259, 23);
            projectNameText.TabIndex = 6;
            // 
            // includeGroup
            // 
            includeGroup.BackColor = SystemColors.ControlLight;
            includeGroup.Controls.Add(gamesIncludeContainer);
            includeGroup.Dock = DockStyle.Fill;
            includeGroup.Location = new Point(4, 3);
            includeGroup.Margin = new Padding(4, 3, 4, 3);
            includeGroup.Name = "includeGroup";
            includeGroup.Padding = new Padding(4, 3, 4, 3);
            includeGroup.Size = new Size(742, 392);
            includeGroup.TabIndex = 5;
            includeGroup.TabStop = false;
            includeGroup.Text = "Software";
            // 
            // includeMediaGroup
            // 
            includeMediaGroup.BackColor = SystemColors.ControlLight;
            includeMediaGroup.Controls.Add(mediaIncludeContainer);
            includeMediaGroup.Dock = DockStyle.Fill;
            includeMediaGroup.Location = new Point(4, 401);
            includeMediaGroup.Margin = new Padding(4, 3, 4, 3);
            includeMediaGroup.Name = "includeMediaGroup";
            includeMediaGroup.Padding = new Padding(4, 3, 4, 3);
            includeMediaGroup.Size = new Size(742, 393);
            includeMediaGroup.TabIndex = 6;
            includeMediaGroup.TabStop = false;
            includeMediaGroup.Text = "Media && Resource";
            // 
            // mediaIncludeContainer
            // 
            mediaIncludeContainer.AutoScroll = true;
            mediaIncludeContainer.BackColor = SystemColors.Control;
            mediaIncludeContainer.Dock = DockStyle.Fill;
            mediaIncludeContainer.Location = new Point(0, 16);
            mediaIncludeContainer.Margin = new Padding(4, 3, 4, 3);
            mediaIncludeContainer.Name = "mediaIncludeContainer";
            mediaIncludeContainer.Size = new Size(742, 377);
            mediaIncludeContainer.TabIndex = 1;
            // 
            // commentGroup
            // 
            commentGroup.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            commentGroup.Controls.Add(commentText);
            commentGroup.Controls.Add(applyCommentEditBtn);
            commentGroup.Controls.Add(cancelCommentEditBtn);
            commentGroup.Controls.Add(editCommentBtn);
            commentGroup.Controls.Add(commentLabel);
            commentGroup.Location = new Point(0, 625);
            commentGroup.Margin = new Padding(4, 3, 4, 3);
            commentGroup.Name = "commentGroup";
            commentGroup.Padding = new Padding(4, 3, 4, 3);
            commentGroup.Size = new Size(365, 197);
            commentGroup.TabIndex = 10;
            commentGroup.TabStop = false;
            commentGroup.Text = "Comment";
            // 
            // commentText
            // 
            commentText.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            commentText.Location = new Point(4, 38);
            commentText.Margin = new Padding(4, 3, 4, 3);
            commentText.Multiline = true;
            commentText.Name = "commentText";
            commentText.ScrollBars = ScrollBars.Vertical;
            commentText.Size = new Size(358, 140);
            commentText.TabIndex = 0;
            // 
            // applyCommentEditBtn
            // 
            applyCommentEditBtn.Cursor = Cursors.Hand;
            applyCommentEditBtn.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            applyCommentEditBtn.Location = new Point(7, 177);
            applyCommentEditBtn.Margin = new Padding(4, 3, 4, 3);
            applyCommentEditBtn.Name = "applyCommentEditBtn";
            applyCommentEditBtn.Size = new Size(205, 21);
            applyCommentEditBtn.TabIndex = 8;
            applyCommentEditBtn.Text = "Apply Edit";
            applyCommentEditBtn.UseVisualStyleBackColor = true;
            applyCommentEditBtn.Click += applyCommentEditBtn_Click;
            // 
            // cancelCommentEditBtn
            // 
            cancelCommentEditBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cancelCommentEditBtn.Cursor = Cursors.Hand;
            cancelCommentEditBtn.Font = new Font("Segoe UI", 8.25F);
            cancelCommentEditBtn.Location = new Point(220, 177);
            cancelCommentEditBtn.Margin = new Padding(4, 3, 4, 3);
            cancelCommentEditBtn.Name = "cancelCommentEditBtn";
            cancelCommentEditBtn.Size = new Size(81, 21);
            cancelCommentEditBtn.TabIndex = 7;
            cancelCommentEditBtn.Text = "Cancel Edit";
            cancelCommentEditBtn.UseVisualStyleBackColor = true;
            cancelCommentEditBtn.Click += cancelCommentEditBtn_Click;
            // 
            // editCommentBtn
            // 
            editCommentBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            editCommentBtn.Cursor = Cursors.Hand;
            editCommentBtn.Font = new Font("Segoe UI", 8.25F);
            editCommentBtn.Location = new Point(309, 177);
            editCommentBtn.Margin = new Padding(4, 3, 4, 3);
            editCommentBtn.Name = "editCommentBtn";
            editCommentBtn.Size = new Size(51, 21);
            editCommentBtn.TabIndex = 6;
            editCommentBtn.Text = "Edit";
            editCommentBtn.UseVisualStyleBackColor = true;
            editCommentBtn.Click += editCommentBtn_Click;
            // 
            // commentLabel
            // 
            commentLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            commentLabel.AutoEllipsis = true;
            commentLabel.BackColor = SystemColors.ControlLight;
            commentLabel.Location = new Point(4, 18);
            commentLabel.Margin = new Padding(4, 0, 4, 0);
            commentLabel.Name = "commentLabel";
            commentLabel.Size = new Size(358, 20);
            commentLabel.TabIndex = 1;
            commentLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // leftContainer
            // 
            leftContainer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            leftContainer.Controls.Add(currentProjectPanel);
            leftContainer.Controls.Add(selectProjectGroup);
            leftContainer.Controls.Add(commentGroup);
            leftContainer.Location = new Point(0, 3);
            leftContainer.Margin = new Padding(4, 3, 4, 3);
            leftContainer.Name = "leftContainer";
            leftContainer.Size = new Size(365, 822);
            leftContainer.TabIndex = 11;
            // 
            // currentProjectPanel
            // 
            currentProjectPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            currentProjectPanel.Controls.Add(currentProjectNameLabel);
            currentProjectPanel.Location = new Point(4, 5);
            currentProjectPanel.Name = "currentProjectPanel";
            currentProjectPanel.Size = new Size(361, 43);
            currentProjectPanel.TabIndex = 1;
            // 
            // currentProjectNameLabel
            // 
            currentProjectNameLabel.BackColor = Color.White;
            currentProjectNameLabel.Dock = DockStyle.Fill;
            currentProjectNameLabel.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            currentProjectNameLabel.Location = new Point(0, 0);
            currentProjectNameLabel.Name = "currentProjectNameLabel";
            currentProjectNameLabel.Size = new Size(361, 43);
            currentProjectNameLabel.TabIndex = 0;
            currentProjectNameLabel.Text = "Current Project Name\r\nCan be 2 lines";
            currentProjectNameLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = ColorDepth.Depth32Bit;
            imageList1.ImageSize = new Size(16, 16);
            imageList1.TransparentColor = Color.Transparent;
            // 
            // rightContainer
            // 
            rightContainer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rightContainer.AutoScroll = true;
            rightContainer.Controls.Add(tabControl);
            rightContainer.Location = new Point(376, 3);
            rightContainer.Margin = new Padding(0);
            rightContainer.Name = "rightContainer";
            rightContainer.Size = new Size(758, 825);
            rightContainer.TabIndex = 8;
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabPage1);
            tabControl.Controls.Add(tabPage2);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 0);
            tabControl.Margin = new Padding(0);
            tabControl.Name = "tabControl";
            tabControl.Padding = new Point(0, 0);
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(758, 825);
            tabControl.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(tableLayoutPanel1);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Margin = new Padding(0);
            tabPage1.Name = "tabPage1";
            tabPage1.Size = new Size(750, 797);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "tabPage1";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.BackColor = SystemColors.Control;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(includeGroup, 0, 0);
            tableLayoutPanel1.Controls.Add(includeMediaGroup, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(750, 797);
            tableLayoutPanel1.TabIndex = 2;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(advancedSettingsView);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Margin = new Padding(0);
            tabPage2.Name = "tabPage2";
            tabPage2.Size = new Size(750, 797);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "tabPage2";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // advancedSettingsView
            // 
            advancedSettingsView.BackColor = SystemColors.Control;
            advancedSettingsView.Dock = DockStyle.Fill;
            advancedSettingsView.Location = new Point(0, 0);
            advancedSettingsView.Margin = new Padding(0);
            advancedSettingsView.Name = "advancedSettingsView";
            advancedSettingsView.Size = new Size(750, 797);
            advancedSettingsView.TabIndex = 0;
            // 
            // advancedSettingsBtn
            // 
            advancedSettingsBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            advancedSettingsBtn.Cursor = Cursors.Hand;
            advancedSettingsBtn.Location = new Point(154, 0);
            advancedSettingsBtn.Margin = new Padding(4, 0, 4, 0);
            advancedSettingsBtn.Name = "advancedSettingsBtn";
            advancedSettingsBtn.Size = new Size(160, 27);
            advancedSettingsBtn.TabIndex = 12;
            advancedSettingsBtn.Text = "Advanced Project Settings";
            advancedSettingsBtn.UseVisualStyleBackColor = true;
            advancedSettingsBtn.Click += advancedSettingsBtn_Click;
            // 
            // btnFlowPanel
            // 
            btnFlowPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnFlowPanel.Controls.Add(saveProjectBtn);
            btnFlowPanel.Controls.Add(cancelAdvancedSettingsBtn);
            btnFlowPanel.Controls.Add(runDatGrouperBtn);
            btnFlowPanel.Controls.Add(advancedSettingsBtn);
            btnFlowPanel.FlowDirection = FlowDirection.RightToLeft;
            btnFlowPanel.Location = new Point(440, 832);
            btnFlowPanel.Margin = new Padding(0);
            btnFlowPanel.Name = "btnFlowPanel";
            btnFlowPanel.Size = new Size(690, 27);
            btnFlowPanel.TabIndex = 13;
            // 
            // cancelAdvancedSettingsBtn
            // 
            cancelAdvancedSettingsBtn.AutoSize = true;
            cancelAdvancedSettingsBtn.Cursor = Cursors.Hand;
            cancelAdvancedSettingsBtn.Location = new Point(490, 0);
            cancelAdvancedSettingsBtn.Margin = new Padding(4, 0, 4, 0);
            cancelAdvancedSettingsBtn.Name = "cancelAdvancedSettingsBtn";
            cancelAdvancedSettingsBtn.Size = new Size(100, 27);
            cancelAdvancedSettingsBtn.TabIndex = 13;
            cancelAdvancedSettingsBtn.Text = "← Cancel";
            cancelAdvancedSettingsBtn.UseVisualStyleBackColor = true;
            cancelAdvancedSettingsBtn.Click += cancelAdvancedSettingsBtn_Click;
            // 
            // ProjectLoaderView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(btnFlowPanel);
            Controls.Add(rightContainer);
            Controls.Add(leftContainer);
            Margin = new Padding(0);
            Name = "ProjectLoaderView";
            Padding = new Padding(0, 3, 0, 0);
            Size = new Size(1134, 864);
            selectProjectGroup.ResumeLayout(false);
            projectListHostPanel.ResumeLayout(false);
            createProjectPanel.ResumeLayout(false);
            createProjectPanel.PerformLayout();
            includeGroup.ResumeLayout(false);
            includeMediaGroup.ResumeLayout(false);
            commentGroup.ResumeLayout(false);
            commentGroup.PerformLayout();
            leftContainer.ResumeLayout(false);
            currentProjectPanel.ResumeLayout(false);
            rightContainer.ResumeLayout(false);
            tabControl.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            btnFlowPanel.ResumeLayout(false);
            btnFlowPanel.PerformLayout();
            ResumeLayout(false);
        }

        #region Component Designer generated code

        #endregion
        private System.Windows.Forms.Button runDatGrouperBtn;
        private System.Windows.Forms.FlowLayoutPanel gamesIncludeContainer;
        private System.Windows.Forms.Button saveProjectBtn;
        private System.Windows.Forms.GroupBox selectProjectGroup;
        private System.Windows.Forms.Panel createProjectPanel;
        private System.Windows.Forms.Button saveNewProjectBtn;
        private System.Windows.Forms.Button cancelNewProjectBtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox projectNameText;
        private System.Windows.Forms.Button newProjectBtn;
        private System.Windows.Forms.Panel projectListHostPanel;
        private ProjectsListBoxUI projectListBox;
        private System.Windows.Forms.GroupBox includeGroup;
        private System.Windows.Forms.GroupBox includeMediaGroup;
        private System.Windows.Forms.FlowLayoutPanel mediaIncludeContainer;
        private System.Windows.Forms.GroupBox commentGroup;
        private System.Windows.Forms.TextBox commentText;
        private System.Windows.Forms.Panel leftContainer;
        private System.Windows.Forms.Label commentLabel;
        private System.Windows.Forms.Button editCommentBtn;
        private System.Windows.Forms.Button cancelCommentEditBtn;
        private System.Windows.Forms.Button applyCommentEditBtn;
        private ImageList imageList1;
        private System.ComponentModel.IContainer components;
        private Panel rightContainer;
        private Panel currentProjectPanel;
        private Label currentProjectNameLabel;
        private TableLayoutPanel tableLayoutPanel1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private DatGrouperProjectSettingsView advancedSettingsView;
        private InvisibleTabControl tabControl;
        private Button advancedSettingsBtn;
        private FlowLayoutPanel btnFlowPanel;
        private Button cancelAdvancedSettingsBtn;
    }
}
