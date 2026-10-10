using Datinate.App.WinForms.View.UI;
using Datinate.Shared;
using Datinate.Shared.DatGrouper;

namespace Datinate.App.WinForms.View.Util
{
    public static class DescriptorChipUtil
    {
        private static readonly Dictionary<string, Bitmap> dic =
            new Dictionary<string, Bitmap>();

        public static IReadOnlySet20<DescriptorDefinitionDTO> DescriptorDefinitions
            => Datinate.Shared.DescriptorDefinitions.All;

        static DescriptorChipUtil()
        {
            foreach (var def in DescriptorDefinitions)
            {
                dic.Add(
                    def.Code,
                    DescriptorChipUI.RenderChevronAndCodeBitmap(
                        def.Code,
                        Color.FromArgb(def.ColourArgb)));
            }
        }

        public static Bitmap GetDescriptorBitmap(string code)
        {
            if (dic.TryGetValue(code, out var bmp))
                return bmp;

            return dic.First().Value;
        }
    }
}