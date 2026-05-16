namespace datinate.app
{
    partial class MiniWebUI
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
            if (disposing) HandleDisposing();
            if (disposing && (components != null))
            {
                components.Dispose();
                DisposeMiniResources();
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
            hostPanel = new Panel();
            SuspendLayout();
            // 
            // hostPanel
            // 
            hostPanel.Dock = DockStyle.Fill;
            hostPanel.Location = new Point(0, 0);
            hostPanel.Margin = new Padding(0);
            hostPanel.Name = "hostPanel";
            hostPanel.Size = new Size(560, 435);
            hostPanel.TabIndex = 0;
            // 
            // MiniWebUI
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(hostPanel);
            Margin = new Padding(0);
            Name = "MiniWebUI";
            Size = new Size(560, 435);
            ResumeLayout(false);
        }

        #endregion

        private Panel hostPanel;
    }
}
