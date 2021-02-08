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

window.combinePath = function(a, b) {
    const sep = process.platform == 'win32' ? '\\' : '/'
    if (a.endsWith(sep) == false) a += sep;
    return a + b;
}

window.repositoryConfigFilePath = function(repositoryPath) {
    return combinePath(repositoryPath, 'config.json');
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

window.textDocumentDidOpen = (uri, languageId, version, text) => ipcRenderer.invoke('textDocumentDidOpen', uri, languageId, version, text);

window.textDocumentDidChange = (uri, version, changes) => ipcRenderer.invoke('textDocumentDidChange', uri, version, changes);

window.textDocumentDidSave = (uri, text) => ipcRenderer.invoke('textDocumentDidSave', uri, text);

window.textDocumentDidClose = (uri) => ipcRenderer.invoke('textDocumentDidClose', uri);

window.textDocumentCompletion = (uri, position, context) => ipcRenderer.invoke('textDocumentCompletion', uri, position, context);

window.textDocumentHover = (uri, position) => ipcRenderer.invoke('textDocumentHover', uri, position);

window.textDocumentSignatureHelp = (uri, position, context) => ipcRenderer.invoke('textDocumentSignatureHelp', uri, position, context);

window.textDocumentDefinition = (uri, position) => ipcRenderer.invoke('textDocumentDefinition', uri, position);

window.textDocumentReferences = (uri, position, context) => ipcRenderer.invoke('textDocumentReferences', uri, position, context);

window.textDocumentHighlight = (uri, position) => ipcRenderer.invoke('textDocumentHighlight', uri, position);

window.textDocumentSymbol = (uri) => ipcRenderer.invoke('textDocumentSymbol', uri);

window.textDocumentCodeAction = (uri, range, context) => ipcRenderer.invoke('textDocumentCodeAction', uri, range, context);

window.textDocumentCodeLens = (uri) => ipcRenderer.invoke('textDocumentCodeLens', uri);

window.textDocumentFormatting = (uri) => ipcRenderer.invoke('textDocumentFormatting', uri);

window.textDocumentRangeFormatting = (uri, range) => ipcRenderer.invoke('textDocumentRangeFormatting', uri, range);

window.textDocumentFoldingRange = (uri) => ipcRenderer.invoke('textDocumentFoldingRange', uri);

ipcRenderer.invoke('GetVersions').then(versions => window.versions = versions)