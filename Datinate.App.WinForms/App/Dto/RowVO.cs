namespace Datinate.App.WinForms.View.Dto
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
