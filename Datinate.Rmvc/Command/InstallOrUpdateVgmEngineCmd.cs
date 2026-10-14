using Datinate.Shared;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class InstallOrUpdateVgmEngineCmd : CommandBase<RVoid>
    {
        protected override async Task<RVoid> Run()
        {
            if (base.AudioProxy == null || base.Shell == null)
                return await Complete();

            bool vgmInstalled = base.AudioProxy.IsVgmEngineInstalled;

            string title = vgmInstalled
                ? "Update VGMPlay-JS-2?"
                : "Install VGMPlay-JS-2?";

            string body = vgmInstalled
                ? @"Datinate can update its optional VGMPlay-JS-2 audio support to the latest available version.

VGMPlay-JS-2 is independent third-party software and is not part of Datinate. It incorporates software from a number of other projects under their respective licences.

If you continue, Datinate will download the unmodified VGMPlay-JS-2 extension directly from its official GitHub release and replace the currently installed version.

Do you want to download and install the latest version?"
                : @"Datinate can use the optional VGMPlay-JS-2 project to provide playback support for additional video-game audio formats.

VGMPlay-JS-2 is independent third-party software and is not part of Datinate. It incorporates software from a number of other projects under their respective licences.

If you continue, Datinate will download the unmodified VGMPlay-JS-2 extension directly from its official GitHub release and install it for use by Datinate.

Datinate does not require VGMPlay-JS-2 and will continue to operate normally without it.

Do you want to download and install VGMPlay-JS-2?";

            bool okClicked = await base.Shell.ShowMessageBox(title, body, true);

            if (!okClicked)
                return await Complete();

            string operation = vgmInstalled ? "Updating" : "Installing";

            bool success = await base.AudioProxy.InstallOrUpdateVgmEngine(
                    Constants.VGM_DOWNLOAD_URL,
                    async (current, total, txt) =>
                    {
                        string message =
                            operation + " VGMPlay-JS-2 " +
                            current + " / " + total + ": " +
                            txt;

                        await base.Execute(
                            new ShowProgressCmd(message, current, total));
                    });

            await base.Execute(new ClearProgressCmd());

            if (success)
            {
                string successMessage = vgmInstalled
                    ? "VGMPlay-JS-2 has been updated successfully."
                    : "VGMPlay-JS-2 has been installed successfully.";

                _ = await base.Shell.ShowMessageBox(
                    "Complete",
                    successMessage);
            }
            else
            {
                string errorMessage = vgmInstalled
                    ? "The VGMPlay-JS-2 update was unsuccessful."
                    : "The VGMPlay-JS-2 installation was unsuccessful.";

                _ = await base.Shell.ShowMessageBox(
                    "Error",
                    errorMessage);
            }

            return await Complete();
        }
    }
}