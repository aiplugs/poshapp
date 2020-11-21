import { ipcRenderer } from 'electron'

window.electron = true

window.ipcRenderer = ipcRenderer;

window.ipcOn = (channel, callback) => ipcRenderer.on(channel, callback)

window.selectFile = () => ipcRenderer.invoke('SelectFile')

window.selectFiles = () => ipcRenderer.invoke('SelectFiles')

window.selectDirectory = () => ipcRenderer.invoke('SelectDirectory')

window.selectDirectories = ()=> ipcRenderer.invoke('SelectDirectories')

window.copyToClipboard = text => ipcRenderer.invoke('CopyToClipboard', text)

window.openExternal =  url => ipcRenderer.invoke('OpenExternal', url)

window.openDirectory = path => ipcRenderer.invoke('OpenDirectory', path)

window.getActivation = () => ipcRenderer.invoke("GetActivation")

window.refleshActivation = () => ipcRenderer.invoke('RefleshActivation')

window.activate = activationCode => ipcRenderer.invoke('PostActivation', activationCode)

window.getScripts = () => ipcRenderer.invoke('GetScripts');

const updateMethods = {
    list: 'UpdateListScript',
    detail: 'UpdateDetailScript',
    singleton: 'UpdateSingletonScript',
    action: 'UpdateActionScript',
}
window.updateScript = (type, repositoryName, scriptId, script) => ipcRenderer.invoke(updateMethods[type.toLowerCase()], repositoryName, scriptId, script)

const createMethods = {
    list: 'CreateListScript',
    detail: 'CreateDetailScript',
    singleton: 'CreateSingletonScript',
    action: 'CreateActionScript',
}
window.createScript = (type, repositoryName, script) => ipcRenderer.invoke(createMethods[type.toLowerCase()], repositoryName, script)

window.deleteScript = (repositoryName, scriptId) => ipcRenderer.invoke('DeleteScript', repositoryName, scriptId)

window.getScriptContent = (repositoryName, scriptId) => ipcRenderer.invoke('GetScriptContent', repositoryName, scriptId)

window.putScriptContent = (repositoryName, scriptId, content) => ipcRenderer.invoke('PutScriptContent', repositoryName, scriptId, content)

window.getRepositories = () => ipcRenderer.invoke('GetRepositories')

window.createRepository = (repositoryName, path) => ipcRenderer.invoke('CreateRepository', repositoryName, path)

window.deleteRepository = repositoryName => ipcRenderer.invoke('DeleteRepository', repositoryName, false);

window.repositoryConfigFilePath = function(respositoryPath) {
    const suffix = process.platform == 'win32' ? '\\' : '/'
    if (respositoryPath.endsWith(suffix) == false) respositoryPath += suffix
    return respositoryPath + 'config.json'
}

window.invokeGetParameters = scriptId => ipcRenderer.invoke('InvokeGetParameters', scriptId)

window.invokeWithParameters = (scriptId, input) => ipcRenderer.invoke('InvokeWithParameters', scriptId, input)

window.invokeWithPipeline = (scriptId, input) =>  ipcRenderer.invoke('InvokeWithPipeline', scriptId, input)

window.invokeWithPipelines = (scriptId, input) =>  ipcRenderer.invoke('InvokeWithPipelines', scriptId, input)

window.sendReadLine = input => ipcRenderer.send('ReadLine', input)

window.sendReadLineAsSecureString = input => ipcRenderer.send('ReadLineAsSecureString', input)

window.sendPrompt = input => ipcRenderer.send('Prompt', input)

window.sendPromptForChoice = index => ipcRenderer.send('PromptForChoice', index)

window.sendPromptForCredential = (username, password) => ipcRenderer.send('PromptForCredential', username, password)

window.sendPromptForGitCredential = (username, password) => ipcRenderer.send('PromptForGitCredential', username, password);

window.getGitLog = (path, name) => ipcRenderer.invoke('GitLog', path);

window.getGitStatus = (path, name) => ipcRenderer.invoke('GitStatus', path);

window.fetchGit = (path, name) => ipcRenderer.invoke('GitFetch', path);

window.resetGit = (path, name) => ipcRenderer.invoke('GitReset', path);

window.cloneGit = (origin, path, name) =>ipcRenderer.invoke('GitClone', origin, path);

window.quitAndInstall = () => ipcRenderer.send('QuitAndInstall')

ipcRenderer.invoke('GetVersions').then(versions => window.versions = versions)