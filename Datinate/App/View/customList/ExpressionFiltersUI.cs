using datinate.shared;
using WinFormsView = System.Windows.Forms.View;

namespace Datinate.App.View.customList
{
    public partial class ExpressionFiltersUI : UserControl
    {
        public event Action? LoadExpressionsEvt;
        public event Action<List<DatFilter>>? ExpressionsChangedEvt;

        public event Action<DatFilter[]>? SaveExpressionsEvt;
        
        private List<DatFilter> expressions = new List<DatFilter>();
        
        public ExpressionFiltersUI()
        {
            InitializeComponent();

            expressionListView_lv.View = WinFormsView.Details;
            expressionListView_lv.GridLines = true;
            expressionListView_lv.HideSelection = false;
        }

        public void SetExpressionsUI(List<DatFilter> expressions)
        {
            ClearUI();
            this.expressions = expressions;
            UpdateConditionsView();
        }
        public void ClearUI()
        {
            expressions.Clear();
            expressionListView_lv.Items.Clear();
        }

        public void AddExpression(DatFilter expression)
        {
            expressions.Add(expression);

            UpdateConditionsView();
            ExpressionsChangedEvt?.Invoke(expressions);
        }

        public void UnselectAll()
        {
            if (expressionListView_lv.SelectedItems.Count > 0)
            {
                foreach (var item in expressionListView_lv.SelectedItems.Cast<ListViewItem>().ToArray())
                    item.Selected = false;
            }
        }
        private void loadExpressionsBtn_Click(object sender, EventArgs e)
        {
            LoadExpressionsEvt?.Invoke();
        }

        private void saveExpressionsBtn_Click(object sender, EventArgs e)
        {
            SaveExpressionsEvt?.Invoke(expressions.ToArray());
        }
        private void deleteExpression_btn_Click(object sender, EventArgs e)
        {
            if (expressionListView_lv.SelectedItems.Count == 0)
                return;

            var currentIndex = expressionListView_lv.SelectedItems[0].Index;

            if ((uint)currentIndex >= (uint)expressions.Count)
                return;

            expressions.RemoveAt(currentIndex);
            UpdateConditionsView();

            if (expressionListView_lv.Items.Count > 0)
            {
                var toSelect = Math.Min(currentIndex, expressionListView_lv.Items.Count - 1);
                expressionListView_lv.Items[toSelect].Selected = true;
            }

            ExpressionsChangedEvt?.Invoke(expressions);
        }

        private void moveDownExpression_btn_Click(object sender, EventArgs e)
        {
            if (expressionListView_lv.SelectedItems.Count == 0)
                return;

            var index = expressionListView_lv.SelectedItems[0].Index;
            if (index >= expressionListView_lv.Items.Count - 1)
                return;

            swapExpressions(index, index + 1);
            expressionListView_lv.Items[index + 1].Selected = true;

            ExpressionsChangedEvt?.Invoke(expressions);
        }

        private void moveUpExpression_btn_Click(object sender, EventArgs e)
        {
            if (expressionListView_lv.SelectedItems.Count == 0)
                return;

            var index = expressionListView_lv.SelectedItems[0].Index;
            if (index <= 0)
                return;

            swapExpressions(index, index - 1);
            expressionListView_lv.Items[index - 1].Selected = true;

            ExpressionsChangedEvt?.Invoke(expressions);
        }

        private void swapExpressions(int upIndex, int replaceIndex)
        {
            var temp = expressions[replaceIndex];
            expressions[replaceIndex] = expressions[upIndex];
            expressions[upIndex] = temp;

            UpdateConditionsView();
        }

        private void UpdateConditionsView()
        {
            expressionListView_lv.BeginUpdate();
            try
            {
                if (expressionListView_lv.Columns.Count == 0)
                {
                    expressionListView_lv.Columns.Add("Priority", 50, HorizontalAlignment.Left);
                    expressionListView_lv.Columns.Add("Effect", 150);
                    expressionListView_lv.Columns.Add("Expression", 200);
                }

                expressionListView_lv.Items.Clear();

                for (int i = 0; i < expressions.Count; i++)
                {
                    string action;
                    var colour = DatFilterHelper.GetExpressionColour(expressions[i].GetExpressionAction());

                    if (expressions[i].ExcludeAlways())
                        action = "EXCLUDE";
                    else if (expressions[i].IncludeAlways())
                        action = "INCLUDE";
                    else
                        action = "CONDITIONAL EXCLUDE";

                    var item = expressionListView_lv.Items.Add((i + 1).ToString());
                    item.UseItemStyleForSubItems = false;

                    var subItem = item.SubItems.Add(action);
                    subItem.ForeColor = colour;

                    item.SubItems.Add(expressions[i].GetUserFriendlyExpression());
                }
            }
            finally
            {
                expressionListView_lv.EndUpdate();
            }
        }
    }
}
