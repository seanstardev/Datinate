using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public class DatGrouperSessionModel : RModel
    {
        public bool ContentPathsResolved { get; set; }
        public bool MediaInitialisd { get; set; } = false;

        public long PartsTotal { get; set; } = 0L;
        public long PartsCurated { get; set; } = 0L;

        public DAT_GROUPER_LAYOUT_ENUM CurrentLayout { get; private set; } = DAT_GROUPER_LAYOUT_ENUM.AutoGrouper;

        public bool IsInMediaReadOnlyMode =>
            CurrentLayout == DAT_GROUPER_LAYOUT_ENUM.Media_Auto_ReadOnly ||
            CurrentLayout == DAT_GROUPER_LAYOUT_ENUM.Media_Curated_ReadOnly;
    
        public bool IsInMediaAssignmentMode =>
            CurrentLayout == DAT_GROUPER_LAYOUT_ENUM.Media_Auto_Assign ||
            CurrentLayout == DAT_GROUPER_LAYOUT_ENUM.Media_Curated_Assign;
        
        public bool IsInMediaMode =>
            CurrentLayout == DAT_GROUPER_LAYOUT_ENUM.Media_Auto_ReadOnly ||
            CurrentLayout == DAT_GROUPER_LAYOUT_ENUM.Media_Curated_ReadOnly ||
            CurrentLayout == DAT_GROUPER_LAYOUT_ENUM.Media_Curated_Assign || 
            CurrentLayout == DAT_GROUPER_LAYOUT_ENUM.Media_Auto_Assign;

        private DAT_GROUPER_LAYOUT_ENUM LastStandardLayout = DAT_GROUPER_LAYOUT_ENUM.Curated_Standard;

        public DAT_GROUPER_LAYOUT_ENUM ClearSession()
        {
            ContentPathsResolved = false;
            MediaInitialisd = false;

            PartsTotal = 0L;
            PartsCurated = 0L;

            CurrentLayout = DAT_GROUPER_LAYOUT_ENUM.AutoGrouper;
            LastStandardLayout = DAT_GROUPER_LAYOUT_ENUM.AutoGrouper;
            return CurrentLayout;
        }
        public DAT_GROUPER_LAYOUT_ENUM ActivateCurationMode()
        {
            LastStandardLayout = CurrentLayout = DAT_GROUPER_LAYOUT_ENUM.Curated_Standard;
            return CurrentLayout;
        }

        public DAT_GROUPER_LAYOUT_ENUM EnterMediaMode(bool isFromAuto, bool enterPreviewMode)
        {
            if (isFromAuto)
            {
                if (enterPreviewMode)
                {
                    if (CurrentLayout != DAT_GROUPER_LAYOUT_ENUM.Media_Auto_ReadOnly)
                        CurrentLayout = DAT_GROUPER_LAYOUT_ENUM.Media_Auto_ReadOnly;
                    
                    return CurrentLayout;
                }
                else
                {
                    if (CurrentLayout != DAT_GROUPER_LAYOUT_ENUM.Media_Auto_Assign)
                        CurrentLayout = DAT_GROUPER_LAYOUT_ENUM.Media_Auto_Assign;

                    return CurrentLayout;
                }
            }
            else
            {
                if (enterPreviewMode)
                {
                    if (CurrentLayout != DAT_GROUPER_LAYOUT_ENUM.Media_Curated_ReadOnly)
                        CurrentLayout = DAT_GROUPER_LAYOUT_ENUM.Media_Curated_ReadOnly;

                    return CurrentLayout;
                }
                else
                {
                    if (CurrentLayout != DAT_GROUPER_LAYOUT_ENUM.Media_Curated_Assign)
                        CurrentLayout = DAT_GROUPER_LAYOUT_ENUM.Media_Curated_Assign;

                    return CurrentLayout;
                }
            }
        }

        public DAT_GROUPER_LAYOUT_ENUM ExitMediaMode()
        {
            CurrentLayout = LastStandardLayout;
            return CurrentLayout;
        }

        public DAT_GROUPER_LAYOUT_ENUM ToggleCuratedLayout()
        {
            if (LastStandardLayout == DAT_GROUPER_LAYOUT_ENUM.Curated_Standard)
                LastStandardLayout = DAT_GROUPER_LAYOUT_ENUM.Curated_WebInMiddle;
             
            else
                LastStandardLayout = DAT_GROUPER_LAYOUT_ENUM.Curated_Standard;

            CurrentLayout = LastStandardLayout;
            return LastStandardLayout;
        }
    }
}
