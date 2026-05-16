using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace datinate.app
{
    public static class DatinatePerformanceUtil
    {
        private const bool Disable = false;
        public const bool SCROLLER_UseStock = Disable;

        public const bool FLOW_LAYOUT_UseStock = Disable;

        public const bool TREEVIEW_UseStockDraw = Disable;
        public const bool TREEVIEW_UseStockDraw_SkipDrawFocusNode = Disable;
        public const bool TREEVIEW_AwaitHandleCreation = Disable;
        public const bool TREEVIEW_SkipFastSpawn = Disable;

        public const bool WV2_FullySupress = Disable;
        public const bool WV2_SkipUseManager = Disable;
        public const bool WV2_DoNotUseGpu = Disable;

        internal static class DatinateWebView2KillSwitch
        {
            private sealed class KillState
            {
                public bool EventsHooked;
                public bool CoreHooked;
            }

            private static readonly ConcurrentDictionary<int, WeakReference<WebView2>> Registry = new();
            private static readonly ConditionalWeakTable<WebView2, KillState> StateByView = new();

            public static void Register(WebView2 webView)
            {
                if (!WV2_FullySupress)
                    return;

                if (webView == null)
                    return;

                if (webView.InvokeRequired)
                {
                    try { webView.BeginInvoke((Action)(() => Register(webView))); } catch { }
                    return;
                }

                RegisterInternal(webView);
            }

            private static void RegisterInternal(WebView2 webView)
            {
                var key = RuntimeHelpers.GetHashCode(webView);
                Registry[key] = new WeakReference<WebView2>(webView);

                var st = StateByView.GetOrCreateValue(webView);

                if (!st.EventsHooked)
                {
                    st.EventsHooked = true;

                    try { webView.HandleCreated += WebView_HandleCreated; } catch { }
                    try { webView.VisibleChanged += WebView_VisibleChanged; } catch { }
                    try { webView.EnabledChanged += WebView_EnabledChanged; } catch { }
                    try { webView.Disposed += WebView_Disposed; } catch { }

                    try { webView.CoreWebView2InitializationCompleted += WebView_CoreInitCompleted; } catch { }
                }

                Suppress(webView);
                TryHookCore(webView);
            }

            public static void Unregister(WebView2? webView)
            {
                if (!WV2_FullySupress)
                    return;

                if (webView == null)
                    return;

                if (webView.InvokeRequired)
                {
                    try { webView.BeginInvoke((Action)(() => Unregister(webView))); } catch { }
                    return;
                }

                var key = RuntimeHelpers.GetHashCode(webView);
                Registry.TryRemove(key, out _);

                TryUnhookEvents(webView);
            }

            private static void TryUnhookEvents(WebView2 webView)
            {
                try { webView.HandleCreated -= WebView_HandleCreated; } catch { }
                try { webView.VisibleChanged -= WebView_VisibleChanged; } catch { }
                try { webView.EnabledChanged -= WebView_EnabledChanged; } catch { }
                try { webView.Disposed -= WebView_Disposed; } catch { }

                try { webView.CoreWebView2InitializationCompleted -= WebView_CoreInitCompleted; } catch { }

                if (StateByView.TryGetValue(webView, out var st))
                {
                    st.EventsHooked = false;
                    st.CoreHooked = false;
                }
            }

            private static void WebView_HandleCreated(object? sender, EventArgs e)
            {
                if (!WV2_FullySupress)
                    return;

                if (sender is WebView2 wv)
                {
                    Suppress(wv);
                    TryHookCore(wv);
                }
            }

            private static void WebView_VisibleChanged(object? sender, EventArgs e)
            {
                if (!WV2_FullySupress)
                    return;

                if (sender is WebView2 wv)
                    Suppress(wv);
            }

            private static void WebView_EnabledChanged(object? sender, EventArgs e)
            {
                if (!WV2_FullySupress)
                    return;

                if (sender is WebView2 wv)
                    Suppress(wv);
            }

            private static void WebView_Disposed(object? sender, EventArgs e)
            {
                if (sender is WebView2 wv)
                    Unregister(wv);
            }

            private static void WebView_CoreInitCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
            {
                if (!WV2_FullySupress)
                    return;

                if (sender is WebView2 wv)
                {
                    Suppress(wv);
                    TryHookCore(wv);
                }
            }

            private static void TryHookCore(WebView2 wv)
            {
                if (!WV2_FullySupress)
                    return;

                if (wv.IsDisposed)
                    return;

                var st = StateByView.GetOrCreateValue(wv);
                if (st.CoreHooked)
                    return;

                var core = wv.CoreWebView2;
                if (core == null)
                    return;

                st.CoreHooked = true;

                try { core.NavigationStarting += Core_NavigationStarting; } catch { }
                try { core.FrameNavigationStarting += Core_FrameNavigationStarting; } catch { }
                try { core.NewWindowRequested += Core_NewWindowRequested; } catch { }

                try
                {
                    var s = core.Settings;
                    s.IsScriptEnabled = false;
                    s.AreDefaultContextMenusEnabled = false;
                    s.AreBrowserAcceleratorKeysEnabled = false;
                }
                catch { }

                try { core.Stop(); } catch { }
                try { core.Navigate("about:blank"); } catch { }
                try { _ = core.TrySuspendAsync(); } catch { }
            }

            private static void Core_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
            {
                if (WV2_FullySupress)
                    e.Cancel = true;
            }

            private static void Core_FrameNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
            {
                if (WV2_FullySupress)
                    e.Cancel = true;
            }

            private static void Core_NewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
            {
                if (WV2_FullySupress)
                    e.Handled = true;
            }

            private static void Suppress(WebView2 wv)
            {
                if (wv.IsDisposed)
                    return;

                try { wv.AllowExternalDrop = false; } catch { }

                try { wv.Enabled = false; } catch { }
                try { wv.Visible = false; } catch { }

                try
                {
                    var core = wv.CoreWebView2;
                    if (core != null)
                    {
                        try { core.Stop(); } catch { }
                        try { core.Navigate("about:blank"); } catch { }
                        try { _ = core.TrySuspendAsync(); } catch { }
                    }
                }
                catch { }
            }
        }
    }
}
