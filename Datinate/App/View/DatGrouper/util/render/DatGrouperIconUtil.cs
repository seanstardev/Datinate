using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using static datinate.app.TreeRenderUtil;

namespace Datinate.App.View.projects.gameFamily
{
    public sealed class DatGrouperIconUtil
    {

        public const int NodeItemHeight = 26;
        public string DefaultIcon => "DefaultIcon";
        public string RootIcon => "RootIcon";

        public string GameFamilyIcon => "GameFamilyIcon";
        public string GameFamilyGreenIcon => "GameFamilyGreenIcon";
        public string GameFamilyAmberIcon => "GameFamilyAmberIcon";
        public string GameFamilyRedIcon => "GameFamilyRedIcon";

        public string GameParentIcon => "GameParentIcon";
        public string GameParentGreenIcon => "GameParentGreenIcon";
        public string GameParentAmberIcon => "GameParentAmberIcon";
        public string GameParentRedIcon => "GameParentRedIcon";

        public string GameChildIcon => "GameChildIcon";
        public string GameChildGreenIcon => "GameChildGreenIcon";
        public string GameChildAmberIcon => "GameChildAmberIcon";
        public string GameChildRedIcon => "GameChildRedIcon";

        public string ExcludeIcon => "ExcludeIcon";
        public string ExcludeExtraIcon => "ExcludeExtraIcon";
        public string IncludeIcon => "IncludeIcon";
        public string ShallowIconAlpha => "ShallowIconAlpha";
        public string IncludeExtraIcon => "IncludeExtraIcon";
        public string QuestionExtraIcon => "QuestionIcon";
        public string QuestionExtra => "QuestionExtraIcon";
        public string IncludedReluctantIcon => "IncludedReluctantIcon";
        public string ClapperBoardIcon => "ClapperBoardIcon";

        public string WarningTriangleIcon => "WarningTriangleIcon";
        public string SpeechBubbleLeftIcon => "SpeechBubbleLeftIcon";
        public string Star5Icon => "Star5Icon";

        public Bitmap IncludeIconImage { get; }
        public Bitmap AlwaysIncludeIconImage { get; }
        public Bitmap ReluctantIncludeIconImage { get; }
        public Bitmap ShallowIncludeIconImage { get; }
        public Bitmap ExcludeIconImage { get; }
        public Bitmap AlwaysExcludeIconImage { get; }
        public Bitmap OrphanedIconImage { get; }

        public Bitmap ClapperBoardAutoImage { get; }
        public Bitmap ClapperBoardCuratedImage { get; }
        public Bitmap ClapperBoardCuratedInvertedImage { get; }
        public Bitmap ClapperBoardAutoUnavailableImage { get; }
        public Bitmap ClapperBoardCuratedUnavailableImage { get; }
        public Bitmap ClapperBoardCuratedUnavailableInvertedImage { get; }
        public Bitmap WarningTriangleIconImage { get; }
        public Bitmap SpeechBubbleLeftIconImage { get; }
        public Bitmap Star5IconImage { get; }


        private static int IconSize = 26;

        private const int DesignIconSize = 20;

        private const float PaddingScale = 1.00f;
        private const float GameIconPaddingScale = 1.00f;
        private const float StatusIconPaddingScale = 1.00f;

        private const int DefaultIconPaddingBase = 0;

        private const int GameFamilyIconPaddingBase = 1;
        private const int GameParentIconPaddingBase = 3;
        private const int GameChildIconPaddingBase = 6;

        private const int StatusIconPaddingBase = 2;

        private static int ScalePadding(int basePaddingAtDesignSize, float localScale)
        {
            float s = IconSize / (float)DesignIconSize;
            int p = (int)Math.Round(basePaddingAtDesignSize * s * PaddingScale * localScale, MidpointRounding.AwayFromZero);

            int max = Math.Max(0, (IconSize / 2) - 1);

            if (p < 0) p = 0;
            if (p > max) p = max;

            return p;
        }

        private static int DefaultIconPadding => ScalePadding(DefaultIconPaddingBase, 1f);

        private static int GameFamilyIconPadding => ScalePadding(GameFamilyIconPaddingBase, GameIconPaddingScale);
        private static int GameParentIconPadding => ScalePadding(GameParentIconPaddingBase, GameIconPaddingScale);
        private static int GameChildIconPadding => ScalePadding(GameChildIconPaddingBase, GameIconPaddingScale);

        private static int StatusIconPadding => ScalePadding(StatusIconPaddingBase, StatusIconPaddingScale);

        private static readonly Color BackgroundColour = Color.White;

        private static readonly DatGrouperIconUtil _instance = new DatGrouperIconUtil();
        public static DatGrouperIconUtil Instance => _instance;

        public ImageList TreeImageList { get; }

