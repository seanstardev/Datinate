using Datinate.Shared;
using Datinate.Shared.Radio;
using RMVC;

namespace Datinate.Rmvc.Command
{
    public class SetDatGrouperViewMediaCache : RCommand
    {
        private readonly IReadOnlyDictionary<IGameFamily, IMediaCollection> mediaCache;

        public SetDatGrouperViewMediaCache(IReadOnlyDictionary<IGameFamily, IMediaCollection> mediaCache)
        {
            this.mediaCache = mediaCache;
        }

        protected override void Run()
        {
            Facade.Instance?.DatGrouperMediator?.SetMediaCache(mediaCache);
        }
    }
}
