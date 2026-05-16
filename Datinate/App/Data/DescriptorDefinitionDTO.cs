namespace datinate.app
{
    public class DescriptorDefinitionDTO
    {
        public DescriptorDefinitionDTO(string code, Color colour, string description)
        {
            Code = code;
            Colour = colour;
            Description = description;
        }

        public string Code { get; }
        public Color Colour { get; }
        public string Description { get; }
    }
}
