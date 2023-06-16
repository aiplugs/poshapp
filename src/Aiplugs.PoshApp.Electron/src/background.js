'use strict'

import path from 'path'
import cp  from 'child_process'
import { app, protocol, BrowserWindow, ipcMain, Menu, dialog, clipboard, shell, nativeTheme } from 'electron'
import { createProtocol } from 'vue-cli-plugin-electron-builder/lib'
import installExtension, { VUEJS_DEVTOOLS } from 'electron-devtools-installer'
import { startPSES } from './pses.js'
import { parseGitConfig } from './gitconfig.js'
import {autoUpdater} from 'electron-updater'
import * as fs from 'fs'
import * as http from 'isomorphic-git/http/node'
import * as git from 'isomorphic-git'
import * as keytar from 'keytar'
import * as rpc from 'vscode-jsonrpc'

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
        label: 'Community',
        click: async () => {
          const { shell } = require('electron')
          await shell.openExternal('https://github.com/aiplugs/poshapp/discussions')
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
  if (nativeTheme.shouldUseDarkColors) {
    nativeTheme.themeSource = 'dark'
  }
  // Create the browser window.
  win = new BrowserWindow({
    width: 800,
    height: 600,
    webPreferences: {
      nodeIntegration: false,
      contextIsolation: false,
      worldSafeExecuteJavaScript: true,
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
autoUpdater.on('update-available', () => {
})
autoUpdater.on('update-not-available', () => {
})
autoUpdater.on('error', (err) => {
  dialog.showMessageBox(win, {
    message: JSON.stringify(err)
  })
})
autoUpdater.on('download-progress', (progress) => {
  win.webContents.send('UpdateDownloading', progress.percent);
})
autoUpdater.on('update-downloaded', () => {
  win.webContents.send('UpdateAvailable')
});
ipcMain.on('QuitAndInstall', () => {
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
  startPSES();
  startPowerShellDeamon();
  const settingsChannel = await connection.sendRequest(new rpc.RequestType0('GetChannel'));
  const currentChannel = app.getVersion().indexOf('beta') != -1 ? 'beta' : null;
  autoUpdater.channel = settingsChannel || currentChannel;
  autoUpdater.autoDownload = true;
  autoUpdater.autoInstallOnAppQuit = !isMac;
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
  const deamon = isDevelopment ? `bin/deamon/bin/Common/${binary}` : path.join(__dirname, '../bin/deamon/bin/Common/', binary)
  const childProcess = cp.spawn(deamon);

  // Use stdin and stdout for communication:
  connection = rpc.createMessageConnection(
      new rpc.StreamMessageReader(childProcess.stdout),
      new rpc.StreamMessageWriter(childProcess.stdin));

  const getRepositories = new rpc.RequestType0('GetRepositories');
  ipcMain.handle("GetRepositories", async (event) => {
    return await connection.sendRequest(getRepositories);
  });

  const createRepository = new rpc.RequestType2('CreateRepository');
  ipcMain.handle("CreateRepository", async (event, name, path) => {
    return await connection.sendRequest(createRepository, name, path);
  });

  const deleteRepository = new rpc.RequestType2('DeleteRepository');
  ipcMain.handle("DeleteRepository", async (event, repositoryName) => {
    return await connection.sendRequest(deleteRepository, repositoryName, false);
  });

  const getScripts = new rpc.RequestType0('GetScripts');
  ipcMain.handle("GetScripts", async (event) => {
    return await connection.sendRequest(getScripts);
  });

  const getScript = new rpc.RequestType2('GetScript');
  ipcMain.handle("GetScript", async (event, repositoryName, scriptId) => {
    return await connection.sendRequest(getScript, repositoryName, scriptId);
  });

  const deleteScript = new rpc.RequestType2('DeleteScript');
  ipcMain.handle("DeleteScript", async (event, repositoryName, scriptId) => {
    return await connection.sendRequest(deleteScript, repositoryName, scriptId);
  });

  const createListScript = new rpc.RequestType2('CreateListScript');
  ipcMain.handle("CreateListScript", async (event, respositoryName, data) => {
    return await connection.sendRequest(createListScript, respositoryName, data);
  });

  const createDetailScript = new rpc.RequestType2('CreateDetailScript');
  ipcMain.handle("CreateDetailScript", async (event, respositoryName, data) => {
    return await connection.sendRequest(createDetailScript, respositoryName, data);
  });

  const createSingletonScript = new rpc.RequestType2('CreateSingletonScript');
  ipcMain.handle("CreateSingletonScript", async (event, respositoryName, data) => {
    return await connection.sendRequest(createSingletonScript, respositoryName, data);
  });

  const createActionScript = new rpc.RequestType2('CreateActionScript');
  ipcMain.handle("CreateActionScript", async (event, respositoryName, data) => {
    return await connection.sendRequest(createActionScript, respositoryName, data);
  });

  const updateListScript = new rpc.RequestType3('UpdateListScript');
  ipcMain.handle("UpdateListScript", async (event, respositoryName, scriptId, data) => {
    return await connection.sendRequest(updateListScript, respositoryName, scriptId, data);
  });

  const updateDetailScript = new rpc.RequestType3('UpdateDetailScript');
  ipcMain.handle("UpdateDetailScript", async (event, respositoryName, scriptId, data) => {
    return await connection.sendRequest(updateDetailScript, respositoryName, scriptId, data);
  });

  const updateSingletonScript = new rpc.RequestType3('UpdateSingletonScript');
  ipcMain.handle("UpdateSingletonScript", async (event, respositoryName, scriptId, data) => {
    return await connection.sendRequest(updateSingletonScript, respositoryName, scriptId, data);
  });

  const updateActionScript = new rpc.RequestType3('UpdateActionScript');
  ipcMain.handle("UpdateActionScript", async (event, respositoryName, scriptId, data) => {
    return await connection.sendRequest(updateActionScript, respositoryName, scriptId, data);
  });

  const getScriptContent = new rpc.RequestType2('GetScriptContent');
  ipcMain.handle("GetScriptContent", async (event, respositoryName, scriptId) => {
    return await connection.sendRequest(getScriptContent, respositoryName, scriptId);
  });

  const putScriptContent = new rpc.RequestType3('PutScriptContent');
  ipcMain.handle("PutScriptContent", async (event, respositoryName, scriptId, content) => {
    return await connection.sendRequest(putScriptContent, respositoryName, scriptId, content);
  });

  const invokeWithParameters = new rpc.RequestType2('InvokeWithParameters');
  ipcMain.handle("InvokeWithParameters", async (event, scriptId, input) => {
    return await connection.sendRequest(invokeWithParameters, scriptId, input);
  });

  const invokeWithPipeline = new rpc.RequestType2('InvokeWithPipeline');
  ipcMain.handle("InvokeWithPipeline", async (event, scriptId, input) => {
    return await connection.sendRequest(invokeWithPipeline, scriptId, input);
  });

  const invokeWithPipelines = new rpc.RequestType2('InvokeWithPipelines');
  ipcMain.handle("InvokeWithPipelines", async (event, scriptId, input) => {
    return await connection.sendRequest(invokeWithPipelines, scriptId, input);
  });

  const invokeGetParameters = new rpc.RequestType1('InvokeGetParameters');
  ipcMain.handle("InvokeGetParameters", async (event, scriptId) => {
    return await connection.sendRequest(invokeGetParameters, scriptId);
  });

  connection.onRequest(new rpc.RequestType3('Prompt'), (caption, message, descriptions) => {
    return new Promise(function(resolve) {
      ipcMain.once('Prompt', function(event, input) {
        resolve(input);
      })
      win.webContents.send('Prompt', caption, message, descriptions);
    })
  })

  connection.onRequest(new rpc.RequestType3('PromptForChoice'), (caption, message, choices) => {
    return new Promise(function(resolve) {
      ipcMain.once('PromptForChoice', function(event, index) {
        resolve(index);
      })
      win.webContents.send('PromptForChoice', caption, message, choices);
    })
  })

  connection.onRequest(new rpc.RequestType4('PromptForCredential'), (caption, message, userName, targetName) => {
    return new Promise(function(resolve) {
      ipcMain.once('PromptForCredential', function(event, username, password) {
        resolve({username, password});
      })
      win.webContents.send('PromptForCredential', caption, message, userName, targetName);
    })
  })

  connection.onRequest(new rpc.RequestType0('ReadLine'), () => {
    return new Promise(function(resolve) {
      ipcMain.once('ReadLine', function(event, input) {
        resolve(input);
      })
      win.webContents.send('ReadLine');
    })
  })

  connection.onRequest(new rpc.RequestType0('ReadLineAsSecureString'), () => {
    return new Promise(function(resolve) {
      ipcMain.once('ReadLineAsSecureString', function(event, input) {
        resolve(input);
      })
      win.webContents.send('ReadLineAsSecureString');
    })
  })

  connection.onNotification(new rpc.NotificationType3('WriteWithColor'), (foregroundColor, backgroundColor, value) => {
    win.webContents.send('WriteWithColor', foregroundColor, backgroundColor, value);
  });

  connection.onNotification(new rpc.NotificationType1('Write'), (value) => {
    console.log(value)
    win.webContents.send('WriteWithColor', 15, 0, value);
  });

  connection.onNotification(new rpc.NotificationType1('WriteDebugLine'), (message) => {
    win.webContents.send('WriteDebugLine', message);
  });

  connection.onNotification(new rpc.NotificationType1('WriteErrorLine'), (value) => {
    win.webContents.send('WriteErrorLine', value);
  });

  connection.onNotification(new rpc.NotificationType('WriteLine'), (value) => {
    win.webContents.send('WriteWithColor', 15, 0, value);
  });

  connection.onNotification(new rpc.NotificationType1('WriteProgress'), (sourceId, record) => {
    win.webContents.send('WriteProgress', sourceId, record);
  });

  connection.onNotification(new rpc.NotificationType1('WriteVerboseLine'), (message) => {
    win.webContents.send('WriteVerboseLine', message);
  });

  connection.onNotification(new rpc.NotificationType1('WriteWarningLine'), (message) => {
    win.webContents.send('WriteWarningLine', message);
  });

  connection.onNotification(new rpc.NotificationType1('ParseError'), (message) => {
    win.webContents.send('ParseError', message);
  });

  function createAuthFactory (event) {
    return async (url, faildAuth) => {
      if (faildAuth) {
        try {
        await keytar.deletePassword(url, faildAuth.username);
        } catch {}
      }
      const credentials = await keytar.findCredentials(url);
      if (credentials.length > 0) {
        return {
          username: credentials[0].account, 
          password: credentials[0].password,
        }
      }
      return await (new Promise(function(resolve){
        ipcMain.once('PromptForGitCredential', function(event, username, password) {
          resolve({
            username,
            password,
          })
        })
        event.sender.send('PromptForGitCredential', url, faildAuth.username);
      }))
    };
  };

  ipcMain.handle("GitLog", async (event, path) => {
    let branch = null;
    try {
      branch = await git.currentBranch({
        fs,
        dir: path,
        fullname: false
      })
    }
    catch {
      return null;
    }

    const mapping = result => ({
      commit: result.oid,
      name: result.commit.author.name,
      email: result.commit.author.email,
      when: result.commit.author.timestamp * 1000,
      message: result.commit.message,
      messageShort:result.commit.message.split('\n')[0], 
    });

    const localLogs = (await git.log({
      fs,
      dir: path,
      depth: 100,
      ref: 'refs/heads/' + branch
    })).map(mapping);
    
    let remoteLogs = [];
    
    try {
      remoteLogs = (await git.log({
        fs,
        dir: path,
        depth: 100,
        ref: 'origin/' + branch,
      })).map(mapping);
    } catch {}

    let logs = [];
    let i = 0, j = 0, d = new Date().getTime();
    while(i < localLogs.length && j < remoteLogs.length) {
      if (localLogs[i].commit == remoteLogs[j].commit && localLogs[i].when < d ) {
        logs.push(localLogs[i]);
        d = localLogs[i].when;
        i++;
        j++;
      }
      else if (localLogs[i].when > remoteLogs[i].when) {
        logs.push(localLogs[i]);
        d = localLogs[i].when;
        i++;
      }
      else if (localLogs[i].when < remoteLogs[i].when) {
        logs.push(remoteLogs[j]);
        d = localLogs[j].when;
        j++;
      }
      else {
        logs.push(localLogs[i]);
        logs.push(remoteLogs[j]);
        d = localLogs[i].when;
        i++;
        j++;
      }
    }

    let local = null, origin = null;
    try {
      local = await git.resolveRef({
        fs,
        dir: path,
        depth: 1,
        ref: 'refs/heads/' + branch
      });
      origin = await git.resolveRef({
        fs,
        dir: path,
        depth: 1,
        ref: 'origin/' + branch
      });
    }
    catch{}
    
    return  { logs, origin, local };
  })

  ipcMain.handle("GitStatus", async (event, path) => {
    
    const statuses = await git.statusMatrix({
      fs,
      dir: path,
    })
    function labels(status) {
      const items = [];
      if (status[1] === 1 && status[2] === 1 && status[1] === 1) {
        return items;
      }
      if (status[2] === 2) { items.push("NEW"); }
      if (status[1] === 1 && status[2] !== 0) { items.push("MODIFIED"); }
      if (status[1] === 1 && status[2] === 0) { items.push("DELETE"); }
      return items;
    }
    return statuses.map(status => ({
      file: status[0],
      labels: labels(status)
    })).filter(status => status.labels.length > 0);
  })

  ipcMain.handle("GitFetch", async (event, path) => {
    try {
      const branch = await git.currentBranch({
        fs,
        dir: path,
        fullname: false
      })
      const url = await git.getConfig({
        fs,
        dir: path,
        path: 'remote.origin.url'
      })
      const tryAuth = createAuthFactory(event);
      await git.fetch({
        fs,
        http,
        url,
        dir: path,
        ref: branch,
        depth: 1,
        singleBranch: true,
        onAuth: tryAuth,
        onAuthFailure: tryAuth,
        onAuthSuccess: async (url, auth) => {
          await keytar.setPassword(url, auth.username, auth.password);
        },
        onProgress: progress => {
          if (progress.total) {
            const percent = (progress.loaded / progress.total * 100).toFixed(1) + '%';
            event.sender.send('GitProgress', percent);
          }
        }
      })
      return true;
    }
    catch {
      return false;
    }
  })

  ipcMain.handle("GitClone", async (event, origin, path) => {
    const tryAuth = createAuthFactory(event);
    await git.clone({
      fs,
      http,
      url: origin,
      dir: path,
      singleBranch: true,
      onAuth: tryAuth,
      onAuthFailure: tryAuth,
      onAuthSuccess: async (url, auth) => {
        await keytar.setPassword(url, auth.username, auth.password);
      },
      onProgress: progress => {
        if (progress.total) {
          const percent = (progress.loaded / progress.total * 100).toFixed(1) + '%';
          event.sender.send('GitProgress', percent);
        }
      }
    })
    
    return true;
  })

  ipcMain.handle("GitReset", async (event, path) => {
    const name = await git.getConfig({ fs, dir: path, path: 'user.name' });
    const email = await git.getConfig({ fs, dir: path, path: 'user.email' });
    const config = await parseGitConfig();
    const branch = await git.currentBranch({
      fs,
      dir: path,
      fullname: false
    })
    await git.checkout({
      fs,
      dir: path,
      ref: branch,
      force: true,
    })
    await git.pull({
      fs,
      http,
      dir: path,
      ref: branch,
      singleBranch: true,
      author: {
        name: name || config['user.name'],
        email: email || config['user.email'],
      }
    })
    return true;
  })

  connection.listen();
}

ipcMain.handle('GetVersions', (event) => {
  return {
    node: process.versions.node,
    chrome: process.versions.chrome,
    electron: process.versions.electron,
    app: app.getVersion()
  }
})

ipcMain.handle('SelectFile', async (event) => {
  const {filePaths:[path]} = await dialog.showOpenDialog({ properties: ['openFile'] })
  return path
})

ipcMain.handle('SelectFiles', async (event) => {
  const {filePaths} = await dialog.showOpenDialog({ properties: ['openFile', 'multiSelections'] })
  return filePaths
})

ipcMain.handle('SelectDirectory', async (event) => {
  const {filePaths:[path]} = await dialog.showOpenDialog({ properties: ['openDirectory', 'createDirectory'] })
  return path
})

ipcMain.handle('SelectDirectories', async (event) => {
  const {filePaths} = await dialog.showOpenDialog({ properties: ['openDirectory', 'createDirectory', 'multiSelections'] })
  return filePaths
})

ipcMain.handle('CopyToClipboard', (event, text) => {
  clipboard.writeText(text)
})

ipcMain.handle('OpenExternal', (event, url) => {
  shell.openExternal(url)
})

ipcMain.handle('OpenDirectory', (event, path) => {
  shell.showItemInFolder(path)
})
