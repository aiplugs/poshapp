'use strict'

import path from 'path'
import cp  from 'child_process'
import { app, protocol, BrowserWindow, ipcMain, Menu } from 'electron'
import { createProtocol } from 'vue-cli-plugin-electron-builder/lib'
import installExtension, { VUEJS_DEVTOOLS } from 'electron-devtools-installer'
const {autoUpdater} = require("electron-updater");
const Git = require("nodegit")
const keytar = require('keytar')
const rpc = require('vscode-jsonrpc')
const isDevelopment = process.env.NODE_ENV !== 'production'

const isMac = process.platform === 'darwin'
Menu.setApplicationMenu(Menu.buildFromTemplate([
  // { role: 'appMenu' }
  ...(isMac ? [{
    label: app.name,
    submenu: [
      { role: 'about' },
      { type: 'separator' },
      { role: 'services' },
      { type: 'separator' },
      { role: 'hide' },
      { role: 'hideothers' },
      { role: 'unhide' },
      { type: 'separator' },
      { role: 'quit' }
    ]
  }] : []),
  // { role: 'editMenu' }
  {
    label: 'Edit',
    submenu: [
      { role: 'undo' },
      { role: 'redo' },
      { type: 'separator' },
      { role: 'cut' },
      { role: 'copy' },
      { role: 'paste' },
      ...(isMac ? [
        { role: 'pasteAndMatchStyle' },
        { role: 'delete' },
        { role: 'selectAll' },
        { type: 'separator' },
        {
          label: 'Speech',
          submenu: [
            { role: 'startspeaking' },
            { role: 'stopspeaking' }
          ]
        }
      ] : [
        { role: 'delete' },
        { type: 'separator' },
        { role: 'selectAll' }
      ])
    ]
  },
  // { role: 'viewMenu' }
  {
    label: 'View',
    submenu: [
      { role: 'reload' },
      { role: 'forcereload' },
      { role: 'toggledevtools' },
      { type: 'separator' },
      { role: 'togglefullscreen' }
    ]
  },
  // { role: 'windowMenu' }
  {
    label: 'Window',
    submenu: [
      { role: 'minimize' },
      { role: 'zoom' },
      ...(isMac ? [
        { type: 'separator' },
        { role: 'front' },
        { type: 'separator' },
        { role: 'window' }
      ] : [
        { role: 'close' }
      ])
    ]
  },
  {
    role: 'help',
    submenu: [
      {
        label: 'Documentation',
        click: async () => {
          const { shell } = require('electron')
          await shell.openExternal('https://github.com/aiplugs/poshapp/wiki')
        }
      },
      {
        label: 'Release Notes',
        click: async () => {
          const { shell } = require('electron')
          await shell.openExternal('https://github.com/aiplugs/poshapp/releases')
        }
      },
      {
        label: 'Search Issues',
        click: async () => {
          const { shell } = require('electron')
          await shell.openExternal('https://github.com/aiplugs/poshapp/issues')
        }
      },
    ]
  }
]))

let win

protocol.registerSchemesAsPrivileged([
  { scheme: 'app', privileges: { secure: true, standard: true } }
])

function createWindow() {
  // Create the browser window.
  win = new BrowserWindow({
    width: 800,
    height: 600,
    webPreferences: {
      // Use pluginOptions.nodeIntegration, leave this alone
      // See nklayman.github.io/vue-cli-plugin-electron-builder/guide/security.html#node-integration for more info
      nodeIntegration: process.env.ELECTRON_NODE_INTEGRATION,
      preload: path.join(__dirname, 'preload.js')
    }
  })

  if (process.env.WEBPACK_DEV_SERVER_URL) {
    // Load the url of the dev server if in development mode
    win.loadURL(process.env.WEBPACK_DEV_SERVER_URL)
    if (!process.env.IS_TEST) win.webContents.openDevTools()
  } else {
    createProtocol('app')
    // Load the index.html when not in development
    win.loadURL('app://./index.html')
  }

  win.on('closed', () => {
    win = null
  })
}


