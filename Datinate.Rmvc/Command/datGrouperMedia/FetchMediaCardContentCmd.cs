using Datinate.Shared;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class FetchMediaCardContentCmd : CommandBase<RVoid>
    {
        private ILookupSet lookupSet;
        private string lookupName;
        private bool entryWasSelectedByUser;
        string entryName;

        public FetchMediaCardContentCmd(
            ILookupSet lookupSet, 
            string lookupName, 
            bool entryWasSelectedByUser, 
            string entryName)
        {
            this.lookupSet = lookupSet;
            this.lookupName = lookupName;
            this.entryWasSelectedByUser = entryWasSelectedByUser;
            this.entryName = entryName;
        }

        protected override async Task<RVoid> Run()
        {
            var source = lookupSet.RadioSource;

            if (base.MediaMediator is { } mediaMediator &&
                base.RbAuxItemLoaderProxy is { } loaderProxy &&
                base.ModelDataProxy is { } modelDataProxy)
            {
                string? uri = loaderProxy.CreateURI(
                    lookupName, 
                    source,
                    modelDataProxy.R2DatResources);
                
                string? resourceHtml = null;

                if (source.IsRadioResource &&
                    base.RadioDatModel is { } radioDatModel &&
                    uri != null &&
                    !string.IsNullOrWhiteSpace(uri))
                {
                    var resourceDetails = radioDatModel.GetResourceDetails(source.Id, lookupName);

                    if (resourceDetails?.Info != null && !string.IsNullOrWhiteSpace(source.ContentPath))
                        resourceHtml = loaderProxy.CreateResourceHtml(
                            resourceDetails,
                            source,
                            uri,
                            true);
                }

                if (resourceHtml != null && !string.IsNullOrWhiteSpace(resourceHtml))
                {
                mediaMediator.SetMediaCardContent(lookupSet, resourceHtml, true, entryWasSelectedByUser, entryName);
                }
                else if (!string.IsNullOrWhiteSpace(uri) && uri != null)
                {
                mediaMediator.SetMediaCardContent(lookupSet, uri, false, entryWasSelectedByUser, entryName);
                }
                else
                {
                mediaMediator.SetMediaCardContentNotAvailable(lookupSet, entryName, entryWasSelectedByUser);
                }
            }

            return await Complete();
        }
    }
}
