using com.RADIO.Datinate;
using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared;
using System.ComponentModel;

namespace datinate.app
{
    public partial class ProgressView : UserControl, IProgressView
    {
        private long lastUiTick;
        private int lastBarValue = int.MinValue;
        private string lastMessage = string.Empty;

        public ProgressView()
        {
            InitializeComponent();
            progressBar.ForeColor = UIHelper.POP_COLOUR;

            Facade.RegisterActor(this);
        }

        [Category("Behavior")]
        [DefaultValue(true)]
        public bool UiThrottleEnabled { get; set; } = true;

        [Category("Behavior")]
        [DefaultValue(80)]
        public int UiThrottleMinIntervalMs { get; set; } = 80;

        [Category("Behavior")]
        [DefaultValue(1)]
        public int UiThrottleMinValueDelta { get; set; } = 1;

        [Category("Behavior")]
        [DefaultValue(true)]
        public bool UiThrottleByMessage { get; set; } = true;

        public void ClearProgress()
        {
            Ui(() =>
            {
                progressMessage.Text = "Done (" + DateTime.Now.ToString("h:mm:ss tt") + "): " + progressMessage.Text;
                progressBar.Value = Math.Min(Math.Max(progressBar.Minimum, 0), progressBar.Maximum);

                lastUiTick = 0;
                lastBarValue = int.MinValue;
                lastMessage = string.Empty;
            });
        }

        public void UpdateProgress(string message, int part, int total)
        {
            Ui(() =>
            {
                if (!message.EndsWith("."))
                    message += ".";

                int barValue;
                int percent;

                if (total <= 0)
                {
                    percent = 0;
                    barValue = progressBar.Minimum;
                }
                else
                {
                    if (part < 0) part = 0;
                    if (part > total) part = total;

                    percent = (int)Math.Round((part * 100.0) / total, MidpointRounding.AwayFromZero);
                    if (percent < 0) percent = 0;
                    if (percent > 100) percent = 100;

                    var min = progressBar.Minimum;
                    var max = progressBar.Maximum;

                    barValue = min + (int)Math.Round(((max - min) * (percent / 100.0)), MidpointRounding.AwayFromZero);
                    if (barValue < min) barValue = min;
                    if (barValue > max) barValue = max;
                }

                if (UiThrottleEnabled && ShouldSkipUiUpdate(message, barValue))
                    return;

                lastUiTick = Environment.TickCount64;
                lastBarValue = barValue;
                lastMessage = message;

                if (!string.Equals(progressMessage.Text, message, StringComparison.Ordinal))
                    progressMessage.Text = message;

                if (progressBar.Value != barValue)
                    progressBar.Value = barValue;
            });
        }

        private bool ShouldSkipUiUpdate(string message, int newBarValue)
        {
            var now = Environment.TickCount64;

            if (lastBarValue == int.MinValue)
                return false;

            var dt = now - lastUiTick;
            if (dt >= UiThrottleMinIntervalMs)
                return false;

            var valueDelta = Math.Abs(newBarValue - lastBarValue);
            if (valueDelta >= UiThrottleMinValueDelta)
                return false;

            if (!UiThrottleByMessage)
                return true;

            return string.Equals(message, lastMessage, StringComparison.Ordinal);
        }

        private void Ui(Action action)
        {
            if (InvokeRequired)
            {
                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke(action);
                return;
            }

            action();
        }

        private void HandleDisposing()
        {
            Facade.UnregisterActor(this);
        }
    }
}
