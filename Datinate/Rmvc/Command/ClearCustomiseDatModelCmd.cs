using RMVC;

namespace com.RADIO.Datinate.RMVC
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
