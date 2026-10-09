using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class ClearCustomiseDatModelCmd:RCommand 
    {
        protected override void Run() 
        {
            if (Facade.Instance?.ActiveDatsModel != null)
                Facade.Instance.ActiveDatsModel.CustomiseDat = null;
        }
    }
}
