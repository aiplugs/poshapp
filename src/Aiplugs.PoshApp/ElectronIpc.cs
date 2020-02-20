using ElectronNET.API;
using ElectronNET.API.Entities;
using System.Linq;

namespace Aiplugs.PoshApp
{
    public class ElectronIpc
    {
        public static void Setup()
        {
            var menu = new MenuItem[] {
                    new MenuItem {
                        Label = "View",
                        Submenu = new MenuItem[] {
                            new MenuItem
                            {
                                Label = "Reload",
                                Accelerator = "CmdOrCtrl+R",
                                Click = () =>
                                {
                                    // on reload, start fresh and close any old
                                    // open secondary windows
                                    Electron.WindowManager.BrowserWindows.ToList().ForEach(browserWindow => {
                                        if(browserWindow.Id != 1)
                                        {
                                            browserWindow.Close();
                                        }
                                        else
                                        {
                                            browserWindow.Reload();
                                        }
                                    });
                                }
                            },
                            new MenuItem
                            {
                                Label = "Open Developer Tools",
                                Accelerator = "CmdOrCtrl+I",
                                Click = () => Electron.WindowManager.BrowserWindows.First().WebContents.OpenDevTools()
                            }
                        }
                    },
                    new MenuItem {
                        Label = "Help",
                        Role = MenuRole.help,
                        Submenu = new MenuItem[] {
                            new MenuItem
                            {
                                Label = "Learn More",
                                Click = async () => await Electron.Shell.OpenExternalAsync("https://github.com/aiplugs/poshapp")
                            }
                        }
                    }
                };

            Electron.Menu.SetApplicationMenu(menu);

            Electron.IpcMain.On("select-directory", async (args) =>
            {
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

            Electron.IpcMain.On("select-file", async (args) =>
            {
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

            Electron.IpcMain.On("check-update", async (args) =>
            {
                await Electron.AutoUpdater.CheckForUpdatesAndNotifyAsync();
            });
        }
    }
}
