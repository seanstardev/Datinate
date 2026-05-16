using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace datinate.app
{
    partial class ProjectDatUI
    {
        private IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            removeBtn = new Button();
            datChipUI = new DatChipUI();
            firstBtn = new Button();
            rightBtn = new Button();
            leftBtn = new Button();
            indexLabel = new Label();
            softwareStripPanel = new Panel();
            collectionTypeLabel = new Label();
            friendlyNameTextBox = new TextBox();
            datNameLabel = new Label();
            subsetEntryLabel = new Label();
            subsetArrow = new Label();
            commentBtn = new Button();
            filtersStripPanel = new Panel();
            loadExpressionsBtn = new Button();
            expressionsNameTextBox = new TextBox();
            clearExpressionsBtn2 = new Button();
            editExpressionsBtn = new Button();
            topPanel = new Panel();
            mediaIconUI = new MediaIconUI();
            flowLayoutPanel2 = new FlowLayoutPanel();
            lastBtn = new Button();
            SubsetStrip = new Panel();
            subsetLabels = new CenteredFlowLayoutPanel();
            subsetPathLabel = new Label();
            softwareStripPanel.SuspendLayout();
            filtersStripPanel.SuspendLayout();
            topPanel.SuspendLayout();
            ((ISupportInitialize)mediaIconUI).BeginInit();
            flowLayoutPanel2.SuspendLayout();
            SubsetStrip.SuspendLayout();
            subsetLabels.SuspendLayout();
            SuspendLayout();
            // 
            // removeBtn
            // 
            removeBtn.Location = new Point(0, 0);
            removeBtn.Name = "removeBtn";
            removeBtn.Size = new Size(20, 24);
            removeBtn.TabIndex = 0;
            removeBtn.Text = "✖";
            removeBtn.UseVisualStyleBackColor = true;
            removeBtn.Click += removeBtn_Click;
            // 
            // datChipUI
            // 
            datChipUI.Location = new Point(53, 2);
            datChipUI.Margin = new Padding(0);
            datChipUI.MaximumSize = new Size(54, 20);
            datChipUI.MinimumSize = new Size(54, 20);
            datChipUI.Name = "datChipUI";
            datChipUI.Size = new Size(54, 20);
            datChipUI.Strong = true;
            datChipUI.TabIndex = 1;
            datChipUI.TabStop = false;
            // 
            // firstBtn
            // 
            firstBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            firstBtn.Image = Datinate.Properties.Resources.btn_hard_left;
            firstBtn.Location = new Point(5, 3);
            firstBtn.Margin = new Padding(0, 3, 3, 3);
            firstBtn.Name = "firstBtn";
            firstBtn.Size = new Size(20, 24);
            firstBtn.TabIndex = 2;
            firstBtn.Text = "⯇⯇";
            firstBtn.UseVisualStyleBackColor = true;
            firstBtn.Click += setAsParentBtn_Click;
            // 
            // rightBtn
            // 
            rightBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            rightBtn.Location = new Point(51, 3);
            rightBtn.Margin = new Padding(0, 3, 3, 3);
            rightBtn.Name = "rightBtn";
            rightBtn.Size = new Size(20, 24);
            rightBtn.TabIndex = 3;
            rightBtn.Text = "▶";
            rightBtn.UseVisualStyleBackColor = true;
            rightBtn.Click += rightBtn_Click;
            // 
            // leftBtn
            // 
            leftBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            leftBtn.Location = new Point(28, 3);
            leftBtn.Margin = new Padding(0, 3, 3, 3);
            leftBtn.Name = "leftBtn";
            leftBtn.Size = new Size(20, 24);
            leftBtn.TabIndex = 4;
            leftBtn.Text = "◀";
            leftBtn.UseVisualStyleBackColor = true;
            leftBtn.Click += leftBtn_Click;
            // 
            // indexLabel
            // 
            indexLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            indexLabel.BackColor = Color.FromArgb(246, 250, 255);
            indexLabel.ForeColor = Color.FromArgb(55, 63, 74);
            indexLabel.Location = new Point(308, 1);
            indexLabel.Name = "indexLabel";
            indexLabel.Size = new Size(30, 22);
            indexLabel.TabIndex = 5;
            indexLabel.Text = "#1";
            indexLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // softwareStripPanel
            // 
            softwareStripPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            softwareStripPanel.BackColor = Color.FromArgb(248, 250, 253);
            softwareStripPanel.Controls.Add(collectionTypeLabel);
            softwareStripPanel.Controls.Add(friendlyNameTextBox);
            softwareStripPanel.Location = new Point(0, 27);
            softwareStripPanel.Name = "softwareStripPanel";
            softwareStripPanel.Size = new Size(341, 24);
            softwareStripPanel.TabIndex = 6;
            // 
            // collectionTypeLabel
            // 
            collectionTypeLabel.Location = new Point(6, 4);
            collectionTypeLabel.Name = "collectionTypeLabel";
            collectionTypeLabel.Size = new Size(162, 16);
            collectionTypeLabel.TabIndex = 0;
            collectionTypeLabel.Text = "Software";
            collectionTypeLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // friendlyNameTextBox
            // 
            friendlyNameTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            friendlyNameTextBox.BorderStyle = BorderStyle.FixedSingle;
            friendlyNameTextBox.ForeColor = Color.FromArgb(33, 37, 41);
            friendlyNameTextBox.Location = new Point(180, 1);
            friendlyNameTextBox.Name = "friendlyNameTextBox";
            friendlyNameTextBox.PlaceholderText = "Quick Reference Name...";
            friendlyNameTextBox.Size = new Size(160, 23);
            friendlyNameTextBox.TabIndex = 1;
            // 
            // datNameLabel
            // 
            datNameLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            datNameLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            datNameLabel.ForeColor = Color.FromArgb(33, 37, 41);
            datNameLabel.Location = new Point(2, 52);
            datNameLabel.Name = "datNameLabel";
            datNameLabel.Size = new Size(336, 33);
            datNameLabel.TabIndex = 7;
            datNameLabel.Text = "DAT Name\r\nLine 2...";
            datNameLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // subsetEntryLabel
            // 
            subsetEntryLabel.ForeColor = Color.FromArgb(70, 82, 94);
            subsetEntryLabel.Location = new Point(36, 0);
            subsetEntryLabel.Name = "subsetEntryLabel";
            subsetEntryLabel.Size = new Size(120, 18);
            subsetEntryLabel.TabIndex = 8;
            subsetEntryLabel.Text = "Discs";
            subsetEntryLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // subsetArrow
            // 
            subsetArrow.ForeColor = Color.FromArgb(70, 82, 94);
            subsetArrow.Location = new Point(162, 0);
            subsetArrow.Name = "subsetArrow";
            subsetArrow.Size = new Size(16, 19);
            subsetArrow.TabIndex = 9;
            subsetArrow.Text = "▶";
            subsetArrow.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // commentBtn
            // 
            commentBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            commentBtn.Location = new Point(316, 1);
            commentBtn.Name = "commentBtn";
            commentBtn.Size = new Size(22, 24);
            commentBtn.TabIndex = 11;
            commentBtn.Text = "C";
            commentBtn.UseVisualStyleBackColor = true;
            commentBtn.Click += OnCommentBtnClicked;
            // 
            // filtersStripPanel
            // 
            filtersStripPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            filtersStripPanel.Controls.Add(loadExpressionsBtn);
            filtersStripPanel.Controls.Add(expressionsNameTextBox);
            filtersStripPanel.Controls.Add(clearExpressionsBtn2);
            filtersStripPanel.Controls.Add(editExpressionsBtn);
            filtersStripPanel.Location = new Point(0, 112);
            filtersStripPanel.Name = "filtersStripPanel";
            filtersStripPanel.Size = new Size(341, 26);
            filtersStripPanel.TabIndex = 12;
            // 
            // loadExpressionsBtn
            // 
            loadExpressionsBtn.Location = new Point(2, 0);
            loadExpressionsBtn.Name = "loadExpressionsBtn";
            loadExpressionsBtn.Size = new Size(56, 25);
            loadExpressionsBtn.TabIndex = 0;
            loadExpressionsBtn.Text = "Change";
            loadExpressionsBtn.UseVisualStyleBackColor = true;
            loadExpressionsBtn.Click += expressionsBtn_Click;
            // 
            // expressionsNameTextBox
            // 
            expressionsNameTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            expressionsNameTextBox.BorderStyle = BorderStyle.None;
            expressionsNameTextBox.ForeColor = Color.FromArgb(70, 82, 94);
            expressionsNameTextBox.Location = new Point(64, 5);
            expressionsNameTextBox.Multiline = true;
            expressionsNameTextBox.Name = "expressionsNameTextBox";
            expressionsNameTextBox.PlaceholderText = "DAT Filter File";
            expressionsNameTextBox.ReadOnly = true;
            expressionsNameTextBox.Size = new Size(186, 17);
            expressionsNameTextBox.TabIndex = 1;
            // 
            // clearExpressionsBtn2
            // 
            clearExpressionsBtn2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            clearExpressionsBtn2.Location = new Point(256, 1);
            clearExpressionsBtn2.Name = "clearExpressionsBtn2";
            clearExpressionsBtn2.Size = new Size(20, 24);
            clearExpressionsBtn2.TabIndex = 7;
            clearExpressionsBtn2.Text = "✖";
            clearExpressionsBtn2.UseVisualStyleBackColor = true;
            clearExpressionsBtn2.Click += clearExpressionsBtn_Click;
            // 
            // editExpressionsBtn
            // 
            editExpressionsBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            editExpressionsBtn.Location = new Point(281, 0);
            editExpressionsBtn.Name = "editExpressionsBtn";
            editExpressionsBtn.Size = new Size(56, 25);
            editExpressionsBtn.TabIndex = 3;
            editExpressionsBtn.Text = "Edit";
            editExpressionsBtn.UseVisualStyleBackColor = true;
            editExpressionsBtn.Click += loadExpressionsBtn_Click;
            // 
            // topPanel
            // 
            topPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            topPanel.Controls.Add(datChipUI);
            topPanel.Controls.Add(mediaIconUI);
            topPanel.Controls.Add(flowLayoutPanel2);
            topPanel.Controls.Add(removeBtn);
            topPanel.Controls.Add(indexLabel);
            topPanel.Location = new Point(0, 0);
            topPanel.Name = "topPanel";
            topPanel.Size = new Size(341, 27);
            topPanel.TabIndex = 14;
            // 
            // mediaIconUI
            // 
            mediaIconUI.Image = Datinate.Properties.Resources.media_icons_ROM;
            mediaIconUI.ImageKey = null;
            mediaIconUI.Location = new Point(20, -2);
            mediaIconUI.Name = "mediaIconUI";
            mediaIconUI.Size = new Size(30, 30);
            mediaIconUI.SizeMode = PictureBoxSizeMode.Zoom;
            mediaIconUI.TabIndex = 7;
            mediaIconUI.TabStop = false;
            // 
            // flowLayoutPanel2
            // 
            flowLayoutPanel2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            flowLayoutPanel2.Controls.Add(lastBtn);
            flowLayoutPanel2.Controls.Add(rightBtn);
            flowLayoutPanel2.Controls.Add(leftBtn);
            flowLayoutPanel2.Controls.Add(firstBtn);
            flowLayoutPanel2.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanel2.Location = new Point(208, 0);
            flowLayoutPanel2.Margin = new Padding(0);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.Size = new Size(97, 26);
            flowLayoutPanel2.TabIndex = 6;
            // 
            // lastBtn
            // 
            lastBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lastBtn.Image = Datinate.Properties.Resources.btn_hard_right;
            lastBtn.Location = new Point(74, 3);
            lastBtn.Margin = new Padding(0, 3, 3, 3);
            lastBtn.Name = "lastBtn";
            lastBtn.Size = new Size(20, 24);
            lastBtn.TabIndex = 5;
            lastBtn.Text = "⯇⯇";
            lastBtn.UseVisualStyleBackColor = true;
            lastBtn.Click += setAsLastBtn_Click;
            // 
            // SubsetStrip
            // 
            SubsetStrip.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            SubsetStrip.Controls.Add(commentBtn);
            SubsetStrip.Controls.Add(subsetLabels);
            SubsetStrip.Location = new Point(0, 85);
            SubsetStrip.Name = "SubsetStrip";
            SubsetStrip.Size = new Size(341, 26);
            SubsetStrip.TabIndex = 15;
            // 
            // subsetLabels
            // 
            subsetLabels.BackColor = Color.FromArgb(248, 250, 253);
            subsetLabels.Controls.Add(subsetEntryLabel);
            subsetLabels.Controls.Add(subsetArrow);
            subsetLabels.Controls.Add(subsetPathLabel);
            subsetLabels.Dock = DockStyle.Fill;
            subsetLabels.Location = new Point(0, 0);
            subsetLabels.Name = "subsetLabels";
            subsetLabels.Size = new Size(341, 26);
            subsetLabels.TabIndex = 16;
            // 
            // subsetPathLabel
            // 
            subsetPathLabel.ForeColor = Color.FromArgb(70, 82, 94);
            subsetPathLabel.Location = new Point(184, 0);
            subsetPathLabel.Name = "subsetPathLabel";
            subsetPathLabel.Size = new Size(120, 18);
            subsetPathLabel.TabIndex = 12;
            subsetPathLabel.Text = "Path";
            subsetPathLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // ProjectDatUI
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(215, 228, 242);
            BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(topPanel);
            Controls.Add(softwareStripPanel);
            Controls.Add(datNameLabel);
            Controls.Add(SubsetStrip);
            Controls.Add(filtersStripPanel);
            Name = "ProjectDatUI";
            Size = new Size(341, 141);
            softwareStripPanel.ResumeLayout(false);
            softwareStripPanel.PerformLayout();
            filtersStripPanel.ResumeLayout(false);
            filtersStripPanel.PerformLayout();
            topPanel.ResumeLayout(false);
            ((ISupportInitialize)mediaIconUI).EndInit();
            flowLayoutPanel2.ResumeLayout(false);
            SubsetStrip.ResumeLayout(false);
            subsetLabels.ResumeLayout(false);
            ResumeLayout(false);
        }

        private Button removeBtn;
        private Button firstBtn;
        private Button rightBtn;
        private Button leftBtn;
        private Label indexLabel;
        private Panel softwareStripPanel;
        private Label collectionTypeLabel;
        private TextBox friendlyNameTextBox;
        private Label datNameLabel;
        private Label subsetEntryLabel;
        private Label subsetArrow;
        private Button commentBtn;
        private Panel filtersStripPanel;
        private Button loadExpressionsBtn;
        private TextBox expressionsNameTextBox;
        private Button editExpressionsBtn;
        private DatChipUI datChipUI;
        private Panel topPanel;
        private Panel SubsetStrip;
        private Label subsetPathLabel;
        private CenteredFlowLayoutPanel subsetLabels;
        private FlowLayoutPanel flowLayoutPanel2;
        private Button clearExpressionsBtn2;
        private MediaIconUI mediaIconUI;
        private Button lastBtn;
    }
}
