using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC.Shared
{
    public class DatHeaderVO {
        
        public DAT_FORMAT_ENUM DatTypeEnum { get; }
        public string Name { get; }
        public string Description { get; }
        public string Category { get; }
        public string Version { get; }
        public string Author { get; }
        public string Comment { get; }

        public DatHeaderVO(
            DAT_FORMAT_ENUM datTypeEnum
            , string name
            , string description
            , string category
            , string version
            , string author
            , string comment
        ) {
            this.Name = name;
            this.Description = description;
            this.Version = version;
            this.Author = author;
            this.Category = category;
            this.Comment = comment;
            this.DatTypeEnum = datTypeEnum;
        }

        override public string ToString() {

            string str =
                "dat type   : " + DatTypeEnum + "\n" +
                "name       : " + Name + "\n" +
                "description: " + Description + "\n" +
                "category   : " + Category + "\n" +
                "version    : " + Version + "\n" +
                "author     : " + Author + "\n" +
                "comment    : " + Comment + "\n";

            return str;
        }

        public DatHeaderVO Clone() {
            return new DatHeaderVO(
                DatTypeEnum
                , Name
                , Description
                , Category
                , Version
                , Author
                , Comment
            );
        }
    }
}
