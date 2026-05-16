namespace datinate.app {
    partial class DatPathUpdateUI {
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
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            oldPathLabel = new System.Windows.Forms.Label();
            newPathLabel = new System.Windows.Forms.Label();
            newPathBtn = new System.Windows.Forms.Button();
            oldDatFilenameLabel = new System.Windows.Forms.Label();
            groupBox1 = new System.Windows.Forms.GroupBox();
            groupBox2 = new System.Windows.Forms.GroupBox();
            quickFindBtn = new System.Windows.Forms.LinkLabel();
            datTypeLabel = new System.Windows.Forms.Label();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // oldPathLabel
            // 
            oldPathLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            oldPathLabel.BackColor = System.Drawing.Color.White;
            oldPathLabel.Location = new System.Drawing.Point(7, 44);
            oldPathLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            oldPathLabel.Name = "oldPathLabel";
            oldPathLabel.Size = new System.Drawing.Size(590, 33);
            oldPathLabel.TabIndex = 1;
            oldPathLabel.Text = "line1\r\nline2";
            // 
            // newPathLabel
            // 
            newPathLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            newPathLabel.BackColor = System.Drawing.Color.White;
            newPathLabel.Location = new System.Drawing.Point(7, 52);
            newPathLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            newPathLabel.Name = "newPathLabel";
            newPathLabel.Size = new System.Drawing.Size(583, 33);
            newPathLabel.TabIndex = 2;
            newPathLabel.Text = "line1\r\nline2";
            // 
            // newPathBtn
            // 
            newPathBtn.Location = new System.Drawing.Point(7, 22);
            newPathBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            newPathBtn.Name = "newPathBtn";
            newPathBtn.Size = new System.Drawing.Size(88, 27);
            newPathBtn.TabIndex = 3;
            newPathBtn.Text = "New Path";
            newPathBtn.UseVisualStyleBackColor = true;
            newPathBtn.Click += NewPathBtn_Click;
            // 
            // oldDatFilenameLabel
            // 
            oldDatFilenameLabel.AutoSize = true;
            oldDatFilenameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            oldDatFilenameLabel.Location = new System.Drawing.Point(5, 20);
            oldDatFilenameLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            oldDatFilenameLabel.Name = "oldDatFilenameLabel";
            oldDatFilenameLabel.Size = new System.Drawing.Size(49, 13);
            oldDatFilenameLabel.TabIndex = 4;
            oldDatFilenameLabel.Text = "Filename";
            // 
            // groupBox1
            // 
            groupBox1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            groupBox1.Controls.Add(oldPathLabel);
            groupBox1.Controls.Add(oldDatFilenameLabel);
            groupBox1.Location = new System.Drawing.Point(4, 39);
            groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Size = new System.Drawing.Size(604, 89);
            groupBox1.TabIndex = 5;
            groupBox1.TabStop = false;
            groupBox1.Text = "Old File";
            // 
            // groupBox2
            // 
            groupBox2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            groupBox2.Controls.Add(quickFindBtn);
            groupBox2.Controls.Add(newPathBtn);
            groupBox2.Controls.Add(newPathLabel);
            groupBox2.Location = new System.Drawing.Point(4, 135);
            groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox2.Size = new System.Drawing.Size(604, 92);
            groupBox2.TabIndex = 6;
            groupBox2.TabStop = false;
            groupBox2.Text = "New File";
            // 
            // quickFindBtn
            // 
            quickFindBtn.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            quickFindBtn.Location = new System.Drawing.Point(481, 18);
            quickFindBtn.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            quickFindBtn.Name = "quickFindBtn";
            quickFindBtn.Size = new System.Drawing.Size(117, 27);
            quickFindBtn.TabIndex = 4;
            quickFindBtn.TabStop = true;
            quickFindBtn.Text = "Quick Find";
            quickFindBtn.TextAlign = System.Drawing.ContentAlignment.TopRight;
            quickFindBtn.LinkClicked += quickFindBtn_LinkClicked;
            // 
            // datTypeLabel
            // 
            datTypeLabel.AutoSize = true;
            datTypeLabel.Location = new System.Drawing.Point(10, 10);
            datTypeLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            datTypeLabel.Name = "datTypeLabel";
            datTypeLabel.Size = new System.Drawing.Size(58, 15);
            datTypeLabel.TabIndex = 7;
            datTypeLabel.Text = "DAT Type:";
            // 
            // DatPathUpdateUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            Controls.Add(datTypeLabel);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "DatPathUpdateUI";
            Size = new System.Drawing.Size(615, 227);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Label oldPathLabel;
        private System.Windows.Forms.Label newPathLabel;
        private System.Windows.Forms.Button newPathBtn;
        private System.Windows.Forms.Label oldDatFilenameLabel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label datTypeLabel;
        private System.Windows.Forms.LinkLabel quickFindBtn;
    }
}
