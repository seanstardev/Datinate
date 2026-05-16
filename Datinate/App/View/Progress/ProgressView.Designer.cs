namespace datinate.app
{
    partial class ProgressView
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
            if (disposing)
                HandleDisposing();
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
            progressMessage = new Label();
            progressBar = new ColouredProgressBar();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(progressMessage);
            panel1.Controls.Add(progressBar);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Margin = new Padding(4, 3, 4, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(919, 74);
            panel1.TabIndex = 2;
            // 
            // progressMessage
            // 
            progressMessage.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            progressMessage.AutoSize = true;
            progressMessage.Location = new Point(4, 56);
            progressMessage.Margin = new Padding(4, 0, 4, 0);
            progressMessage.Name = "progressMessage";
            progressMessage.Size = new Size(39, 15);
            progressMessage.TabIndex = 2;
            progressMessage.Text = "Ready";
            progressMessage.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.BackColor = Color.Transparent;
            progressBar.FillEndColor = Color.MediumSeaGreen;
            progressBar.FillStartColor = SystemColors.GradientInactiveCaption;
            progressBar.ForeColor = SystemColors.GradientInactiveCaption;
            progressBar.Location = new Point(0, 0);
            progressBar.Margin = new Padding(4, 3, 4, 3);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(919, 53);
            progressBar.TabIndex = 1;
            // 
            // ProgressView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(panel1);
            Name = "ProgressView";
            Size = new Size(919, 74);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Panel panel1;
        private Label progressMessage;
        private ColouredProgressBar progressBar;
    }
}
