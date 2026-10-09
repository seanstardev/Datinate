namespace datinate.app
{
    partial class ExportPriorityItemUI
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
            datChipUI = new DatChipUI();
            mediaLabel = new Label();
            panel1 = new Panel();
            indexLabel = new Label();
            closeRightBtn = new Button();
            flowLayoutPanel1 = new FlowLayoutPanel();
            panel2 = new Panel();
            button2 = new Button();
            button1 = new Button();
            entryCountLabel = new Label();
            panel1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // datChipUI
            // 
            datChipUI.Location = new Point(31, 2);
            datChipUI.Margin = new Padding(0);
            datChipUI.Name = "datChipUI";
            datChipUI.Size = new Size(75, 23);
            datChipUI.TabIndex = 0;
            datChipUI.TabStop = false;
            datChipUI.Text = "datChipui1";
            // 
            // mediaLabel
            // 
            mediaLabel.AutoSize = true;
            mediaLabel.Location = new Point(32, 25);
            mediaLabel.Name = "mediaLabel";
            mediaLabel.Size = new Size(38, 15);
            mediaLabel.TabIndex = 1;
            mediaLabel.Text = "label1";
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            panel1.BackColor = SystemColors.ActiveCaption;
            panel1.Controls.Add(indexLabel);
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(28, 45);
            panel1.TabIndex = 2;
            // 
            // indexLabel
            // 
            indexLabel.Dock = DockStyle.Fill;
            indexLabel.Location = new Point(0, 0);
            indexLabel.Name = "indexLabel";
            indexLabel.Size = new Size(28, 45);
            indexLabel.TabIndex = 0;
            indexLabel.Text = "12";
            indexLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // closeRightBtn
            // 
            closeRightBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            closeRightBtn.BackColor = Color.White;
            closeRightBtn.Cursor = Cursors.Hand;
            closeRightBtn.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            closeRightBtn.Location = new Point(43, 3);
            closeRightBtn.Name = "closeRightBtn";
            closeRightBtn.Padding = new Padding(2, 0, 0, 0);
            closeRightBtn.Size = new Size(34, 34);
            closeRightBtn.TabIndex = 6;
            closeRightBtn.Text = "✕";
            closeRightBtn.UseVisualStyleBackColor = false;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            flowLayoutPanel1.Controls.Add(closeRightBtn);
            flowLayoutPanel1.Controls.Add(panel2);
            flowLayoutPanel1.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanel1.Location = new Point(207, 0);
            flowLayoutPanel1.Margin = new Padding(0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(80, 45);
            flowLayoutPanel1.TabIndex = 7;
            // 
            // panel2
            // 
            panel2.Controls.Add(button2);
            panel2.Controls.Add(button1);
            panel2.Location = new Point(3, 3);
            panel2.Name = "panel2";
            panel2.Size = new Size(34, 41);
            panel2.TabIndex = 8;
            // 
            // button2
            // 
            button2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button2.BackColor = Color.White;
            button2.Cursor = Cursors.Hand;
            button2.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            button2.Location = new Point(0, 20);
            button2.Name = "button2";
            button2.Padding = new Padding(2, 0, 0, 0);
            button2.Size = new Size(34, 20);
            button2.TabIndex = 8;
            button2.Text = "Down";
            button2.UseVisualStyleBackColor = false;
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button1.BackColor = Color.White;
            button1.Cursor = Cursors.Hand;
            button1.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            button1.Location = new Point(0, 0);
            button1.Name = "button1";
            button1.Padding = new Padding(2, 0, 0, 0);
            button1.Size = new Size(34, 20);
            button1.TabIndex = 7;
            button1.Text = "Up";
            button1.UseVisualStyleBackColor = false;
            // 
            // entryCountLabel
            // 
            entryCountLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            entryCountLabel.Location = new Point(76, 25);
            entryCountLabel.Name = "entryCountLabel";
            entryCountLabel.Size = new Size(128, 15);
            entryCountLabel.TabIndex = 8;
            entryCountLabel.Text = "sfsf asfs fs f sfas";
            entryCountLabel.TextAlign = ContentAlignment.TopRight;
            // 
            // ExportPriorityItemUI
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(flowLayoutPanel1);
            Controls.Add(panel1);
            Controls.Add(mediaLabel);
            Controls.Add(datChipUI);
            Controls.Add(entryCountLabel);
            Name = "ExportPriorityItemUI";
            Size = new Size(287, 45);
            panel1.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DatChipUI datChipUI;
        private Label mediaLabel;
        private Panel panel1;
        private Label indexLabel;
        private Button closeRightBtn;
        private FlowLayoutPanel flowLayoutPanel1;
        private Panel panel2;
        private Button button2;
        private Button button1;
        private Label entryCountLabel;
    }
}
