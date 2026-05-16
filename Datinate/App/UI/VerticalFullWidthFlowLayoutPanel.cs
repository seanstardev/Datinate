namespace datinate.app
{
    public sealed class VerticalFullWidthFlowLayoutPanel : FlowLayoutPanel
    {
        public VerticalFullWidthFlowLayoutPanel()
        {
            FlowDirection = FlowDirection.TopDown;
            WrapContents = false;
            AutoScroll = true;
            Margin = new Padding(0);
            Padding = new Padding(0);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            int w = ClientSize.Width - Padding.Horizontal;
            if (VerticalScroll.Visible)
                w -= SystemInformation.VerticalScrollBarWidth;

            for (int i = 0; i < Controls.Count; i++)
            {
                var c = Controls[i];

                if (c.Dock != DockStyle.None)
                    c.Dock = DockStyle.None;

                c.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

                int targetW = Math.Max(0, w - c.Margin.Horizontal);
                if (c.Width != targetW)
                    c.Width = targetW;
            }
        }
    }
}
