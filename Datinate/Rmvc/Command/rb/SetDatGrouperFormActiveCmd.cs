using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class SetDatGrouperFormActiveCmd : RCommand
    {
        private readonly string? title;

        public SetDatGrouperFormActiveCmd(string? title = null)
        {
            this.title = title;
        }

        protected override void Run()
        {
            Facade.Instance?.Shell?.ShowDatGrouperWindowView();

            if (string.IsNullOrWhiteSpace(title) == false)
                Facade.Instance?.Shell?.SetProjectsFormTitle(title);
        }
    }
}