        private DatGrouperIconUtil()
        {
            var imageList = new ImageList
            {
                ImageSize = new Size(IconSize, IconSize),
                ColorDepth = ColorDepth.Depth32Bit
            };

            var red = Color.FromArgb(255, 179, 179);
            
            imageList.Images.Add(DefaultIcon, CreateSolidIcon(Color.Pink, Color.Purple, ":)", DefaultIconPadding, NodeVAlign.Center));

            imageList.Images.Add(RootIcon, CreateSolidIcon(Color.Black, null, null, DefaultIconPadding, NodeVAlign.Center));

            imageList.Images.Add(GameFamilyIcon, CreateSolidIcon(Color.DarkGray, null, null, GameFamilyIconPadding, NodeVAlign.Center));
            imageList.Images.Add(GameFamilyGreenIcon, CreateSolidIcon(Color.LightGreen, null, null, GameFamilyIconPadding, NodeVAlign.Center));
            imageList.Images.Add(GameFamilyAmberIcon, CreateSolidIcon(Color.PeachPuff, null, null, GameFamilyIconPadding, NodeVAlign.Center));
            imageList.Images.Add(GameFamilyRedIcon, CreateSolidIcon(red, null, null, GameFamilyIconPadding, NodeVAlign.Center));
            
            imageList.Images.Add(GameParentIcon, CreateSolidIcon(Color.LightGray, null, null, GameParentIconPadding, NodeVAlign.Center));
            imageList.Images.Add(GameParentGreenIcon, CreateSolidIcon(Color.LightGreen, null, null, GameParentIconPadding, NodeVAlign.Center));
            imageList.Images.Add(GameParentAmberIcon, CreateSolidIcon(Color.PeachPuff, null, null, GameParentIconPadding, NodeVAlign.Center));
            imageList.Images.Add(GameParentRedIcon, CreateSolidIcon(red, null, null, GameParentIconPadding, NodeVAlign.Center));
            
            imageList.Images.Add(GameChildIcon, CreateSolidIcon(Color.LightGray, null, null, GameChildIconPadding, NodeVAlign.Center));
            imageList.Images.Add(GameChildGreenIcon, CreateSolidIcon(Color.LightGreen, null, null, GameChildIconPadding, NodeVAlign.Center));
            imageList.Images.Add(GameChildAmberIcon, CreateSolidIcon(Color.PeachPuff, null, null, GameChildIconPadding, NodeVAlign.Center));
            imageList.Images.Add(GameChildRedIcon, CreateSolidIcon(red, null, null, GameChildIconPadding, NodeVAlign.Center));

            const int KeyIconPadding = 4;

            // TODO: Hack.
            IconSize = 34;
            
            IncludeIconImage = CreateTickIcon(Color.LimeGreen, false, KeyIconPadding, NodeVAlign.Center);
            AlwaysIncludeIconImage = CreateTickIcon(Color.LimeGreen, true, KeyIconPadding, NodeVAlign.Center);
            ShallowIncludeIconImage = CreateTickIconAlpha(Color.LimeGreen, false, KeyIconPadding, NodeVAlign.Center);
            ReluctantIncludeIconImage = CreateNoEntryIcon(Color.Orange, false, KeyIconPadding, NodeVAlign.Center);
            ExcludeIconImage = CreateCrossIcon(Color.Red, false, KeyIconPadding, NodeVAlign.Center);
            AlwaysExcludeIconImage = CreateCrossIcon(Color.Red, true, KeyIconPadding, NodeVAlign.Center);
            OrphanedIconImage = CreateQuestionIcon(Color.MediumPurple, true, KeyIconPadding, NodeVAlign.Center);

            ClapperBoardAutoImage = CreateClapperBoardIcon(Color.Red, false, 0, NodeVAlign.Center);
            ClapperBoardCuratedImage = CreateClapperBoardIcon(Color.Black, false, 0, NodeVAlign.Center);
            ClapperBoardCuratedInvertedImage = CreateClapperBoardIcon(Color.White, false, 0, NodeVAlign.Center);

            ClapperBoardAutoUnavailableImage = CreateClapperBoardUnavailableIcon(Color.Red, false, 0, NodeVAlign.Center);
            ClapperBoardCuratedUnavailableImage = CreateClapperBoardUnavailableIcon(Color.Black, false, 0, NodeVAlign.Center);
            ClapperBoardCuratedUnavailableInvertedImage = CreateClapperBoardUnavailableIcon(Color.White, false, 0, NodeVAlign.Center);

            IconSize = NodeItemHeight;
            
            imageList.Images.Add(IncludeIcon, CreateTickIcon(Color.LimeGreen, false, StatusIconPadding, NodeVAlign.Top));
            
            imageList.Images.Add(IncludeExtraIcon, CreateTickIcon(Color.LimeGreen, true, StatusIconPadding, NodeVAlign.Top));

            imageList.Images.Add(ShallowIconAlpha, CreateSolidIcon(Color.LightGray, null, null, StatusIconPadding, NodeVAlign.Top));

            imageList.Images.Add(IncludedReluctantIcon, CreateNoEntryIcon(Color.Orange, false, StatusIconPadding, NodeVAlign.Top));

            imageList.Images.Add(ExcludeIcon, CreateCrossIcon(Color.Red, false, StatusIconPadding, NodeVAlign.Top));

            imageList.Images.Add(ExcludeExtraIcon, CreateCrossIcon(Color.Red, true, StatusIconPadding, NodeVAlign.Top));

            imageList.Images.Add(QuestionExtra, CreateQuestionIcon(Color.MediumPurple, true, StatusIconPadding, NodeVAlign.Top));

            imageList.Images.Add(QuestionExtraIcon, CreateQuestionIcon(Color.MediumPurple, false, StatusIconPadding, NodeVAlign.Top));

            WarningTriangleIconImage = CreateWarningTriangleIcon(0, NodeVAlign.Center);
            SpeechBubbleLeftIconImage = CreateSpeechBubbleLeftIcon(0, NodeVAlign.Center);
            Star5IconImage = CreateStar5Icon(0, NodeVAlign.Center);

            imageList.Images.Add(WarningTriangleIcon, WarningTriangleIconImage);
            imageList.Images.Add(SpeechBubbleLeftIcon, SpeechBubbleLeftIconImage);
            imageList.Images.Add(Star5Icon, Star5IconImage);


            TreeImageList = imageList;
        }

