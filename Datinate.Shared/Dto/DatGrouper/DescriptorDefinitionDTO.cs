namespace Datinate.Shared.DatGrouper
{
    public class DescriptorDefinitionDTO
    {
        public DescriptorDefinitionDTO(string code, int colourArgb, string description)
        {
            Code = code;
            ColourArgb = colourArgb;
            Description = description;
        }

        public string Code { get; }
        public int ColourArgb { get; }
        public string Description { get; }
    }
}
