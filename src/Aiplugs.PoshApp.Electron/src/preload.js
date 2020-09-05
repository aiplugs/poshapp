import { ipcRenderer, remote } from 'electron'

window.ipcRenderer = ipcRenderer;

window.versions = {
    chrome: process.versions.chrome
}

window.selectFile = async function () {
    const {filePaths:[path]} = await remote.dialog.showOpenDialog({ properties: ['openFile'] })
    return path
}

window.selectFiles = async function () {
    const {filePaths} = await remote.dialog.showOpenDialog({ properties: ['openFile', 'multiSelections'] })
    return filePaths
}

window.selectDirectory = async function () {
    const {filePaths:[path]} = await remote.dialog.showOpenDialog({ properties: ['openDirectory', 'createDirectory'] })
    return path
}

window.selectDirectories = async function () {
    const {filePaths} = await remote.dialog.showOpenDialog({ properties: ['openDirectory', 'createDirectory', 'multiSelections'] })
    return filePaths
}

window.copyToClipboard = function (text) {
    remote.clipboard.writeText(text)
}

window.openExternal =  function (url) {
    remote.shell.openExternal(url)
}

window.openDirectory = function (path) {
    remote.shell.showItemInFolder(path);
}
