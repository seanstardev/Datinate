using Datinate.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class RbAuxItemLoaderProxy : RModel
    {
        public string? CreateURI(string lookupName, RadioSourceDTO source)
        {
            var resources = Facade.Instance?.ModelDataProxy?.R2DatResources;
            
            if (resources == null)
                return null;

            var uri = DatinateMediaResolver.CreateURI(lookupName, source, resources);
            return uri;
        }

        public string CreateResourceHtml(
            ResourceDetailsDTO details, 
            RadioSourceDTO source, 
            string url,
            bool createSampleVersion)
        {
            var path = Path.Combine(source.ContentPath, details.LookupName);

            return ResourceInfoWebPageBuilder.BuildDocumentHtml(
                details.Info, 
                path,
                url,
                createSampleVersion);
        }
    }
}
