using Datinate.App.UI;

namespace datinate.app
{
    partial class MainWebView
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
            tableLayoutPanel = new TableLayoutPanel();
            headerContainer = new Panel();
            controlsContainer = new Panel();
            urlText = new TextBox();
            backBtn = new Button();
            closeBtn = new LinkLabelHackUI();
            REAL_closeBtn = new Panel();
            goBtn = new Button();
            browserBtn = new LinkLabelHackUI();
            REAL_browserBtn = new Panel();
            filler = new Panel();
            webContainer = new Panel();
            dragDropOverlay = new BackgroundPanel();
            browser = new Microsoft.Web.WebView2.WinForms.WebView2();
            tableLayoutPanel.SuspendLayout();
            headerContainer.SuspendLayout();
            controlsContainer.SuspendLayout();
            webContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)browser).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.ColumnCount = 1;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel.Controls.Add(headerContainer, 0, 0);
            tableLayoutPanel.Controls.Add(filler, 0, 1);
            tableLayoutPanel.Controls.Add(webContainer, 0, 2);
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.Location = new Point(0, 0);
            tableLayoutPanel.Margin = new Padding(0);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 3;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel.Size = new Size(902, 532);
            tableLayoutPanel.TabIndex = 0;
            // 
            // headerContainer
            // 
            headerContainer.Controls.Add(controlsContainer);
            headerContainer.Dock = DockStyle.Fill;
            headerContainer.Location = new Point(3, 3);
            headerContainer.Name = "headerContainer";
            headerContainer.Size = new Size(896, 29);
            headerContainer.TabIndex = 36;
            // 
            // controlsContainer
            // 
            controlsContainer.Controls.Add(urlText);
            controlsContainer.Controls.Add(backBtn);
            controlsContainer.Controls.Add(closeBtn);
            controlsContainer.Controls.Add(REAL_closeBtn);
            controlsContainer.Controls.Add(goBtn);
            controlsContainer.Controls.Add(browserBtn);
            controlsContainer.Controls.Add(REAL_browserBtn);
            controlsContainer.Dock = DockStyle.Fill;
            controlsContainer.Location = new Point(0, 0);
            controlsContainer.Margin = new Padding(0);
            controlsContainer.Name = "controlsContainer";
            controlsContainer.Size = new Size(896, 29);
            controlsContainer.TabIndex = 1;
            // 
            // urlText
            // 
            urlText.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            urlText.ForeColor = SystemColors.ControlDarkDark;
            urlText.Location = new Point(95, 3);
            urlText.Name = "urlText";
            urlText.Size = new Size(708, 23);
            urlText.TabIndex = 33;
            urlText.KeyUp += urlTextBox_KeyUp;
            // 
            // backBtn
            // 
            backBtn.Cursor = Cursors.Hand;
            backBtn.Location = new Point(3, 3);
            backBtn.Name = "backBtn";
            backBtn.Size = new Size(60, 23);
            backBtn.TabIndex = 32;
            backBtn.Text = "← Back";
            backBtn.UseVisualStyleBackColor = true;
            backBtn.Click += backBtn_Click;
            // 
            // closeBtn
            // 
            closeBtn.ActiveLinkColor = Color.Black;
            closeBtn.AutoSize = true;
            closeBtn.BackColor = Color.Transparent;
            closeBtn.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            closeBtn.LinkBehavior = LinkBehavior.NeverUnderline;
            closeBtn.LinkColor = Color.Black;
            closeBtn.Location = new Point(65, 1);
            closeBtn.Margin = new Padding(0);
            closeBtn.Name = "closeBtn";
            closeBtn.Size = new Size(29, 25);
            closeBtn.TabIndex = 36;
            closeBtn.TabStop = true;
            closeBtn.Text = "✕";
            closeBtn.TextAlign = ContentAlignment.TopRight;
            closeBtn.VisitedLinkColor = Color.Black;
            // 
            // REAL_closeBtn
            // 
            REAL_closeBtn.BackColor = SystemColors.Control;
            REAL_closeBtn.Cursor = Cursors.Hand;
            REAL_closeBtn.Location = new Point(62, 2);
            REAL_closeBtn.Name = "REAL_closeBtn";
            REAL_closeBtn.Size = new Size(33, 34);
            REAL_closeBtn.TabIndex = 37;
            REAL_closeBtn.Click += closeBtn_Click;
            // 
            // goBtn
            // 
            goBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            goBtn.Cursor = Cursors.Hand;
            goBtn.Location = new Point(833, 3);
            goBtn.Name = "goBtn";
            goBtn.Size = new Size(60, 23);
            goBtn.TabIndex = 34;
            goBtn.Text = "Go";
            goBtn.UseVisualStyleBackColor = true;
            goBtn.Click += goBtn_Click;
            // 
            // browserBtn
            // 
            browserBtn.ActiveLinkColor = Color.Black;
            browserBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            browserBtn.AutoSize = true;
            browserBtn.BackColor = Color.Transparent;
            browserBtn.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            browserBtn.LinkBehavior = LinkBehavior.NeverUnderline;
            browserBtn.LinkColor = Color.Black;
            browserBtn.Location = new Point(803, -4);
            browserBtn.Margin = new Padding(0);
            browserBtn.Name = "browserBtn";
            browserBtn.Size = new Size(33, 32);
            browserBtn.TabIndex = 0;
            browserBtn.TabStop = true;
            browserBtn.Text = "↗";
            browserBtn.TextAlign = ContentAlignment.TopRight;
            browserBtn.VisitedLinkColor = Color.Black;
            // 
            // REAL_browserBtn
            // 
            REAL_browserBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            REAL_browserBtn.BackColor = SystemColors.Control;
            REAL_browserBtn.Cursor = Cursors.Hand;
            REAL_browserBtn.Location = new Point(803, -1);
            REAL_browserBtn.Name = "REAL_browserBtn";
            REAL_browserBtn.Size = new Size(33, 34);
            REAL_browserBtn.TabIndex = 35;
            REAL_browserBtn.Click += browserBtn_Click;
            // 
            // filler
            // 
            filler.BackColor = SystemColors.ControlDarkDark;
            filler.Dock = DockStyle.Fill;
            filler.Location = new Point(0, 35);
            filler.Margin = new Padding(0);
            filler.Name = "filler";
            filler.Size = new Size(902, 1);
            filler.TabIndex = 3;
            // 
            // webContainer
            // 
            webContainer.Controls.Add(dragDropOverlay);
            webContainer.Controls.Add(browser);
            webContainer.Dock = DockStyle.Fill;
            webContainer.Location = new Point(0, 36);
            webContainer.Margin = new Padding(0);
            webContainer.Name = "webContainer";
            webContainer.Size = new Size(902, 496);
            webContainer.TabIndex = 2;
            // 
            // dragDropOverlay
            // 
            dragDropOverlay.BackColor = SystemColors.Control;
            dragDropOverlay.CornerColor = Color.LightGray;
            dragDropOverlay.Dock = DockStyle.Fill;
            dragDropOverlay.Location = new Point(0, 0);
            dragDropOverlay.Margin = new Padding(0);
            dragDropOverlay.Name = "dragDropOverlay";
            dragDropOverlay.Size = new Size(902, 496);
            dragDropOverlay.TabIndex = 0;
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
            browser.Size = new Size(902, 496);
            browser.Source = new Uri("about:blank", UriKind.Absolute);
            browser.TabIndex = 0;
            browser.ZoomFactor = 1D;
            browser.SourceChanged += browser_SourceChanged;
            // 
            // MainWebView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel);
            Margin = new Padding(0);
            Name = "MainWebView";
            Size = new Size(902, 532);
            tableLayoutPanel.ResumeLayout(false);
            headerContainer.ResumeLayout(false);
            controlsContainer.ResumeLayout(false);
            controlsContainer.PerformLayout();
            webContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)browser).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel;
        private Microsoft.Web.WebView2.WinForms.WebView2 browser;
        private Panel webContainer;
        private Panel filler;
        private Panel headerContainer;
        private Panel controlsContainer;
        private Button goBtn;
        private LinkLabelHackUI browserBtn;
        private Button backBtn;
        private TextBox urlText;
        private Panel REAL_browserBtn;
        private BackgroundPanel dragDropOverlay;
        private LinkLabelHackUI closeBtn;
        private Panel REAL_closeBtn;
    }
}