autoUpdater.on('checking-for-update', () => {
})
autoUpdater.on('update-available', (info) => {
  win.webContents.send('UpdateAvailable')
})
autoUpdater.on('update-not-available', (info) => {
})
autoUpdater.on('error', (err) => {
})
autoUpdater.on('download-progress', (progress) => {
  win.webContents.send('UpdateDownloading', progress.percent);
})
autoUpdater.on('update-downloaded', (info) => {
});
ipcMain.handle('QuitAndInstall', () => {
  autoUpdater.quitAndInstall();
})

// Quit when all windows are closed.
app.on('window-all-closed', () => {
  // On macOS it is common for applications and their menu bar
  // to stay active until the user quits explicitly with Cmd + Q
  if (process.platform !== 'darwin') {
    app.quit()
  }
})

app.on('activate', () => {
  // On macOS it's common to re-create a window in the app when the
  // dock icon is clicked and there are no other windows open.
  if (win === null) {
    createWindow()
  }
})

// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
// Some APIs can only be used after this event occurs.
app.on('ready', async () => {
  if (isDevelopment && !process.env.IS_TEST) {
    // Install Vue Devtools
    try {
      await installExtension(VUEJS_DEVTOOLS)
    } catch (e) {
      console.error('Vue Devtools failed to install:', e.toString())
    }
  }
  createWindow()
  startPowerShellDeamon();
  const channel = await connection.sendRequest(new rpc.RequestType('GetChannel'));
  const version = app.getVersion().split('-')
  autoUpdater.channel = channel || version[version.length - 1];
  autoUpdater.checkForUpdatesAndNotify();
})

// Exit cleanly on request from parent process in development mode.
if (isDevelopment) {
  if (process.platform === 'win32') {
    process.on('message', (data) => {
      if (data === 'graceful-exit') {
        app.quit()
      }
    })
  } else {
    process.on('SIGTERM', () => {
      app.quit()
    })
  }
}

