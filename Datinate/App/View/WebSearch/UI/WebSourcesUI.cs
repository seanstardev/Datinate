using System.Drawing.Imaging;

namespace datinate.app
{
    public partial class WebSourcesUI : UserControl
    {
        private static readonly Color SelectedBack = Color.Transparent;
        private static readonly Color NormalBack = Color.Transparent;
        private static readonly Color UnderlineColor = Color.Black;

        private const float UnselectedAlpha = 0.45f;

        public enum WEB_SOURCE_ENUM
        {
            Google,
            Youtube,
            ChatGPT,
            Wikipedia
        }

        public event Action<WEB_SOURCE_ENUM>? SourceSelectedEvt;

        private WEB_SOURCE_ENUM selectedSource = WEB_SOURCE_ENUM.Google;
        public WEB_SOURCE_ENUM SelectedSource => selectedSource;

        private ImageList? imageListWebSourcesDim;

        public WebSourcesUI()
        {
            InitializeComponent();

            DoubleBuffered = true;

            googleBtn.Tag = WEB_SOURCE_ENUM.Google;
            youtubeBtn.Tag = WEB_SOURCE_ENUM.Youtube;
            chatgptBtn.Tag = WEB_SOURCE_ENUM.ChatGPT;
            wikipediaBtn.Tag = WEB_SOURCE_ENUM.Wikipedia;

            googleBtn.Click += OnSourceBtnClick;
            youtubeBtn.Click += OnSourceBtnClick;
            chatgptBtn.Click += OnSourceBtnClick;
            wikipediaBtn.Click += OnSourceBtnClick;

            ConfigureButton(googleBtn);
            ConfigureButton(youtubeBtn);
            ConfigureButton(chatgptBtn);
            ConfigureButton(wikipediaBtn);

            EnsureDimImageList();
        }

        public void SetSourceSelected(WEB_SOURCE_ENUM source)
        {
            selectedSource = source;
            ApplySelectionVisuals();

            SourceSelectedEvt?.Invoke(selectedSource);
        }

        private void OnSourceBtnClick(object? sender, EventArgs e)
        {
            if (sender is not Button btn)
                return;

            if (btn.Tag is WEB_SOURCE_ENUM src)
                SetSourceSelected(src);
            else
                SetSourceSelected(WEB_SOURCE_ENUM.Google);
        }

        private void ApplySelectionVisuals()
        {
            EnsureDimImageList();

            foreach (var b in new[] { googleBtn, youtubeBtn, chatgptBtn, wikipediaBtn })
            {
                var isSelected = b.Tag is WEB_SOURCE_ENUM src && src == selectedSource;

                b.BackColor = isSelected ? SelectedBack : NormalBack;

                if (imageListWebSourcesDim != null)
                    b.ImageList = isSelected ? imageListWebSources : imageListWebSourcesDim;
            }

            Invalidate();
        }

        private void EnsureDimImageList()
        {
            if (imageListWebSourcesDim != null)
                return;

            if (imageListWebSources == null || imageListWebSources.Images.Count == 0)
                return;

            var dim = new ImageList
            {
                ColorDepth = imageListWebSources.ColorDepth,
                ImageSize = imageListWebSources.ImageSize,
                TransparentColor = imageListWebSources.TransparentColor
            };

            for (var i = 0; i < imageListWebSources.Images.Count; i++)
            {
                var key = imageListWebSources.Images.Keys[i];
                var src = imageListWebSources.Images[i];;
                dim.Images.Add(key, MakeAlphaImage(src, UnselectedAlpha));

            }

            imageListWebSourcesDim = dim;
            components?.Add(imageListWebSourcesDim);
        }

        private static Image MakeAlphaImage(Image src, float alpha)
        {
            var bmp = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);

            using var g = Graphics.FromImage(bmp);
            using var ia = new ImageAttributes();

            var cm = new ColorMatrix
            {
                Matrix00 = 1f,
                Matrix11 = 1f,
                Matrix22 = 1f,
                Matrix33 = alpha,
                Matrix44 = 1f
            };

            ia.SetColorMatrix(cm, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            g.DrawImage(
                src,
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                0,
                0,
                src.Width,
                src.Height,
                GraphicsUnit.Pixel,
                ia);

            return bmp;
        }

        private static void ConfigureButton(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.UseVisualStyleBackColor = false;
            b.TabStop = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            return;
        }
    }
}
