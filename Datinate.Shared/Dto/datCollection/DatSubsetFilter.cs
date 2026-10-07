namespace com.RADIO.Datinate.RMVC.Shared
{
    /*

    For scenarios like this in MAME Multimedia DATs. Here, the Item is "snap_SL" and the Folder is "32x".

    <machine name="snap_SL">
        <description>snap_SL</description>
        <rom name="32x\36great.png" size="39969" crc="6d1935de" sha1="5f08d83eba415f511885563872e956d578d2e635"/>   
        ...

    */
    public class DatSubsetFilter
    {
        public string Entry { get; }
        public string? Path { get; }

        public bool HasFolder => !string.IsNullOrWhiteSpace(Path);
        public DatSubsetFilter(string item, string? folder = null)
        {
            Entry = item;
            Path = folder;
        }
        public DatSubsetFilter Clone()
        {
            return new DatSubsetFilter(Entry, Path);
        }

        public bool Equals(DatSubsetFilter subset)
        {
            if (Entry == subset.Entry && Path == subset.Path)
                return true;
            else
                return false;
        }

        public static bool Equals(DatSubsetFilter? subset1, DatSubsetFilter? subset2)
        {

            if (subset1 == null && subset2 == null) return true;
            else if (subset1 == null && subset2 != null) return false;
            else if (subset1 != null && subset2 == null) return false;


            if (subset1!.Entry == subset2!.Entry && subset1.Path == subset2.Path)
                return true;
            else
                return false;
        }
    }
}