        private static Bitmap CreateWarningTriangleIcon(int padding, NodeVAlign align)
        {
            return CreateBitmap(padding, GetYOffset(padding, align), (g, rect) =>
            {
                var oldPix = g.PixelOffsetMode;
                try
                {
                    g.PixelOffsetMode = PixelOffsetMode.Default;

                    float strokeW = Math.Max(1f, rect.Width / 18f);
                    var r = InsetRectForStroke(rect, strokeW);

                    var fill = Color.FromArgb(255, 255, 215, 0);
                    var outline = Color.FromArgb(255, 25, 25, 25);

                    float x = r.Left;
                    float y = r.Top;
                    float w = r.Width;
                    float h = r.Height;

                    var pTop = new PointF(x + w * 0.50f, y + h * 0.04f);
                    var pR = new PointF(x + w * 0.96f, y + h * 0.96f);
                    var pL = new PointF(x + w * 0.04f, y + h * 0.96f);

                    using (var gp = new GraphicsPath())
                    {
                        gp.AddPolygon(new[] { pTop, pR, pL });
                        gp.CloseFigure();

                        using (var b = new SolidBrush(fill))
                            g.FillPath(b, gp);

                        using (var p = new Pen(outline, strokeW) { LineJoin = LineJoin.Round, Alignment = PenAlignment.Inset })
                            g.DrawPath(p, gp);
                    }

                    float cx = x + w * 0.50f;

                    float stemW = Math.Max(1f, w * 0.085f);
                    float stemH = h * 0.34f;
                    float stemY = y + h * 0.38f;

                    var stemRect = new RectangleF(
                        cx - (stemW * 0.50f),
                        stemY,
                        stemW,
                        stemH);

                    float dotD = Math.Max(2f, w * 0.10f);
                    float dotY = y + h * 0.78f;

                    var dotRect = new RectangleF(
                        cx - (dotD * 0.50f),
                        dotY,
                        dotD,
                        dotD);

                    using (var b = new SolidBrush(outline))
                    {
                        g.FillRectangle(b, stemRect);
                        g.FillEllipse(b, dotRect);
                    }
                }
                finally
                {
                    g.PixelOffsetMode = oldPix;
                }
            });
        }
        private static Bitmap CreateSpeechBubbleLeftIcon(int padding, NodeVAlign align)
        {
            int sizePx = IconSize;
            if (sizePx < 10) sizePx = 10;

            var bmp = new Bitmap(sizePx, sizePx, PixelFormat.Format32bppArgb);

            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.Half;

                float stroke = MathF.Max(1.0f, sizePx * 0.060f);
                float textStroke = MathF.Max(1.0f, stroke * 0.85f);

                float pad = MathF.Ceiling(stroke) + 1.0f;

                float tailH = MathF.Max(3.0f, sizePx * 0.120f);
                float bubbleH = (sizePx - (pad * 2.0f)) - tailH;
                if (bubbleH < 4.0f) bubbleH = 4.0f;

                float bubbleW = sizePx - (pad * 2.0f);
                if (bubbleW < 4.0f) bubbleW = 4.0f;

                var bubbleRect = new RectangleF(pad, pad, bubbleW, bubbleH);

                float r = MathF.Max(2.0f, sizePx * 0.160f);
                float maxR = MathF.Min(bubbleRect.Width, bubbleRect.Height) * 0.45f;
                if (r > maxR) r = maxR;

                float cx = sizePx * 0.50f;
                float tailBaseY = bubbleRect.Bottom;
                float tailTipY = tailBaseY + tailH;

                float tailHalfW = MathF.Max(4.0f, sizePx * 0.200f);
                float minTailHalfW = r + 2.0f;
                float maxTailHalfW = (bubbleRect.Width * 0.50f) - (r + 2.0f);
                if (tailHalfW < minTailHalfW) tailHalfW = minTailHalfW;
                if (tailHalfW > maxTailHalfW) tailHalfW = maxTailHalfW;

                using var outlinePen = new Pen(Color.Black, stroke)
                {
                    LineJoin = LineJoin.Round,
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round
                };

                using var bubblePath = CreateSpeechBubblePath(bubbleRect, r, cx, tailHalfW, tailTipY);

                using (var fill = new SolidBrush(BackgroundColour))
                    g.FillPath(fill, bubblePath);

                g.DrawPath(outlinePen, bubblePath);

                g.SmoothingMode = SmoothingMode.None;
                g.PixelOffsetMode = PixelOffsetMode.Default;

                float innerPadX = MathF.Max(3.0f, bubbleRect.Width * 0.18f);
                float innerPadY = MathF.Max(3.0f, bubbleRect.Height * 0.24f);

                float x1 = bubbleRect.Left + innerPadX;
                float x2 = bubbleRect.Right - innerPadX;

                float yLine1 = bubbleRect.Top + innerPadY;
                float yLine2 = bubbleRect.Top + (bubbleRect.Height * 0.58f);

                const float SPEECH_BUBBLE_LINE1_NUDGE_DOWN_PX_20260207 = 0.8f;
                const float SPEECH_BUBBLE_LINE2_NUDGE_UP_PX_20260207 = 0.8f;

                yLine1 += SPEECH_BUBBLE_LINE1_NUDGE_DOWN_PX_20260207;
                yLine2 -= SPEECH_BUBBLE_LINE2_NUDGE_UP_PX_20260207;

                yLine1 = MathF.Round(yLine1) + 0.5f;
                yLine2 = MathF.Round(yLine2) + 0.5f;

                using var textPen = new Pen(Color.Black, textStroke)
                {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round
                };

                g.DrawLine(textPen, x1, yLine1, x2, yLine1);

                float x2b = x1 + ((x2 - x1) * 0.70f);
                g.DrawLine(textPen, x1, yLine2, x2b, yLine2);
            }

