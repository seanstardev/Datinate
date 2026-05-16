namespace datinate.app
{
    partial class Media2View
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
            tableLayoutPanel1 = new TableLayoutPanel();
            titleAreaContainer = new Panel();
            closeRightBtn = new Button();
            closeLeftBtn = new Button();
            tableLayoutPanel2 = new TableLayoutPanel();
            titleUI = new DatGrouperTitleUI();
            filler = new Panel();
            dragDropContainer = new Panel();
            MaskUI = new Panel();
            tableLayoutPanel3 = new TableLayoutPanel();
            loadingSpinnerPic = new PictureBox();
            mediaContainer = new CenteredFlowLayoutPanel();
            tableLayoutPanel1.SuspendLayout();
            titleAreaContainer.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            dragDropContainer.SuspendLayout();
            MaskUI.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)loadingSpinnerPic).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(titleAreaContainer, 0, 0);
            tableLayoutPanel1.Controls.Add(dragDropContainer, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(587, 551);
            tableLayoutPanel1.TabIndex = 3;
            // 
            // titleAreaContainer
            // 
            titleAreaContainer.Controls.Add(closeRightBtn);
            titleAreaContainer.Controls.Add(closeLeftBtn);
            titleAreaContainer.Controls.Add(tableLayoutPanel2);
            titleAreaContainer.Dock = DockStyle.Fill;
            titleAreaContainer.Location = new Point(0, 0);
            titleAreaContainer.Margin = new Padding(0);
            titleAreaContainer.Name = "titleAreaContainer";
            titleAreaContainer.Size = new Size(587, 36);
            titleAreaContainer.TabIndex = 1;
            // 
            // closeRightBtn
            // 
            closeRightBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            closeRightBtn.BackColor = Color.White;
            closeRightBtn.Cursor = Cursors.Hand;
            closeRightBtn.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            closeRightBtn.Location = new Point(553, 0);
            closeRightBtn.Name = "closeRightBtn";
            closeRightBtn.Padding = new Padding(2, 0, 0, 0);
            closeRightBtn.Size = new Size(34, 34);
            closeRightBtn.TabIndex = 5;
            closeRightBtn.Text = "✕";
            closeRightBtn.UseVisualStyleBackColor = false;
            closeRightBtn.Click += closeBtn_Click;
            // 
            // closeLeftBtn
            // 
            closeLeftBtn.BackColor = Color.White;
            closeLeftBtn.Cursor = Cursors.Hand;
            closeLeftBtn.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            closeLeftBtn.Location = new Point(0, 0);
            closeLeftBtn.Name = "closeLeftBtn";
            closeLeftBtn.Padding = new Padding(2, 0, 0, 0);
            closeLeftBtn.Size = new Size(34, 34);
            closeLeftBtn.TabIndex = 0;
            closeLeftBtn.Text = "✕";
            closeLeftBtn.UseVisualStyleBackColor = false;
            closeLeftBtn.Click += closeBtn_Click;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.BackColor = Color.Black;
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(titleUI, 0, 0);
            tableLayoutPanel2.Controls.Add(filler, 0, 1);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(0, 0);
            tableLayoutPanel2.Margin = new Padding(0);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
            tableLayoutPanel2.Size = new Size(587, 36);
            tableLayoutPanel2.TabIndex = 1;
            // 
            // titleUI
            // 
            titleUI.Anchor = AnchorStyles.None;
            titleUI.BackColor = Color.Transparent;
            titleUI.CornerRadius = 2;
            titleUI.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            titleUI.Location = new Point(207, 0);
            titleUI.Margin = new Padding(0);
            titleUI.Name = "titleUI";
            titleUI.Size = new Size(172, 35);
            titleUI.TabIndex = 0;
            titleUI.TabStop = false;
            titleUI.Text = "Media";
            // 
            // filler
            // 
            filler.BackColor = SystemColors.ControlDarkDark;
            filler.Dock = DockStyle.Fill;
            filler.Location = new Point(0, 35);
            filler.Margin = new Padding(0);
            filler.Name = "filler";
            filler.Size = new Size(587, 1);
            filler.TabIndex = 1;
            // 
            // dragDropContainer
            // 
            dragDropContainer.BackColor = Color.Black;
            dragDropContainer.Controls.Add(MaskUI);
            dragDropContainer.Controls.Add(mediaContainer);
            dragDropContainer.Dock = DockStyle.Fill;
            dragDropContainer.Location = new Point(0, 36);
            dragDropContainer.Margin = new Padding(0);
            dragDropContainer.Name = "dragDropContainer";
            dragDropContainer.Size = new Size(587, 515);
            dragDropContainer.TabIndex = 0;
            // 
            // MaskUI
            // 
            MaskUI.BackColor = Color.Black;
            MaskUI.Controls.Add(tableLayoutPanel3);
            MaskUI.Dock = DockStyle.Fill;
            MaskUI.Location = new Point(0, 0);
            MaskUI.Margin = new Padding(0);
            MaskUI.Name = "MaskUI";
            MaskUI.Size = new Size(587, 515);
            MaskUI.TabIndex = 0;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 1;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tableLayoutPanel3.Controls.Add(loadingSpinnerPic, 0, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(0, 0);
            tableLayoutPanel3.Margin = new Padding(0);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 1;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Size = new Size(587, 515);
            tableLayoutPanel3.TabIndex = 1;
            // 
            // loadingSpinnerPic
            // 
            loadingSpinnerPic.Anchor = AnchorStyles.None;
            loadingSpinnerPic.Location = new Point(243, 207);
            loadingSpinnerPic.Name = "loadingSpinnerPic";
            loadingSpinnerPic.Size = new Size(100, 100);
            loadingSpinnerPic.TabIndex = 0;
            loadingSpinnerPic.TabStop = false;
            // 
            // mediaContainer
            // 
            mediaContainer.Dock = DockStyle.Fill;
            mediaContainer.Location = new Point(0, 0);
            mediaContainer.Name = "mediaContainer";
            mediaContainer.Size = new Size(587, 515);
            mediaContainer.TabIndex = 1;
            // 
            // Media2View
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel1);
            Margin = new Padding(0);
            Name = "Media2View";
            Size = new Size(587, 551);
            tableLayoutPanel1.ResumeLayout(false);
            titleAreaContainer.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            dragDropContainer.ResumeLayout(false);
            MaskUI.ResumeLayout(false);
            tableLayoutPanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)loadingSpinnerPic).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private Panel dragDropContainer;
        private Panel titleAreaContainer;
        private TableLayoutPanel tableLayoutPanel2;
        private DatGrouperTitleUI titleUI;
        private Panel filler;
        private Panel MaskUI;
        private CenteredFlowLayoutPanel mediaContainer;
        private TableLayoutPanel tableLayoutPanel3;
        private PictureBox loadingSpinnerPic;
        private Button closeLeftBtn;
        private Button closeRightBtn;
    }
}
