using Datinate.Rmvc.Mediator;
using Datinate.Rmvc.Model;
using Datinate.Rmvc.Proxy;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal abstract class CommandBase<TResult> : RCommand<TResult>
    {
        protected IShell? Shell => base.AppShell as IShell;

        // View - main:
        protected MainMediator? MainMediator => base.Mediator<MainMediator>();
        protected CustomListMediator? CustomListMediator => base.Mediator<CustomListMediator>();
        protected CompareMediator? CompareMediator => base.Mediator<CompareMediator>();
        protected ProblemListMediator? ProblemListMediator => base.Mediator<ProblemListMediator>();
        protected CreateDatMediator? CreateDatMediator => base.Mediator<CreateDatMediator>();
        protected DatPathsUpdateMediator? DatPathsUpdateMediator => base.Mediator<DatPathsUpdateMediator>();
        protected DatGrouperControlsMediator? DatGrouperControlsMediator => base.Mediator<DatGrouperControlsMediator>();
        protected ProgressMediator? ProgressMediator => base.Mediator<ProgressMediator>();

        // View - controls
        protected DatSummaryMediator? DatSummaryMediator => base.Mediator<DatSummaryMediator>();
        protected DatDetailsMediator? DatDetailsMediator => base.Mediator<DatDetailsMediator>();
        protected LandingMediator? LandingMediator => base.Mediator<LandingMediator>();
        protected MainControlsMediator? MainControlsMediator => base.Mediator<MainControlsMediator>();
        protected DatGrouperMediator? DatGrouperMediator => base.Mediator<DatGrouperMediator>();
        protected ProjectLoaderMediator? ProjectLoaderMediator => base.Mediator<ProjectLoaderMediator>();
        protected AddToProjectMediator? AddToProjectMediator => base.Mediator<AddToProjectMediator>();
        protected MainWebMediator? MainWebMediator => base.Mediator<MainWebMediator>();
        protected RbWebSearchMediator? RbWebSearchMediator => base.Mediator<RbWebSearchMediator>();
        protected Media2Mediator? MediaMediator => base.Mediator<Media2Mediator>();
        protected MediaAssignmentMediator? MediaAssignmentMediator => base.Mediator<MediaAssignmentMediator>();
        protected MediaWebMediator? MediaWebMediator => base.Mediator<MediaWebMediator>();
        protected ExportMediator? ExportMediator => base.Mediator<ExportMediator>();

        // Proxy
        protected DatHierarchyProxy? DatHierarchyProxy => base.Model<DatHierarchyProxy>();
        protected DatDetailsProxy? DatDetailsProxy => base.Model<DatDetailsProxy>();
        protected CreateDatProxy? CreateDatProxy => base.Model<CreateDatProxy>();
        protected ExpressionsProxy? ExpressionsProxy => base.Model<ExpressionsProxy>();
        protected ProjectProxy? ProjectProxy => base.Model<ProjectProxy>();
        protected GlobalSettingsProxy? GlobalSettingsProxy => base.Model<GlobalSettingsProxy>();
        protected DatDbProxy? DatDbProxy => base.Model<DatDbProxy>();
        protected ModelDataProxy? ModelDataProxy => base.Model<ModelDataProxy>();
        protected RbAuxResourceProxy? RbAuxResourceProxy => base.Model<RbAuxResourceProxy>();
        protected RbAuxMediaProxy? RbAuxMediaProxy => base.Model<RbAuxMediaProxy>();
        protected CuratedDatProxy? CuratedDatProxy => base.Model<CuratedDatProxy>();
        protected RbAuxItemLoaderProxy? RbAuxItemLoaderProxy => base.Model<RbAuxItemLoaderProxy>();
        protected ExportDatGrouperProjectProxy? ExportDatGrouperProjectProxy => base.Model<ExportDatGrouperProjectProxy>();
        protected AudioProxy? AudioProxy => base.Model<AudioProxy>();

        // Model
        protected UnitDisplayModel? UnitDisplayModel => base.Model<UnitDisplayModel>();
        protected ActiveDatsModel? ActiveDatsModel => base.Model<ActiveDatsModel>();
        protected RadioDatModel? RadioDatModel => base.Model<RadioDatModel>();
        protected AutoGrouperModel? AutoGrouperModel => base.Model<AutoGrouperModel>();
        protected DatGrouperSessionModel? DatGrouperSessionModel => base.Model<DatGrouperSessionModel>();
        protected DatGrouperModel? DatGrouperModel => base.Model<DatGrouperModel>();
    }
}