            return bmp;
        }

        private static GraphicsPath CreateSpeechBubblePath(
            RectangleF bubbleRect,
            float radius,
            float tailCenterX,
            float tailHalfWidth,
            float tailTipY)
        {
            float left = bubbleRect.Left;
            float top = bubbleRect.Top;
            float right = bubbleRect.Right;
            float bottom = bubbleRect.Bottom;

            float d = radius * 2.0f;

            float tailBaseLeft = tailCenterX - tailHalfWidth;
            float tailBaseRight = tailCenterX + tailHalfWidth;

            float minBase = left + radius + 1.0f;
            float maxBase = right - radius - 1.0f;

            if (tailBaseLeft < minBase) tailBaseLeft = minBase;
            if (tailBaseRight > maxBase) tailBaseRight = maxBase;

            var p = new GraphicsPath();

            p.StartFigure();

            p.AddArc(left, top, d, d, 180.0f, 90.0f);
            p.AddArc(right - d, top, d, d, 270.0f, 90.0f);
            p.AddArc(right - d, bottom - d, d, d, 0.0f, 90.0f);

            p.AddLine(right - radius, bottom, tailBaseRight, bottom);
            p.AddLine(tailBaseRight, bottom, tailCenterX, tailTipY);
            p.AddLine(tailCenterX, tailTipY, tailBaseLeft, bottom);
            p.AddLine(tailBaseLeft, bottom, left + radius, bottom);

            p.AddArc(left, bottom - d, d, d, 90.0f, 90.0f);

            p.CloseFigure();

            return p;
        }

        private static Bitmap CreateStar5Icon(int padding, NodeVAlign align)
        {
            return CreateBitmap(padding, GetYOffset(padding, align), (g, rect) =>
            {
                float strokeW = Math.Max(1f, rect.Width / 14f);
                var r = InsetRectForStroke(rect, strokeW);

                var fill = Color.FromArgb(255, 255, 215, 0);
                var outline = Color.FromArgb(255, 25, 25, 25);

                float cx = r.Left + (r.Width * 0.50f);
                float cy = r.Top + (r.Height * 0.52f);

                float outerR = Math.Min(r.Width, r.Height) * 0.46f;
                float innerR = outerR * 0.46f;

                var pts = new PointF[10];
                float a0 = -90f * (MathF.PI / 180f);
                float step = (MathF.PI * 2f) / 10f;

                for (int i = 0; i < 10; i++)
                {
                    float rr = (i % 2 == 0) ? outerR : innerR;
                    float a = a0 + (i * step);
                    pts[i] = new PointF(
                        cx + (MathF.Cos(a) * rr),
                        cy + (MathF.Sin(a) * rr));
                }

                using (var gp = new GraphicsPath())
                {
                    gp.AddPolygon(pts);
                    gp.CloseFigure();

                    using (var b = new SolidBrush(fill))
                        g.FillPath(b, gp);

                    using (var p = new Pen(outline, strokeW) { LineJoin = LineJoin.Round })
                        g.DrawPath(p, gp);
                }
            });
        }

