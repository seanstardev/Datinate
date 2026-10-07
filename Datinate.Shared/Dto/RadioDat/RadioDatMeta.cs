namespace RadioLibCore.RadioDat
{
    public class RadioDatMeta
    {
        public static RadioDatMeta Empty
            => new RadioDatMeta(Array.Empty<string>(), Array.Empty<string>(), string.Empty);

        public string[] IncludeDescriptors { get; private set; }
        public string[] ExcludeDescriptors { get; private set; }
        public string Notes { get; private set; }

        public RadioDatMeta(
            string[] includeDescriptors
            , string[] excludeDescriptor
            , string notes)
        {
            this.IncludeDescriptors = includeDescriptors;
            this.ExcludeDescriptors = excludeDescriptor;
            this.Notes = notes;
        }
    }
}
