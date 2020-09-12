import { ipcRenderer } from 'electron'

window.ipcRenderer = ipcRenderer;

window.selectFile = () => ipcRenderer.invoke('SelectFile')

window.selectFiles = () => ipcRenderer.invoke('SelectFiles')

window.selectDirectory = () => ipcRenderer.invoke('SelectDirectory')

window.selectDirectories = ()=> ipcRenderer.invoke('SelectDirectories')

window.copyToClipboard = text => ipcRenderer.invoke('CopyToClipboard', text)

window.openExternal =  url => ipcRenderer.invoke('OpenExternal', url)

window.openDirectory = path => ipcRenderer.invoke('OpenDirectory', path)

window.repositoryConfigFilePath = function(respositoryPath) {
    const suffix = process.platform == 'win32' ? '\\' : '/'
    if (respositoryPath.endsWith(suffix) == false) respositoryPath += suffix
    return respositoryPath + 'config.json'
}

ipcRenderer.invoke('GetVersions').then(versions => window.versions = versions)