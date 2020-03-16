using ElectronNET.API;
using ElectronNET.API.Entities;
using System.Linq;

namespace Aiplugs.PoshApp
{
    public class ElectronIpc
    {
        public static void Setup(ScriptsService scriptsService)
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

            Electron.IpcMain.On("select-repository-path", async (args) =>
            {
                var mainWindow = Electron.WindowManager.BrowserWindows.First();
                var options = new OpenDialogOptions
                {
                    Properties = new OpenDialogProperty[] {
                        OpenDialogProperty.openDirectory,
                    }
                };

                string[] dirs = await Electron.Dialog.ShowOpenDialogAsync(mainWindow, options);
                if (dirs != null && dirs.Length > 0)
                {
                    Electron.IpcMain.Send(mainWindow, "select-repository-path-reply", dirs.First());
                }
            });

            Electron.IpcMain.On("select-directory", async (args) =>
            {
                var mainWindow = Electron.WindowManager.BrowserWindows.First();
                var options = new OpenDialogOptions
                {
                    Properties = new OpenDialogProperty[] {
                        OpenDialogProperty.openDirectory,
                    }
                };

                string[] dirs = await Electron.Dialog.ShowOpenDialogAsync(mainWindow, options);
                if (dirs != null && dirs.Length > 0)
                {
                    Electron.IpcMain.Send(mainWindow, "select-directory-reply", dirs.First(), args);
                }
            });

            Electron.IpcMain.On("select-directories", async (args) =>
            {
                var mainWindow = Electron.WindowManager.BrowserWindows.First();
                var options = new OpenDialogOptions
                {
                    Properties = new OpenDialogProperty[] {
                        OpenDialogProperty.openDirectory,
                        OpenDialogProperty.multiSelections,
                    }
                };

                string[] dirs = await Electron.Dialog.ShowOpenDialogAsync(mainWindow, options);
                Electron.IpcMain.Send(mainWindow, "select-directories-reply", dirs, args);
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

                string[] files = await Electron.Dialog.ShowOpenDialogAsync(mainWindow, options);
                if (files != null && files.Length > 0)
                {
                    Electron.IpcMain.Send(mainWindow, "select-file-reply", files.First(), args);
                }
            });

            Electron.IpcMain.On("select-files", async (args) =>
            {
                var mainWindow = Electron.WindowManager.BrowserWindows.First();
                var options = new OpenDialogOptions
                {
                    Properties = new OpenDialogProperty[] {
                        OpenDialogProperty.openFile,
                        OpenDialogProperty.multiSelections,
                    }
                };

                string[] files = await Electron.Dialog.ShowOpenDialogAsync(mainWindow, options);
                Electron.IpcMain.Send(mainWindow, "select-files-reply", files, args);
            });

            Electron.IpcMain.On("open-activation", async (args) =>
            {
                await Electron.Shell.OpenExternalAsync("https://poshapp.aiplugs.com/licenses");
            });

            Electron.IpcMain.On("open-repository-dir", async (name) =>
            {
                var repository = await scriptsService.GetRepository(name.ToString());
                await Electron.Shell.OpenItemAsync(repository.Path);
            });

            Electron.IpcMain.On("copy-to", (text) =>
            {
                Electron.Clipboard.WriteText(text.ToString());
            });


            Electron.IpcMain.On("check-for-updates", _ =>
            {
                Electron.AutoUpdater.CheckForUpdatesAndNotifyAsync().ConfigureAwait(false);
            });

            Electron.IpcMain.On("quit-and-install", _ =>
            {
                Electron.AutoUpdater.QuitAndInstall();
            });

            Electron.AutoUpdater.OnUpdateAvailable += (info) =>
            {
                var mainWindow = Electron.WindowManager.BrowserWindows.First();
                Electron.IpcMain.Send(mainWindow, "update-available");
            };
        }
    }
}
