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
                Electron.IpcMain.Send(mainWindow, "select-directory-reply", dirs);
            });
        }
    }
}
