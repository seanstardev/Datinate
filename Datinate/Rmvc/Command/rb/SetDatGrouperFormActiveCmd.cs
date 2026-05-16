using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class SetDatGrouperFormActiveCmd : RCommand
    {
        private readonly string title;

        public SetDatGrouperFormActiveCmd(string title)
        {
            this.title = title;
        }

        protected override void Run()
        {
            Facade.Instance?.Shell?.ShowDatGrouperWindowView();
            Facade.Instance?.Shell?.SetProjectsFormTitle(title);
        }
    }
}
