namespace datinate.app
{
    partial class WebSourcesUI
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WebSourcesUI));
            tableLayoutPanel1 = new TableLayoutPanel();
            googleBtn = new NoFocusButton();
            imageListWebSources = new ImageList(components);
            youtubeBtn = new NoFocusButton();
            chatgptBtn = new NoFocusButton();
            wikipediaBtn = new NoFocusButton();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 4;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 28F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 28F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 28F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 28F));
            tableLayoutPanel1.Controls.Add(googleBtn, 0, 0);
            tableLayoutPanel1.Controls.Add(youtubeBtn, 1, 0);
            tableLayoutPanel1.Controls.Add(chatgptBtn, 2, 0);
            tableLayoutPanel1.Controls.Add(wikipediaBtn, 3, 0);
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tableLayoutPanel1.Size = new Size(112, 28);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // googleBtn
            // 
            googleBtn.BackColor = Color.White;
            googleBtn.Dock = DockStyle.Fill;
            googleBtn.FlatAppearance.BorderSize = 0;
            googleBtn.FlatAppearance.MouseDownBackColor = Color.White;
            googleBtn.FlatAppearance.MouseOverBackColor = Color.Gainsboro;
            googleBtn.FlatStyle = FlatStyle.Flat;
            googleBtn.ImageIndex = 0;
            googleBtn.ImageList = imageListWebSources;
            googleBtn.Location = new Point(0, 0);
            googleBtn.Margin = new Padding(0);
            googleBtn.Name = "googleBtn";
            googleBtn.Size = new Size(28, 28);
            googleBtn.TabIndex = 0;
            googleBtn.TabStop = false;
            googleBtn.UseVisualStyleBackColor = false;
            // 
            // imageListWebSources
            // 
            imageListWebSources.ColorDepth = ColorDepth.Depth32Bit;
            imageListWebSources.ImageStream = (ImageListStreamer)resources.GetObject("imageListWebSources.ImageStream");
            imageListWebSources.TransparentColor = Color.Transparent;
            imageListWebSources.Images.SetKeyName(0, "google.png");
            imageListWebSources.Images.SetKeyName(1, "yourube.png");
            imageListWebSources.Images.SetKeyName(2, "chatgpt.png");
            imageListWebSources.Images.SetKeyName(3, "wikipedia.png");
            // 
            // youtubeBtn
            // 
            youtubeBtn.BackColor = Color.Transparent;
            youtubeBtn.Dock = DockStyle.Fill;
            youtubeBtn.FlatAppearance.BorderSize = 0;
            youtubeBtn.FlatAppearance.MouseDownBackColor = Color.White;
            youtubeBtn.FlatAppearance.MouseOverBackColor = Color.Gainsboro;
            youtubeBtn.FlatStyle = FlatStyle.Flat;
            youtubeBtn.ImageIndex = 1;
            youtubeBtn.ImageList = imageListWebSources;
            youtubeBtn.Location = new Point(28, 0);
            youtubeBtn.Margin = new Padding(0);
            youtubeBtn.Name = "youtubeBtn";
            youtubeBtn.Size = new Size(28, 28);
            youtubeBtn.TabIndex = 1;
            youtubeBtn.TabStop = false;
            youtubeBtn.UseVisualStyleBackColor = false;
            // 
            // chatgptBtn
            // 
            chatgptBtn.BackColor = Color.Transparent;
            chatgptBtn.Dock = DockStyle.Fill;
            chatgptBtn.FlatAppearance.BorderSize = 0;
            chatgptBtn.FlatAppearance.MouseDownBackColor = Color.White;
            chatgptBtn.FlatAppearance.MouseOverBackColor = Color.Gainsboro;
            chatgptBtn.FlatStyle = FlatStyle.Flat;
            chatgptBtn.ImageIndex = 2;
            chatgptBtn.ImageList = imageListWebSources;
            chatgptBtn.Location = new Point(56, 0);
            chatgptBtn.Margin = new Padding(0);
            chatgptBtn.Name = "chatgptBtn";
            chatgptBtn.Size = new Size(28, 28);
            chatgptBtn.TabIndex = 2;
            chatgptBtn.TabStop = false;
            chatgptBtn.UseVisualStyleBackColor = false;
            // 
            // wikipediaBtn
            // 
            wikipediaBtn.BackColor = Color.Transparent;
            wikipediaBtn.Dock = DockStyle.Fill;
            wikipediaBtn.FlatAppearance.BorderSize = 0;
            wikipediaBtn.FlatAppearance.MouseDownBackColor = Color.White;
            wikipediaBtn.FlatAppearance.MouseOverBackColor = Color.Gainsboro;
            wikipediaBtn.FlatStyle = FlatStyle.Flat;
            wikipediaBtn.ImageIndex = 3;
            wikipediaBtn.ImageList = imageListWebSources;
            wikipediaBtn.Location = new Point(84, 0);
            wikipediaBtn.Margin = new Padding(0);
            wikipediaBtn.Name = "wikipediaBtn";
            wikipediaBtn.Size = new Size(28, 28);
            wikipediaBtn.TabIndex = 3;
            wikipediaBtn.TabStop = false;
            wikipediaBtn.UseVisualStyleBackColor = false;
            // 
            // WebSourcesUI
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel1);
            Margin = new Padding(0);
            Name = "WebSourcesUI";
            Size = new Size(112, 33);
            tableLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private ImageList imageListWebSources;
        private NoFocusButton googleBtn;
        private NoFocusButton youtubeBtn;
        private NoFocusButton chatgptBtn;
        private NoFocusButton wikipediaBtn;

        internal sealed class NoFocusButton : Button
        {
            public NoFocusButton()
            {
                TabStop = false;
                SetStyle(ControlStyles.Selectable, false);
            }

            protected override bool ShowFocusCues => false;
        }
    }
}
