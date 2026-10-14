using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class ClearCustomiseDatModelCmd : CommandBase<RVoid> 
    {
        protected override async Task<RVoid> Run() 
        {
            if (base.ActiveDatsModel != null)
                base.ActiveDatsModel.CustomiseDat = null;

            return await Complete();
        }
    }
}
