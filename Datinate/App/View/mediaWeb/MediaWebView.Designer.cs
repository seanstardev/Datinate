namespace datinate.app
{
    partial class MediaWebView
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
                HandleDisposing();
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
            webContainer = new Panel();
            browser = new Microsoft.Web.WebView2.WinForms.WebView2();
            tableLayoutPanel1 = new TableLayoutPanel();
            webContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)browser).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // webContainer
            // 
            webContainer.BackColor = Color.Black;
            webContainer.Controls.Add(browser);
            webContainer.Dock = DockStyle.Fill;
            webContainer.Location = new Point(0, 0);
            webContainer.Margin = new Padding(0);
            webContainer.Name = "webContainer";
            webContainer.Size = new Size(550, 620);
            webContainer.TabIndex = 2;
            // 
            // browser
            // 
            browser.AllowExternalDrop = true;
            browser.CreationProperties = null;
            browser.DefaultBackgroundColor = Color.White;
            browser.Dock = DockStyle.Fill;
            browser.Location = new Point(0, 0);
            browser.Margin = new Padding(0);
            browser.Name = "browser";
            browser.Size = new Size(550, 620);
            browser.Source = new Uri("about:blank", UriKind.Absolute);
            browser.TabIndex = 0;
            browser.ZoomFactor = 1D;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(webContainer, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(550, 620);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // MediaWebView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel1);
            Margin = new Padding(0);
            Name = "MediaWebView";
            Size = new Size(550, 620);
            webContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)browser).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel webContainer;
        private Microsoft.Web.WebView2.WinForms.WebView2 browser;
        private TableLayoutPanel tableLayoutPanel1;
    }
}
