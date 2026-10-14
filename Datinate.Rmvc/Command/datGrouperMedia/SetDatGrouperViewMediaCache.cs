using Datinate.Shared;
using Datinate.Shared.Radio;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class SetDatGrouperViewMediaCache : CommandBase<RVoid>
    {
        private readonly IReadOnlyDictionary<IGameFamily, IMediaCollection> mediaCache;

        public SetDatGrouperViewMediaCache(IReadOnlyDictionary<IGameFamily, IMediaCollection> mediaCache)
        {
            this.mediaCache = mediaCache;
        }

        protected override async Task<RVoid> Run()
        {
            base.DatGrouperMediator?.SetMediaCache(mediaCache);

            return await Complete();
        }
    }
}
