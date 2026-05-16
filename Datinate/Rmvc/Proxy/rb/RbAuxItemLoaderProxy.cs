using com.RADIO.Datinate.RMVC.Shared;
using datinate.app;
using Datinate.Shared.Rb;
using RMVC;
using System.Diagnostics;

namespace com.RADIO.Datinate.RMVC
{
    public class RbAuxItemLoaderProxy : RModel
    {
        protected override void Initialise()
        {
        }
        public string? CreateURI(string lookupName, RadioSourceDTO source)
        {
            var resources = Facade.Instance?.AppDataProxy?.R2DatResources;
            
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
