using Datinate.Shared;
using RMVC;

namespace Datinate.Rmvc.Proxy
{
    public class RbAuxItemLoaderProxy : IRModel
    {
        public string? CreateURI(
            string lookupName, 
            RadioSourceDTO source,
            IReadOnlyList<R2DatResourceDTO> r2DatResources)
        {
            var uri = DatinateMediaResolver.CreateURI(lookupName, source, r2DatResources);
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
