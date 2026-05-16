using Datinate.Shared.Rb;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class FetchMediaCardContentCmd : RCommand
    {
        private ILookupSet lookupSet;
        private string lookupName;
        private bool entryWasSelectedByUser;
        string entryName;

        public FetchMediaCardContentCmd(ILookupSet lookupSet, string lookupName, bool entryWasSelectedByUser, string entryName)
        {
            this.lookupSet = lookupSet;
            this.lookupName = lookupName;
            this.entryWasSelectedByUser = entryWasSelectedByUser;
            this.entryName = entryName;
        }

        protected override void Run()
        {
            var source = lookupSet.RadioSource;

            if (Facade.Instance?.MediaMediator is { } mediaMediator &&
                Facade.Instance?.RbAuxItemLoaderProxy is { } proxy)
            {
                string? uri = proxy.CreateURI(lookupName, source);
                string? resourceHtml = null;

                if (source.IsRadioResource && 
                    Facade.Instance?.RadioDatModel is { } radioDatModel &&
                    !string.IsNullOrWhiteSpace(uri))
                {
                    var resourceDetails = radioDatModel.GetResourceDetails(source.Id, lookupName);

                    if (resourceDetails?.Info != null && !string.IsNullOrWhiteSpace(source.ContentPath))
                        resourceHtml = proxy.CreateResourceHtml(
                            resourceDetails,
                            source,
                            uri,
                            true);
                }

                if (!string.IsNullOrWhiteSpace(resourceHtml))
                {
                    mediaMediator.SetMediaCardContent(lookupSet, resourceHtml, true, entryWasSelectedByUser, entryName);
                }
                else if (!string.IsNullOrWhiteSpace(uri))
                {
                    mediaMediator.SetMediaCardContent(lookupSet, uri, false, entryWasSelectedByUser, entryName);
                }
                else
                {
                    mediaMediator.SetMediaCardContentNotAvailable(lookupSet, entryName, entryWasSelectedByUser);
                }
            }
        }
    }
}
