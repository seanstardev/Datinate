namespace Datinate.Rmvc.Delegate.DatDetails
{
    internal sealed class RawFileData
    {
        public string RawText { get; }
        public string? Sha1 { get; }

        public RawFileData(string raw, string? sha1)
        {
            RawText = raw;
            Sha1 = sha1;
        }
    }

}
