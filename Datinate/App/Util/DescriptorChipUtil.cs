using Datinate.Shared;
using Datinate.Shared.DatGrouper;

namespace datinate.app
{
    public static class DescriptorChipUtil
    {
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
        private static readonly Dictionary<string, Bitmap> dic = new Dictionary<string, Bitmap>();

        // TODO:
        public static readonly IReadOnlySet20<DescriptorDefinitionDTO> DescriptorDefinitions =
            ReadOnlySet20.From(new HashSet<DescriptorDefinitionDTO>()
            {
                new DescriptorDefinitionDTO("PD", Color.FromArgb(0, 140, 212).ToArgb(), "Not Commercial"),
                new DescriptorDefinitionDTO("Un", Color.LimeGreen.ToArgb(), "Unreleased"),
                new DescriptorDefinitionDTO("NA", Color.FromArgb(255, 128, 0).ToArgb(), "Not A Game"),
                new DescriptorDefinitionDTO("NG", Color.DarkGoldenrod.ToArgb(), "No Good Dump"),
                new DescriptorDefinitionDTO("Ed", Color.FromArgb(128, 128, 255).ToArgb(), "Educational"),
                new DescriptorDefinitionDTO("Co", Color.Green.ToArgb(), "Compilation"),
                new DescriptorDefinitionDTO("BL", Color.Red.ToArgb(), "Bootleg"),
                new DescriptorDefinitionDTO("Af", Color.FromArgb(196, 7, 213).ToArgb(), "Aftermarket"),
                new DescriptorDefinitionDTO("CD", Color.FromArgb(0, 30, 255).ToArgb(), "Cover / Demo Disc"),
                new DescriptorDefinitionDTO("WS", Color.Gray.ToArgb(), "Wrong Set"),
                new DescriptorDefinitionDTO("WIP", Color.HotPink.ToArgb(), "TODO"),
                new DescriptorDefinitionDTO("Ad", Color.MidnightBlue.ToArgb(), "Adult"),
            });

        public static Bitmap GetDescriptorBitmap(string code)
        {
            if (dic.TryGetValue(code, out var bmp))
            {
                return bmp;
            }
            else return dic.First().Value;
        }
    }
}
