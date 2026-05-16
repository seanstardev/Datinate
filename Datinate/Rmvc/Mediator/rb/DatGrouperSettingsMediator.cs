using com.RADIO.Datinate.RMVC.Shared;
using datinate.app;
using Datinate.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class DatGrouperSettingsMediator : RMediator
    {
        private IDatGrouperProjectSettingsView? view => (IDatGrouperProjectSettingsView?)base.viewBase;
        public DatGrouperSettingsMediator(Type view) : base(view)
        {
        }
        public void SetView(DatGrouperProjectDTO project, IReadOnlySet<DescriptorDefinitionDTO> descriptors)
        {
            view?.SetView(project, descriptors);
        }

        public void ClearView()
        {
            view?.ClearView();
        }
        private void OnBack()
        {
            base.ExecuteCommand(new SetProjectsViewCmd());
        }

        protected override void Initialsed()
        {
            if (view == null)
                return;

            view.BackEvt += OnBack;
            view.SaveEvt += OnSave;
            view.LoadingPercentEvt += OnLoadingPercent;
            view.ClearLoadingEvt += OnClearLoading;
        }

        private void OnClearLoading()
        {
            base.ExecuteCommand(new ClearProgressCmd());
        }

        private void OnLoadingPercent(int percent)
        {
            base.ExecuteCommand(new ShowProgressCmd("Counting Resources", percent, 100));
        }

        private void OnSave(DatGrouperProjectDTO project)
        {
            ExecuteCommand(new SaveProjectCmd(project, false));
        }


        protected override void Disposing()
        {
            if (view == null)
                return;

            view.SaveEvt -= OnSave;
            view.BackEvt -= OnBack;

            view.LoadingPercentEvt -= OnLoadingPercent;
            view.ClearLoadingEvt -= OnClearLoading;
        }
    }
}
