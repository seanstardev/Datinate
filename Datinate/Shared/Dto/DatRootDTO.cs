namespace com.RADIO.Datinate.RMVC.Shared
{
    public class DatRootDTO 
    {
        public string RootPath { get; }
        public string ReferenceName { get; }
        public DatRootDTO(
            string rootPath, string referenceName) 
        {
            this.RootPath = rootPath;
            ReferenceName = referenceName;
        }
    }
}