        private static Bitmap CreateTickIconAlpha(Color col, bool invert, int padding, NodeVAlign align = NodeVAlign.Center)
        {
            return SetBitmapAlpha(CreateTickIcon(col, invert, padding, align), 0.6f);
        }
        private static Bitmap SetBitmapAlpha(Bitmap source, float alpha)
        {
            if (alpha < 0f) alpha = 0f;
            if (alpha > 1f) alpha = 1f;

            var result = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);

            using (var g = Graphics.FromImage(result))
            using (var attrs = new ImageAttributes())
            {
                var matrix = new ColorMatrix { Matrix33 = alpha };
                attrs.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                g.DrawImage(
                    source,
                    new Rectangle(0, 0, source.Width, source.Height),
                    0, 0, source.Width, source.Height,
                    GraphicsUnit.Pixel,
                    attrs);
            }

            return result;
        }

        private static Bitmap CreateBitmap(
            int padding,
            int yOffset,
            Action<Graphics, Rectangle> render,
            bool enableTextAntialias = false)
        {
            var bmp = new Bitmap(IconSize, IconSize, PixelFormat.Format32bppArgb);

            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.Half;

                if (enableTextAntialias)
                    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                g.Clear(Color.Transparent);

                var rect = GetCircleRect(padding, yOffset);
                render(g, rect);
            }

            return bmp;
        }

        private static Rectangle InsetRectForStroke(Rectangle rect, float strokeWidth)
        {
            int inset = (int)Math.Ceiling(strokeWidth / 2f);
            if (inset <= 0)
                return rect;

            rect.Inflate(-inset, -inset);

            if (rect.Width < 1) rect.Width = 1;
            if (rect.Height < 1) rect.Height = 1;

            return rect;
        }
        private static Bitmap CreateSolidIcon(Color fill, Color? outline, string? text, int padding, NodeVAlign align)
        {
            float strokeW = outline != null ? 1f : 1f;

            return CreateBitmap(padding, GetYOffset(padding, align), (g, rect) =>
            {
                var r = InsetRectForStroke(rect, strokeW);

                using (var brush = new SolidBrush(fill))
                    g.FillEllipse(brush, r);

                using (var pen = new Pen(outline ?? fill, strokeW) { Alignment = PenAlignment.Inset })
                    g.DrawEllipse(pen, r);

                if (text == null)
                    return;

                using var textBrush = new SolidBrush(outline ?? Color.Gray);

                float fontSize = r.Height - 6;
                if (fontSize < 6f) fontSize = 6f;

                using var font = new Font(SystemFonts.DefaultFont.FontFamily, fontSize, FontStyle.Bold, GraphicsUnit.Pixel);

                var format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                g.DrawString(text, font, textBrush, r, format);
            }, enableTextAntialias: true);
        }
        private static Bitmap CreateTickIcon(Color col, bool invert, int padding, NodeVAlign align)
        {
            return CreateBitmap(padding, GetYOffset(padding, align), (g, rect) =>
            {
                var fillColor = invert ? BackgroundColour : col;
                var tickColor = invert ? col : BackgroundColour;

                using (var brush = new SolidBrush(fillColor))
                    g.FillEllipse(brush, rect);

                float borderW = Math.Max(1f, rect.Width / 10f);
                using (var borderPen = new Pen(col, borderW) { Alignment = PenAlignment.Inset })
                    g.DrawEllipse(borderPen, rect);

                float tickW = Math.Max(1f, rect.Width / 7f);
                var cap = tickW <= 1.25f ? LineCap.Square : LineCap.Round;

                float x = rect.Left;
                float y = rect.Top;
                float w = rect.Width;
                float h = rect.Height;

                var p1 = new PointF(x + w * 0.22f, y + h * 0.58f);
                var p2 = new PointF(x + w * 0.44f, y + h * 0.78f);
                var p3 = new PointF(x + w * 0.78f, y + h * 0.30f);

                using (var pen = new Pen(tickColor, tickW)
                {
                    StartCap = cap,
                    EndCap = cap,
                    LineJoin = LineJoin.Round
                })
                {
                    g.DrawLines(pen, new[] { p1, p2, p3 });
                }
            });
        }
        private static Bitmap CreateClapperBoardIcon(Color col, bool invert, int padding, NodeVAlign align, bool drawPlayGraphic = true)
        {
            return CreateBitmap(padding, GetYOffset(padding, align), (g, rect) =>
            {
                var boardColor = invert ? BackgroundColour : col;
                var stripeColor = invert ? col : Color.Transparent;

                static GraphicsPath RoundedRect(RectangleF r, float radius)
                {
                    var path = new GraphicsPath();

                    float d = radius * 2f;
                    if (d < 1f) d = 1f;
                    if (d > r.Width) d = r.Width;
                    if (d > r.Height) d = r.Height;

                    path.AddArc(r.X, r.Y, d, d, 180f, 90f);
                    path.AddArc(r.Right - d, r.Y, d, d, 270f, 90f);
                    path.AddArc(r.Right - d, r.Bottom - d, d, d, 0f, 90f);
                    path.AddArc(r.X, r.Bottom - d, d, d, 90f, 90f);
                    path.CloseFigure();

                    return path;
                }

                var bounds = (RectangleF)rect;
                float inset = Math.Max(1f, bounds.Width * 0.06f);
                bounds.Inflate(-inset, -inset);

                float x = bounds.Left;
                float y = bounds.Top;
                float w = bounds.Width;
                float h = bounds.Height;

                var bodyRect = new RectangleF(
                    x + w * 0.10f,
                    y + h * 0.42f,
                    w * 0.80f,
                    h * 0.50f);

                var topRect = new RectangleF(
                    x + w * 0.12f,
                    y + h * 0.18f,
                    w * 0.86f,
                    h * 0.22f);

                float bodyRadius = Math.Max(1f, bodyRect.Height * 0.22f);
                float topRadius = Math.Max(1f, topRect.Height * 0.55f);

                using var boardBrush = new SolidBrush(boardColor);

                using (var bodyPath = RoundedRect(bodyRect, bodyRadius))
                    g.FillPath(boardBrush, bodyPath);

                float hingeX = topRect.X;
                float hingeY = topRect.Y + topRect.Height * 0.90f;

                using var m = new Matrix();
                m.RotateAt(-18f, new PointF(hingeX, hingeY));

                using (var topPath = RoundedRect(topRect, topRadius))
                {
                    topPath.Transform(m);
                    g.FillPath(boardBrush, topPath);
                }

                float sy = topRect.Y + topRect.Height * 0.16f;
                float sh = topRect.Height * 0.52f;

                float sw = Math.Max(1f, topRect.Width * 0.15f);
                float gap = Math.Max(1f, topRect.Width * 0.075f);

                float startX = topRect.X + topRect.Width * 0.17f;
                float slant = topRect.Height * 0.28f;

                var prevComp = g.CompositingMode;
                var prevSmooth = g.SmoothingMode;
                var prevPix = g.PixelOffsetMode;

                for (int i = 0; i < 3; i++)
                {
                    float sx = startX + i * (sw + gap);

                    var pts = new[]
                    {
                        new PointF(sx + slant, sy),
                        new PointF(sx + sw + slant, sy),
                        new PointF(sx + sw, sy + sh),
                        new PointF(sx, sy + sh)
                    };

                    m.TransformPoints(pts);

                    var ip = new[]
                    {
                        new Point((int)Math.Round(pts[0].X), (int)Math.Round(pts[0].Y)),
                        new Point((int)Math.Round(pts[1].X), (int)Math.Round(pts[1].Y)),
                        new Point((int)Math.Round(pts[2].X), (int)Math.Round(pts[2].Y)),
                        new Point((int)Math.Round(pts[3].X), (int)Math.Round(pts[3].Y))
                    };

                    using var stripePath = new GraphicsPath();
                    stripePath.AddPolygon(ip);

                    g.SmoothingMode = SmoothingMode.None;
                    g.PixelOffsetMode = PixelOffsetMode.None;

                    if (stripeColor.A == 0)
                    {
                        g.CompositingMode = CompositingMode.SourceCopy;
                        using var clearBrush = new SolidBrush(Color.Transparent);
                        g.FillPath(clearBrush, stripePath);
                    }
                    else
                    {
                        g.CompositingMode = prevComp;
                        using var stripeBrush = new SolidBrush(stripeColor);
                        g.FillPath(stripeBrush, stripePath);
                    }
                }

                g.CompositingMode = prevComp;
                g.SmoothingMode = prevSmooth;
                g.PixelOffsetMode = prevPix;

                if (drawPlayGraphic)
                {
                    const float PLAY_SIZE_FRAC_OF_BODYH = 0.8f;
                    const float PLAY_INSET_FRAC = 0.22f;

                    // tweakables (fractions of play size "s")
                    const float PLAY_OFFSET_X_FRAC = -0.06f; // negative = further left
                    const float PLAY_OFFSET_Y_FRAC = -0.05f; // negative = further up

                    float s = MathF.Min(bodyRect.Width, bodyRect.Height) * PLAY_SIZE_FRAC_OF_BODYH;
                    if (s < 2f) s = 2f;

                    float cx = bodyRect.Left + bodyRect.Width * 0.56f;
                    float cy = bodyRect.Top + bodyRect.Height * 0.58f;

                    cx += s * PLAY_OFFSET_X_FRAC;
                    cy += s * PLAY_OFFSET_Y_FRAC;

                    float left = cx - (s * 0.50f);
                    float top = cy - (s * 0.50f);

                    float insetTri = s * PLAY_INSET_FRAC;

                    var p1 = new PointF(left + insetTri, top + insetTri);
                    var p2 = new PointF(left + insetTri, top + s - insetTri);
                    var p3 = new PointF(left + s - (insetTri * 0.55f), top + (s * 0.50f));

                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.PixelOffsetMode = PixelOffsetMode.Half;

                    if (!invert)
                    {
                        g.CompositingMode = CompositingMode.SourceCopy;
                        using var clear = new SolidBrush(Color.Transparent);
                        using var gp = new GraphicsPath();
                        gp.AddPolygon(new[] { p1, p2, p3 });
                        g.FillPath(clear, gp);
                        g.CompositingMode = prevComp;
                    }
                    else
                    {
                        using var b = new SolidBrush(col);
                        g.FillPolygon(b, new[] { p1, p2, p3 });
                    }

                    g.SmoothingMode = prevSmooth;
                    g.PixelOffsetMode = prevPix;
                }

            });
        }


        private static Bitmap CreateNoEntryIcon(Color col, bool invert, int padding, NodeVAlign align)
        {
            return CreateBitmap(padding, GetYOffset(padding, align), (g, rect) =>
            {
                var fillColor = invert ? BackgroundColour : col;
                var barColor = invert ? col : BackgroundColour;

                using (var brush = new SolidBrush(fillColor))
                    g.FillEllipse(brush, rect);

                float borderW = Math.Max(1f, rect.Width / 10f);
                using (var borderPen = new Pen(col, borderW) { Alignment = PenAlignment.Inset })
                    g.DrawEllipse(borderPen, rect);

                using (var path = new GraphicsPath())
                {
                    path.AddEllipse(rect);
                    g.SetClip(path);

                    int barHeight = Math.Max(1, (int)Math.Round(rect.Height / 6f));
                    int horizontalMargin = Math.Max(1, (int)Math.Round(rect.Width / 5f));

                    int bw = Math.Max(1, rect.Width - (2 * horizontalMargin));
                    int bx = rect.Left + horizontalMargin;
                    int by = rect.Top + (rect.Height - barHeight) / 2;

                    var barRect = new Rectangle(bx, by, bw, barHeight);

                    using (var barBrush = new SolidBrush(barColor))
                        g.FillRectangle(barBrush, barRect);

                    g.ResetClip();
                }
            });
        }

        private static Bitmap CreateCrossIcon(Color fg, bool invert, NodeVAlign align = NodeVAlign.Center)
            => CreateCrossIcon(fg, invert, DefaultIconPadding, align);

        private static Bitmap CreateCrossIcon(Color fg, bool invert, int padding, NodeVAlign align)
        {
            return CreateBitmap(padding, GetYOffset(padding, align), (g, rect) =>
            {
                var fillColor = invert ? BackgroundColour : fg;
                var crossColor = invert ? fg : BackgroundColour;

                using (var brush = new SolidBrush(fillColor))
                    g.FillEllipse(brush, rect);

                using (var path = new GraphicsPath())
                {
                    path.AddEllipse(rect);
                    g.SetClip(path);

                    int inset = Math.Max(1, (int)Math.Round(rect.Width / 6f));
                    float crossW = Math.Max(1f, rect.Width / 6f);

                    using (var pen = new Pen(crossColor, crossW) { StartCap = LineCap.Square, EndCap = LineCap.Square })
                    {
                        g.DrawLine(pen, rect.Left + inset, rect.Top + inset, rect.Right - inset, rect.Bottom - inset);
                        g.DrawLine(pen, rect.Right - inset, rect.Top + inset, rect.Left + inset, rect.Bottom - inset);
                    }

                    g.ResetClip();
                }

                float borderW = Math.Max(1f, rect.Width / 10f);
                using (var borderPen = new Pen(fg, borderW) { Alignment = PenAlignment.Inset })
                    g.DrawEllipse(borderPen, rect);
            });
        }

        private static Bitmap CreateQuestionIcon(Color col, bool invert, NodeVAlign align = NodeVAlign.Center)
            => CreateQuestionIcon(col, invert, DefaultIconPadding, align);

        private static Bitmap CreateQuestionIcon(Color col, bool invert, int padding, NodeVAlign align)
        {
            return CreateBitmap(padding, GetYOffset(padding, align), (g, rect) =>
            {
                var fillColor = invert ? BackgroundColour : col;
                var fgColor = invert ? col : BackgroundColour;

                using (var brush = new SolidBrush(fillColor))
                    g.FillEllipse(brush, rect);

                float borderW = Math.Max(1f, rect.Width / 10f);
                using (var borderPen = new Pen(col, borderW) { Alignment = PenAlignment.Inset })
                    g.DrawEllipse(borderPen, rect);

                var textRect = RectangleF.Inflate(rect, -borderW * 1.25f, -borderW * 1.25f);

                float fontSize = Math.Max(6f, textRect.Height * 0.92f);

                using var textBrush = new SolidBrush(fgColor);
                using var font = new Font(SystemFonts.DefaultFont.FontFamily, fontSize, FontStyle.Bold, GraphicsUnit.Pixel);

                using var format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                g.DrawString("?", font, textBrush, textRect, format);
            }, enableTextAntialias: true);
        }

        private static Rectangle GetCircleRect(int padding, int yOffset)
        {
            int p = Math.Max(0, padding);
            int w = IconSize - (p * 2);
            if (w < 1) w = 1;

            int x = p;
            int y = yOffset;

            int minY = 0;
            int maxY = IconSize - w;

            if (y < minY) y = minY;
            if (y > maxY) y = maxY;

            return new Rectangle(x, y, w, w);
        }
        private static Bitmap CreateClapperBoardUnavailableIcon(Color boardColor, bool invert, int padding, NodeVAlign align)
        {
            var bmp = new Bitmap(IconSize, IconSize, PixelFormat.Format32bppArgb);

            using var boardBmp = CreateClapperBoardIcon(boardColor, invert, padding, align, drawPlayGraphic: true);
            using var g = Graphics.FromImage(bmp);

            g.Clear(Color.Transparent);
            g.CompositingMode = CompositingMode.SourceOver;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            g.DrawImageUnscaled(boardBmp, 0, 0);

            var unavailableRed = Color.FromArgb(255, 235, 32, 32);

            float outerInset = MathF.Max(1f, IconSize * 0.07f);
            var outer = new RectangleF(
                outerInset,
                outerInset,
                IconSize - (outerInset * 2f),
                IconSize - (outerInset * 2f));

            float ringThickness = MathF.Max(2f, outer.Width * 0.12f);

            var inner = outer;
            inner.Inflate(-ringThickness, -ringThickness);

            using var ringPath = new GraphicsPath(FillMode.Alternate);
            ringPath.AddEllipse(outer);
            ringPath.AddEllipse(inner);

            using var redBrush = new SolidBrush(unavailableRed);
            g.FillPath(redBrush, ringPath);

            float slashWidth = outer.Width * 0.88f;
            float slashHeight = MathF.Max(2.5f, outer.Width * 0.15f);

            var slashRect = new RectangleF(
                (IconSize - slashWidth) * 0.5f,
                (IconSize - slashHeight) * 0.5f,
                slashWidth,
                slashHeight);

            using var slashPath = CreateCapsulePath(slashRect);
            using var rotate = new Matrix();

            rotate.RotateAt(-38f, new PointF(IconSize * 0.5f, IconSize * 0.5f));
            slashPath.Transform(rotate);

            using var clipPath = new GraphicsPath();
            clipPath.AddEllipse(outer);

            var state = g.Save();
            g.SetClip(clipPath);
            g.FillPath(redBrush, slashPath);
            g.Restore(state);

            return bmp;
        }

        private static GraphicsPath CreateCapsulePath(RectangleF rect)
        {
            var path = new GraphicsPath();

            float d = MathF.Min(rect.Width, rect.Height);
            if (d < 1f)
                d = 1f;

            path.AddArc(rect.Left, rect.Top, d, d, 90f, 180f);
            path.AddArc(rect.Right - d, rect.Top, d, d, 270f, 180f);
            path.CloseFigure();

            return path;
        }
        private static int GetYOffset(int padding, NodeVAlign align)
        {
            return GetIconContentYOffset(IconSize, padding, align);
        }

        private static int GetIconContentYOffset(int iconSize, int padding, NodeVAlign align)
        {
            int p = Math.Max(0, padding);

            int w = iconSize - (p * 2);
            if (w < 1) w = 1;

            int maxY = iconSize - w;

            int y;
            switch (align)
            {
                case NodeVAlign.Top:
                    y = 0;
                    y += 1;
                    break;

                case NodeVAlign.Bottom:
                    y = maxY;
                    y -= 1;
                    break;

                default:
                    y = maxY / 2;
                    break;
            }

            if (y < 0) y = 0;
            if (y > maxY) y = maxY;

            return y;
        }
    }
}
