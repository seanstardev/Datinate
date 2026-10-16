using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Datinate.App.Shared.Web.Audio
{
    /// <summary>
    /// Uses one shared headless audio facade to determine whether Datinate's audio
    /// system can handle a complete source. The visible browser is not touched.
    /// Probes are deliberately serialised. Each host keeps its own coordinator.
    /// </summary>
    public sealed class DatinateAudioWebProbe
    {
        private const bool VERBOSE_DEBUG = false;

        private readonly Func<string, DatinateAudioWebSessionBase> createProbe;
        private readonly SemaphoreSlim gate = new(1, 1);

        private DatinateAudioWebSessionBase? sharedProbe;

        public DatinateAudioWebProbe(Func<string, DatinateAudioWebSessionBase> createProbe)
        {
            this.createProbe = createProbe;
        }

        public async Task<bool> CanPlayAsync(
            Uri source,
            string audioWebRoot,
            CancellationToken cancellationToken = default)
        {
            string root = DatinateAudioWebSessionBase.NormaliseAudioWebRoot(audioWebRoot);

            VerboseDebug($"[AUDIO PROBE] START: {source}");
            VerboseDebug($"[AUDIO PROBE] ROOT: {root}");

            if (!DatinateAudioWebSessionBase.HasRequiredFacadeFiles(root))
            {
                Debug.WriteLine($"[AUDIO PROBE] FAILED: required facade files are missing under '{root}'.");

                return false;
            }

            await gate.WaitAsync(cancellationToken);

            try
            {
                VerboseDebug("[AUDIO PROBE] Getting shared probe environment...");

                DatinateAudioWebSessionBase probe =
                    await GetSharedProbeAsync(root, cancellationToken);

                VerboseDebug("[AUDIO PROBE] Shared probe environment ready.");
                VerboseDebug("[AUDIO PROBE] Probing source...");

                bool result = await probe.ProbeAsync(source, cancellationToken);

                VerboseDebug($"[AUDIO PROBE] RESULT: {result}");

                return result;
            }
            catch (OperationCanceledException)
            {
                VerboseDebug("[AUDIO PROBE] CANCELLED");
                DisposeSharedProbe();
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[AUDIO PROBE] FAILED: {ex.GetType().FullName}\r\n" +
                    $"HRESULT: 0x{ex.HResult:X8}\r\n" +
                    $"MESSAGE: {ex.Message}\r\n" +
                    $"STACK:\r\n{ex.StackTrace}");

                DisposeSharedProbe();

                return false;
            }
            finally
            {
                gate.Release();
            }
        }

        private async Task<DatinateAudioWebSessionBase> GetSharedProbeAsync(
            string audioWebRoot,
            CancellationToken cancellationToken)
        {
            if (sharedProbe != null && sharedProbe.AudioWebRoot == audioWebRoot)
                return sharedProbe;

            DisposeSharedProbe();

            DatinateAudioWebSessionBase probe = createProbe(audioWebRoot);

            try
            {
                await probe.InitialiseProbeAsync(cancellationToken);
                sharedProbe = probe;

                return probe;
            }
            catch
            {
                probe.Dispose();
                throw;
            }
        }

        private void DisposeSharedProbe()
        {
            DatinateAudioWebSessionBase? probe = sharedProbe;
            sharedProbe = null;
            probe?.Dispose();
        }

        /// <summary>
        /// Optional application-shutdown cleanup for the shared hidden probe browser.
        /// An active probe is left alone. Later calls may create another probe.
        /// </summary>
        public void Shutdown()
        {
            if (gate.Wait(0))
            {
                try { DisposeSharedProbe(); }
                finally { gate.Release(); }
            }
        }

        private static void VerboseDebug(string message)
        {
            if (VERBOSE_DEBUG)
#pragma warning disable CS0162 // Unreachable code detected
                Debug.WriteLine(message);
#pragma warning restore CS0162 // Unreachable code detected
        }
    }
}
