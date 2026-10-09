using System.ComponentModel;

namespace Datinate.App.UI
{
    internal class LinkLabelHackUI : LinkLabel
    {
        private const int WM_NCHITTEST = 0x0084;
        private static readonly IntPtr HTTRANSPARENT = new IntPtr(-1);

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ClickThrough { get; set; } = true;

        protected override void WndProc(ref Message m)
        {
            if (ClickThrough && m.Msg == WM_NCHITTEST)
            {
                m.Result = HTTRANSPARENT;
                return;
            }

            base.WndProc(ref m);
        }
    }
}
