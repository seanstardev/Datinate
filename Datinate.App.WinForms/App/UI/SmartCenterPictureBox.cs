using System.Drawing.Drawing2D;

namespace datinate.app
{
    public sealed class SmartCenterPictureBox : PictureBox
    {
        public SmartCenterPictureBox()
        {
            SizeMode = PictureBoxSizeMode.Normal;
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            var g = pe.Graphics;

            if (BackColor.A > 0)
                g.Clear(BackColor);
            else
                base.OnPaintBackground(pe);

            var img = Image;
            if (img == null)
                return;

            var client = ClientRectangle;
            if (client.Width <= 0 || client.Height <= 0)
                return;

            int x;
            if (img.Width <= client.Width)
                x = client.Left + (client.Width - img.Width) / 2;
            else
                x = client.Left;

            int y;
            if (img.Height <= client.Height)
                y = client.Top + (client.Height - img.Height) / 2;
            else
                y = client.Top;

            var dst = new Rectangle(x, y, img.Width, img.Height);

            var oldIM = g.InterpolationMode;
            var oldPO = g.PixelOffsetMode;
            try
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor; // keeps pixel art crisp
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImageUnscaled(img, dst.Location);
            }
            finally
            {
                g.InterpolationMode = oldIM;
                g.PixelOffsetMode = oldPO;
            }
        }
    }
}
