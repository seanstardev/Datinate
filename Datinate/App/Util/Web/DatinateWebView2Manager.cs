using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Reflection;
using System.Runtime.CompilerServices;
using Timer = System.Windows.Forms.Timer;

namespace datinate.app
{
    internal static class DatinateWebView2Manager
    {
        private sealed class State
        {
            public bool IsActive = true;
            public bool Transitioning;
            public bool Registered;

            public int VisibleTicks;
            public int InvisibleTicks;
        }

        private const int TickMs = 200;

        private const int HideAfterTicks = 2;
        private const int ShowAfterTicks = 1;

        private static readonly ConditionalWeakTable<WebView2, State> StateByView = [];
        private static readonly List<WeakReference<WebView2>> Views = [];
        private static readonly Timer Timer;

        private static readonly PropertyInfo? ControllerProp;
        private static readonly PropertyInfo? ControllerIsVisibleProp;

        static DatinateWebView2Manager()
        {
            ControllerProp =
                typeof(WebView2).GetProperty("CoreWebView2Controller", BindingFlags.Instance | BindingFlags.Public)
                ?? typeof(WebView2).GetProperty("CoreWebView2Controller", BindingFlags.Instance | BindingFlags.NonPublic);

            ControllerIsVisibleProp =
                typeof(CoreWebView2Controller).GetProperty("IsVisible", BindingFlags.Instance | BindingFlags.Public)
                ?? typeof(CoreWebView2Controller).GetProperty("IsVisible", BindingFlags.Instance | BindingFlags.NonPublic);

            Timer = new Timer { Interval = TickMs, Enabled = true };
            Timer.Tick += (_, __) => Tick();
        }

        public static void Register(WebView2 view)
        {
            DatinatePerformanceUtil.DatinateWebView2KillSwitch.Register(view);

            if (DatinatePerformanceUtil.WV2_FullySupress)
                return;

#pragma warning disable CS0162
            if (DatinatePerformanceUtil.WV2_SkipUseManager == false)
                RegisterInternal(view);
#pragma warning restore CS0162
        }

        public static void Unregister(WebView2? view)
        {
            DatinatePerformanceUtil.DatinateWebView2KillSwitch.Unregister(view);

            if (DatinatePerformanceUtil.WV2_FullySupress)
                return;

#pragma warning disable CS0162
            if (DatinatePerformanceUtil.WV2_SkipUseManager == false)
                UnregisterInternal(view);
#pragma warning restore CS0162
        }

        private static void RegisterInternal(WebView2 view)
        {
            if (view.InvokeRequired)
            {
                try { view.BeginInvoke((Action)(() => Register(view))); } catch { }
                return;
            }

            var state = StateByView.GetOrCreateValue(view);
            if (state.Registered)
                return;

            state.Registered = true;

            lock (Views)
            {
                Views.Add(new WeakReference<WebView2>(view));
            }

            view.HandleDestroyed += View_Teardown;
            view.Disposed += View_Teardown;

            UpdateOne(view);
        }

        private static void UnregisterInternal(WebView2? view)
        {
            if (view == null)
                return;

            if (view.InvokeRequired)
            {
                try { view.BeginInvoke((Action)(() => Unregister(view))); } catch { }
                return;
            }

            var state = StateByView.GetOrCreateValue(view);
            state.Registered = false;

            try { view.HandleDestroyed -= View_Teardown; } catch { }
            try { view.Disposed -= View_Teardown; } catch { }

            lock (Views)
            {
                for (int i = Views.Count - 1; i >= 0; i--)
                {
                    if (!Views[i].TryGetTarget(out var v) || ReferenceEquals(v, view))
                        Views.RemoveAt(i);
                }
            }
        }

        private static void View_Teardown(object? sender, EventArgs e)
        {
            if (sender is WebView2 wv)
                Unregister(wv);
        }

        private static void Tick()
        {
            List<WebView2> live = new();

            lock (Views)
            {
                for (int i = Views.Count - 1; i >= 0; i--)
                {
                    if (!Views[i].TryGetTarget(out var v) || v.IsDisposed)
                    {
                        Views.RemoveAt(i);
                        continue;
                    }

                    live.Add(v);
                }
            }

            for (int i = 0; i < live.Count; i++)
                UpdateOne(live[i]);
        }

        private static async void UpdateOne(WebView2 view)
        {
            if (view.IsDisposed || !view.IsHandleCreated)
                return;

            if (view.InvokeRequired)
            {
                try { view.BeginInvoke((Action)(() => UpdateOne(view))); } catch { }
                return;
            }

            var state = StateByView.GetOrCreateValue(view);
            if (!state.Registered)
                return;

            var core = view.CoreWebView2;
            if (core == null)
                return;

            bool visibleNow = IsEffectivelyVisible(view);

            if (visibleNow)
            {
                state.VisibleTicks++;
                state.InvisibleTicks = 0;
            }
            else
            {
                state.InvisibleTicks++;
                state.VisibleTicks = 0;
            }

            bool shouldBeActive =
                state.IsActive
                    ? state.InvisibleTicks < HideAfterTicks
                    : state.VisibleTicks >= ShowAfterTicks;

            if (state.Transitioning)
                return;

            if (shouldBeActive == state.IsActive)
                return;

            state.Transitioning = true;

            try
            {
                if (!shouldBeActive)
                {
                    TrySetControllerIsVisible(view, false);
                    try { await core.TrySuspendAsync(); } catch { }
                    state.IsActive = false;
                    return;
                }

                try { core.Resume(); } catch { }
                TrySetControllerIsVisible(view, true);
                state.IsActive = true;
            }
            finally
            {
                state.Transitioning = false;
            }
        }

        private static void TrySetControllerIsVisible(WebView2 view, bool visible)
        {
            if (ControllerProp == null || ControllerIsVisibleProp == null)
                return;

            try
            {
                var controller = ControllerProp.GetValue(view) as CoreWebView2Controller;
                if (controller == null)
                    return;

                ControllerIsVisibleProp.SetValue(controller, visible);
            }
            catch
            {
            }
        }

        private static bool IsEffectivelyVisible(Control c)
        {
            if (c.IsDisposed || !c.IsHandleCreated)
                return false;

            if (!c.Visible || c.Width <= 0 || c.Height <= 0)
                return false;

            var form = c.FindForm();
            if (form != null)
            {
                if (!form.Visible)
                    return false;

                if (form.WindowState == FormWindowState.Minimized)
                    return false;
            }

            Rectangle r;
            try { r = c.RectangleToScreen(c.ClientRectangle); }
            catch { return false; }

            if (r.Width <= 0 || r.Height <= 0)
                return false;

            var screen = Screen.FromControl(c).Bounds;
            r = Rectangle.Intersect(r, screen);
            if (r.Width <= 0 || r.Height <= 0)
                return false;

            Control? p = c.Parent;
            while (p != null)
            {
                if (!p.Visible)
                    return false;

                Rectangle pr;
                try { pr = p.RectangleToScreen(p.ClientRectangle); }
                catch { return false; }

                r = Rectangle.Intersect(r, pr);
                if (r.Width <= 0 || r.Height <= 0)
                    return false;

                p = p.Parent;
            }

            return true;
        }
    }
}
