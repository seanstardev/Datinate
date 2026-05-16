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
                    DescriptorChipUI.RenderChevronAndCodeBitmap(def.Code, def.Colour));
            }

        }
        private static readonly Dictionary<string, Bitmap> dic = new Dictionary<string, Bitmap>();

        // TODO:
        public static readonly IReadOnlySet<DescriptorDefinitionDTO> DescriptorDefinitions = new HashSet<DescriptorDefinitionDTO>()
        {
            new DescriptorDefinitionDTO("PD", Color.FromArgb(0, 140, 212), "Not Commercial"),
            new DescriptorDefinitionDTO("Un", Color.LimeGreen, "Unreleased"),
            new DescriptorDefinitionDTO("NA", Color.FromArgb(255, 128, 0), "Not A Game"),
            new DescriptorDefinitionDTO("NG", Color.DarkGoldenrod, "No Good Dump"),
            new DescriptorDefinitionDTO("Ed", Color.FromArgb(128, 128, 255), "Educational"),
            new DescriptorDefinitionDTO("Co", Color.Green, "Compilation"),
            new DescriptorDefinitionDTO("UL", Color.Red, "Unlicensed"),
            new DescriptorDefinitionDTO("Af", Color.FromArgb(196, 7, 213), "Aftermarket"),
            new DescriptorDefinitionDTO("CD", Color.FromArgb(0, 30, 255), "Cover / Demo Disc"),
            new DescriptorDefinitionDTO("DA", Color.Gray, "DAT Artefact"),
            new DescriptorDefinitionDTO("WIP", Color.HotPink, "TODO"),
            new DescriptorDefinitionDTO("Ad", Color.MidnightBlue, "Adult"),
        };

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
