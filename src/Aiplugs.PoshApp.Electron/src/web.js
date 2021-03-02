export default async function () {
    if (window.electron)
        return;

    const callbacks = {}

    window.ipcOn = (channel, callback) => {
        callbacks[channel] = callback
    }

    class jsonrpc
    {
        constructor(ep, callbacks, respondFactory) {
            this._id = 0
            this._resolves = {}
            this._callbacks = callbacks
            this.socket = new WebSocket(`wss://${location.host}/${ep}`)
            this.socket.addEventListener('message', (event) => {
                console.log(event.data);
                const data = JSON.parse(event.data)
        
                if (data.error)
                    throw data.error.message
        
                const { id, result } = data
                if (id && this._resolves[id]) {
                    this._resolves[id](result)
                    delete this._resolves[id]
                    return
                }
                
                const { method, params } = data 
                if(this._callbacks[method]) {
                    respondFactory.apply(this, [method, id])
                    this._callbacks[method](null, ...params);
                    return
                }
            });
        }
        start() {
            return new Promise((resolve) => {
                this.socket.addEventListener('open', function () {
                    resolve();
                })
            })
        }
        invoke(method, params) {
            return new Promise((resolve) => {
                const id = ++this._id;
                this._resolves[id] = resolve
                this.socket.send(JSON.stringify({ jsonrpc: '2.0', method, params, id }))
            })
        }
        send(method, params) {
            const id = ++this._id;
            this.socket.send(JSON.stringify({ jsonrpc: '2.0', method, params, id }))
        }
        respond(id, result) {
            this.socket.send(JSON.stringify({ jsonrpc: '2.0', id, result }))
        }
    }
    
    const powershell = new jsonrpc('powershell', callbacks, (method, id) => {
        if (method == 'ReadLine')
            window.sendReadLine = input => powershell.respond(id, input)

        else if (method == 'ReadLineAsSecureString')
            window.sendReadLineAsSecureString = input => powershell.respond(id, input)

        else if (method == 'Prompt')
            window.sendPrompt = input => powershell.respond(id, input)

        else if (method == 'PromptForChoice')
            window.sendPromptForChoice = index => powershell.respond(id, index)

        else if (method == 'PromptForCredential')
            window.sendPromptForCredential = (username, password) => powershell.respond(id, {username, password})
    })

    const git = new jsonrpc('git', callbacks, (method, id) => {
        if (method == 'PromptForGitCredential')
            window.sendPromptForGitCredential = (username, password) => git.respond(id, {username, password})
    })

    await powershell.start();
    await git.start();

    window.selectFile = () => alert('Not imepelemented on Web version.')
    window.selectFiles = () => alert('Not imepelemented on Web version.')
    window.selectDirectory = () => alert('Not imepelemented on Web version.')
    window.selectDirectories = ()=> alert('Not imepelemented on Web version.')
    window.copyToClipboard = text => window.navigator.clipboard.writeText(text)
    window.openExternal =  url => window.open(url, '_blank')
    window.openDirectory = path => alert('Not imepelemented on Web version.\n' + path)
    window.combinePath = (a, b) => a + '/' + b;
    window.repositoryConfigFilePath = (respositoryPath) => respositoryPath + '/config.json'
    window.getActivation = () => powershell.invoke('GetActivation')
    window.refleshActivation = powershell.invoke('RefleshActivation')
    window.activate = activationCode => powershell.invoke('PostActivation', [activationCode])
    window.getScripts = () => powershell.invoke('GetScripts')

    const updateMethods = {
        list: 'UpdateListScript',
        detail: 'UpdateDetailScript',
        singleton: 'UpdateSingletonScript',
        action: 'UpdateActionScript',
    }
    window.updateScript = (type, repositoryName, scriptId, script) => powershell.invoke(updateMethods[type.toLowerCase()], [repositoryName, scriptId, script])

    const createMethods = {
        list: 'CreateListScript',
        detail: 'CreateDetailScript',
        singleton: 'CreateSingletonScript',
        action: 'CreateActionScript',
    }
    window.createScript = (type, repositoryName, script) => powershell.invoke(createMethods[type.toLowerCase()], [repositoryName, script])
    
    window.deleteScript = (repositoryName, scriptId) => powershell.invoke('DeleteScript', [repositoryName, scriptId])

    window.getScriptContent = (repositoryName, scriptId) => powershell.invoke('GetScriptContent', [repositoryName, scriptId])

    window.putScriptContent = (repositoryName, scriptId, content) => powershell.invoke('PutScriptContent', [repositoryName, scriptId, content])

    window.getRepositories = () => powershell.invoke('GetRepositories')

    window.createRepository = (repositoryName, path) => powershell.invoke('CreateRepository', [repositoryName, path])

    window.deleteRepository = repositoryName => powershell.invoke('DeleteRepository', [repositoryName, true])

    window.invokeGetParameters = scriptId => powershell.invoke('InvokeGetParameters', [scriptId])

    window.invokeWithParameters = (scriptId, input) => powershell.invoke('InvokeWithParameters', [scriptId, input])

    window.invokeWithPipeline = (scriptId, input) => powershell.invoke('InvokeWithPipeline', [scriptId, input])
    
    window.invokeWithPipelines = (scriptId, input) => powershell.invoke('InvokeWithPipelines', [scriptId, input])
    
    window.getGitLog = (path, name) => git.invoke('GetLog', [name])

    window.getGitStatus = (path, name) => git.invoke('GetStatus', [name])

    window.fetchGit = (path, name) => git.invoke('Fetch', [name])

    window.resetGit = (path, name) => git.invoke('Reset', [name])

    window.cloneGit = (origin, path, name) => git.invoke('Clone', [origin, name])

    window.quitAndInstall = () => {}

    window.versions = {
        app: '-',
        node: '-',
        chrome: '-',
        electron: '-',
    }
}