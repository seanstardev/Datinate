using com.RADIO.Datinate.RMVC.Shared;
using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace Datinate.Shared
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

        void ReloadCurrentProject();
        void ClearView();
    }
}
