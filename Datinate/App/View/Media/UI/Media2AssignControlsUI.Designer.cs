using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace datinate.app
{
    partial class Media2AssignControlsUI
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
            tabControl = new BorderlessTabControl();
            namePage = new TabPage();
            entryNameTxt = new Label();
            mediaPage = new TabPage();
            panel2 = new Panel();
            assignmentBtnStrip = new Media2AssignButtonBar();
            listPage = new TabPage();
            panel1 = new Panel();
            filterTxt = new TextBox();
            notFoundBtn = new Button();
            label1 = new Label();
            matchPage = new TabPage();
            panel3 = new Panel();
            matchBarsUI = new MatchBarsUI();
            matchPercentageLabel = new Label();
            label2 = new Label();
            tabControl.SuspendLayout();
            namePage.SuspendLayout();
            mediaPage.SuspendLayout();
            panel2.SuspendLayout();
            listPage.SuspendLayout();
            panel1.SuspendLayout();
            matchPage.SuspendLayout();
            panel3.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Controls.Add(namePage);
            tabControl.Controls.Add(mediaPage);
            tabControl.Controls.Add(listPage);
            tabControl.Controls.Add(matchPage);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 0);
            tabControl.Margin = new Padding(0);
            tabControl.Name = "tabControl";
            tabControl.Padding = new Point(0, 0);
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(588, 191);
            tabControl.TabIndex = 0;
            tabControl.TopEdgeCoverColor = Color.Black;
            // 
            // namePage
            // 
            namePage.Controls.Add(entryNameTxt);
            namePage.Location = new Point(0, 20);
            namePage.Margin = new Padding(0);
            namePage.Name = "namePage";
            namePage.Size = new Size(588, 171);
            namePage.TabIndex = 0;
            namePage.Text = "namePage";
            namePage.UseVisualStyleBackColor = true;
            // 
            // entryNameTxt
            // 
            entryNameTxt.AutoEllipsis = true;
            entryNameTxt.Dock = DockStyle.Fill;
            entryNameTxt.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            entryNameTxt.Location = new Point(0, 0);
            entryNameTxt.Margin = new Padding(0);
            entryNameTxt.Name = "entryNameTxt";
            entryNameTxt.Size = new Size(588, 171);
            entryNameTxt.TabIndex = 0;
            entryNameTxt.Text = "Entry Name...";
            entryNameTxt.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // mediaPage
            // 
            mediaPage.Controls.Add(panel2);
            mediaPage.Location = new Point(0, 20);
            mediaPage.Name = "mediaPage";
            mediaPage.Size = new Size(588, 171);
            mediaPage.TabIndex = 1;
            mediaPage.Text = "mediaPage";
            mediaPage.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            panel2.Controls.Add(assignmentBtnStrip);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(0, 0);
            panel2.Margin = new Padding(0);
            panel2.Name = "panel2";
            panel2.Size = new Size(588, 171);
            panel2.TabIndex = 1;
            // 
            // assignmentBtnStrip
            // 
            assignmentBtnStrip.AssignedSelectedBackColor = Color.FromArgb(224, 245, 224);
            assignmentBtnStrip.Dock = DockStyle.Fill;
            assignmentBtnStrip.Location = new Point(0, 0);
            assignmentBtnStrip.Margin = new Padding(0);
            assignmentBtnStrip.Name = "assignmentBtnStrip";
            assignmentBtnStrip.NotFoundSelectedBackColor = Color.FromArgb(235, 235, 235);
            assignmentBtnStrip.Selected = MEDIA_ASSIGNMENT_ENUM.Assigned;
            assignmentBtnStrip.Size = new Size(588, 171);
            assignmentBtnStrip.TabIndex = 0;
            assignmentBtnStrip.UnassignedSelectedBackColor = Color.FromArgb(235, 235, 235);
            // 
            // listPage
            // 
            listPage.Controls.Add(panel1);
            listPage.Location = new Point(0, 20);
            listPage.Name = "listPage";
            listPage.Size = new Size(588, 171);
            listPage.TabIndex = 2;
            listPage.Text = "listPage";
            listPage.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            panel1.Controls.Add(filterTxt);
            panel1.Controls.Add(notFoundBtn);
            panel1.Controls.Add(label1);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Margin = new Padding(0);
            panel1.Name = "panel1";
            panel1.Size = new Size(588, 171);
            panel1.TabIndex = 0;
            // 
            // filterTxt
            // 
            filterTxt.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            filterTxt.Location = new Point(3, 3);
            filterTxt.Name = "filterTxt";
            filterTxt.PlaceholderText = "Filter Entries";
            filterTxt.Size = new Size(448, 23);
            filterTxt.TabIndex = 1;
            filterTxt.TextChanged += filterTxt_TextChanged;
            filterTxt.DoubleClick += filterTxt_DoubleClick;
            filterTxt.KeyUp += filterTxt_KeyUp;
            // 
            // notFoundBtn
            // 
            notFoundBtn.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            notFoundBtn.Cursor = Cursors.Hand;
            notFoundBtn.Location = new Point(494, -1);
            notFoundBtn.Name = "notFoundBtn";
            notFoundBtn.Size = new Size(94, 173);
            notFoundBtn.TabIndex = 0;
            notFoundBtn.Text = "Not Found";
            notFoundBtn.UseVisualStyleBackColor = true;
            notFoundBtn.Click += notFoundBtn_Click;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Cursor = Cursors.Hand;
            label1.Location = new Point(454, 7);
            label1.Name = "label1";
            label1.Size = new Size(43, 15);
            label1.TabIndex = 2;
            label1.Text = "Sort ⏷";
            label1.Click += toggleSort_Click;
            // 
            // matchPage
            // 
            matchPage.Controls.Add(panel3);
            matchPage.Location = new Point(0, 20);
            matchPage.Name = "matchPage";
            matchPage.Size = new Size(588, 171);
            matchPage.TabIndex = 3;
            matchPage.Text = "matchPage";
            matchPage.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            panel3.Controls.Add(matchBarsUI);
            panel3.Controls.Add(matchPercentageLabel);
            panel3.Controls.Add(label2);
            panel3.Dock = DockStyle.Fill;
            panel3.Location = new Point(0, 0);
            panel3.Margin = new Padding(0);
            panel3.Name = "panel3";
            panel3.Size = new Size(588, 171);
            panel3.TabIndex = 0;
            // 
            // matchBarsUI
            // 
            matchBarsUI.BackColor = Color.Transparent;
            matchBarsUI.BarEnabledColour = Color.Black;
            matchBarsUI.BarWarningColour = Color.Black;
            matchBarsUI.Dock = DockStyle.Right;
            matchBarsUI.Location = new Point(558, 0);
            matchBarsUI.Name = "matchBarsUI";
            matchBarsUI.Size = new Size(30, 171);
            matchBarsUI.TabIndex = 2;
            // 
            // matchPercentageLabel
            // 
            matchPercentageLabel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            matchPercentageLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            matchPercentageLabel.Location = new Point(75, 0);
            matchPercentageLabel.Name = "matchPercentageLabel";
            matchPercentageLabel.Size = new Size(81, 171);
            matchPercentageLabel.TabIndex = 1;
            matchPercentageLabel.Text = "100%";
            matchPercentageLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            label2.Font = new Font("Segoe UI", 9.75F, FontStyle.Italic, GraphicsUnit.Point, 0);
            label2.Location = new Point(3, 0);
            label2.Name = "label2";
            label2.Size = new Size(81, 171);
            label2.TabIndex = 0;
            label2.Text = "Best Match:";
            label2.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // Media2AssignControlsUI
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tabControl);
            Margin = new Padding(0);
            Name = "Media2AssignControlsUI";
            Size = new Size(588, 191);
            tabControl.ResumeLayout(false);
            namePage.ResumeLayout(false);
            mediaPage.ResumeLayout(false);
            panel2.ResumeLayout(false);
            listPage.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            matchPage.ResumeLayout(false);
            panel3.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private TabPage namePage;
        private TabPage mediaPage;
        private Label entryNameTxt;
        private Media2AssignButtonBar assignmentBtnStrip;
        private TabPage listPage;
        private Panel panel1;
        private Button notFoundBtn;
        private TextBox filterTxt;
        private Panel panel2;
        private Label label1;
        private BorderlessTabControl tabControl;
        private TabPage matchPage;
        private Panel panel3;
        private Label matchPercentageLabel;
        private Label label2;
        private MatchBarsUI matchBarsUI;
    }
}
