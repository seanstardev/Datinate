namespace com.RADIO.Datinate.RMVC.Shared
{
    public static class DatinateEnums
    {

        public enum MEDIA_TYPE_ENUM
        {
            NOT_SET
            , Unspecified

            // Media (and Resource):
            , Advert
            , Box
            , Box_Back
            , Box_Inlay
            , Box_Side
            , Manual
            , Media
            , Media_Label
            , Media_Top
            , Other_Map
            , Other_Overlay
            , Snap
            , Soundtrack
            , Title
            , Video

            // Resource:
            , Info
            , Info_Credits
            , Info_About
            , Info_Releases
            , Thumb
            , Thumb_Release
            , Thumb_Screen
            //, Screen
            , Manual_Back
            , Manual_Front
            //, Box_Inside
            , Box_Top
            , Box_Bottom
            , Media_Back
            , Other
            //, Other_Advertisement
            , Other_Reference_Card
            , Other_Hardware

            // Resource - System:
            //, Logo
            //, Background

            // Mame:
            //, Cabinet
            //, Control_Panel
            //, Flyer
            //, Marquee
            //, Pcb

            // Support:
            //, Artwork
            //, Samples
        }

        public enum DAT_GROUPER_EXPORT_ENUM
        {
            NOT_SET,
            Software,
            Media
        }

        public enum DAT_FORMAT_ENUM
        {
            ClrMamePro, 
            LogiqxXml, 
            MameSoftwareListXml, 
            DatinateNative,       // internal name
            MameListXml,
            DosCenter
        }
        public enum DAT_GROUP_ENUM
        {
            NOT_SET
            , MAME
            , MAME_MEDIA
            , MAME_SL
            , NO_INTRO
            , R2DAT_EMUMOVIES
            , R2DAT_REPLACEMENT_DOCS
            , RADIO_INTERNAL
            , R2DAT_WEB
            , REDUMP
            , T_EN
            , TDC
            , TOSEC
            , TOSEC_ISO
            , TOSEC_PIX
            , VGMARCHIVE
            // WHD_LOAD
        }
        public enum COLLECTION_SET_ENUM
        {
            NOT_SET, 
            Media, 
            Resource, 
            Software, 
            Support
        }

        public enum MEDIA_ASSIGNMENT_ENUM
        {
            None,
            Assigned,
            NotFound
        }
        public enum DAT_GROUPER_ACTION_ENUM
        {
            NOT_SET,
            FamiliesExcludedHide,
            FamiliesExcludedShow,
            PartAliasesHide,
            PartAliasesShow,
            StateRedo,
            StateUndo,
        }
        public enum DAT_GROUPER_MEDIA_MODE
        {
            NOT_SET,
            MediaAssign,
            MediaReadOnly,
        }
        public enum DAT_GROUPER_LAYOUT_ENUM
        {
            NOT_SET,
            AutoGrouper,
            Curated_Standard,
            Curated_WebInMiddle,
            Media_Auto_Assign,
            Media_Curated_Assign,
            Media_Auto_ReadOnly,
            Media_Curated_ReadOnly
        }

        public enum DAT_GROUP_TARGET_ENUM 
        {
            NOT_SET,
            GAMES_INCLUDE_GROUP,
            MEDIA_INCLUDE_GROUP,
            RESOURCE_INCLUDE_GROUP
        }
        public enum DAT_GROUPER_MEMBERSHIP_ENUM
        {
            NOT_SET,
            INCLUDE_EXPLICIT,
            INCLUDE_IMPLICIT,
            INCLUDE_RELATIVE,
            EXCLUDE_RELATIVE,
            EXCLUDE_EXPLICIT
        }

        public enum EXPRESSIONS_FILE_TARGET_ENUM 
        {
            INVALID
            , CUSTOM_LIST
            , PROJECT_LOADER
        }

        public enum DAT_SCREEN_ENUM 
        {
            NOT_SET
            , Landing
            , DatManager
        }
        public enum MAIN_CONTROL_ENUM
        {
            NOT_SET,
            DatGrouper,
            DatCustomiser,
            DatCompare
        }
    }
}
