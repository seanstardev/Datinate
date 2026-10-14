using Datinate.Shared;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class ShowMediaCardCmd : CommandBase<RVoid>
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

        protected override async Task<RVoid> Run()
        {
            if (base.RbAuxItemLoaderProxy == null ||
                base.RadioDatModel == null)
            {
                return await Complete();
            }

            var source = base.RadioDatModel.GetSource(sourceId);

            if (source == null) 
                return await Complete();

                var lookup = lookupSet.GetLookup(entryName);
            string? url = null;
            string? resourceHtml = null;

            if (lookup != null && !string.IsNullOrWhiteSpace(lookup))
            {
                url = base.RbAuxItemLoaderProxy.CreateURI(
                    lookup, 
                    source,
                    base.ModelDataProxy?.R2DatResources ?? new List<R2DatResourceDTO>());
            }

                if (url == null) return await Complete();

            if (source.IsRadioResource)
            {
                var resourceDetails = base.RadioDatModel.GetResourceDetails(source.Id, lookup!);
                
                if (resourceDetails?.Info != null && !string.IsNullOrWhiteSpace(source.ContentPath))
                    resourceHtml = base.RbAuxItemLoaderProxy.CreateResourceHtml(
                        resourceDetails, 
                        source,
                        url,
                        false);
            }

                base.MediaAssignmentMediator?.SetViewMediaItem(entryName, source);

            if (!string.IsNullOrWhiteSpace(resourceHtml) && resourceHtml != null)
                await base.Execute(new LoadWebMediaHtmlCmd(resourceHtml));
            else
                await base.Execute(new LoadWebMediaUrlCmd(url));

            if (!isAutoLoaded)
                base.MediaAssignmentMediator?.SetScoringActive(false);

            return await Complete();
        }
    }
}
