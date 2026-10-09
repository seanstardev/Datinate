using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class SetCustomiseViewVisibleCmd : RCommand 
    {
        private readonly bool makeVisible;

        public SetCustomiseViewVisibleCmd(bool makeVisible) 
        {
            this.makeVisible = makeVisible;
        }

        protected override void Run() 
        {
            Facade.Instance?.Shell?.SetCustomFormVisible(makeVisible);
        }
    }
}
