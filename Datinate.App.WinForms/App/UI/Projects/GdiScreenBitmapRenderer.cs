using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace datinate.app
{
    internal static class GdiScreenBitmapRenderer
    {
        public static Bitmap RenderToBitmap(int widthPx, int heightPx, Action<Graphics> draw)
        {
            if (widthPx < 1 || heightPx < 1 || draw == null)
                return new Bitmap(1, 1, PixelFormat.Format32bppPArgb);

            IntPtr screenDc = IntPtr.Zero;
            IntPtr memDc = IntPtr.Zero;
            IntPtr hBmp = IntPtr.Zero;
            IntPtr oldObj = IntPtr.Zero;

            try
            {
                screenDc = GetDC(IntPtr.Zero);
                if (screenDc == IntPtr.Zero)
                    return null;

                memDc = CreateCompatibleDC(screenDc);
                if (memDc == IntPtr.Zero)
                    return null;

                hBmp = CreateCompatibleBitmap(screenDc, widthPx, heightPx);
                if (hBmp == IntPtr.Zero)
                    return null;

                oldObj = SelectObject(memDc, hBmp);

                using (var g = Graphics.FromHdc(memDc))
                {
                    g.PageUnit = GraphicsUnit.Pixel;
                    draw(g);
                }

                using (var src = Image.FromHbitmap(hBmp))
                    return new Bitmap(src);
            }
            finally
            {
                if (oldObj != IntPtr.Zero && memDc != IntPtr.Zero)
                    SelectObject(memDc, oldObj);

                if (hBmp != IntPtr.Zero)
                    DeleteObject(hBmp);

                if (memDc != IntPtr.Zero)
                    DeleteDC(memDc);

                if (screenDc != IntPtr.Zero)
                    ReleaseDC(IntPtr.Zero, screenDc);
            }
        }

    
        public static string RenderToPngBase64(int widthPx, int heightPx, Action<Graphics> draw)
        {
            if (widthPx < 1 || heightPx < 1 || draw == null)
                return string.Empty;

            IntPtr screenDc = IntPtr.Zero;
            IntPtr memDc = IntPtr.Zero;
            IntPtr hBmp = IntPtr.Zero;
            IntPtr oldObj = IntPtr.Zero;

            try
            {
                screenDc = GetDC(IntPtr.Zero);
                if (screenDc == IntPtr.Zero)
                    return string.Empty;

                memDc = CreateCompatibleDC(screenDc);
                if (memDc == IntPtr.Zero)
                    return string.Empty;

                hBmp = CreateCompatibleBitmap(screenDc, widthPx, heightPx);
                if (hBmp == IntPtr.Zero)
                    return string.Empty;

                oldObj = SelectObject(memDc, hBmp);

                using (var g = Graphics.FromHdc(memDc))
                {
                    g.PageUnit = GraphicsUnit.Pixel;
                    draw(g);
                }

                using (var src = Image.FromHbitmap(hBmp))
                using (var bmp = new Bitmap(src))
                using (var ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Png);
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
            finally
            {
                if (oldObj != IntPtr.Zero && memDc != IntPtr.Zero)
                    SelectObject(memDc, oldObj);

                if (hBmp != IntPtr.Zero)
                    DeleteObject(hBmp);

                if (memDc != IntPtr.Zero)
                    DeleteDC(memDc);

                if (screenDc != IntPtr.Zero)
                    ReleaseDC(IntPtr.Zero, screenDc);
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int cx, int cy);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr ho);
    }
}
