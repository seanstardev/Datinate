using Datinate.App.Shared.DatGrouper;
using Datinate.Shared;
using Datinate.Shared.Radio;

namespace Datinate.App.WinForms.View.UI.DatGrouper
{
    internal static class TreeNodeToolTipHelper
    {
        private static readonly Dictionary<string, int> datGroupTabCountCache =
            new(StringComparer.Ordinal);
        private static readonly object datGroupTabCountLock = new();
        private const int DatGroupTargetColumnPx = 120;

        internal static string CreateTooltip(
            IGameEntity entity,
            IMediaCollection? mediaCollection = null)
            => DatGrouperTextBuilder.CreateTooltip(
                entity, GetDatGroupTabCount, mediaCollection);

        private static int GetDatGroupTabCount(string datGroup)
        {
            lock (datGroupTabCountLock)
            {
                if (datGroupTabCountCache.TryGetValue(datGroup, out int cached))
                    return cached;

                var font = SystemFonts.StatusFont;

                const int maxTabs = 10;

                for (int tabCount = 1; tabCount <= maxTabs; tabCount++)
                {
                    string test =
                        datGroup + new string('\t', tabCount);

                    int width = TextRenderer.MeasureText(
                        test,
                        font,
                        Size.Empty,
                        TextFormatFlags.NoPadding |
                        TextFormatFlags.SingleLine |
                        TextFormatFlags.ExpandTabs).Width;

                    if (width >= DatGroupTargetColumnPx)
                    {
                        datGroupTabCountCache[datGroup] = tabCount;
                        return tabCount;
                    }
                }

                datGroupTabCountCache[datGroup] = maxTabs;
                return maxTabs;
            }
        }

    }
}
