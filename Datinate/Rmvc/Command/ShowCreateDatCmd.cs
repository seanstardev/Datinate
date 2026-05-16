using com.RADIO.Datinate.RMVC.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    internal class ShowCreateDatCmd : RCommand 
    {
        private readonly DatVO dat;

        public ShowCreateDatCmd(DatVO dat) 
        {
            this.dat = dat;
        }

        protected override void Run() 
        {
            if (dat.ContainsChds)
            {
                Facade.Instance?.Shell?.ShowMessageBox("Attention", "Cannot proceed. DATs with CHDs cannot currently be created by Datinate.");
                return;
            }

            Facade.Instance?.Shell?.SetCreateDatFormVisible(true);
            Facade.Instance?.CreateDatMediator?.SetView(dat);
        } 
    }
}
