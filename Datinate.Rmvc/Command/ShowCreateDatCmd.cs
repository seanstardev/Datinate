using Datinate.Shared.Dat;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class ShowCreateDatCmd : CommandBase<RVoid>
    {
        private readonly DatVO dat;

        public ShowCreateDatCmd(DatVO dat) 
        {
            this.dat = dat;
        }

        protected override async Task<RVoid> Run() 
        {
            if (dat.ContainsChds)
            {
                base.Shell?.ShowMessageBox(
                    "Attention", 
                    "Cannot proceed. DATs with CHDs cannot currently be created by Datinate.");
                
                return await Complete();
            }

            base.Shell?.SetCreateDatFormVisible(true);
            base.CreateDatMediator?.SetView(dat);

            return await Complete();
        } 
    }
}