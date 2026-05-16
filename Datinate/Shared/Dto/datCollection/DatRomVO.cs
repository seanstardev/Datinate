namespace com.RADIO.Datinate.RMVC.Shared
{
    public class DatRomVO : IByteReporter
    {
        public string Name { get; }
        public bool IsDisk { get; }
        public ulong Size { get; }
        public string Crc { get; }
        public string Md5 { get; }
        public string Sha1 { get; }

        public DatRomVO(
            string name
            , bool isDisk
            , ulong size
            , string crc
            , string md5
            , string sha1) 
        {
            Name = name;
            IsDisk = isDisk;
            Size = size;
            Md5 = md5;
            Sha1 = sha1;

            Crc = crc.ToLower();
        }

        public ulong GetTotalSize() 
        {
            return Size;
        }
    }
}
