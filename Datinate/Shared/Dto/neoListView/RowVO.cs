namespace com.RADIO.Datinate.RMVC.Shared
{
    public class RowVO 
    {
        public ColumnHeaderVO.NameEnum columnName;
        public string value;

        public RowVO(
            ColumnHeaderVO.NameEnum columnName
            , string value) 
        {
            this.columnName = columnName;
            this.value = value;
        }
    }
}
