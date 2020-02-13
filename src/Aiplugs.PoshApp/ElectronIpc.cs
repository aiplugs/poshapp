using ElectronNET.API;
using ElectronNET.API.Entities;
using System.Linq;

namespace Aiplugs.PoshApp
{
    public class ElectronIpc
    {
        public static void Setup()
        {
            Electron.IpcMain.On("select-directory", async (args) => {
                var mainWindow = Electron.WindowManager.BrowserWindows.First();
                var options = new OpenDialogOptions
                {
                    Properties = new OpenDialogProperty[] {
                        OpenDialogProperty.openDirectory
                    }
                };

                string[] dirs = await Electron.Dialog.ShowOpenDialogAsync(mainWindow, options);
                Electron.IpcMain.Send(mainWindow, "select-directory-reply", dirs, args);
            });

            Electron.IpcMain.On("select-file", async (args) => {
                var mainWindow = Electron.WindowManager.BrowserWindows.First();
                var options = new OpenDialogOptions
                {
                    Properties = new OpenDialogProperty[] {
                        OpenDialogProperty.openFile,
                    }
                };

                string[] dirs = await Electron.Dialog.ShowOpenDialogAsync(mainWindow, options);
                Electron.IpcMain.Send(mainWindow, "select-file-reply", dirs, args);
            });

            Electron.IpcMain.On("open-activation", async (args) =>
            {
                await Electron.Shell.OpenExternalAsync("https://poshapp.aiplugs.com/licenses");
            });

            Electron.IpcMain.On("copy-to", (text) =>
            {
                Electron.Clipboard.WriteText(text.ToString());
            });
        }
    }
}
