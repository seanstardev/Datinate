namespace datinate.app
{
    public static class FormsHelper
    {
        public static void ShowWarningDialog(string body)
        {
            MessageBox.Show(body, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
        public static void HideTabs(TabControl tabControl)
        {
            tabControl.Appearance = TabAppearance.FlatButtons;
            tabControl.ItemSize = new Size(0, 1);
            tabControl.SizeMode = TabSizeMode.Fixed;
        }
    }
}
