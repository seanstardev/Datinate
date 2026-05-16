namespace datinate.app 
{
    partial class ProjectLoaderView 
    {
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            buildProjectBtn = new Button();
            gamesIncludeContainer = new FlowLayoutPanel();
            saveProjectBtn = new Button();
            selectProjectGroup = new GroupBox();
            newProjectBtn = new Button();
            projectListHostPanel = new Panel();
            projectListBox = new ListBox();
            createProjectPanel = new Panel();
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
            settingsPanel = new Panel();
            currentProjectPanel = new Panel();
            currentProjectNameLabel = new Label();
            familyTreeBtn = new LinkLabel();
            imageList1 = new ImageList(components);
            referenceContainer2 = new Panel();
            tableLayoutPanel1 = new TableLayoutPanel();
            selectProjectGroup.SuspendLayout();
            projectListHostPanel.SuspendLayout();
            createProjectPanel.SuspendLayout();
            includeGroup.SuspendLayout();
            includeMediaGroup.SuspendLayout();
            commentGroup.SuspendLayout();
            settingsPanel.SuspendLayout();
            currentProjectPanel.SuspendLayout();
            referenceContainer2.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // buildProjectBtn
            // 
            buildProjectBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buildProjectBtn.Cursor = Cursors.Hand;
            buildProjectBtn.Location = new Point(970, 831);
            buildProjectBtn.Margin = new Padding(4, 3, 4, 3);
            buildProjectBtn.Name = "buildProjectBtn";
            buildProjectBtn.Size = new Size(160, 27);
            buildProjectBtn.TabIndex = 0;
            buildProjectBtn.Text = "Run DAT Grouper";
            buildProjectBtn.UseVisualStyleBackColor = true;
            buildProjectBtn.Click += buildProjectBtn_Click;
            // 
            // gamesIncludeContainer
            // 
            gamesIncludeContainer.AutoScroll = true;
            gamesIncludeContainer.Dock = DockStyle.Fill;
            gamesIncludeContainer.Location = new Point(4, 19);
            gamesIncludeContainer.Margin = new Padding(4, 3, 4, 3);
            gamesIncludeContainer.Name = "gamesIncludeContainer";
            gamesIncludeContainer.Size = new Size(742, 384);
            gamesIncludeContainer.TabIndex = 1;
            // 
            // saveProjectBtn
            // 
            saveProjectBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            saveProjectBtn.Cursor = Cursors.Hand;
            saveProjectBtn.Location = new Point(874, 831);
            saveProjectBtn.Margin = new Padding(4, 3, 4, 3);
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
            projectListBox.BorderStyle = BorderStyle.None;
            projectListBox.Dock = DockStyle.Fill;
            projectListBox.FormattingEnabled = true;
            projectListBox.Location = new Point(1, 1);
            projectListBox.Margin = new Padding(0);
            projectListBox.Name = "projectListBox";
            projectListBox.Size = new Size(347, 510);
            projectListBox.TabIndex = 0;
            projectListBox.DrawItem += projectListBox_DrawItem;
            projectListBox.SelectedIndexChanged += projectListBox_SelectedIndexChanged;
            projectListBox.MouseLeave += projectListBox_MouseLeave;
            projectListBox.MouseMove += projectListBox_MouseMove;
            // 
            // createProjectPanel
            // 
            createProjectPanel.BackColor = Color.White;
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
            projectNameText.Location = new Point(0, 17);
            projectNameText.Margin = new Padding(4, 3, 4, 3);
            projectNameText.Name = "projectNameText";
            projectNameText.Size = new Size(259, 23);
            projectNameText.TabIndex = 6;
            // 
            // includeGroup
            // 
            includeGroup.Controls.Add(gamesIncludeContainer);
            includeGroup.Dock = DockStyle.Fill;
            includeGroup.Location = new Point(4, 3);
            includeGroup.Margin = new Padding(4, 3, 4, 3);
            includeGroup.Name = "includeGroup";
            includeGroup.Padding = new Padding(4, 3, 4, 3);
            includeGroup.Size = new Size(750, 406);
            includeGroup.TabIndex = 5;
            includeGroup.TabStop = false;
            includeGroup.Text = "Software";
            // 
            // includeMediaGroup
            // 
            includeMediaGroup.Controls.Add(mediaIncludeContainer);
            includeMediaGroup.Dock = DockStyle.Fill;
            includeMediaGroup.Location = new Point(4, 415);
            includeMediaGroup.Margin = new Padding(4, 3, 4, 3);
            includeMediaGroup.Name = "includeMediaGroup";
            includeMediaGroup.Padding = new Padding(4, 3, 4, 3);
            includeMediaGroup.Size = new Size(750, 407);
            includeMediaGroup.TabIndex = 6;
            includeMediaGroup.TabStop = false;
            includeMediaGroup.Text = "Media && Resource";
            // 
            // mediaIncludeContainer
            // 
            mediaIncludeContainer.AutoScroll = true;
            mediaIncludeContainer.BackColor = SystemColors.Control;
            mediaIncludeContainer.Dock = DockStyle.Fill;
            mediaIncludeContainer.Location = new Point(4, 19);
            mediaIncludeContainer.Margin = new Padding(4, 3, 4, 3);
            mediaIncludeContainer.Name = "mediaIncludeContainer";
            mediaIncludeContainer.Size = new Size(742, 385);
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
            // settingsPanel
            // 
            settingsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            settingsPanel.Controls.Add(currentProjectPanel);
            settingsPanel.Controls.Add(selectProjectGroup);
            settingsPanel.Controls.Add(commentGroup);
            settingsPanel.Location = new Point(0, 3);
            settingsPanel.Margin = new Padding(4, 3, 4, 3);
            settingsPanel.Name = "settingsPanel";
            settingsPanel.Size = new Size(365, 822);
            settingsPanel.TabIndex = 11;
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
            // familyTreeBtn
            // 
            familyTreeBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            familyTreeBtn.AutoSize = true;
            familyTreeBtn.LinkColor = Color.Black;
            familyTreeBtn.Location = new Point(720, 836);
            familyTreeBtn.Margin = new Padding(4, 0, 4, 0);
            familyTreeBtn.Name = "familyTreeBtn";
            familyTreeBtn.Size = new Size(146, 15);
            familyTreeBtn.TabIndex = 12;
            familyTreeBtn.TabStop = true;
            familyTreeBtn.Text = "Show DAT Grouper Project";
            familyTreeBtn.Visible = false;
            familyTreeBtn.LinkClicked += familyTreeBtn_LinkClicked;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = ColorDepth.Depth32Bit;
            imageList1.ImageSize = new Size(16, 16);
            imageList1.TransparentColor = Color.Transparent;
            // 
            // referenceContainer2
            // 
            referenceContainer2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            referenceContainer2.AutoScroll = true;
            referenceContainer2.Controls.Add(tableLayoutPanel1);
            referenceContainer2.Location = new Point(376, 3);
            referenceContainer2.Margin = new Padding(0);
            referenceContainer2.Name = "referenceContainer2";
            referenceContainer2.Size = new Size(758, 825);
            referenceContainer2.TabIndex = 8;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(includeGroup, 0, 0);
            tableLayoutPanel1.Controls.Add(includeMediaGroup, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Size = new Size(758, 825);
            tableLayoutPanel1.TabIndex = 2;
            // 
            // ProjectLoaderView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(referenceContainer2);
            Controls.Add(familyTreeBtn);
            Controls.Add(settingsPanel);
            Controls.Add(buildProjectBtn);
            Controls.Add(saveProjectBtn);
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
            settingsPanel.ResumeLayout(false);
            currentProjectPanel.ResumeLayout(false);
            referenceContainer2.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #region Component Designer generated code

        #endregion
        private System.Windows.Forms.Button buildProjectBtn;
        private System.Windows.Forms.FlowLayoutPanel gamesIncludeContainer;
        private System.Windows.Forms.Button saveProjectBtn;
        private System.Windows.Forms.GroupBox selectProjectGroup;
        private System.Windows.Forms.Panel createProjectPanel;
        private System.Windows.Forms.Button cancelNewProjectBtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox projectNameText;
        private System.Windows.Forms.Button newProjectBtn;
        private System.Windows.Forms.Panel projectListHostPanel;
        private System.Windows.Forms.ListBox projectListBox;
        private System.Windows.Forms.GroupBox includeGroup;
        private System.Windows.Forms.GroupBox includeMediaGroup;
        private System.Windows.Forms.FlowLayoutPanel mediaIncludeContainer;
        private System.Windows.Forms.GroupBox commentGroup;
        private System.Windows.Forms.TextBox commentText;
        private System.Windows.Forms.Panel settingsPanel;
        private System.Windows.Forms.LinkLabel familyTreeBtn;
        private System.Windows.Forms.Label commentLabel;
        private System.Windows.Forms.Button editCommentBtn;
        private System.Windows.Forms.Button cancelCommentEditBtn;
        private System.Windows.Forms.Button applyCommentEditBtn;
        private ImageList imageList1;
        private System.ComponentModel.IContainer components;
        private Panel referenceContainer2;
        private Panel currentProjectPanel;
        private Label currentProjectNameLabel;
        private TableLayoutPanel tableLayoutPanel1;
    }
}
