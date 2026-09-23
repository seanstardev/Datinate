using System.ComponentModel;

namespace datinate.app
{
    public sealed class InvisibleTabControl : TabControl
    {
        private bool IsDesignTime =>
            LicenseManager.UsageMode == LicenseUsageMode.Designtime ||
            Site?.DesignMode == true;

        public override Rectangle DisplayRectangle
        {
            get
            {
                // Keep completely normal TabControl behaviour in the designer.
                if (IsDesignTime)
                    return base.DisplayRectangle;

                // Runtime: no TabControl chrome/inset at all.
                return ClientRectangle;
            }
        }
    }
}