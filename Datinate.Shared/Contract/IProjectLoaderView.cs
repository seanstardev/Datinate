using Datinate.Shared.DatGrouper;
using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Shared.Rmvc
{
    public interface IProjectLoaderView : IRContract
    {
        event Action? ViewInitialisedEvt;
        event Action? LoadingProjectEvt;
        event Action<DatGrouperProjectDTO>? BuildProjectEvt;
        event Action<DatGrouperProjectDTO>? SaveProjectEvt;
        event Action? LoadExpressionsFileEvt;
        event Action<DatGrouperProjectEntry>? EditExpressionsFileEvt;
        event Action<DatGrouperProjectDTO>? HighlightEvt;
        event Action<DatGrouperProjectDTO>? DatFullpathsChangedEvt;
        event Action<string>? ShowTreeViewEvt;

        bool IsProjectLoaded { get; }
        void SetView(DatGrouperProjectDTO[] projectVOs, string? projectToLoad = null);

        void AddDat(
            DatGrouperProjectEntry datHeadlineVO, 
            DAT_GROUP_TARGET_ENUM datGroupTargetEnum,
            bool forceAddAsFirst);

        void SetExpressionsFileForLastSelected(string expressionsXmlFullpath);
        DatGrouperProjectEntry[] GetAllDatHeadlines();
        void ClearView();
    }
}
