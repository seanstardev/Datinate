using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace datinate.app
{
    public sealed class DragDropPreviewForm : Form
    {
        private enum INSERTION_POINT_ENUM
        {
            Before,
            None,
            After
        }
        protected override bool ShowWithoutActivation => true;

        private static DragDropPreviewForm? Active;

        private Bitmap? image;

        private const int MaxPreviewHeightPx = 600;         // Generic control capture cap
        private const int MaxPreviewWidthPx = 600;          // Hard cap for preview bitmap width; anything wider is cropped + right-fade hint may show.

        private const bool ApplyScreenClamping = false;

        private const int CaptionLineGapPx = 6;             // gap between hint line 1 and line 2

        private const int MaxRowsToCapture = 6;             // Max number of visible TreeView rows captured into the preview (starting at the dragged node).
        private const int CaptureImagePadding = 2;          // Vertical padding (px) added above/below the captured rows when cropping the screenshot.

        private const int MouseOffset = 6;                  // Cursor offset (px) so the preview window doesn’t sit directly under the mouse pointer.

        private const byte PreviewImageAlpha = 255;         // Global alpha (0–255) applied to the preview bitmap when drawn onto the form.

        private const int FadeSizePx = 70;                  // Size (px) of the right/bottom gradient fade overlay when content continues off-capture.

        private const int BorderAlpha = 20;                 // Border alpha (0–255) for the preview window outline.
        private const float BorderWidth = 1f;               // Border thickness (px) for the preview window outline.

        private const int CropPadLeftPx = 2;                // Extra left padding (px) included when computing the crop X for captured rows.
        private const bool CropIncludeNodeImage = true;     // Whether to include the TreeView node image/state image area in the crop region.
        private const int NodeImageGapPx = 3;               // Gap (px) assumed between node images and text when expanding crop to include icons.

        private const int MinPreviewWidthPx = 20;           // Minimum content width (px) for the form, even if the captured bitmap is narrower.

        private const int CaptionGapTopPx = 6;              // Gap (px) between the bottom of the bitmap and the start of the caption block.
        private const int CaptionPadLeftPx = 4;             // Left padding (px) inside the caption block.
        private const int CaptionPadRightPx = 10;           // Right padding (px) inside the caption block (helps ellipsis/breathing room).
        private const int CaptionPadTopPx = 4;              // Top padding (px) inside the caption block before the first caption line.
        private const int CaptionPadBottomPx = 6;           // Bottom padding (px) inside the caption block after the second caption line.

        private string captionLine1 = string.Empty;
        private string captionLine2 = string.Empty;

        private readonly Font captionFont1;
        private readonly Font captionFont2;
        private readonly object captionSync = new object();
        private volatile int captionDirty;
        private const int CaptionDebounceMs = 200;

        private const int CaptionChangeAnimDurationMs = 360;
        private const int CaptionChangeAnimMaxAlpha = 120;
        private const int CaptionAnimTimerIntervalMs = 15;

        private long captionAnimStartTicks;
        private bool captionAnimActive;

        private string pendingCaptionLine1 = string.Empty;
        private string pendingCaptionLine2 = string.Empty;

        private bool hasMoreRight;
        private bool hasMoreBottom;
        private Point lastCursor;

        private System.Windows.Forms.Timer? followTimer;
        private System.Windows.Forms.Timer? clearCaptionTimer;
        private System.Windows.Forms.Timer? captionAnimTimer;

        public static void Initialise(Control control, string? line1 = null, string? line2 = null)
        {
            if (control == null)
                throw new ArgumentNullException(nameof(control));

            Teardown();

            var f = new DragDropPreviewForm();
            Active = f;

            var owner = control.FindForm();
            if (owner != null)
                f.Owner = owner;

            var cap = CaptureControlSnippet(control);
            f.SetImage(cap.Bitmap, cap.HasMoreRight, cap.HasMoreBottom);

            f.UpdateCaptionInternal(line1, line2);
            f.BeginFollowCursor();

            if (owner != null)
                f.Show(owner);
            else
                f.Show();

            f.BeginPatchWebView2Async(control);
        }

        private bool HasCaption()
        {
            lock (captionSync)
            {
                return
                    !string.IsNullOrWhiteSpace(captionLine1) ||
                    !string.IsNullOrWhiteSpace(captionLine2);
            }
        }

        private Rectangle GetCaptionAreaBounds()
        {
            if (image == null || !HasCaption())
                return Rectangle.Empty;

            int x = 1;
            int y = 1 + image.Height;
            int w = Math.Max(1, ClientSize.Width - 2);
            int h = Math.Max(1, ClientSize.Height - 1 - y);

            return new Rectangle(x, y, w, h);
        }

        private void EnsureCaptionAnimTimer()
        {
            if (captionAnimTimer != null)
                return;

            captionAnimTimer = new System.Windows.Forms.Timer
            {
                Interval = CaptionAnimTimerIntervalMs
            };

            captionAnimTimer.Tick += CaptionAnimTimer_Tick;
        }

        private void StopCaptionAnimTimer()
        {
            if (captionAnimTimer == null)
                return;

            captionAnimTimer.Stop();
        }

        private void StopCaptionChangeAnimation()
        {
            captionAnimActive = false;
            StopCaptionAnimTimer();
        }

        private void StartCaptionChangeAnimation()
        {
            if (IsDisposed)
                return;

            if (!HasCaption())
                return;

            EnsureCaptionAnimTimer();

            captionAnimStartTicks = Environment.TickCount64;
            captionAnimActive = true;
            captionAnimTimer!.Start();

            var r = GetCaptionAreaBounds();
            if (!r.IsEmpty)
                Invalidate(r);
            else
                Invalidate();
        }

        private int GetCaptionChangeAnimAlpha()
        {
            if (!captionAnimActive)
                return 0;

            long elapsed = Environment.TickCount64 - captionAnimStartTicks;
            if (elapsed <= 0)
                return CaptionChangeAnimMaxAlpha;

            if (elapsed >= CaptionChangeAnimDurationMs)
            {
                captionAnimActive = false;
                StopCaptionAnimTimer();
                return 0;
            }

            float t = elapsed / (float)CaptionChangeAnimDurationMs;
            float eased = 1f - t;
            eased *= eased;

            return Math.Max(0, Math.Min(255, (int)Math.Round(CaptionChangeAnimMaxAlpha * eased)));
        }

        private void EnsureClearCaptionTimer()
        {
            if (clearCaptionTimer != null)
                return;

            clearCaptionTimer = new System.Windows.Forms.Timer
            {
                Interval = CaptionDebounceMs
            };

            clearCaptionTimer.Tick += ClearCaptionTimer_Tick;
        }

        private static Bitmap CaptureTreeRectBitmap(TreeView tv, Rectangle rect)
        {
            var safe = Rectangle.Intersect(new Rectangle(Point.Empty, tv.ClientSize), rect);

            using var full = new Bitmap(
                Math.Max(1, tv.ClientSize.Width),
                Math.Max(1, tv.ClientSize.Height),
                PixelFormat.Format32bppPArgb);

            bool captured = false;

            using (var g = Graphics.FromImage(full))
            {
                var hdc = g.GetHdc();
                try
                {
                    if (tv.IsHandleCreated && PrintWindow(tv.Handle, hdc, 0x00000001))
                        captured = true;
                }
                finally
                {
                    g.ReleaseHdc(hdc);
                }
            }

            if (!captured)
            {
                using var g = Graphics.FromImage(full);
                tv.DrawToBitmap(full, new Rectangle(Point.Empty, full.Size));
            }

            var bmp = new Bitmap(
                Math.Max(1, safe.Width),
                Math.Max(1, safe.Height),
                PixelFormat.Format32bppPArgb);

            using (var g = Graphics.FromImage(bmp))
            {
                g.DrawImage(
                    full,
                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                    safe,
                    GraphicsUnit.Pixel);
            }

            return bmp;
        }

        private void StopClearCaptionTimer()
        {
            if (clearCaptionTimer == null)
                return;

            clearCaptionTimer.Stop();
        }

        private void ClearCaptionTimer_Tick(object? sender, EventArgs e)
        {
            if (clearCaptionTimer != null)
                clearCaptionTimer.Stop();

            CommitPendingCaption();
        }

        private static bool IsCaptionBlank(string? line1, string? line2)
        {
            return string.IsNullOrWhiteSpace(line1) &&
                   string.IsNullOrWhiteSpace(line2);
        }

        private void CommitPendingCaption()
        {
            if (IsDisposed)
                return;

            bool blank;
            lock (captionSync)
            {
                captionLine1 = pendingCaptionLine1;
                captionLine2 = pendingCaptionLine2;
                captionDirty = 1;

                blank = IsCaptionBlank(captionLine1, captionLine2);
            }

            if (blank)
                StopCaptionChangeAnimation();

            ApplyCaptionIfDirty();
        }

        private void BeginPatchWebView2Async(Control root)
        {
            if (image == null)
                return;

            var webViews = new List<WebView2>();
            FindWebView2(root, webViews);

            if (webViews.Count == 0)
                return;

            var baseImage = image;

            try
            {
                root.BeginInvoke(new Action(async () =>
                {
                    try
                    {
                        if (IsDisposed)
                            return;

                        var patched = await PatchWebView2IntoBitmapAsync(root, baseImage, webViews);
                        if (patched == null)
                            return;

                        if (IsDisposed)
                        {
                            patched.Dispose();
                            return;
                        }

                        SetImage(patched, hasMoreRight, hasMoreBottom);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DragPreview] WebView2 patch failed: {ex.Message}");
                    }
                }));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DragPreview] BeginInvoke failed: {ex.Message}");
            }
        }

        private static void FindWebView2(Control root, List<WebView2> list)
        {
            for (int i = 0; i < root.Controls.Count; i++)
            {
                var c = root.Controls[i];

                if (c is WebView2 wv)
                    list.Add(wv);

                if (c.HasChildren)
                    FindWebView2(c, list);
            }
        }

        private static CaptureResult CaptureControlSnippet(Control c)
        {
            using var full = CaptureControlClientBitmap(c);

            int fullW = full.Width;
            int fullH = full.Height;

            int cropW = Math.Max(1, Math.Min(fullW, MaxPreviewWidthPx));
            int cropH = Math.Max(1, Math.Min(fullH, MaxPreviewHeightPx));

            bool hasMoreRight = fullW > cropW;
            bool hasMoreBottom = fullH > cropH;

            var rect = new Rectangle(0, 0, cropW, cropH);
            var cropped = CropBitmap(full, rect);

            return new CaptureResult(cropped, hasMoreRight, hasMoreBottom);
        }

        private static Bitmap CaptureControlClientBitmap(Control c)
        {
            var size = c.ClientSize;

            var bmp = new Bitmap(
                Math.Max(1, size.Width),
                Math.Max(1, size.Height),
                PixelFormat.Format32bppPArgb);

            using (var g = Graphics.FromImage(bmp))
            {
                var hdc = g.GetHdc();
                try
                {
                    if (c.IsHandleCreated && PrintWindow(c.Handle, hdc, 0x00000001))
                        return bmp;
                }
                finally
                {
                    g.ReleaseHdc(hdc);
                }

                var screenPt = c.PointToScreen(Point.Empty);
                g.CopyFromScreen(screenPt, Point.Empty, size);
            }

            return bmp;
        }

        private static async Task<Bitmap?> PatchWebView2IntoBitmapAsync(Control root, Bitmap baseBitmap, List<WebView2> webViews)
        {
            var result = (Bitmap)baseBitmap.Clone();

            using var g = Graphics.FromImage(result);
            g.CompositingMode = CompositingMode.SourceOver;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;

            for (int i = 0; i < webViews.Count; i++)
            {
                var wv = webViews[i];
                if (wv.IsDisposed || !wv.IsHandleCreated)
                    continue;

                Bitmap? wvBmp = null;

                try
                {
                    wvBmp = await CaptureWebView2BitmapAsync(wv);
                    if (wvBmp == null)
                        continue;

                    var webRectScreen = wv.RectangleToScreen(wv.ClientRectangle);
                    var webRectInRoot = root.RectangleToClient(webRectScreen);

                    var clip = Rectangle.Intersect(new Rectangle(Point.Empty, result.Size), webRectInRoot);
                    if (clip.Width <= 0 || clip.Height <= 0)
                        continue;

                    g.SetClip(clip);
                    g.DrawImage(wvBmp, webRectInRoot);
                    g.ResetClip();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[DragPreview] WebView2 capture failed: {ex.Message}");
                }
                finally
                {
                    wvBmp?.Dispose();
                }
            }

            return result;
        }

        private static async Task<Bitmap?> CaptureWebView2BitmapAsync(WebView2 wv)
        {
            if (wv.IsDisposed || !wv.IsHandleCreated)
                return null;

            if (wv.CoreWebView2 == null)
                await wv.EnsureCoreWebView2Async();

            var core = wv.CoreWebView2;
            if (core == null)
                return null;

            using var ms = new MemoryStream();
            await core.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, ms);

            ms.Position = 0;
            using var img = Image.FromStream(ms);
            return new Bitmap(img);
        }

        public static void Initialise(Bitmap bitmap, string? line1 = null, string? line2 = null)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            Teardown();

            var f = new DragDropPreviewForm();
            Active = f;

            f.SetImage((Bitmap)bitmap.Clone(), hasMoreRight: false, hasMoreBottom: false);
            f.UpdateCaptionInternal(line1, line2);
            f.BeginFollowCursor();
            f.Show();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x00020000;
                const int WS_EX_NOACTIVATE = 0x08000000;
                const int WS_EX_TOOLWINDOW = 0x00000080;
                const int WS_EX_TRANSPARENT = 0x00000020;

                var cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                cp.ExStyle |= WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW | WS_EX_TRANSPARENT;
                return cp;
            }
        }
        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x0084;
            const int HTTRANSPARENT = -1;

            if (m.Msg == WM_NCHITTEST)
            {
                m.Result = (IntPtr)HTTRANSPARENT;
                return;
            }

            base.WndProc(ref m);
        }
        private readonly struct CaptureResult
        {
            public CaptureResult(Bitmap bitmap, bool hasMoreRight, bool hasMoreBottom)
            {
                Bitmap = bitmap;
                HasMoreRight = hasMoreRight;
                HasMoreBottom = hasMoreBottom;
            }

            public Bitmap Bitmap { get; }
            public bool HasMoreRight { get; }
            public bool HasMoreBottom { get; }
        }

        private DragDropPreviewForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = false;
            DoubleBuffered = true;

            float fontSize1 = 12;
            float fontSize2 = 10;

            var baseFont = ResolveBaseFont(SystemFonts.MessageBoxFont);

            captionFont1 = new Font(baseFont.FontFamily, fontSize1, FontStyle.Bold, GraphicsUnit.Point);
            captionFont2 = new Font(baseFont.FontFamily, fontSize2, FontStyle.Regular, GraphicsUnit.Point);
        }

        public static void Initialise(TreeView treeView, TreeNode node)
        {
            Teardown();

            var f = new DragDropPreviewForm();
            Active = f;

            var owner = treeView.FindForm();
            if (owner != null)
                f.Owner = owner;

            var cap = CaptureEntitySnippet(treeView, node);
            f.SetImage(cap.Bitmap, cap.HasMoreRight, cap.HasMoreBottom);

            f.UpdateCaptionInternal(null, null);

            f.BeginFollowCursor();

            if (owner != null)
                f.Show(owner);
            else
                f.Show();
        }

        private void BeginFollowCursor()
        {
            followTimer ??= new System.Windows.Forms.Timer { Interval = 33 };
            followTimer.Tick -= FollowTimer_Tick;
            followTimer.Tick += FollowTimer_Tick;

            lastCursor = Cursor.Position;

            followTimer.Start();

            MoveNear(lastCursor, MouseOffset, MouseOffset);
        }

        public static void Teardown()
        {
            var f = Active;
            if (f == null)
                return;

            if (f.IsDisposed)
            {
                Active = null;
                return;
            }

            if (f.InvokeRequired)
            {
                try { f.BeginInvoke(Teardown); }
                catch { Active = null; }
                return;
            }
            Active = null;

            try { f.Close(); } catch { }
            try { f.Dispose(); } catch { }
        }

        public static void UpdateCaption(string? line1, string? line2 = null)
        {
            var f = Active;
            if (f == null || f.IsDisposed)
                return;

            f.UpdateCaptionInternal(line1, line2);
        }

        public static void UpdatePosition(Point cursorScreenLocation)
        {
            UpdatePositionCore(cursorScreenLocation);
        }

        private static void UpdatePositionCore(Point cursorScreenLocation)
        {
            var f = Active;
            if (f == null || f.IsDisposed)
                return;

            if (f.InvokeRequired)
            {
                try
                {
                    f.BeginInvoke(new Action(() => UpdatePositionCore(cursorScreenLocation)));
                }
                catch { }

                return;
            }

            f.lastCursor = cursorScreenLocation;
            f.MoveNear(cursorScreenLocation, MouseOffset, MouseOffset);
        }

        private static void UpdatePositionCore(
            Point cursorScreenLocation,
            TreeView? hoverTreeView,
            TreeNode? hoverNode)
        {
            var f = Active;
            if (f == null || f.IsDisposed)
                return;

            if (f.InvokeRequired)
            {
                try
                {
                    f.BeginInvoke(new Action(() =>
                        UpdatePositionCore(cursorScreenLocation, hoverTreeView, hoverNode)));
                }
                catch { }
                return;
            }

            f.MoveNear(cursorScreenLocation, MouseOffset, MouseOffset);
        }

        private void UpdateCaptionInternal(string? line1, string? line2 = null)
        {
            if (IsDisposed)
                return;

            void UpdatePending()
            {
                string newLine1 = line1 ?? string.Empty;
                string newLine2 = line2 ?? string.Empty;

                bool same;

                lock (captionSync)
                {
                    same =
                        string.Equals(captionLine1, newLine1, StringComparison.Ordinal) &&
                        string.Equals(captionLine2, newLine2, StringComparison.Ordinal) &&
                        string.Equals(pendingCaptionLine1, newLine1, StringComparison.Ordinal) &&
                        string.Equals(pendingCaptionLine2, newLine2, StringComparison.Ordinal);

                    if (!same)
                    {
                        pendingCaptionLine1 = newLine1;
                        pendingCaptionLine2 = newLine2;
                    }
                }

                if (same)
                    return;

                bool blank = IsCaptionBlank(newLine1, newLine2);

                if (!blank)
                {
                    StopClearCaptionTimer();

                    lock (captionSync)
                    {
                        captionLine1 = pendingCaptionLine1;
                        captionLine2 = pendingCaptionLine2;
                        captionDirty = 1;
                    }

                    ApplyCaptionIfDirty();
                    StartCaptionChangeAnimation();
                    return;
                }

                EnsureClearCaptionTimer();

                if (!clearCaptionTimer!.Enabled)
                    clearCaptionTimer.Start();
            }

            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(new Action(UpdatePending));
                }
                catch { }

                return;
            }

            UpdatePending();
        }
        private static Font ResolveBaseFont(Font? preferred)
        {
            if (preferred is not null)
            {
                try
                {
                    _ = preferred.FontFamily;
                    return preferred;
                }
                catch { }
            }
            return SystemFonts.MessageBoxFont ?? SystemFonts.DefaultFont;
        }

        private void ApplyCaptionIfDirty()
        {
            if (IsDisposed)
                return;

            if (captionDirty == 0)
                return;

            captionDirty = 0;

            ApplyLayout();
            Invalidate();
        }

        private void SetImage(Bitmap bmp, bool hasMoreRight, bool hasMoreBottom)
        {
            image?.Dispose();
            image = bmp;

            this.hasMoreRight = hasMoreRight;
            this.hasMoreBottom = hasMoreBottom;

            ApplyLayout();
            Invalidate();
        }

        private int GetCaptionHeightPx(int contentWidth)
        {
            var textW = Math.Max(1, contentWidth - (CaptionPadLeftPx + CaptionPadRightPx));

            int h1 = TextRenderer.MeasureText("Mg", captionFont1, new Size(textW, 10_000), TextFormatFlags.NoPadding).Height;
            int h2 = TextRenderer.MeasureText("Mg", captionFont2, new Size(textW, 10_000), TextFormatFlags.NoPadding).Height;

            return CaptionGapTopPx + CaptionPadTopPx + h1 + CaptionLineGapPx + h2 + CaptionPadBottomPx;
        }

        private void ApplyLayout()
        {
            if (image == null)
                return;

            int contentW = Math.Max(image.Width, MinPreviewWidthPx);
            contentW = Math.Min(contentW, MaxPreviewWidthPx);

            int captionH = HasCaption() ? GetCaptionHeightPx(contentW) : 0;

            int contentH = image.Height + captionH;

            ClientSize = new Size(
                Math.Max(1, contentW + 2),
                Math.Max(1, contentH + 2));
        }

        private void StopFollowCursor()
        {
            if (followTimer == null)
                return;

            followTimer.Stop();
            followTimer.Tick -= FollowTimer_Tick;
            followTimer.Dispose();
            followTimer = null;
        }

        private void MoveNear(Point cursorScreenLocation, int offsetX, int offsetY)
        {
            if (!ApplyScreenClamping)
                Location = new Point(cursorScreenLocation.X + offsetX, cursorScreenLocation.Y + offsetY);
            else
            {
#pragma warning disable CS0162
                var wa = Screen.FromPoint(cursorScreenLocation).WorkingArea;
#pragma warning restore CS0162

                int x = cursorScreenLocation.X + offsetX;
                int y = cursorScreenLocation.Y + offsetY;

                x = Math.Min(x, wa.Right - Width);
                y = Math.Min(y, wa.Bottom - Height);
                x = Math.Max(x, wa.Left);
                y = Math.Max(y, wa.Top);

                Location = new Point(x, y);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (image == null)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(Color.Transparent);

            using (var back = new SolidBrush(Color.White))
                e.Graphics.FillRectangle(back, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);

            DrawImageWithAlpha(e.Graphics, image, new Point(1, 1), PreviewImageAlpha);

            var fadePx = FadeSizePx;

            if (hasMoreRight)
            {
                int w = Math.Max(1, Math.Min(fadePx, image.Width));
                var r = new Rectangle(1 + image.Width - w, 1, w, image.Height);

                using var brush = new LinearGradientBrush(
                    r,
                    Color.FromArgb(0, 255, 255, 255),
                    Color.FromArgb(255, 255, 255, 255),
                    LinearGradientMode.Horizontal);

                e.Graphics.FillRectangle(brush, r);
            }

            if (hasMoreBottom)
            {
                int h = Math.Max(1, Math.Min(fadePx, image.Height));
                var r = new Rectangle(1, 1 + image.Height - h, image.Width, h);

                using var brush = new LinearGradientBrush(
                    r,
                    Color.FromArgb(0, 255, 255, 255),
                    Color.FromArgb(255, 255, 255, 255),
                    LinearGradientMode.Vertical);

                e.Graphics.FillRectangle(brush, r);
            }

            if (HasCaption())
            {
                int dividerY = 1 + image.Height + (CaptionGapTopPx / 2);
                using var dividerPen = new Pen(Color.FromArgb(40, 0, 0, 0), 1f);
                e.Graphics.DrawLine(dividerPen, 1, dividerY, ClientSize.Width - 2, dividerY);

                int contentW = Math.Max(1, ClientSize.Width - 2);
                int textW = contentW;
                int x = 1;
                int y = 1 + image.Height + CaptionGapTopPx + CaptionPadTopPx;

                var flags =
                    TextFormatFlags.NoPrefix |
                    TextFormatFlags.SingleLine |
                    TextFormatFlags.EndEllipsis |
                    TextFormatFlags.HorizontalCenter;

                int h1 = TextRenderer.MeasureText("Mg", captionFont1, new Size(textW, 10_000), TextFormatFlags.NoPadding).Height;
                int h2 = TextRenderer.MeasureText("Mg", captionFont2, new Size(textW, 10_000), TextFormatFlags.NoPadding).Height;

                var r1 = new Rectangle(x, y, textW, h1);
                var r2 = new Rectangle(x, y + h1 + CaptionLineGapPx, textW, h2);

                string l1, l2;
                lock (captionSync)
                {
                    l1 = captionLine1;
                    l2 = captionLine2;
                }

                int captionAnimAlpha = GetCaptionChangeAnimAlpha();
                if (captionAnimAlpha > 0)
                {
                    var captionArea = GetCaptionAreaBounds();

                    if (!captionArea.IsEmpty)
                    {
                        using var pulseBrush = new LinearGradientBrush(
                            captionArea,
                            Color.FromArgb(captionAnimAlpha, 170, 210, 255),
                            Color.FromArgb(Math.Max(0, captionAnimAlpha / 4), 235, 244, 255),
                            LinearGradientMode.Vertical);

                        e.Graphics.FillRectangle(pulseBrush, captionArea);
                    }
                }

                TextRenderer.DrawText(e.Graphics, l1, captionFont1, r1, Color.Black, flags);
                TextRenderer.DrawText(e.Graphics, l2, captionFont2, r2, SystemColors.GrayText, flags);
            }

            using var pen = new Pen(Color.FromArgb(BorderAlpha, 0, 0, 0), BorderWidth);
            e.Graphics.DrawRectangle(pen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopFollowCursor();
                StopCaptionChangeAnimation();

                if (captionAnimTimer != null)
                {
                    captionAnimTimer.Stop();
                    captionAnimTimer.Tick -= CaptionAnimTimer_Tick;
                    captionAnimTimer.Dispose();
                    captionAnimTimer = null;
                }

                if (clearCaptionTimer != null)
                {
                    clearCaptionTimer.Stop();
                    clearCaptionTimer.Tick -= ClearCaptionTimer_Tick;
                    clearCaptionTimer.Dispose();
                    clearCaptionTimer = null;
                }

                image?.Dispose();
                captionFont1.Dispose();
                captionFont2.Dispose();
            }

            base.Dispose(disposing);
        }

        private void CaptionAnimTimer_Tick(object? sender, EventArgs e)
        {
            if (IsDisposed)
            {
                StopCaptionChangeAnimation();
                return;
            }

            if (!captionAnimActive)
            {
                StopCaptionAnimTimer();
                return;
            }

            var alpha = GetCaptionChangeAnimAlpha();
            var r = GetCaptionAreaBounds();

            if (alpha <= 0 || r.IsEmpty)
            {
                StopCaptionChangeAnimation();
                if (!r.IsEmpty)
                    Invalidate(r);
                return;
            }

            Invalidate(r);
        }

        private void FollowTimer_Tick(object? sender, EventArgs e)
        {
            var p = Cursor.Position;
            if (p != lastCursor)
            {
                lastCursor = p;
                MoveNear(p, MouseOffset, MouseOffset);
            }

            ApplyCaptionIfDirty();
        }

        private static int GetCropLeftX(TreeView tv, TreeNode anchor)
        {
            int x = anchor.Bounds.Left;

            if (CropIncludeNodeImage)
            {
                var imgW = tv.ImageList?.ImageSize.Width ?? 0;
                var stateW = tv.StateImageList?.ImageSize.Width ?? 0;

                if (imgW > 0) x -= (imgW + NodeImageGapPx);
                if (stateW > 0) x -= (stateW + NodeImageGapPx);
            }

            x -= CropPadLeftPx;
            return Math.Max(0, x);
        }

        private static void DrawImageWithAlpha(Graphics g, Image img, Point dest, byte alpha)
        {
            if (alpha >= 255)
            {
                g.DrawImageUnscaled(img, dest);
                return;
            }

            using var ia = new ImageAttributes();
            var cm = new ColorMatrix
            {
                Matrix00 = 1f,
                Matrix11 = 1f,
                Matrix22 = 1f,
                Matrix33 = alpha / 255f,
                Matrix44 = 1f
            };

            ia.SetColorMatrix(cm, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            var destRect = new Rectangle(dest.X, dest.Y, img.Width, img.Height);
            g.DrawImage(img, destRect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);
        }

        private static CaptureResult CaptureEntitySnippet(TreeView tv, TreeNode familyNode)
        {
            var nodes = new List<TreeNode>(Math.Max(1, MaxRowsToCapture));

            var client = tv.ClientRectangle;
            var clientBottom = client.Bottom;

            var cur = familyNode;

            for (int i = 0; i < MaxRowsToCapture; i++)
            {
                var b = cur.Bounds;

                if (b.Height > 0 && b.Top < clientBottom)
                    nodes.Add(cur);

                var next = cur.NextVisibleNode;
                if (next == null || !IsDescendantOf(familyNode, next))
                    break;

                var nb = next.Bounds;
                if (nb.Height <= 0 || nb.Top >= clientBottom)
                    break;

                cur = next;
            }

            if (nodes.Count == 0)
                nodes.Add(familyNode);

            var last = nodes[nodes.Count - 1];
            var nextAfterLast = last.NextVisibleNode;

            bool hasMoreBottom = nextAfterLast != null && IsDescendantOf(familyNode, nextAfterLast);

            var rect = ComputeRowsRect(tv, nodes, CaptureImagePadding);

            bool hasMoreRight = rect.Width > Math.Max(1, MaxPreviewWidthPx);

            rect = new Rectangle(
                rect.X,
                rect.Y,
                Math.Min(rect.Width, Math.Max(1, MaxPreviewWidthPx)),
                rect.Height);

            var cropped = CaptureTreeRectBitmap(tv, rect);

            return new CaptureResult(cropped, hasMoreRight, hasMoreBottom);
        }

        private static Rectangle ComputeRowsRect(TreeView tv, List<TreeNode> rows, int padY)
        {
            var client = tv.ClientRectangle;

            int top = int.MaxValue;
            int bottom = int.MinValue;

            for (int i = 0; i < rows.Count; i++)
            {
                var b = rows[i].Bounds;
                if (b.Height <= 0)
                    continue;

                if (b.Top < top) top = b.Top;
                if (b.Bottom > bottom) bottom = b.Bottom;
            }

            if (top == int.MaxValue || bottom == int.MinValue)
                return new Rectangle(0, 0, Math.Max(1, client.Width), Math.Max(1, tv.ItemHeight));

            top = Math.Max(client.Top, top - padY);
            bottom = Math.Min(client.Bottom, bottom + padY);

            int left = int.MaxValue;

            for (int i = 0; i < rows.Count; i++)
            {
                int lx = GetCropLeftX(tv, rows[i]);
                if (lx < left) left = lx;
            }

            if (left == int.MaxValue)
                left = 0;

            int x = Math.Min(Math.Max(0, left), Math.Max(0, client.Right - 1));
            int w = Math.Max(1, client.Right - x);

            return new Rectangle(
                x,
                top,
                w,
                Math.Max(1, bottom - top));
        }

        private static Bitmap CropBitmap(Bitmap source, Rectangle r)
        {
            var safe = Rectangle.Intersect(new Rectangle(0, 0, source.Width, source.Height), r);

            var bmp = new Bitmap(
                Math.Max(1, safe.Width),
                Math.Max(1, safe.Height),
                PixelFormat.Format32bppPArgb);

            using (var g = Graphics.FromImage(bmp))
                g.DrawImage(source, new Rectangle(0, 0, bmp.Width, bmp.Height), safe, GraphicsUnit.Pixel);

            return bmp;
        }

        private static bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags)
            => PrintWindowNative(hwnd, hdcBlt, nFlags);

        [DllImport("user32.dll", EntryPoint = "PrintWindow", SetLastError = true)]
        private static extern bool PrintWindowNative(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

        private static bool IsDescendantOf(TreeNode ancestor, TreeNode node)
        {
            for (var n = node; n != null; n = n.Parent)
                if (ReferenceEquals(n, ancestor))
                    return true;

            return false;
        }
    }
}