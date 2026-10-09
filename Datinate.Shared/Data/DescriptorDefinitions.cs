using Datinate.Shared.DatGrouper;

namespace Datinate.Shared
{
    public static class DescriptorDefinitions
    {
        public static readonly IReadOnlySet20<DescriptorDefinitionDTO> All =
            ReadOnlySet20.From(new HashSet<DescriptorDefinitionDTO>
            {
                new DescriptorDefinitionDTO("PD", unchecked((int)0xFF008CD4), "Not Commercial"), // Color.FromArgb(0, 140, 212)
                new DescriptorDefinitionDTO("Un", unchecked((int)0xFF32CD32), "Unreleased"), // Color.LimeGreen
                new DescriptorDefinitionDTO("NA", unchecked((int)0xFFFF8000), "Not A Game"), // Color.FromArgb(255, 128, 0)
                new DescriptorDefinitionDTO("NG", unchecked((int)0xFFB8860B), "No Good Dump"), // Color.DarkGoldenrod
                new DescriptorDefinitionDTO("Ed", unchecked((int)0xFF8080FF), "Educational"), // Color.FromArgb(128, 128, 255)
                new DescriptorDefinitionDTO("Co", unchecked((int)0xFF008000), "Compilation"), // Color.Green
                new DescriptorDefinitionDTO("BL", unchecked((int)0xFFFF0000), "Bootleg"), // Color.Red
                new DescriptorDefinitionDTO("Af", unchecked((int)0xFFC407D5), "Aftermarket"), // Color.FromArgb(196, 7, 213)
                new DescriptorDefinitionDTO("CD", unchecked((int)0xFF001EFF), "Cover / Demo Disc"), // Color.FromArgb(0, 30, 255)
                new DescriptorDefinitionDTO("WS", unchecked((int)0xFF808080), "Wrong Set"), // Color.Gray
                new DescriptorDefinitionDTO("WIP", unchecked((int)0xFFFF69B4), "TODO"), // Color.HotPink
                new DescriptorDefinitionDTO("Ad", unchecked((int)0xFF191970), "Adult"), // Color.MidnightBlue
            });
    }
}