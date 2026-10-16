using Datinate.App.Shared.Web.Audio;
using Datinate.App.WinForms.View.Util;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace Datinate.App.WinForms.View.Data
{
    /// <summary>
    /// Owns one Datinate Audio Web Facade player session for one existing WebView2.
    /// Audio probing is handled by one shared hidden WebView2 so candidate checks do
    /// not disturb the target MiniWebUI / MediaWebView. The probe WebView/Core is
    /// reused across candidate checks, while local-source mappings may require the
    /// facade document to be reloaded.
    /// </summary>
    internal sealed class DatinateAudioWebSession : DatinateAudioWebSessionBase
    {
        private static readonly DatinateAudioWebProbe SharedProbe = new(CreateProbe);

        protected override DatinateAudioWebProbe AudioProbe => SharedProbe;

        private readonly WebView2 webView;
        private readonly ProbeHostForm? ownedProbeForm;

        private CoreWebView2? core;
        private string? mediaHostName;
        private bool coreConfigured;

        private Control UiOwner => ownedProbeForm ?? (Control)webView;

        private CoreWebView2 BrowserCore => core
            ?? throw new InvalidOperationException("The audio browser core has not initialised.");

        protected override bool IsBrowserAvailable =>
            !webView.IsDisposed && webView.IsHandleCreated;

        protected override bool IsOnBrowserThread => !UiOwner.InvokeRequired;

        protected override void PostToBrowserThread(Action action)
        {
            Control owner = UiOwner;

            if (owner.IsDisposed || !owner.IsHandleCreated)
            {
                string message = ownedProbeForm == null
                    ? "WebView2 handle is unavailable."
                    : "Shared audio probe UI is unavailable.";

                throw new InvalidOperationException(message);
            }

            owner.BeginInvoke(action);
        }

        internal DatinateAudioWebSession(WebView2 webView, string audioWebRoot)
            : base(audioWebRoot)
            => this.webView = webView;

        /// <summary>
        /// One hidden WebView2, kept alive for Datinate's lifetime and reused for all
        /// audio capability probes. It is created lazily on the WinForms UI thread.
        /// </summary>
        private DatinateAudioWebSession(string audioWebRoot)
            : base(audioWebRoot)
        {
            ownedProbeForm = new ProbeHostForm
            {
                FormBorderStyle = FormBorderStyle.None,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual,
                Location = new Point(-32000, -32000),
                ClientSize = new Size(2, 2),
                Opacity = 0.01
            };

            webView = new WebView2
            {
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                AllowExternalDrop = false,
                DefaultBackgroundColor = Color.Black
            };

            try
            {
                WebView2Helper.ApplyCreationProperties(webView);
                ownedProbeForm.Controls.Add(webView);
                ownedProbeForm.Show();
            }
            catch
            {
                webView.Dispose();
                ownedProbeForm.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Uses the one shared headless audio facade to determine whether Datinate's
        /// audio system can handle this complete source. The target WebView2 is not
        /// touched. Probes are deliberately serialised.
        /// </summary>
        internal static Task<bool> CanPlayAsync(
            Uri source,
            string audioWebRoot,
            CancellationToken cancellationToken = default)
            => SharedProbe.CanPlayAsync(source, audioWebRoot, cancellationToken);

        protected override async Task<Uri?> InitialiseBrowserAsync(string audioWebRoot)
        {
            CoreWebView2 ensured = await WebView2Helper.EnsureCoreAsync(webView);

            if (!ReferenceEquals(core, ensured))
            {
                if (core != null)
                {
                    try { core.WebMessageReceived -= Core_WebMessageReceived; } catch { }
                }

                core = ensured;
                coreConfigured = false;
                core.WebMessageReceived += Core_WebMessageReceived;
            }

            if (!coreConfigured)
            {
                if (ownedProbeForm != null)
                    core.Settings.AreDefaultContextMenusEnabled = false;

                core.SetVirtualHostNameToFolderMapping(
                    AudioHostName,
                    audioWebRoot,
                    CoreWebView2HostResourceAccessKind.DenyCors);

                coreConfigured = true;
            }

            return AudioDocumentRoot;
        }

        protected override Uri? ExposeLocalFile(string filePath, string mappingId)
        {
            string? folder = Path.GetDirectoryName(filePath);

            if (string.IsNullOrWhiteSpace(folder))
                return null;

            string hostName = CreateMediaHostName(mappingId);

            BrowserCore.SetVirtualHostNameToFolderMapping(
                hostName,
                folder,
                CoreWebView2HostResourceAccessKind.Allow);

            mediaHostName = hostName;

            return CreateMediaSourceUri(hostName, Path.GetFileName(filePath));
        }

        protected override void ReleaseLocalFile()
        {
            string? hostName = mediaHostName;
            mediaHostName = null;

            if (string.IsNullOrWhiteSpace(hostName))
                return;

            core?.ClearVirtualHostNameToFolderMapping(hostName);
        }

        protected override void Navigate(Uri uri)
            => BrowserCore.Navigate(uri.AbsoluteUri);

        protected override Task<string> ExecuteScriptAsync(string javascript)
            => BrowserCore.ExecuteScriptAsync(javascript);

        private void Core_WebMessageReceived(
            object? sender,
            CoreWebView2WebMessageReceivedEventArgs e)
        {
            string json;

            try { json = e.TryGetWebMessageAsString(); }
            catch { json = e.WebMessageAsJson; }

            base.ReceiveBrowserMessage(json);
        }

        protected override void ReleaseBrowserResources()
        {
            if (core != null)
            {
                try { core.WebMessageReceived -= Core_WebMessageReceived; } catch { }

                if (coreConfigured)
                {
                    try { core.ClearVirtualHostNameToFolderMapping(AudioHostName); } catch { }
                }
            }

            core = null;
            coreConfigured = false;

            // The visible browser belongs to its view; only the hidden probe owns one.
            if (ownedProbeForm != null)
            {
                try { webView.Dispose(); } catch { }
                try { ownedProbeForm.Close(); } catch { }
                try { ownedProbeForm.Dispose(); } catch { }
            }
        }

        /// <summary>
        /// Optional application-shutdown cleanup for the shared hidden probe WebView.
        /// </summary>
        internal static void ShutdownSharedProbe()
            => SharedProbe.Shutdown();

        private static DatinateAudioWebSession CreateProbe(string audioWebRoot)
        {
            if (!Application.MessageLoop)
                throw new InvalidOperationException(
                    "The shared Datinate audio probe must first be created on the WinForms UI thread.");

            return new DatinateAudioWebSession(audioWebRoot);
        }

        private sealed class ProbeHostForm : Form
        {
            protected override bool ShowWithoutActivation => true;
        }
    }
}
