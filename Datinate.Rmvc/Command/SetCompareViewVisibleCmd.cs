using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    internal class SetCompareViewVisibleCmd : RCommand 
    {
        private readonly bool makeVisible;

        public SetCompareViewVisibleCmd(bool makeVisible)
        {
            this.makeVisible = makeVisible;
        }
        protected override void Run() 
        {
            Facade.Instance?.Shell?.SetCompareFormVisible(makeVisible);
        }
    }
}
