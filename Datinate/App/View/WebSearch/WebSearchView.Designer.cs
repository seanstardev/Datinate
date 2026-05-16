namespace datinate.app
{
    partial class WebSearchView
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                HandleDisposing();
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            webSourcesUI = new WebSourcesUI();
            topRowPanel = new Panel();
            searchGameText = new TextBox();
            searchSystemText = new TextBox();
            rightPanel = new Panel();
            topRowPanel.SuspendLayout();
            rightPanel.SuspendLayout();
            SuspendLayout();
            // 
            // webSourcesUI
            // 
            webSourcesUI.Cursor = Cursors.Hand;
            webSourcesUI.Location = new Point(4, -4);
            webSourcesUI.Margin = new Padding(0);
            webSourcesUI.Name = "webSourcesUI";
            webSourcesUI.Size = new Size(112, 33);
            webSourcesUI.TabIndex = 0;
            // 
            // topRowPanel
            // 
            topRowPanel.Controls.Add(searchGameText);
            topRowPanel.Controls.Add(searchSystemText);
            topRowPanel.Controls.Add(rightPanel);
            topRowPanel.Dock = DockStyle.Top;
            topRowPanel.Location = new Point(0, 0);
            topRowPanel.Margin = new Padding(0);
            topRowPanel.Name = "topRowPanel";
            topRowPanel.Padding = new Padding(3, 3, 3, 0);
            topRowPanel.Size = new Size(900, 33);
            topRowPanel.TabIndex = 100;
            // 
            // searchGameText
            // 
            searchGameText.Dock = DockStyle.Fill;
            searchGameText.Location = new Point(143, 3);
            searchGameText.Margin = new Padding(0);
            searchGameText.Name = "searchGameText";
            searchGameText.Size = new Size(637, 23);
            searchGameText.TabIndex = 38;
            // 
            // searchSystemText
            // 
            searchSystemText.Dock = DockStyle.Left;
            searchSystemText.Location = new Point(3, 3);
            searchSystemText.Margin = new Padding(0);
            searchSystemText.Name = "searchSystemText";
            searchSystemText.Size = new Size(140, 23);
            searchSystemText.TabIndex = 33;
            // 
            // rightPanel
            // 
            rightPanel.Controls.Add(webSourcesUI);
            rightPanel.Dock = DockStyle.Right;
            rightPanel.Location = new Point(780, 3);
            rightPanel.Margin = new Padding(0);
            rightPanel.Name = "rightPanel";
            rightPanel.Size = new Size(117, 30);
            rightPanel.TabIndex = 101;
            // 
            // RbWebSearchView
            // 
            AutoScaleMode = AutoScaleMode.None;
            Controls.Add(topRowPanel);
            Name = "RbWebSearchView";
            Size = new Size(900, 31);
            topRowPanel.ResumeLayout(false);
            topRowPanel.PerformLayout();
            rightPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel topRowPanel;
        private Panel rightPanel;
        private TextBox searchSystemText;
        private TextBox searchGameText;
        private WebSourcesUI webSourcesUI;
    }
}
