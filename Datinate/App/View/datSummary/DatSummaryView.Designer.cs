namespace datinate.app {
    partial class DatSummaryView {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
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
            listView = new CustomDetailsListView();
            tableLayoutPanel1 = new TableLayoutPanel();
            panel1 = new Panel();
            summaryLabel = new Label();
            panel2 = new Panel();
            headerLabel = new Label();
            filterUI = new FilterUI();
            tableLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // listView
            // 
            listView.AllowColumnReorder = true;
            listView.Dock = DockStyle.Fill;
            listView.FullRowSelect = true;
            listView.GridLines = true;
            listView.Location = new Point(5, 36);
            listView.Margin = new Padding(4, 3, 4, 3);
            listView.MultiSelect = false;
            listView.Name = "listView";
            listView.Size = new Size(573, 335);
            listView.TabIndex = 0;
            listView.UseCompatibleStateImageBehavior = false;
            listView.View = View.Details;
            listView.ColumnClick += onColumnClick;
            listView.ItemSelectionChanged += OnDatSelected;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(listView, 0, 1);
            tableLayoutPanel1.Controls.Add(panel1, 0, 2);
            tableLayoutPanel1.Controls.Add(panel2, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(4, 3, 4, 3);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
            tableLayoutPanel1.Size = new Size(583, 407);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // panel1
            // 
            panel1.Controls.Add(summaryLabel);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(5, 378);
            panel1.Margin = new Padding(4, 3, 4, 3);
            panel1.Name = "panel1";
            panel1.RightToLeft = RightToLeft.Yes;
            panel1.Size = new Size(573, 25);
            panel1.TabIndex = 2;
            // 
            // summaryLabel
            // 
            summaryLabel.Dock = DockStyle.Fill;
            summaryLabel.Location = new Point(0, 0);
            summaryLabel.Margin = new Padding(4, 0, 4, 0);
            summaryLabel.Name = "summaryLabel";
            summaryLabel.RightToLeft = RightToLeft.No;
            summaryLabel.Size = new Size(573, 25);
            summaryLabel.TabIndex = 0;
            summaryLabel.Text = "No DATs loaded";
            summaryLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panel2
            // 
            panel2.Controls.Add(headerLabel);
            panel2.Controls.Add(filterUI);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(5, 4);
            panel2.Margin = new Padding(4, 3, 4, 3);
            panel2.Name = "panel2";
            panel2.Size = new Size(573, 25);
            panel2.TabIndex = 3;
            // 
            // headerLabel
            // 
            headerLabel.Anchor = AnchorStyles.Left;
            headerLabel.AutoSize = true;
            headerLabel.Location = new Point(0, 4);
            headerLabel.Margin = new Padding(4, 0, 4, 0);
            headerLabel.Name = "headerLabel";
            headerLabel.Size = new Size(127, 15);
            headerLabel.TabIndex = 2;
            headerLabel.Text = "Select a DAT for Details";
            // 
            // filterUI
            // 
            filterUI.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            filterUI.Location = new Point(315, 0);
            filterUI.Margin = new Padding(5, 3, 5, 3);
            filterUI.Name = "filterUI";
            filterUI.Size = new Size(261, 24);
            filterUI.TabIndex = 3;
            // 
            // DatSummaryView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel1);
            Margin = new Padding(4, 3, 4, 3);
            Name = "DatSummaryView";
            Size = new Size(583, 407);
            tableLayoutPanel1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private CustomDetailsListView listView;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label headerLabel;
        private System.Windows.Forms.Label summaryLabel;
        private FilterUI filterUI;
    }
}
