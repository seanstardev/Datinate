using System.Collections;

namespace com.RADIO.Datinate.RMVC.Shared
{
    public class ColumnHeaderVO 
    {

        public enum NameEnum {
            Folder
            , DAT
            , Version
            , Size      // special case - has info added on end -> e.g. (KB)
            , Entries   // int
            , ROMs      // int
            , ROM       // This is the ROM NAME
            , Description
            , Entry     // used as 'Game Name' -> string
            , Ext
            , Size_PC   // needs double / decimal comparer?
        }

        public bool isAscending = false;
        public bool sortAsNumber = false;
        public NameEnum name;
 
        public ColumnHeaderVO(NameEnum name) 
        {
            switch (name) 
            {
                case NameEnum.ROMs:
                case NameEnum.Entries:
                case NameEnum.Size:
                case NameEnum.Size_PC:
                    sortAsNumber = true;
                    break;
                default:
                    sortAsNumber = false;
                    break;
            }   
        }

        public void switchSorting() 
        {
            isAscending = !isAscending;
        }

        public IComparer getSorter(int column) 
        {
            if (sortAsNumber)
                return new ListNumberComparer(column, isAscending);
            else
                return new ListStringComparer(column, isAscending);
        }

        internal class ListStringComparer : IComparer 
        {
            private int col;
            private bool invert;
            public ListStringComparer() {
                col = 0;
            }
            public ListStringComparer(int column, bool invert) 
            {
                col = column;
                this.invert = invert;
            }
            public int Compare(object x, object y) {
                String textX = ((ListViewItem)x).SubItems[col].Text;
                String textY = ((ListViewItem)y).SubItems[col].Text;

                if (!invert)
                    return String.Compare(textX, textY);
                else
                    return String.Compare(textY, textX);
            }
        }

        internal class ListNumberComparer : IComparer 
        {
            private int col;
            private bool invert;
            public ListNumberComparer() 
            {
                col = 0;
            }
            public ListNumberComparer(int column, bool invert) 
            {
                col = column;
                this.invert = invert;
            }

            public int Compare(object x, object y) 
            {
                string labelX = ((ListViewItem)x).SubItems[col].Text;
                string labelY = ((ListViewItem)y).SubItems[col].Text;

                // With this ' ' space check we can ignore percent, (MB), etc....
                labelX = labelX.Split(' ')[0];
                labelY = labelY.Split(' ')[0];

                Double nbrX = Convert.ToDouble(labelX);
                Double nbrY = Convert.ToDouble(labelY);

                if (nbrX == nbrY) 
                    return 0;
                
                else if (nbrX < nbrY) 
                    return invert ? 0 : 1;
                
                else
                    return invert ? 1 : 0;
            }
        }
    }
}
