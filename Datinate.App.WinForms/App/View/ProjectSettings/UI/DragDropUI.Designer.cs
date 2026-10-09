namespace datinate.app
{
    partial class DragDropUI
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
            tableLayoutPanel1 = new TableLayoutPanel();
            selectedPanel = new FlowLayoutPanel();
            allPanel = new FlowLayoutPanel();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(selectedPanel, 1, 0);
            tableLayoutPanel1.Controls.Add(allPanel, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(883, 447);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // selectedPanel
            // 
            selectedPanel.BorderStyle = BorderStyle.FixedSingle;
            selectedPanel.Dock = DockStyle.Fill;
            selectedPanel.Location = new Point(447, 6);
            selectedPanel.Margin = new Padding(6);
            selectedPanel.Name = "selectedPanel";
            selectedPanel.Size = new Size(430, 435);
            selectedPanel.TabIndex = 1;
            // 
            // allPanel
            // 
            allPanel.BorderStyle = BorderStyle.FixedSingle;
            allPanel.Dock = DockStyle.Fill;
            allPanel.Location = new Point(6, 6);
            allPanel.Margin = new Padding(6);
            allPanel.Name = "allPanel";
            allPanel.Size = new Size(429, 435);
            allPanel.TabIndex = 0;
            // 
            // DragDropUI
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(tableLayoutPanel1);
            Name = "DragDropUI";
            Size = new Size(883, 447);
            tableLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private FlowLayoutPanel selectedPanel;
        private FlowLayoutPanel allPanel;
    }
}
