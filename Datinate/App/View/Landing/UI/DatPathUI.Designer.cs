namespace com.RADIO.Datinate.view.datPaths.ui
{
    partial class DatPathUI
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
            pathLabel = new Label();
            button1 = new Button();
            referenceTextBox = new TextBox();
            SuspendLayout();
            // 
            // pathLabel
            // 
            pathLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pathLabel.AutoEllipsis = true;
            pathLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            pathLabel.Location = new Point(179, 6);
            pathLabel.Margin = new Padding(0);
            pathLabel.Name = "pathLabel";
            pathLabel.Size = new Size(380, 18);
            pathLabel.TabIndex = 1;
            pathLabel.Text = "[path value]";
            pathLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            button1.Cursor = Cursors.Hand;
            button1.Location = new Point(564, 3);
            button1.Margin = new Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new Size(74, 24);
            button1.TabIndex = 2;
            button1.Text = "Remove";
            button1.UseVisualStyleBackColor = false;
            button1.Click += onRemoveClick;
            // 
            // referenceTextBox
            // 
            referenceTextBox.BorderStyle = BorderStyle.FixedSingle;
            referenceTextBox.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            referenceTextBox.ForeColor = Color.FromArgb(33, 37, 41);
            referenceTextBox.Location = new Point(3, 3);
            referenceTextBox.Name = "referenceTextBox";
            referenceTextBox.PlaceholderText = "Reference Name...";
            referenceTextBox.Size = new Size(172, 23);
            referenceTextBox.TabIndex = 3;
            // 
            // DatPathUI
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(pathLabel);
            Controls.Add(referenceTextBox);
            Controls.Add(button1);
            Name = "DatPathUI";
            Size = new Size(641, 30);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label pathLabel;
        private System.Windows.Forms.Button button1;
        private TextBox referenceTextBox;
    }
}