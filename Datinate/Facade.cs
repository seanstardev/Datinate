using com.RADIO.Datinate.RMVC;
using Datinate.Shared;
using RMVC;

namespace com.RADIO.Datinate
{
    public class Facade : RFacade 
    {
        internal static Facade? Instance => RFacade.FacadeInstance<Facade>();
        internal IShell? Shell => (AppShell as IShell != null) ? (IShell)AppShell : null;

        // View - main:
        internal MainMediator? MainMediator => base.Mediator<MainMediator>();
        internal CustomListMediator? CustomListMediator => base.Mediator<CustomListMediator>();
        internal CompareMediator? CompareMediator => base.Mediator <CompareMediator>();
        internal ProblemListMediator? ProblemListMediator => base.Mediator <ProblemListMediator>();
        internal CreateDatMediator? CreateDatMediator => base.Mediator<CreateDatMediator>();
        internal DatPathsUpdateMediator? DatPathsUpdateMediator => base.Mediator<DatPathsUpdateMediator>();
        internal DatGrouperControlsMediator? DatGrouperControlsMediator => base.Mediator<DatGrouperControlsMediator>();
        internal ProgressMediator? ProgressMediator => base.Mediator<ProgressMediator>();

        // View - controls
        internal DatSummaryMediator? DatSummaryMediator => base.Mediator <DatSummaryMediator>();
        internal DatDetailsMediator? DatDetailsMediator => base.Mediator <DatDetailsMediator>();
        internal LandingMediator? LandingMediator => base.Mediator <LandingMediator>();
        internal MainControlsMediator? MainControlsMediator => base.Mediator <MainControlsMediator>();
        internal DatGrouperMediator? DatGrouperMediator => base.Mediator <DatGrouperMediator>();
        internal ProjectLoaderMediator? ProjectLoaderMediator => base.Mediator <ProjectLoaderMediator>();
        internal AddToProjectMediator? AddToProjectMediator => base.Mediator <AddToProjectMediator>();
        internal DatGrouperSettingsMediator? ContentPathsMediator => base.Mediator <DatGrouperSettingsMediator>();
        internal MainWebMediator? MainWebMediator => base.Mediator <MainWebMediator>();
        internal RbWebSearchMediator? RbWebSearchMediator => base.Mediator<RbWebSearchMediator>();
        internal Media2Mediator? MediaMediator => base.Mediator<Media2Mediator>();
        internal MediaAssignmentMediator? MediaAssignmentMediator => base.Mediator<MediaAssignmentMediator>();
        internal MediaWebMediator? MediaWebMediator  => base.Mediator<MediaWebMediator>();
        internal ExportMediator? ExportMediator => base.Mediator<ExportMediator>();

        // Proxy
        internal DatHierarchyProxy? DatHierarchyProxy => base.Model<DatHierarchyProxy>();
        internal DatDetailsProxy? DatDetailsProxy => base.Model<DatDetailsProxy>();
        internal CreateDatProxy? CreateDatProxy => base.Model<CreateDatProxy>();
        internal ExpressionsProxy? ExpressionsProxy => base.Model<ExpressionsProxy>();
        internal ProjectProxy? ProjectProxy => base.Model<ProjectProxy>();
        internal GlobalSettingsProxy? GlobalSettingsProxy => base.Model<GlobalSettingsProxy>();
        internal DatDbProxy? DatDbProxy => base.Model<DatDbProxy>();
        internal AppDataProxy? AppDataProxy => base.Model<AppDataProxy>();
        internal RbAuxResourceProxy? RbAuxResourceProxy => base.Model<RbAuxResourceProxy>();
        internal RbAuxMediaProxy? RbAuxMediaProxy => base.Model<RbAuxMediaProxy>();
        internal CuratedDatProxy? CuratedDatProxy => base.Model<CuratedDatProxy>();
        internal RbAuxItemLoaderProxy? RbAuxItemLoaderProxy => base.Model<RbAuxItemLoaderProxy>();
        internal ExportDatGrouperProjectProxy? ExportDatGrouperProjectProxy => base.Model<ExportDatGrouperProjectProxy>();

        // Model
        internal UnitDisplayModel? UnitDisplayModel => base.Model<UnitDisplayModel>();
        internal ActiveDatsModel? ActiveDatsModel => base.Model<ActiveDatsModel>();
        internal RadioDatModel? RadioDatModel => base.Model<RadioDatModel>();
        internal AutoGrouperModel? AutoGrouperModel => base.Model<AutoGrouperModel>();
        internal DatGrouperSessionModel? DatGrouperSessionModel => base.Model<DatGrouperSessionModel>();
        internal DatGrouperModel? DatGrouperModel => base.Model<DatGrouperModel>();

        protected override RMediator[] RegisterMediators()
        {
            return new RMediator[]
            {
                new MainMediator(typeof(IMainView)),
                new CustomListMediator(typeof(ICustomListView)),
                new CompareMediator(typeof(ICompareView)),
                new ProblemListMediator(typeof(IProblemListView)),
                new CreateDatMediator(typeof(ICreateDatView)),
                new DatPathsUpdateMediator(typeof(IDatPathsUpdateView)),
                new DatSummaryMediator(typeof(IDatSummaryView)),
                new DatDetailsMediator(typeof(IDatDetailsView)),
                new LandingMediator(typeof(ILandingView)),
                new MainControlsMediator(typeof(IMainControlsView)),
                new DatGrouperMediator(typeof(IDatGrouperView)),
                new ProjectLoaderMediator(typeof(IProjectLoaderView)),
                new AddToProjectMediator(typeof(IAddToProjectView)),
                new DatGrouperSettingsMediator(typeof(IDatGrouperProjectSettingsView)),
                new MainWebMediator(typeof(IMainWebView)),
                new DatGrouperControlsMediator(typeof(IDatGrouperControlsView)),
                new RbWebSearchMediator(typeof(IRbWebSearchView)),
                new Media2Mediator(typeof(IMedia2View)),
                new MediaAssignmentMediator(typeof(IMediaAssignmentView)),
                new MediaWebMediator(typeof(IWebMediaView)),
                new ProgressMediator(typeof(IProgressView)),
                new ExportMediator(typeof(IExportView))
            };
        }

        protected override RModel[] RegisterModels()
        {
            return new RModel[] {
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
                new AppDataProxy(),
                new RbAuxResourceProxy(),
                new RbAuxMediaProxy(),
                new CuratedDatProxy(),
                new RbAuxItemLoaderProxy(),
                new DatGrouperModel(),
                new ExportDatGrouperProjectProxy()
            };
        }
        
        // Shell
        public void HandleCompareFormClose() 
        {
            //base.ExecuteCommand(new ClearCompareDatsModelAndViewCmd());    
        }
        public void HandleProjectsFormClose() 
        {
            // TODO: What is this?
            HighlightProjectDatsCmd.Execute();
        }

        protected override RCommandBase RegisterStartupCommand()
        {
            return new StartupCmd();
        }
    }
}
