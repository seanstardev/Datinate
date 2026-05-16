using Datinate.Shared.Rb;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class ShowMediaCardCmd : RCommand
    {
        private readonly ILookupSet lookupSet;
        private readonly string entryName;
        private readonly bool isAutoLoaded;
        private readonly string sourceId;

        public ShowMediaCardCmd(ILookupSet lookupSet, string entryName, bool isAutoLoaded)
        {
            this.lookupSet = lookupSet;
            this.entryName = entryName;
            this.isAutoLoaded = isAutoLoaded;
            this.sourceId = lookupSet.Id;
        }

        protected override void Run()
        {
            var mediaProxy = Facade.Instance?.RbAuxItemLoaderProxy;
            var radioDatModel = Facade.Instance?.RadioDatModel;

            if (mediaProxy == null ||
                radioDatModel == null)
            {
                return;
            }

            var source = radioDatModel.GetSource(sourceId);

            if (source == null) return;

            var lookup = lookupSet.GetLookup(entryName);
            string? url = null;
            string? resourceHtml = null;

            if (!string.IsNullOrWhiteSpace(lookup))
            {
                url = mediaProxy.CreateURI(lookup, source);
            }

            if (url == null) return;

            if (source.IsRadioResource)
            {
                var resourceDetails = radioDatModel.GetResourceDetails(source.Id, lookup!);
                
                if (resourceDetails?.Info != null && !string.IsNullOrWhiteSpace(source.ContentPath))
                    resourceHtml = mediaProxy.CreateResourceHtml(
                        resourceDetails, 
                        source,
                        url,
                        false);
            }


            Facade.Instance?.MediaAssignmentMediator?.SetViewMediaItem(entryName, source);

            if (!string.IsNullOrWhiteSpace(resourceHtml))
                base.ExecuteCommand(new LoadWebMediaHtmlCmd(resourceHtml));
            else
                base.ExecuteCommand(new LoadWebMediaUrlCmd(url));

            // TODO: Might need to rename this, but trialing the tech:
            if (!isAutoLoaded)
                Facade.Instance?.MediaAssignmentMediator?.SetMediaCardDragStart();
        }
    }
}
