namespace datinate.app {
    partial class ProjectsForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing)
                HandleDisposing();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProjectsForm));
            ProjectsView = new com.RADIO.Datinate.App.View.projects.ProjectsView();
            sizeBar = new WindowSizePresetBar();
            SuspendLayout();
            // 
            // ProjectsView
            // 
            ProjectsView.Dock = DockStyle.Fill;
            ProjectsView.Location = new Point(0, 0);
            ProjectsView.Name = "ProjectsView";
            ProjectsView.Size = new Size(1148, 687);
            ProjectsView.TabIndex = 0;
            // 
            // sizeBar
            // 
            sizeBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            sizeBar.BackColor = Color.Transparent;
            sizeBar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            sizeBar.ForeColor = Color.White;
            sizeBar.Location = new Point(3, 656);
            sizeBar.Name = "sizeBar";
            sizeBar.Size = new Size(132, 28);
            sizeBar.TabIndex = 1;
            sizeBar.Text = "windowSizePresetBar1";
            // 
            // ProjectsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1148, 687);
            Controls.Add(sizeBar);
            Controls.Add(ProjectsView);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 3, 4, 3);
            MinimumSize = new Size(1164, 726);
            Name = "ProjectsForm";
            StartPosition = FormStartPosition.Manual;
            Text = "DAT Grouper";
            FormClosing += OnFormClosing;
            ResumeLayout(false);
        }

        #endregion

        public com.RADIO.Datinate.App.View.projects.ProjectsView ProjectsView;
        private WindowSizePresetBar sizeBar;
    }
}