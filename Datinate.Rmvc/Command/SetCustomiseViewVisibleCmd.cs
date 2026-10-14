using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class SetCustomiseViewVisibleCmd : CommandBase<RVoid> 
    {
        private readonly bool makeVisible;

        public SetCustomiseViewVisibleCmd(bool makeVisible) 
        {
            this.makeVisible = makeVisible;
        }

        protected override async Task<RVoid> Run() 
        {
            base.Shell?.SetCustomFormVisible(makeVisible);

            return await Complete();
        }
    }
}
