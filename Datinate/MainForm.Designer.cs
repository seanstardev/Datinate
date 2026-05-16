namespace datinate.app {
    partial class MainForm {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            mainView = new com.RADIO.Datinate.MainView();
            sizeBar = new WindowSizePresetBar();
            SuspendLayout();
            // 
            // mainView
            // 
            mainView.Dock = DockStyle.Fill;
            mainView.Location = new Point(0, 0);
            mainView.Margin = new Padding(0);
            mainView.Name = "mainView";
            mainView.Size = new Size(984, 661);
            mainView.TabIndex = 0;
            // 
            // sizeBar
            // 
            sizeBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            sizeBar.BackColor = Color.Transparent;
            sizeBar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            sizeBar.ForeColor = Color.White;
            sizeBar.Location = new Point(3, 630);
            sizeBar.Name = "sizeBar";
            sizeBar.Size = new Size(132, 28);
            sizeBar.TabIndex = 1;
            sizeBar.Text = "windowSizePresetBar1";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(984, 661);
            Controls.Add(sizeBar);
            Controls.Add(mainView);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 3, 4, 3);
            MinimumSize = new Size(1000, 700);
            Name = "MainForm";
            StartPosition = FormStartPosition.Manual;
            Text = "Datinate";
            FormClosing += MainForm_FormClosing;
            FormClosed += MainForm_FormClosed;
            ResumeLayout(false);
        }

        #endregion

        private com.RADIO.Datinate.MainView mainView;
        private WindowSizePresetBar sizeBar;
    }
}

