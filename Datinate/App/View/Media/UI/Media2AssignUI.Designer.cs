namespace datinate.app
{
    partial class Media2AssignUI
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
                OnDispose(disposing);
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
            mediaIconUI = new MediaIconUI();
            datChipUI = new DatChipUI();
            tabControl = new BorderlessTabControl();
            mediaPage = new TabPage();
            mediaContainer = new Panel();
            miniWebUI = new MiniWebUI();
            listPage = new TabPage();
            panel3 = new Panel();
            entryListUI = new FastEntryListUI();
            mediaNameTextBox = new Label();
            tableLayoutPanel = new TableLayoutPanel();
            headerPanel = new Panel();
            assignControls = new Media2AssignControlsUI();
            pageCurl = new PictureBox();
            notFoundPic = new PictureBox();
            okPic = new PictureBox();
            innerContainer = new Panel();
            outerContainer = new Panel();
            bannerBgLeft = new PictureBox();
            bannerBgRight = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)mediaIconUI).BeginInit();
            tabControl.SuspendLayout();
            mediaPage.SuspendLayout();
            mediaContainer.SuspendLayout();
            listPage.SuspendLayout();
            panel3.SuspendLayout();
            tableLayoutPanel.SuspendLayout();
            headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pageCurl).BeginInit();
            ((System.ComponentModel.ISupportInitialize)notFoundPic).BeginInit();
            ((System.ComponentModel.ISupportInitialize)okPic).BeginInit();
            innerContainer.SuspendLayout();
            outerContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)bannerBgLeft).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bannerBgRight).BeginInit();
            SuspendLayout();
            // 
            // mediaIconUI
            // 
            mediaIconUI.ImageKey = null;
            mediaIconUI.Location = new Point(0, 0);
            mediaIconUI.Name = "mediaIconUI";
            mediaIconUI.Size = new Size(50, 50);
            mediaIconUI.SizeMode = PictureBoxSizeMode.Zoom;
            mediaIconUI.TabIndex = 1;
            mediaIconUI.TabStop = false;
            // 
            // datChipUI
            // 
            datChipUI.Location = new Point(50, 26);
            datChipUI.Margin = new Padding(0);
            datChipUI.MaximumSize = new Size(54, 20);
            datChipUI.MinimumSize = new Size(54, 20);
            datChipUI.Name = "datChipUI";
            datChipUI.Size = new Size(54, 20);
            datChipUI.Strong = true;
            datChipUI.TabIndex = 2;
            datChipUI.TabStop = false;
            datChipUI.Text = "datChipui2";
            // 
            // tabControl
            // 
            tabControl.Controls.Add(mediaPage);
            tabControl.Controls.Add(listPage);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(2, 82);
            tabControl.Margin = new Padding(2);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(272, 218);
            tabControl.TabIndex = 4;
            tabControl.TopEdgeCoverColor = Color.Black;
            // 
            // mediaPage
            // 
            mediaPage.Controls.Add(mediaContainer);
            mediaPage.Location = new Point(0, 20);
            mediaPage.Margin = new Padding(0);
            mediaPage.Name = "mediaPage";
            mediaPage.Size = new Size(272, 198);
            mediaPage.TabIndex = 0;
            mediaPage.Text = "mediaPage";
            mediaPage.UseVisualStyleBackColor = true;
            // 
            // mediaContainer
            // 
            mediaContainer.Controls.Add(miniWebUI);
            mediaContainer.Dock = DockStyle.Fill;
            mediaContainer.Location = new Point(0, 0);
            mediaContainer.Margin = new Padding(0);
            mediaContainer.Name = "mediaContainer";
            mediaContainer.Size = new Size(272, 198);
            mediaContainer.TabIndex = 0;
            // 
            // miniWebUI
            // 
            miniWebUI.Dock = DockStyle.Fill;
            miniWebUI.Location = new Point(0, 0);
            miniWebUI.Margin = new Padding(0);
            miniWebUI.Name = "miniWebUI";
            miniWebUI.Size = new Size(272, 198);
            miniWebUI.TabIndex = 0;
            // 
            // listPage
            // 
            listPage.Controls.Add(panel3);
            listPage.Location = new Point(0, 20);
            listPage.Margin = new Padding(0);
            listPage.Name = "listPage";
            listPage.Size = new Size(272, 198);
            listPage.TabIndex = 1;
            listPage.Text = "listPage";
            listPage.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            panel3.Controls.Add(entryListUI);
            panel3.Dock = DockStyle.Fill;
            panel3.Location = new Point(0, 0);
            panel3.Margin = new Padding(0);
            panel3.Name = "panel3";
            panel3.Size = new Size(272, 198);
            panel3.TabIndex = 0;
            // 
            // entryListUI
            // 
            entryListUI.Activation = ItemActivation.TwoClick;
            entryListUI.Dock = DockStyle.Fill;
            entryListUI.FullRowSelect = true;
            entryListUI.HeaderStyle = ColumnHeaderStyle.None;
            entryListUI.Location = new Point(0, 0);
            entryListUI.Margin = new Padding(0);
            entryListUI.MultiSelect = false;
            entryListUI.Name = "entryListUI";
            entryListUI.OwnerDraw = true;
            entryListUI.ShowItemToolTips = true;
            entryListUI.Size = new Size(272, 198);
            entryListUI.TabIndex = 0;
            entryListUI.UseCompatibleStateImageBehavior = false;
            entryListUI.View = View.Details;
            entryListUI.VirtualMode = true;
            // 
            // mediaNameTextBox
            // 
            mediaNameTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            mediaNameTextBox.AutoEllipsis = true;
            mediaNameTextBox.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            mediaNameTextBox.Location = new Point(47, 1);
            mediaNameTextBox.Name = "mediaNameTextBox";
            mediaNameTextBox.Size = new Size(229, 30);
            mediaNameTextBox.TabIndex = 1;
            mediaNameTextBox.Text = "...";
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel.ColumnCount = 1;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel.Controls.Add(headerPanel, 0, 0);
            tableLayoutPanel.Controls.Add(assignControls, 0, 1);
            tableLayoutPanel.Controls.Add(tabControl, 0, 2);
            tableLayoutPanel.Location = new Point(4, 2);
            tableLayoutPanel.Margin = new Padding(0);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 3;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel.Size = new Size(276, 302);
            tableLayoutPanel.TabIndex = 5;
            // 
            // headerPanel
            // 
            headerPanel.Controls.Add(datChipUI);
            headerPanel.Controls.Add(mediaIconUI);
            headerPanel.Controls.Add(mediaNameTextBox);
            headerPanel.Dock = DockStyle.Fill;
            headerPanel.Location = new Point(0, 0);
            headerPanel.Margin = new Padding(0);
            headerPanel.Name = "headerPanel";
            headerPanel.Size = new Size(276, 50);
            headerPanel.TabIndex = 5;
            // 
            // assignControls
            // 
            assignControls.Dock = DockStyle.Fill;
            assignControls.Location = new Point(0, 50);
            assignControls.Margin = new Padding(0);
            assignControls.Name = "assignControls";
            assignControls.Size = new Size(276, 30);
            assignControls.TabIndex = 7;
            // 
            // pageCurl
            // 
            pageCurl.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pageCurl.Image = Datinate.Properties.Resources.pageCurl_2;
            pageCurl.Location = new Point(218, 0);
            pageCurl.Name = "pageCurl";
            pageCurl.Size = new Size(64, 45);
            pageCurl.SizeMode = PictureBoxSizeMode.StretchImage;
            pageCurl.TabIndex = 3;
            pageCurl.TabStop = false;
            // 
            // notFoundPic
            // 
            notFoundPic.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            notFoundPic.BackColor = Color.White;
            notFoundPic.Image = Datinate.Properties.Resources.media_assign_not_found;
            notFoundPic.Location = new Point(-2, 150);
            notFoundPic.Margin = new Padding(0);
            notFoundPic.Name = "notFoundPic";
            notFoundPic.Size = new Size(284, 53);
            notFoundPic.SizeMode = PictureBoxSizeMode.Zoom;
            notFoundPic.TabIndex = 1;
            notFoundPic.TabStop = false;
            // 
            // okPic
            // 
            okPic.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            okPic.BackColor = Color.White;
            okPic.Image = Datinate.Properties.Resources.media_assign_ok;
            okPic.Location = new Point(-2, 150);
            okPic.Margin = new Padding(0);
            okPic.Name = "okPic";
            okPic.Size = new Size(284, 53);
            okPic.SizeMode = PictureBoxSizeMode.Zoom;
            okPic.TabIndex = 6;
            okPic.TabStop = false;
            // 
            // innerContainer
            // 
            innerContainer.BackColor = SystemColors.Control;
            innerContainer.Controls.Add(pageCurl);
            innerContainer.Controls.Add(tableLayoutPanel);
            innerContainer.Dock = DockStyle.Fill;
            innerContainer.Location = new Point(0, 0);
            innerContainer.Margin = new Padding(0);
            innerContainer.Name = "innerContainer";
            innerContainer.Padding = new Padding(2, 0, 2, 2);
            innerContainer.Size = new Size(280, 302);
            innerContainer.TabIndex = 3;
            // 
            // outerContainer
            // 
            outerContainer.BackColor = Color.Black;
            outerContainer.Controls.Add(innerContainer);
            outerContainer.Dock = DockStyle.Fill;
            outerContainer.Location = new Point(0, 0);
            outerContainer.Margin = new Padding(0);
            outerContainer.Name = "outerContainer";
            outerContainer.Size = new Size(280, 302);
            outerContainer.TabIndex = 3;
            // 
            // bannerBgLeft
            // 
            bannerBgLeft.BackColor = Color.Black;
            bannerBgLeft.Image = Datinate.Properties.Resources.media_assign_bg_left;
            bannerBgLeft.Location = new Point(-1, 131);
            bannerBgLeft.Margin = new Padding(0);
            bannerBgLeft.Name = "bannerBgLeft";
            bannerBgLeft.Size = new Size(10, 22);
            bannerBgLeft.SizeMode = PictureBoxSizeMode.StretchImage;
            bannerBgLeft.TabIndex = 3;
            bannerBgLeft.TabStop = false;
            // 
            // bannerBgRight
            // 
            bannerBgRight.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            bannerBgRight.BackColor = Color.Black;
            bannerBgRight.Image = Datinate.Properties.Resources.media_assign_bg_right;
            bannerBgRight.Location = new Point(270, 131);
            bannerBgRight.Margin = new Padding(0);
            bannerBgRight.Name = "bannerBgRight";
            bannerBgRight.Size = new Size(10, 22);
            bannerBgRight.SizeMode = PictureBoxSizeMode.StretchImage;
            bannerBgRight.TabIndex = 7;
            bannerBgRight.TabStop = false;
            // 
            // Media2AssignUI
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            Controls.Add(bannerBgRight);
            Controls.Add(bannerBgLeft);
            Controls.Add(notFoundPic);
            Controls.Add(okPic);
            Controls.Add(outerContainer);
            Margin = new Padding(6);
            Name = "Media2AssignUI";
            Size = new Size(280, 302);
            MouseEnter += Media2AssignUI_MouseEnter;
            MouseLeave += Media2AssignUI_MouseLeave;
            ((System.ComponentModel.ISupportInitialize)mediaIconUI).EndInit();
            tabControl.ResumeLayout(false);
            mediaPage.ResumeLayout(false);
            mediaContainer.ResumeLayout(false);
            listPage.ResumeLayout(false);
            panel3.ResumeLayout(false);
            tableLayoutPanel.ResumeLayout(false);
            headerPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pageCurl).EndInit();
            ((System.ComponentModel.ISupportInitialize)notFoundPic).EndInit();
            ((System.ComponentModel.ISupportInitialize)okPic).EndInit();
            innerContainer.ResumeLayout(false);
            outerContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)bannerBgLeft).EndInit();
            ((System.ComponentModel.ISupportInitialize)bannerBgRight).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private MediaIconUI mediaIconUI;
        private DatChipUI datChipUI;
        private TabPage mediaPage;
        private TabPage listPage;
        private Panel mediaContainer;
        private Label mediaNameTextBox;
        private TableLayoutPanel tableLayoutPanel;
        private Panel headerPanel;
        private FastEntryListUI entryListUI;
        private Panel panel3;
        private MiniWebUI miniWebUI;
        private Media2AssignControlsUI assignControls;
        private PictureBox notFoundPic;
        private PictureBox okPic;
        private PictureBox pageCurl;
        private BorderlessTabControl tabControl;
        private Panel innerContainer;
        private Panel outerContainer;
        private PictureBox bannerBgLeft;
        private PictureBox bannerBgRight;
    }
}
