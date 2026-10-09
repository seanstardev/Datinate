namespace datinate.app
{
    partial class ExportPriorityUI
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
            panel1 = new Panel();
            tableLayoutPanel1 = new TableLayoutPanel();
            panel4 = new Panel();
            flowLayoutPanel2 = new FlowLayoutPanel();
            label1 = new Label();
            panel3 = new Panel();
            includeContainer = new FlowLayoutPanel();
            label2 = new Label();
            scoringLabel = new Label();
            mediaItemUI = new MediaItemUI();
            panel1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            panel4.SuspendLayout();
            panel3.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(tableLayoutPanel1);
            panel1.Controls.Add(scoringLabel);
            panel1.Controls.Add(mediaItemUI);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(894, 240);
            panel1.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(panel4, 1, 0);
            tableLayoutPanel1.Controls.Add(panel3, 0, 0);
            tableLayoutPanel1.Location = new Point(0, 45);
            tableLayoutPanel1.Margin = new Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(893, 194);
            tableLayoutPanel1.TabIndex = 3;
            // 
            // panel4
            // 
            panel4.BorderStyle = BorderStyle.FixedSingle;
            panel4.Controls.Add(flowLayoutPanel2);
            panel4.Controls.Add(label1);
            panel4.Dock = DockStyle.Fill;
            panel4.Location = new Point(449, 3);
            panel4.Name = "panel4";
            panel4.Size = new Size(441, 188);
            panel4.TabIndex = 1;
            // 
            // flowLayoutPanel2
            // 
            flowLayoutPanel2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            flowLayoutPanel2.BackColor = Color.White;
            flowLayoutPanel2.Location = new Point(-1, 18);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.Size = new Size(441, 172);
            flowLayoutPanel2.TabIndex = 3;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(99, 15);
            label1.TabIndex = 0;
            label1.Text = "Excluded Sources";
            // 
            // panel3
            // 
            panel3.BorderStyle = BorderStyle.FixedSingle;
            panel3.Controls.Add(includeContainer);
            panel3.Controls.Add(label2);
            panel3.Dock = DockStyle.Fill;
            panel3.Location = new Point(3, 3);
            panel3.Name = "panel3";
            panel3.Size = new Size(440, 188);
            panel3.TabIndex = 0;
            // 
            // includeContainer
            // 
            includeContainer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            includeContainer.BackColor = Color.White;
            includeContainer.Location = new Point(-1, 18);
            includeContainer.Name = "includeContainer";
            includeContainer.Size = new Size(441, 169);
            includeContainer.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(3, 0);
            label2.Name = "label2";
            label2.Size = new Size(164, 15);
            label2.TabIndex = 1;
            label2.Text = "Preferred Sources (by Priority)";
            // 
            // scoringLabel
            // 
            scoringLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            scoringLabel.AutoSize = true;
            scoringLabel.BackColor = Color.LimeGreen;
            scoringLabel.BorderStyle = BorderStyle.FixedSingle;
            scoringLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            scoringLabel.ForeColor = Color.White;
            scoringLabel.Location = new Point(842, 0);
            scoringLabel.Name = "scoringLabel";
            scoringLabel.Size = new Size(51, 17);
            scoringLabel.TabIndex = 2;
            scoringLabel.Text = "Scoring";
            // 
            // mediaItemUI
            // 
            mediaItemUI.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            mediaItemUI.AutoSize = true;
            mediaItemUI.Location = new Point(-1, 0);
            mediaItemUI.MaximumSize = new Size(0, 48);
            mediaItemUI.MinimumSize = new Size(0, 48);
            mediaItemUI.Name = "mediaItemUI";
            mediaItemUI.Size = new Size(891, 48);
            mediaItemUI.TabIndex = 0;
            // 
            // ExportPriorityUI
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ControlLight;
            Controls.Add(panel1);
            Name = "ExportPriorityUI";
            Size = new Size(894, 240);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private MediaItemUI mediaItemUI;
        private Panel panel4;
        private Label label1;
        private Panel panel3;
        private Label label2;
        private FlowLayoutPanel flowLayoutPanel2;
        private FlowLayoutPanel includeContainer;
        private Label scoringLabel;
        private TableLayoutPanel tableLayoutPanel1;
    }
}
