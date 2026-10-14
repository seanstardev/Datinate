using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class SetCompareViewVisibleCmd : CommandBase<RVoid> 
    {
        private readonly bool makeVisible;

        public SetCompareViewVisibleCmd(bool makeVisible)
        {
            this.makeVisible = makeVisible;
        }
        protected override async Task<RVoid> Run() 
        {
            base.Shell?.SetCompareFormVisible(makeVisible);

            return await Complete();
        }
    }
}
