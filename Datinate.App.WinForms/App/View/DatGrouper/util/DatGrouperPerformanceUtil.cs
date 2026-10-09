using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace datinate.app
{
    internal sealed class DatGrouperPerformanceUtil : IMessageFilter
    {
        public readonly record struct Result(
            string Label,
            DateTime Start,
            DateTime End,
            TimeSpan Duration,
            bool Painted,
            string Reason);

        public static bool VerboseEnabled { get; set; } = false;

        private const int WM_PAINT = 0x000F;
        private const int WM_NCPAINT = 0x0085;

        private static readonly ConditionalWeakTable<Control, Gate> gates = new();

        private sealed class Gate
        {
            private readonly object sync = new();
            private long seq;
            private WeakReference<DatGrouperPerformanceUtil>? active;

            public void Register(DatGrouperPerformanceUtil inst, out long mySeq, out DatGrouperPerformanceUtil? previous)
            {
                lock (sync)
                {
                    mySeq = ++seq;

                    previous = null;
                    if (active != null && active.TryGetTarget(out var p))
                        previous = p;

                    active = new WeakReference<DatGrouperPerformanceUtil>(inst);
                }
            }

            public bool IsCurrent(long mySeq, DatGrouperPerformanceUtil inst)
            {
                lock (sync)
                {
                    if (seq != mySeq)
                        return false;

                    if (active == null)
                        return false;

                    if (!active.TryGetTarget(out var p))
                        return false;

                    return ReferenceEquals(p, inst);
                }
            }

            public void UnregisterIfCurrent(long mySeq, DatGrouperPerformanceUtil inst)
            {
                lock (sync)
                {
                    if (seq != mySeq)
                        return;

                    if (active == null)
                        return;

                    if (!active.TryGetTarget(out var p))
                    {
                        active = null;
                        return;
                    }

                    if (ReferenceEquals(p, inst))
                        active = null;
                }
            }
        }

        private readonly Control target;

        private readonly long startStamp;
        private readonly DateTime startLocal;
        private readonly string label;

        private readonly Action<Result>? onEndStrong;
        private readonly WeakReference<Action<Result>>? onEndWeak;
        private readonly bool runCallbackOnThreadPool;

        private long mySeq;
        private bool finished;
        private bool kicked;
        private bool filterInstalled;

        private System.Windows.Forms.Timer? timeoutTimer;
        private System.Windows.Forms.Timer? verboseWaitTimer;

        public DatGrouperPerformanceUtil(
            Control target,
            Action<Result>? onEnd = null,
            bool weakCallback = false,
            bool runCallbackOnThreadPool = true,
            int timeoutMs = 60000)
        {
            this.target = target;

            startStamp = Stopwatch.GetTimestamp();
            startLocal = DateTime.Now;
            label = BuildLabel(target);

            if (onEnd != null)
            {
                if (weakCallback)
                    onEndWeak = new WeakReference<Action<Result>>(onEnd);
                else
                    onEndStrong = onEnd;
            }

            this.runCallbackOnThreadPool = runCallbackOnThreadPool;

            Debug.WriteLine($"[TIMING] Start: {label}.");
            if (!target.IsHandleCreated)
            {
                target.HandleCreated += Target_HandleCreated;
                target.Disposed += Target_DisposedEarly;
                return;
            }

            InitOnUiThread(timeoutMs);
        }

        private void Target_HandleCreated(object? sender, EventArgs e)
        {
            target.HandleCreated -= Target_HandleCreated;
            target.Disposed -= Target_DisposedEarly;

            if (finished)
                return;

            InitOnUiThread(60000);
        }

        private void Target_DisposedEarly(object? sender, EventArgs e)
        {
            target.HandleCreated -= Target_HandleCreated;
            target.Disposed -= Target_DisposedEarly;

            Finish(painted: false, reason: "disposed before init", allowCallbackIfCurrent: false);
        }

        private void InitOnUiThread(int timeoutMs)
        {
            if (finished)
                return;

            if (target.InvokeRequired)
            {
                target.BeginInvoke(new Action(() => InitOnUiThread(timeoutMs)));
                return;
            }

            var gate = gates.GetOrCreateValue(target);

            gate.Register(this, out mySeq, out var prev);
            if (prev != null && !ReferenceEquals(prev, this))
                prev.RequestSuperseded();

            target.VisibleChanged += Target_StateChanged;
            target.HandleDestroyed += Target_HandleDestroyed;
            target.Disposed += Target_Disposed;

            InstallFilter();

            timeoutTimer = new System.Windows.Forms.Timer { Interval = Math.Max(250, timeoutMs) };
            timeoutTimer.Tick += (_, __) => Finish(painted: false, reason: "timeout", allowCallbackIfCurrent: false);
            timeoutTimer.Start();

            if (VerboseEnabled)
            {
                Debug.WriteLine($"[TIMING][TTFP][v] CHAIN {DumpVisibilityChain(target)}");

                verboseWaitTimer = new System.Windows.Forms.Timer { Interval = 1000 };
                verboseWaitTimer.Tick += (_, __) =>
                {
                    if (finished)
                        return;

                    Debug.WriteLine($"[TIMING][TTFP][v] WAIT {label} {GetBlockReason()}");
                };
                verboseWaitTimer.Start();
            }

            TryKick();
        }

        private void RequestSuperseded()
        {
            if (finished)
                return;

            if (target.IsDisposed)
            {
                Finish(painted: false, reason: "superseded", allowCallbackIfCurrent: false);
                return;
            }

            if (target.InvokeRequired)
            {
                try { target.BeginInvoke(new Action(() => Finish(painted: false, reason: "superseded", allowCallbackIfCurrent: false))); }
                catch { }
                return;
            }

            Finish(painted: false, reason: "superseded", allowCallbackIfCurrent: false);
        }

        public bool PreFilterMessage(ref Message m)
        {
            if (finished)
                return false;

            if (!target.IsHandleCreated)
                return false;

            if (m.HWnd != target.Handle)
                return false;

            if (m.Msg != WM_PAINT && m.Msg != WM_NCPAINT)
                return false;

            if (!IsEffectivelyVisible(target))
                return false;

            Finish(painted: true, reason: $"first paint msg=0x{m.Msg:X}", allowCallbackIfCurrent: true);
            return false;
        }

        private void Target_StateChanged(object? sender, EventArgs e)
        {
            if (finished)
                return;

            TryKick();
        }

        private void Target_HandleDestroyed(object? sender, EventArgs e)
        {
            if (finished)
                return;

            Finish(painted: false, reason: "handle destroyed", allowCallbackIfCurrent: false);
        }

        private void Target_Disposed(object? sender, EventArgs e)
        {
            if (finished)
                return;

            Finish(painted: false, reason: "disposed", allowCallbackIfCurrent: false);
        }

        private void TryKick()
        {
            if (finished)
                return;

            if (!target.IsHandleCreated)
                return;

            if (!IsEffectivelyVisible(target))
                return;

            if (!kicked)
            {
                kicked = true;
                target.Invalidate(true);
            }
        }

        private void Finish(bool painted, string reason, bool allowCallbackIfCurrent)
        {
            if (finished)
                return;

            finished = true;

            var endLocal = DateTime.Now;
            var duration = Stopwatch.GetElapsedTime(startStamp);

            var suffix = reason == "first paint msg=0xF" ? "" : $" [{reason}]";
            var dur = $"{duration.Minutes:00}:{duration.Seconds:00}.{duration.Milliseconds:000}";
            Debug.WriteLine($"[TIMING] End: {label}. Time: {duration}");

            var gate = gates.GetOrCreateValue(target);
            var isCurrent = gate.IsCurrent(mySeq, this);

            if (VerboseEnabled)
                Debug.WriteLine($"[TIMING][TTFP][v] CURRENT={isCurrent} LABEL={label} REASON={reason}");

            if (allowCallbackIfCurrent && isCurrent)
            {
                var r = new Result(label, startLocal, endLocal, duration, painted, reason);
                InvokeCallback(r);
            }

            gate.UnregisterIfCurrent(mySeq, this);
            Teardown();
        }

        private void InvokeCallback(Result r)
        {
            Action<Result>? cb = onEndStrong;

            if (cb == null && onEndWeak != null)
                onEndWeak.TryGetTarget(out cb);

            if (cb == null)
                return;

            if (runCallbackOnThreadPool)
                ThreadPool.QueueUserWorkItem(_ => cb(r));
            else
                cb(r);
        }

        private void Teardown()
        {
            if (timeoutTimer != null)
            {
                timeoutTimer.Stop();
                timeoutTimer.Dispose();
                timeoutTimer = null;
            }

            if (verboseWaitTimer != null)
            {
                verboseWaitTimer.Stop();
                verboseWaitTimer.Dispose();
                verboseWaitTimer = null;
            }

            RemoveFilter();

            target.VisibleChanged -= Target_StateChanged;
            target.HandleDestroyed -= Target_HandleDestroyed;
            target.Disposed -= Target_Disposed;

            target.HandleCreated -= Target_HandleCreated;
            target.Disposed -= Target_DisposedEarly;
        }

        private static string BuildLabel(Control c)
        {
            var n = string.IsNullOrWhiteSpace(c.Name) ? "" : $" '{c.Name}'";
            return $"{c.GetType().Name}{n}";
        }

        private bool IsEffectivelyVisible(Control c)
        {
            if (!c.Visible)
                return false;

            if (c.Width <= 0 || c.Height <= 0)
                return false;

            var f = c.FindForm();
            if (f != null && !f.Visible)
                return false;

            for (Control? p = c.Parent; p != null; p = p.Parent)
            {
                if (!p.Visible)
                    return false;
            }

            return true;
        }

        private string GetBlockReason()
        {
            if (target.IsDisposed)
                return "blocked: disposed";

            if (!target.IsHandleCreated)
                return "blocked: handle not created";

            if (!target.Visible)
                return "blocked: Visible=false";

            if (target.Width <= 0 || target.Height <= 0)
                return $"blocked: size {target.Width}x{target.Height}";

            for (Control? p = target; p != null; p = p.Parent)
            {
                if (!p.Visible)
                    return $"blocked: ancestor Visible=false ({p.GetType().Name} '{p.Name}')";
            }

            var f = target.FindForm();
            if (f != null && !f.Visible)
                return $"blocked: form Visible=false ({f.GetType().Name} '{f.Name}')";

            return "blocked: waiting for paint";
        }

        private string DumpVisibilityChain(Control c)
        {
            var sb = new StringBuilder();
            sb.Append($"{c.GetType().Name} '{c.Name}' V={c.Visible} H={c.IsHandleCreated} S={c.Width}x{c.Height}");

            var f = c.FindForm();
            if (f != null)
                sb.Append($" | Form {f.GetType().Name} '{f.Name}' V={f.Visible} H={f.IsHandleCreated}");

            for (Control? p = c.Parent; p != null; p = p.Parent)
            {
                sb.Append(" <- ");
                sb.Append($"{p.GetType().Name} '{p.Name}' V={p.Visible} H={p.IsHandleCreated} S={p.Width}x{p.Height}");
            }

            return sb.ToString();
        }

        private void InstallFilter()
        {
            if (filterInstalled)
                return;

            Application.AddMessageFilter(this);
            filterInstalled = true;
        }

        private void RemoveFilter()
        {
            if (!filterInstalled)
                return;

            Application.RemoveMessageFilter(this);
            filterInstalled = false;
        }
    }
}
