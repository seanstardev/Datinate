namespace com.RADIO.Datinate.RMVC.Shared
{
    public static class UIHelper 
    {
        public static readonly Color POP_COLOUR = Color.FromArgb(215, 228, 242);

        public static void PopSplitter(SplitContainer splitter)
        {
            Color p1 = splitter.Panel1.BackColor;
            Color p2 = splitter.Panel2.BackColor;

            splitter.BackColor = Color.DarkGray; // splitter “line” colour

            splitter.Panel1.BackColor = p1;
            splitter.Panel2.BackColor = p2;
        }

        public static void PopButton(Button btn) 
        {
            btn.BackColor = POP_COLOUR;
        }
        public static void HideTabs(TabControl tabControl)
        {
            tabControl.Appearance = TabAppearance.FlatButtons;
            tabControl.ItemSize = new Size(0, 1);
            tabControl.SizeMode = TabSizeMode.Fixed;
        }
    }
}
