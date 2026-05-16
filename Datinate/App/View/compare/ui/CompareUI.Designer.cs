namespace datinate.app
{
    partial class CompareUI
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
            tableLayoutPanel5 = new TableLayoutPanel();
            splitContainer3 = new SplitContainer();
            tableLayoutPanel2 = new TableLayoutPanel();
            panel1 = new Panel();
            nameLabel = new Label();
            panel2 = new Panel();
            romListSummary = new Label();
            gameList = new CustomDetailsListView();
            summaryList = new CustomDetailsListView();
            panel9 = new Panel();
            customiseDatBtn = new Button();
            createDatBtn = new Button();
            romSummaryLabel = new Label();
            tableLayoutPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer3).BeginInit();
            splitContainer3.Panel1.SuspendLayout();
            splitContainer3.Panel2.SuspendLayout();
            splitContainer3.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            panel9.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel5
            // 
            tableLayoutPanel5.ColumnCount = 1;
            tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel5.Controls.Add(splitContainer3, 0, 0);
            tableLayoutPanel5.Controls.Add(panel9, 0, 1);
            tableLayoutPanel5.Dock = DockStyle.Fill;
            tableLayoutPanel5.Location = new Point(0, 0);
            tableLayoutPanel5.Margin = new Padding(4, 3, 4, 3);
            tableLayoutPanel5.Name = "tableLayoutPanel5";
            tableLayoutPanel5.RowCount = 2;
            tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Absolute, 41F));
            tableLayoutPanel5.Size = new Size(271, 470);
            tableLayoutPanel5.TabIndex = 2;
            // 
            // splitContainer3
            // 
            splitContainer3.Dock = DockStyle.Fill;
            splitContainer3.Location = new Point(4, 3);
            splitContainer3.Margin = new Padding(4, 3, 4, 3);
            splitContainer3.Name = "splitContainer3";
            splitContainer3.Orientation = Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            splitContainer3.Panel1.Controls.Add(tableLayoutPanel2);
            // 
            // splitContainer3.Panel2
            // 
            splitContainer3.Panel2.Controls.Add(summaryList);
            splitContainer3.Size = new Size(263, 423);
            splitContainer3.SplitterDistance = 251;
            splitContainer3.SplitterWidth = 5;
            splitContainer3.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(panel1, 0, 0);
            tableLayoutPanel2.Controls.Add(panel2, 0, 2);
            tableLayoutPanel2.Controls.Add(gameList, 0, 1);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(0, 0);
            tableLayoutPanel2.Margin = new Padding(4, 3, 4, 3);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 3;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
            tableLayoutPanel2.Size = new Size(263, 251);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.Controls.Add(nameLabel);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(4, 3);
            panel1.Margin = new Padding(4, 3, 4, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(255, 25);
            panel1.TabIndex = 0;
            // 
            // nameLabel
            // 
            nameLabel.AutoSize = true;
            nameLabel.Location = new Point(5, 5);
            nameLabel.Margin = new Padding(4, 0, 4, 0);
            nameLabel.Name = "nameLabel";
            nameLabel.Size = new Size(22, 15);
            nameLabel.TabIndex = 0;
            nameLabel.Text = "---";
            // 
            // panel2
            // 
            panel2.Controls.Add(romListSummary);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(4, 223);
            panel2.Margin = new Padding(4, 3, 4, 3);
            panel2.Name = "panel2";
            panel2.Size = new Size(255, 25);
            panel2.TabIndex = 1;
            // 
            // romListSummary
            // 
            romListSummary.AutoSize = true;
            romListSummary.Location = new Point(5, 5);
            romListSummary.Margin = new Padding(4, 0, 4, 0);
            romListSummary.Name = "romListSummary";
            romListSummary.Size = new Size(22, 15);
            romListSummary.TabIndex = 0;
            romListSummary.Text = "---";
            // 
            // gameList
            // 
            gameList.AllowColumnReorder = true;
            gameList.Dock = DockStyle.Fill;
            gameList.FullRowSelect = true;
            gameList.GridLines = true;
            gameList.Location = new Point(4, 34);
            gameList.Margin = new Padding(4, 3, 4, 3);
            gameList.Name = "gameList";
            gameList.Size = new Size(255, 183);
            gameList.TabIndex = 2;
            gameList.UseCompatibleStateImageBehavior = false;
            gameList.View = View.Details;
            gameList.ColumnClick += onColumnClick;
            // 
            // summaryList
            // 
            summaryList.AllowColumnReorder = true;
            summaryList.Dock = DockStyle.Fill;
            summaryList.FullRowSelect = true;
            summaryList.GridLines = true;
            summaryList.Location = new Point(0, 0);
            summaryList.Margin = new Padding(4, 3, 4, 3);
            summaryList.Name = "summaryList";
            summaryList.Size = new Size(263, 167);
            summaryList.TabIndex = 0;
            summaryList.UseCompatibleStateImageBehavior = false;
            summaryList.View = View.Details;
            summaryList.ColumnClick += onColumnClick;
            // 
            // panel9
            // 
            panel9.BorderStyle = BorderStyle.FixedSingle;
            panel9.Controls.Add(customiseDatBtn);
            panel9.Controls.Add(createDatBtn);
            panel9.Controls.Add(romSummaryLabel);
            panel9.Dock = DockStyle.Fill;
            panel9.Location = new Point(4, 432);
            panel9.Margin = new Padding(4, 3, 4, 3);
            panel9.Name = "panel9";
            panel9.Size = new Size(263, 35);
            panel9.TabIndex = 1;
            // 
            // customiseDatBtn
            // 
            customiseDatBtn.Location = new Point(100, 3);
            customiseDatBtn.Margin = new Padding(4, 3, 4, 3);
            customiseDatBtn.Name = "customiseDatBtn";
            customiseDatBtn.Size = new Size(106, 27);
            customiseDatBtn.TabIndex = 2;
            customiseDatBtn.Text = "Customise DAT";
            customiseDatBtn.UseVisualStyleBackColor = true;
            customiseDatBtn.Click += onCustomise;
            // 
            // createDatBtn
            // 
            createDatBtn.Location = new Point(4, 3);
            createDatBtn.Margin = new Padding(4, 3, 4, 3);
            createDatBtn.Name = "createDatBtn";
            createDatBtn.Size = new Size(88, 27);
            createDatBtn.TabIndex = 1;
            createDatBtn.Text = "Create DAT";
            createDatBtn.UseVisualStyleBackColor = true;
            createDatBtn.Click += createDatBtn_Click;
            // 
            // romSummaryLabel
            // 
            romSummaryLabel.AutoSize = true;
            romSummaryLabel.Location = new Point(214, 9);
            romSummaryLabel.Margin = new Padding(4, 0, 4, 0);
            romSummaryLabel.Name = "romSummaryLabel";
            romSummaryLabel.Size = new Size(22, 15);
            romSummaryLabel.TabIndex = 0;
            romSummaryLabel.Text = "---";
            // 
            // CompareUI
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel5);
            Margin = new Padding(4, 3, 4, 3);
            Name = "CompareUI";
            Size = new Size(271, 470);
            tableLayoutPanel5.ResumeLayout(false);
            splitContainer3.Panel1.ResumeLayout(false);
            splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer3).EndInit();
            splitContainer3.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel9.ResumeLayout(false);
            panel9.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private CustomDetailsListView gameList;
        private CustomDetailsListView summaryList;
        private System.Windows.Forms.Panel panel9;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.Label romSummaryLabel;
        private System.Windows.Forms.Label romListSummary;
        private System.Windows.Forms.Button createDatBtn;
        private System.Windows.Forms.Button customiseDatBtn;
    }
}