let connection;
async function startPowerShellDeamon () {
  const binary = process.platform == 'win32' ? 'Aiplugs.PoshApp.Deamon.exe' : 'Aiplugs.PoshApp.Deamon'
  const prefix = process.platform == 'darwin' ? '../bin' : 'resources/bin'
  const deamon = isDevelopment ? `bin/${binary}` : path.join(__dirname, prefix, binary)
  const childProcess = cp.spawn(deamon);

  // Use stdin and stdout for communication:
  connection = rpc.createMessageConnection(
      new rpc.StreamMessageReader(childProcess.stdout),
      new rpc.StreamMessageWriter(childProcess.stdin));

  const getRepositories = new rpc.RequestType('GetRepositories');
  ipcMain.handle("GetRepositories", async (event) => {
    return await connection.sendRequest(getRepositories);
  });

  const createRepository = new rpc.RequestType('CreateRepository');
  ipcMain.handle("CreateRepository", async (event, name, path) => {
    return await connection.sendRequest(createRepository, [name, path]);
  });

  const deleteRepository = new rpc.RequestType('DeleteRepository');
  ipcMain.handle("DeleteRepository", async (event, repositoryName) => {
    return await connection.sendRequest(deleteRepository, [repositoryName]);
  });

  const getScripts = new rpc.RequestType('GetScripts');
  ipcMain.handle("GetScripts", async (event) => {
    return await connection.sendRequest(getScripts);
  });

  const getScript = new rpc.RequestType('GetScript');
  ipcMain.handle("GetScript", async (event, repositoryName, scriptId) => {
    return await connection.sendRequest(getScript, [repositoryName, scriptId]);
  });

  const deleteScript = new rpc.RequestType('DeleteScript');
  ipcMain.handle("DeleteScript", async (event, repositoryName, scriptId) => {
    return await connection.sendRequest(deleteScript, [repositoryName, scriptId]);
  });

  const createListScript = new rpc.RequestType('CreateListScript');
  ipcMain.handle("CreateListScript", async (event, respositoryName, data) => {
    return await connection.sendRequest(createListScript, [respositoryName, data]);
  });

  const createDetailScript = new rpc.RequestType('CreateDetailScript');
  ipcMain.handle("CreateDetailScript", async (event, respositoryName, data) => {
    return await connection.sendRequest(createDetailScript, [respositoryName, data]);
  });

  const createSingletonScript = new rpc.RequestType('CreateSingletonScript');
  ipcMain.handle("CreateSingletonScript", async (event, respositoryName, data) => {
    return await connection.sendRequest(createSingletonScript, [respositoryName, data]);
  });

  const createActionScript = new rpc.RequestType('CreateActionScript');
  ipcMain.handle("CreateActionScript", async (event, respositoryName, data) => {
    return await connection.sendRequest(createActionScript, [respositoryName, data]);
  });

  const updateListScript = new rpc.RequestType('UpdateListScript');
  ipcMain.handle("UpdateListScript", async (event, respositoryName, scriptId, data) => {
    return await connection.sendRequest(updateListScript, [respositoryName, scriptId, data]);
  });

  const updateDetailScript = new rpc.RequestType('UpdateDetailScript');
  ipcMain.handle("UpdateDetailScript", async (event, respositoryName, scriptId, data) => {
    return await connection.sendRequest(updateDetailScript, [respositoryName, scriptId, data]);
  });

  const updateSingletonScript = new rpc.RequestType('UpdateSingletonScript');
  ipcMain.handle("UpdateSingletonScript", async (event, respositoryName, scriptId, data) => {
    return await connection.sendRequest(updateSingletonScript, [respositoryName, scriptId, data]);
  });

  const updateActionScript = new rpc.RequestType('UpdateActionScript');
  ipcMain.handle("UpdateActionScript", async (event, respositoryName, scriptId, data) => {
    return await connection.sendRequest(updateActionScript, [respositoryName, scriptId, data]);
  });

  const getScriptContent = new rpc.RequestType('GetScriptContent');
  ipcMain.handle("GetScriptContent", async (event, respositoryName, scriptId) => {
    return await connection.sendRequest(getScriptContent, [respositoryName, scriptId]);
  });

  const putScriptContent = new rpc.RequestType('PutScriptContent');
  ipcMain.handle("PutScriptContent", async (event, respositoryName, scriptId, content) => {
    return await connection.sendRequest(putScriptContent, [respositoryName, scriptId, content]);
  });

  const getActivation = new rpc.RequestType('GetActivation');
  ipcMain.handle("GetActivation", async (event) => {
    return await connection.sendRequest(getActivation);
  });

  const postActivation = new rpc.RequestType('PostActivation');
  ipcMain.handle("PostActivation", async (event, activationCode) => {
    return await connection.sendRequest(postActivation, [activationCode]);
  });

  const refleshActivation = new rpc.RequestType('RefleshActivation');
  ipcMain.handle("RefleshActivation", async (event) => {
    return await connection.sendRequest(refleshActivation);
  });

  const invokeWithParameters = new rpc.RequestType('InvokeWithParameters');
  ipcMain.handle("InvokeWithParameters", async (event, scriptId, input) => {
    return await connection.sendRequest(invokeWithParameters, [scriptId, input]);
  });

  const invokeWithPipeline = new rpc.RequestType('InvokeWithPipeline');
  ipcMain.handle("InvokeWithPipeline", async (event, scriptId, input) => {
    return await connection.sendRequest(invokeWithPipeline, [scriptId, input]);
  });

  const invokeWithPipelines = new rpc.RequestType('InvokeWithPipelines');
  ipcMain.handle("InvokeWithPipelines", async (event, scriptId, input) => {
    return await connection.sendRequest(invokeWithPipelines, [scriptId, input]);
  });

  const invokeGetParameters = new rpc.RequestType('InvokeGetParameters');
  ipcMain.handle("InvokeGetParameters", async (event, scriptId) => {
    return await connection.sendRequest(invokeGetParameters, [scriptId]);
  });

  connection.onRequest(new rpc.RequestType('Prompt'), ([caption, message, descriptions]) => {
    return new Promise(function(resolve) {
      ipcMain.once('Prompt', function(event, input) {
        resolve(input);
      })
      win.webContents.send('Prompt', caption, message, descriptions);
    })
  })

  connection.onRequest(new rpc.RequestType('PromptForChoice'), ([caption, message, choices]) => {
    return new Promise(function(resolve) {
      ipcMain.once('PromptForChoice', function(event, index) {
        resolve(index);
      })
      win.webContents.send('PromptForChoice', caption, message, choices);
    })
  })

  connection.onRequest(new rpc.RequestType('PromptForCredential'), ([caption, message, userName, targetName]) => {
    return new Promise(function(resolve) {
      ipcMain.once('PromptForCredential', function(event, username, password) {
        resolve({username, password});
      })
      win.webContents.send('PromptForCredential', caption, message, userName, targetName);
    })
  })

  connection.onRequest(new rpc.RequestType('ReadLine'), () => {
    return new Promise(function(resolve) {
      ipcMain.once('ReadLine', function(event, input) {
        resolve(input);
      })
      win.webContents.send('ReadLine');
    })
  })

  connection.onRequest(new rpc.RequestType('ReadLineAsSecureString'), () => {
    return new Promise(function(resolve) {
      ipcMain.once('ReadLineAsSecureString', function(event, input) {
        resolve(input);
      })
      win.webContents.send('ReadLineAsSecureString');
    })
  })

  connection.onNotification(new rpc.NotificationType('WriteWithColor'), ([foregroundColor, backgroundColor, value]) => {
    win.webContents.send('WriteWithColor', foregroundColor, backgroundColor, value);
  });

  connection.onNotification(new rpc.NotificationType('Write'), ([value]) => {
    console.log(value)
    win.webContents.send('WriteWithColor', 15, 0, value);
  });

  connection.onNotification(new rpc.NotificationType('WriteDebugLine'), ([message]) => {
    win.webContents.send('WriteDebugLine', message);
  });

  connection.onNotification(new rpc.NotificationType('WriteErrorLine'), ([value]) => {
    win.webContents.send('WriteErrorLine', value);
  });

  connection.onNotification(new rpc.NotificationType('WriteLine'), ([value]) => {
    win.webContents.send('WriteWithColor', 15, 0, value);
  });

  connection.onNotification(new rpc.NotificationType('WriteProgress'), ([sourceId, record]) => {
    win.webContents.send('WriteProgress', sourceId, record);
  });

  connection.onNotification(new rpc.NotificationType('WriteVerboseLine'), ([message]) => {
    win.webContents.send('WriteVerboseLine', message);
  });

  connection.onNotification(new rpc.NotificationType('WriteWarningLine'), ([message]) => {
    win.webContents.send('WriteWarningLine', message);
  });

  connection.onNotification(new rpc.NotificationType('ParseError'), ([message]) => {
    win.webContents.send('ParseError', message);
  });

  ipcMain.handle("GitLog", async (event, path) => {
    let repository = null;
    try {
      repository = await Git.Repository.open(path);
    } catch {
      return null;
    }
    let localHead = null;
    try {
      localHead = await repository.getHeadCommit();
    } catch{}
    let originHead = null;
    try {
      originHead = await repository.getBranchCommit('origin/master'); 
    } catch{}

    if (!localHead)
      return  { logs:[], origin: null, local: null };

    let count = 0;
    const walker = repository.createRevWalk();
    walker.sorting(Git.Revwalk.SORT.TOPOLOGICAL)
    walker.push(originHead.sha());
    walker.push(localHead.sha());
    const commits = await walker.getCommits(100);
    const logs = commits.map(commit => ({
      commit: commit.sha(),
      name: commit.author().name(),
      email: commit.author().email(),
      when: commit.date(),
      message: commit.messageRaw(),
      messageShort:commit.message(), 
    }));
    return  { logs, origin: originHead.sha(), local: localHead.sha() };
  })

  ipcMain.handle("GitStatus", async (event, path) => {
    let repository = null;
    try {
      repository = await Git.Repository.open(path);
    } catch {
      return null;
    }

    const statuses = await repository.getStatus()
    function labels(status) {
      const items = [];
      if (status.isNew()) { items.push("NEW"); }
      if (status.isModified()) { items.push("MODIFIED"); }
      if (status.isTypechange()) { items.push("TYPECHANGE"); }
      if (status.isRenamed()) { items.push("RENAMED"); }
      if (status.isIgnored()) { items.push("IGNORED"); }
      return items;
    }
    return statuses.map(status => ({
      file: status.path(),
      labels: labels(status)
    }));
  })
  ipcMain.handle("GitFetch", async (event, path) => {
    let repository = null;
    try {
      repository = await Git.Repository.open(path);
    } catch {
      return null;
    }
    let saveService;
    let saveUserName;
    let savePassword;
    let challenge = 0;
    await repository.fetch('origin', {
      callbacks: {
        credentials: async function (url, userName) {
          const credentials = await keytar.findCredentials(url)
          if (credentials.length > 0) { 
            if (challenge < credentials.length) {
              const index = challenge++;
              return Git.Cred.userpassPlaintextNew(credentials[index].account, credentials[index].password)
            } else {
              await Promise.all(credentials.map(c => keytar.deletePassword(url, c.account)))
            }
          }
          return await (new Promise(function(resolve){
            ipcMain.once('PromptForGitCredential', function(event, userName, password) {
              saveService = url;
              saveUserName = userName;
              savePassword = password;
              resolve(Git.Cred.userpassPlaintextNew(userName, password))
            })
            event.sender.send('PromptForGitCredential', url, userName);
          }))
        },
        certificateCheck: function() {
          // github will fail cert check on some OSX machines
          // this overrides that check
          return 0;
        },
        transferProgress: function(stats) {
          const progress = (stats.receivedObjects() + stats.indexedObjects()) / (stats.totalObjects() * 2)
          event.sender.send('GitProgress', progress);
        }
      }
    });
    if (saveService && saveUserName && savePassword) {
      await keytar.setPassword(saveService, saveUserName, savePassword);
    }
  })

  ipcMain.handle("GitClone", async (event, origin, path) => {
    let saveService;
    let saveUserName;
    let savePassword;
    let challenge = 0;
    Git.Clone(origin, path, { fetchOpts: {
      callbacks: {
        credentials: async function (url, userName) {
          const credentials = await keytar.findCredentials(url)
          if (credentials.length > 0) { 
            if (challenge < credentials.length) {
              const index = challenge++;
              return Git.Cred.userpassPlaintextNew(credentials[index].account, credentials[index].password)
            } else {
              await Promise.all(credentials.map(c => keytar.deletePassword(url, c.account)))
            }
          }
          return await (new Promise(function(resolve){
            ipcMain.once('PromptForGitCredential', function(event, userName, password) {
              saveService = url;
              saveUserName = userName;
              savePassword = password;
              resolve(Git.Cred.userpassPlaintextNew(userName, password))
            })
            event.sender.send('PromptForGitCredential', url, userName);
          }))
        },
        certificateCheck: function() {
          // github will fail cert check on some OSX machines
          // this overrides that check
          return 0;
        },
        transferProgress: function(stats) {
          const progress = (stats.receivedObjects() + stats.indexedObjects()) / (stats.totalObjects() * 2)
          event.sender.send('GitProgress', progress);
        }
      }
     }
    })
    if (saveService && saveUserName && savePassword) {
      await keytar.setPassword(saveService, saveUserName, savePassword);
    }
  })
  ipcMain.handle("GitReset", async (event, path) => {
    let repository = null;
    try {
      repository = await Git.Repository.open(path);
    } catch {
      return null;
    }
    const originHead = await repository.getBranchCommit('origin/master');
    await Git.Reset.reset(repository, originHead, Git.Reset.TYPE.HARD);
  })

  connection.listen();
}