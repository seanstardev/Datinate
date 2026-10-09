namespace datinate.app
{
    partial class DatGrouperSearchUI
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
            searchStartsWithCheckBox = new CheckBox();
            clearSearchBtn2 = new Button();
            countLabel = new Label();
            searchTextBox = new TextBox();
            searchLeftBtn = new Button();
            searchRightBtn = new Button();
            SuspendLayout();
            // 
            // searchStartsWithCheckBox
            // 
            searchStartsWithCheckBox.AutoSize = true;
            searchStartsWithCheckBox.Location = new Point(8, 4);
            searchStartsWithCheckBox.Name = "searchStartsWithCheckBox";
            searchStartsWithCheckBox.Size = new Size(81, 19);
            searchStartsWithCheckBox.TabIndex = 33;
            searchStartsWithCheckBox.Text = "Starts with";
            searchStartsWithCheckBox.UseVisualStyleBackColor = true;
            searchStartsWithCheckBox.CheckedChanged += searchStartsWithCheckBox_CheckedChanged;
            // 
            // clearSearchBtn2
            // 
            clearSearchBtn2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            clearSearchBtn2.FlatAppearance.BorderSize = 0;
            clearSearchBtn2.FlatStyle = FlatStyle.Popup;
            clearSearchBtn2.Location = new Point(839, 0);
            clearSearchBtn2.Name = "clearSearchBtn2";
            clearSearchBtn2.Size = new Size(50, 25);
            clearSearchBtn2.TabIndex = 32;
            clearSearchBtn2.Text = "Clear";
            clearSearchBtn2.UseVisualStyleBackColor = false;
            clearSearchBtn2.Click += clearSearchBtn_Click;
            // 
            // countLabel
            // 
            countLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            countLabel.AutoSize = true;
            countLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            countLabel.Location = new Point(950, 4);
            countLabel.Name = "countLabel";
            countLabel.Size = new Size(13, 17);
            countLabel.TabIndex = 31;
            countLabel.Text = "-";
            // 
            // searchTextBox
            // 
            searchTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            searchTextBox.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            searchTextBox.Location = new Point(90, 0);
            searchTextBox.Name = "searchTextBox";
            searchTextBox.Size = new Size(750, 25);
            searchTextBox.TabIndex = 28;
            searchTextBox.TextChanged += searchTxt_TextChanged;
            searchTextBox.KeyUp += searchTxt_KeyUp;
            searchTextBox.MouseDown += searchTxt_MouseDown;
            searchTextBox.MouseUp += searchTxt_MouseUp;
            // 
            // searchLeftBtn
            // 
            searchLeftBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            searchLeftBtn.Location = new Point(895, -1);
            searchLeftBtn.Name = "searchLeftBtn";
            searchLeftBtn.Size = new Size(27, 27);
            searchLeftBtn.TabIndex = 29;
            searchLeftBtn.Text = "<";
            searchLeftBtn.UseVisualStyleBackColor = true;
            searchLeftBtn.Click += searchLeftBtn_Click;
            // 
            // searchRightBtn
            // 
            searchRightBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            searchRightBtn.Location = new Point(922, -1);
            searchRightBtn.Name = "searchRightBtn";
            searchRightBtn.Size = new Size(27, 27);
            searchRightBtn.TabIndex = 30;
            searchRightBtn.Text = ">";
            searchRightBtn.UseVisualStyleBackColor = true;
            searchRightBtn.Click += searchRightBtn_Click;
            // 
            // DatGrouperSearchUI
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(clearSearchBtn2);
            Controls.Add(countLabel);
            Controls.Add(searchTextBox);
            Controls.Add(searchLeftBtn);
            Controls.Add(searchRightBtn);
            Controls.Add(searchStartsWithCheckBox);
            Name = "DatGrouperSearchUI";
            Size = new Size(1036, 29);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckBox searchStartsWithCheckBox;
        private Button clearSearchBtn2;
        private Label countLabel;
        private TextBox searchTextBox;
        private Button searchLeftBtn;
        private Button searchRightBtn;
    }
}
