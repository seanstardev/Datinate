namespace datinate.app {
    partial class DatPathsUpdateForm {
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
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DatPathsUpdateForm));
            DatPathsUpdateView = new com.RADIO.Datinate.App.View.projects.datPathsUpdate.DatPathsUpdateView();
            SuspendLayout();
            // 
            // DatPathsUpdateView
            // 
            DatPathsUpdateView.Dock = DockStyle.Fill;
            DatPathsUpdateView.Location = new Point(0, 0);
            DatPathsUpdateView.Name = "DatPathsUpdateView";
            DatPathsUpdateView.Size = new Size(673, 567);
            DatPathsUpdateView.TabIndex = 0;
            // 
            // DatPathsUpdateForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(673, 567);
            Controls.Add(DatPathsUpdateView);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MaximumSize = new Size(689, 606);
            MinimizeBox = false;
            MinimumSize = new Size(689, 606);
            Name = "DatPathsUpdateForm";
            Text = "Update DAT Fullpaths";
            FormClosing += OnFormClosing;
            ResumeLayout(false);
        }

        #endregion

        public com.RADIO.Datinate.App.View.projects.datPathsUpdate.DatPathsUpdateView DatPathsUpdateView;
    }
}