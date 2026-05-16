namespace datinate.app {
    partial class CompareForm {
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CompareForm));
            CompareView = new com.RADIO.Datinate.App.View.compare.CompareView();
            SuspendLayout();
            // 
            // compareView
            // 
            CompareView.Dock = System.Windows.Forms.DockStyle.Fill;
            CompareView.Location = new System.Drawing.Point(0, 0);
            CompareView.Name = "compareView";
            CompareView.Size = new System.Drawing.Size(1030, 456);
            CompareView.TabIndex = 0;
            // 
            // CompareForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1030, 456);
            Controls.Add(CompareView);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "CompareForm";
            Text = "DAT Compare";
            FormClosing += onFormClosing;
            ResumeLayout(false);
        }

        #endregion

        public com.RADIO.Datinate.App.View.compare.CompareView CompareView;
    }
}