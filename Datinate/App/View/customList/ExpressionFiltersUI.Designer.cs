namespace Datinate.App.View.customList
{
    partial class ExpressionFiltersUI
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
            expressionFiltersGroupBox = new GroupBox();
            tableLayoutPanel8 = new TableLayoutPanel();
            expressionListView_lv = new ListView();
            panel3 = new Panel();
            loadExpressionsBtn = new Button();
            saveExpressionsBtn = new Button();
            deleteExpression_btn = new Button();
            moveUpExpression_btn = new Button();
            moveDownExpression_btn = new Button();
            expressionFiltersGroupBox.SuspendLayout();
            tableLayoutPanel8.SuspendLayout();
            panel3.SuspendLayout();
            SuspendLayout();
            // 
            // expressionFiltersGroupBox
            // 
            expressionFiltersGroupBox.Controls.Add(tableLayoutPanel8);
            expressionFiltersGroupBox.Dock = DockStyle.Fill;
            expressionFiltersGroupBox.Location = new Point(0, 0);
            expressionFiltersGroupBox.Margin = new Padding(4, 3, 4, 3);
            expressionFiltersGroupBox.Name = "expressionFiltersGroupBox";
            expressionFiltersGroupBox.Padding = new Padding(4, 3, 4, 3);
            expressionFiltersGroupBox.Size = new Size(502, 299);
            expressionFiltersGroupBox.TabIndex = 25;
            expressionFiltersGroupBox.TabStop = false;
            expressionFiltersGroupBox.Text = "Expression Filters";
            // 
            // tableLayoutPanel8
            // 
            tableLayoutPanel8.ColumnCount = 1;
            tableLayoutPanel8.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel8.Controls.Add(expressionListView_lv, 0, 0);
            tableLayoutPanel8.Controls.Add(panel3, 0, 1);
            tableLayoutPanel8.Dock = DockStyle.Fill;
            tableLayoutPanel8.Location = new Point(4, 19);
            tableLayoutPanel8.Margin = new Padding(4, 3, 4, 3);
            tableLayoutPanel8.Name = "tableLayoutPanel8";
            tableLayoutPanel8.RowCount = 2;
            tableLayoutPanel8.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel8.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
            tableLayoutPanel8.Size = new Size(494, 277);
            tableLayoutPanel8.TabIndex = 31;
            // 
            // expressionListView_lv
            // 
            expressionListView_lv.Dock = DockStyle.Fill;
            expressionListView_lv.FullRowSelect = true;
            expressionListView_lv.GridLines = true;
            expressionListView_lv.ImeMode = ImeMode.NoControl;
            expressionListView_lv.Location = new Point(4, 3);
            expressionListView_lv.Margin = new Padding(4, 3, 4, 3);
            expressionListView_lv.MultiSelect = false;
            expressionListView_lv.Name = "expressionListView_lv";
            expressionListView_lv.Size = new Size(486, 240);
            expressionListView_lv.TabIndex = 4;
            expressionListView_lv.UseCompatibleStateImageBehavior = false;
            expressionListView_lv.View = System.Windows.Forms.View.Details;
            // 
            // panel3
            // 
            panel3.Controls.Add(loadExpressionsBtn);
            panel3.Controls.Add(saveExpressionsBtn);
            panel3.Controls.Add(deleteExpression_btn);
            panel3.Controls.Add(moveUpExpression_btn);
            panel3.Controls.Add(moveDownExpression_btn);
            panel3.Dock = DockStyle.Fill;
            panel3.Location = new Point(4, 249);
            panel3.Margin = new Padding(4, 3, 4, 3);
            panel3.Name = "panel3";
            panel3.Size = new Size(486, 25);
            panel3.TabIndex = 0;
            // 
            // loadExpressionsBtn
            // 
            loadExpressionsBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            loadExpressionsBtn.Font = new Font("Microsoft Sans Serif", 6.75F);
            loadExpressionsBtn.Location = new Point(440, -1);
            loadExpressionsBtn.Margin = new Padding(4, 3, 4, 3);
            loadExpressionsBtn.Name = "loadExpressionsBtn";
            loadExpressionsBtn.Size = new Size(42, 27);
            loadExpressionsBtn.TabIndex = 17;
            loadExpressionsBtn.Text = "Load";
            loadExpressionsBtn.UseVisualStyleBackColor = true;
            loadExpressionsBtn.Click += loadExpressionsBtn_Click;
            // 
            // saveExpressionsBtn
            // 
            saveExpressionsBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            saveExpressionsBtn.Font = new Font("Microsoft Sans Serif", 6.75F);
            saveExpressionsBtn.Location = new Point(385, -1);
            saveExpressionsBtn.Margin = new Padding(4, 3, 4, 3);
            saveExpressionsBtn.Name = "saveExpressionsBtn";
            saveExpressionsBtn.Size = new Size(47, 27);
            saveExpressionsBtn.TabIndex = 16;
            saveExpressionsBtn.Text = "Save";
            saveExpressionsBtn.UseVisualStyleBackColor = true;
            saveExpressionsBtn.Click += saveExpressionsBtn_Click;
            // 
            // deleteExpression_btn
            // 
            deleteExpression_btn.Font = new Font("Microsoft Sans Serif", 6.75F);
            deleteExpression_btn.Location = new Point(111, -1);
            deleteExpression_btn.Margin = new Padding(4, 3, 4, 3);
            deleteExpression_btn.Name = "deleteExpression_btn";
            deleteExpression_btn.Size = new Size(47, 27);
            deleteExpression_btn.TabIndex = 11;
            deleteExpression_btn.Text = "Delete";
            deleteExpression_btn.UseVisualStyleBackColor = true;
            deleteExpression_btn.Click += deleteExpression_btn_Click;
            // 
            // moveUpExpression_btn
            // 
            moveUpExpression_btn.Font = new Font("Microsoft Sans Serif", 6.75F);
            moveUpExpression_btn.Location = new Point(4, -1);
            moveUpExpression_btn.Margin = new Padding(4, 3, 4, 3);
            moveUpExpression_btn.Name = "moveUpExpression_btn";
            moveUpExpression_btn.Size = new Size(47, 27);
            moveUpExpression_btn.TabIndex = 14;
            moveUpExpression_btn.Text = "Up";
            moveUpExpression_btn.UseVisualStyleBackColor = true;
            moveUpExpression_btn.Click += moveUpExpression_btn_Click;
            // 
            // moveDownExpression_btn
            // 
            moveDownExpression_btn.Font = new Font("Microsoft Sans Serif", 6.75F);
            moveDownExpression_btn.Location = new Point(57, -1);
            moveDownExpression_btn.Margin = new Padding(4, 3, 4, 3);
            moveDownExpression_btn.Name = "moveDownExpression_btn";
            moveDownExpression_btn.Size = new Size(47, 27);
            moveDownExpression_btn.TabIndex = 15;
            moveDownExpression_btn.Text = "Down";
            moveDownExpression_btn.UseVisualStyleBackColor = true;
            moveDownExpression_btn.Click += moveDownExpression_btn_Click;
            // 
            // ExpressionFiltersUI
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(expressionFiltersGroupBox);
            Name = "ExpressionFiltersUI";
            Size = new Size(502, 299);
            expressionFiltersGroupBox.ResumeLayout(false);
            tableLayoutPanel8.ResumeLayout(false);
            panel3.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private GroupBox expressionFiltersGroupBox;
        private TableLayoutPanel tableLayoutPanel8;
        private ListView expressionListView_lv;
        private Panel panel3;
        private Button loadExpressionsBtn;
        private Button saveExpressionsBtn;
        private Button deleteExpression_btn;
        private Button moveUpExpression_btn;
        private Button moveDownExpression_btn;
    }
}
