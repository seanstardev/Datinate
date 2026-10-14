using Datinate.Rmvc.Command;
using Datinate.Rmvc.Mediator;
using Datinate.Rmvc.Model;
using Datinate.Rmvc.Proxy;
using RMVC;

namespace Datinate.Rmvc
{
    public class Facade : RFacade 
    {
        protected override RMediator[] RegisterMediators()
        {
            return new RMediator[]
            {
                new MainMediator(),
                new CustomListMediator(),
                new CompareMediator(),
                new ProblemListMediator(),
                new CreateDatMediator(),
                new DatPathsUpdateMediator(),
                new DatSummaryMediator(),
                new DatDetailsMediator(),
                new LandingMediator(),
                new MainControlsMediator(),
                new DatGrouperMediator(),
                new ProjectLoaderMediator(),
                new AddToProjectMediator(),
                new MainWebMediator(),
                new DatGrouperControlsMediator(),
                new RbWebSearchMediator(),
                new Media2Mediator(),
                new MediaAssignmentMediator(),
                new MediaWebMediator(),
                new ProgressMediator(),
                new ExportMediator()
            };
        }

        protected override IRModel[] RegisterModels()
        {
            return new IRModel[] 
            {
                new GlobalSettingsProxy(),
                new DatGrouperSessionModel(),
                new DatHierarchyProxy(),
                new DatDetailsProxy(),
                new CreateDatProxy(),
                new ExpressionsProxy(),
                new ProjectProxy(),
                new DatDbProxy(),
                new UnitDisplayModel(),
                new ActiveDatsModel(),
                new RadioDatModel(),
                new AutoGrouperModel(),
                new ModelDataProxy(),
                new RbAuxResourceProxy(),
                new RbAuxMediaProxy(),
                new CuratedDatProxy(),
                new RbAuxItemLoaderProxy(),
                new DatGrouperModel(),
                new ExportDatGrouperProjectProxy(),
                new AudioProxy()
            };
        }
       
        protected override RCommandBase RegisterStartupCommand()
            => new StartupCmd();
    }
}
