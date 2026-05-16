namespace datinate.app {
    partial class AddToProjectForm {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddToProjectForm));
            AddToProjectView = new com.RADIO.Datinate.App.View.datDetails.addToProject.AddToProjectView();
            SuspendLayout();
            // 
            // AddToProjectView
            // 
            AddToProjectView.Dock = DockStyle.Fill;
            AddToProjectView.Location = new Point(0, 0);
            AddToProjectView.Name = "AddToProjectView";
            AddToProjectView.Size = new Size(553, 664);
            AddToProjectView.TabIndex = 0;
            // 
            // AddToProjectForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(553, 664);
            Controls.Add(AddToProjectView);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(569, 405);
            Name = "AddToProjectForm";
            Text = "Add to DAT Grouper Project";
            FormClosing += OnFormClosing;
            ResumeLayout(false);
        }

        #endregion

        public com.RADIO.Datinate.App.View.datDetails.addToProject.AddToProjectView AddToProjectView;
    }
}