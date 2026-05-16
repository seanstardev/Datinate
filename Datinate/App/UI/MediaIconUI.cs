using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Properties;
using System.ComponentModel;
using System.Drawing.Imaging;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace datinate.app
{
    public partial class MediaIconUI : HighQualityPictureBox
    {
        private static readonly Dictionary<string, Bitmap> ImageCache = new(StringComparer.OrdinalIgnoreCase);

        private string? imageKey;

        private int alpha = 100;

        private Bitmap? sourceImage;
        private Bitmap? ownedAdjustedImage;

        public MediaIconUI()
        {
            InitializeComponent();
            SizeMode = PictureBoxSizeMode.Zoom;
        }

        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        public string? ImageKey
        {
            get => imageKey;
            set
            {
                if (string.Equals(imageKey, value, StringComparison.Ordinal))
                    return;

                imageKey = value?.Trim();
                SetImage();
            }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue(100)]
        public int Alpha
        {
            get => alpha;
            set
            {
                int v = value;
                if (v < 0) v = 0;
                if (v > 100) v = 100;

                if (alpha == v)
                    return;

                alpha = v;
                ApplyAlphaAndSetImage();
                Invalidate();
            }
        }

        protected void HandleDisposing()
        {   
            DisposeOwnedAdjustedImage();
        }

        private void SetImage()
        {
            sourceImage = null;

            if (string.IsNullOrWhiteSpace(ImageKey))
            {
                ApplyAlphaAndSetImage();
                return;
            }

            if (ImageCache.TryGetValue(ImageKey, out var cached))
            {
                sourceImage = cached;
                ApplyAlphaAndSetImage();
                return;
            }

            if (Enum.TryParse(ImageKey, ignoreCase: true, out MEDIA_TYPE_ENUM mediaTypeEnum) &&
                mediaTypeEnum != MEDIA_TYPE_ENUM.NOT_SET)
            {
                var bmp = GetMediaIconBmp(mediaTypeEnum);

                if (bmp != null)
                {
                    ImageCache[ImageKey] = bmp;
                    sourceImage = bmp;
                    ApplyAlphaAndSetImage();
                    return;
                }
            }

            var fallbackBmp = Resources.media_icons_Web;
            ImageCache[ImageKey] = fallbackBmp;
            sourceImage = fallbackBmp;
            ApplyAlphaAndSetImage();
        }

        private void ApplyAlphaAndSetImage()
        {
            DisposeOwnedAdjustedImage();

            if (sourceImage == null)
            {
                Image = null;
                return;
            }

            if (alpha >= 100)
            {
                Image = sourceImage;
                return;
            }

            float a = alpha / 100f;

            var adjusted = new Bitmap(sourceImage.Width, sourceImage.Height, PixelFormat.Format32bppArgb);

            using (var g = Graphics.FromImage(adjusted))
            using (var attrs = new ImageAttributes())
            {
                var cm = new ColorMatrix(new float[][]
                {
                    new float[] { 1f, 0f, 0f, 0f, 0f },
                    new float[] { 0f, 1f, 0f, 0f, 0f },
                    new float[] { 0f, 0f, 1f, 0f, 0f },
                    new float[] { 0f, 0f, 0f, a,  0f },
                    new float[] { 0f, 0f, 0f, 0f, 1f }
                });

                attrs.SetColorMatrix(cm, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                g.DrawImage(
                    sourceImage,
                    new Rectangle(0, 0, adjusted.Width, adjusted.Height),
                    0, 0, sourceImage.Width, sourceImage.Height,
                    GraphicsUnit.Pixel,
                    attrs);
            }

            ownedAdjustedImage = adjusted;
            Image = adjusted;
        }

        private void DisposeOwnedAdjustedImage()
        {
            if (ownedAdjustedImage != null)
            {
                if (!ReferenceEquals(Image, sourceImage))
                    Image = null;

                ownedAdjustedImage.Dispose();
                ownedAdjustedImage = null;
            }
        }

        private static Bitmap? GetMediaIconBmp(MEDIA_TYPE_ENUM mediaTypeEnum)
            => DatinateHelper.GetMediaIconBmp(mediaTypeEnum);
    }
}