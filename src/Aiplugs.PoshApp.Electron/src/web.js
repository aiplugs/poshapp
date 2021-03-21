import {
    createProtocolConnection,
    ReadableStreamMessageReader,
    WriteableStreamMessageWriter,
    RAL,
    LogMessageNotification,
    ShowMessageNotification,
    TelemetryEventNotification,
    PublishDiagnosticsNotification,
    InitializedNotification,
    InitializeRequest,
    RequestType
} from 'vscode-languageserver-protocol'
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

    const ContentLength = 'Content-Length: ';
    const CRLF = '\r\n';

    class CustomWriteableStreamMessageWriter extends WriteableStreamMessageWriter{
        constructor(writable) {
            super(writable);
        }

        async write(msg) {
            return this.writeSemaphore.lock(async () => {
                const payload = this.options.contentTypeEncoder.encode(msg, this.options).then((buffer) => {
                    if (this.options.contentEncoder !== undefined) {
                        return this.options.contentEncoder.encode(buffer);
                    }
                    else {
                        return buffer;
                    }
                });
                return payload.then((buffer) => {
                    const headers = [];
                    headers.push(ContentLength, buffer.byteLength.toString(), CRLF);
                    headers.push(CRLF);
                    return this.doWrite(msg, headers, buffer);
                }, (error) => {
                    this.fireError(error);
                    throw error;
                });
            });
        }

        async doWrite(msg, headers, data) {
            try {
                await this.writable.write(headers.join(''), 'utf-8');
                return this.writable.write(data);
            } catch (error) {
                this.handleError(error, msg);
                return Promise.reject(error);
            }
        }
    }
    
    async function openWebSocket(uri) {
        return new Promise(function(resolve) {
            const socket = new WebSocket(uri);
            socket.onopen = () => {
                resolve(socket);
            }
        });
    }

    const socket = await openWebSocket(`wss://${location.host}/pses`);
    const reader = new ReadableStreamMessageReader(RAL().stream.asReadableStream(socket));
    const writer = new CustomWriteableStreamMessageWriter(RAL().stream.asWritableStream(socket));
    const connection = createProtocolConnection(reader, writer);

    connection.onError((e, message, number) => {
        console.error('onError', message);
    });

    connection.onNotification(LogMessageNotification.type, ({type,message}) => {
        console.log('logMessage', message)
    })
    connection.onNotification(ShowMessageNotification.type, (params) => {
        console.log('showMessage',params)
    })
    connection.onNotification(TelemetryEventNotification.type, (params) => {
        console.log('telemetryEvent',params)
    })
    connection.onNotification(PublishDiagnosticsNotification.type, (params) => {
        console.log('publishDiagnostic',params)
    })

    const textDocumentDidOpen = new RequestType('textDocument/didOpen');
    window.textDocumentDidOpen = (uri, languageId, version, text) => {
        return connection.sendRequest(textDocumentDidOpen, {
            textDocument: {
                uri,
                languageId,
                version,
                text
            }
        })
    }

    const textDocumentDidChange = new RequestType('textDocument/didChange');
    window.textDocumentDidChange = (uri, version, changes) => {
        return connection.sendRequest(textDocumentDidChange, {
            textDocument: {
                uri,
                version
            },
            contentChanges: changes
        });
    }

    const textDocumentDidSave = new RequestType('textDocument/didSave');
    window.textDocumentDidSave = (uri, text) => {
        return connection.sendRequest(textDocumentDidSave, {
            textDocument: {
                uri
            },
            text
        })
    }

    const textDocumentDidClose = new RequestType('textDocument/didClose');
    window.textDocumentDidClose = (uri) => {
        return connection.sendRequest(textDocumentDidClose, {
            textDocument: {
                uri
            }
        })
    }

    const textDocumentCompletion = new RequestType('textDocument/completion');
    window.textDocumentCompletion = (uri, position, context) => {
        return connection.sendRequest(textDocumentCompletion, {
            textDocument: {
                uri
            },
            position,
            context
        });
    }

    const textDocumentHover = new RequestType('textDocument/hover');
    window.textDocumentHover = (uri, position) => {
        return connection.sendRequest(textDocumentHover, {
            textDocument: {
                uri
            },
            position
        });
    }

    const textDocumentSignatureHelp = new RequestType('textDocument/signatureHelp');
    window.textDocumentSignatureHelp = (uri, position, context) => {
        return connection.sendRequest(textDocumentSignatureHelp, {
            textDocument: {
                uri
            },
            position,
            context
        });
    }

    const textDocumentDefinition = new RequestType('textDocument/definition');
    window.textDocumentDefinition = (uri, position) => {
        return connection.sendRequest(textDocumentDefinition, {
            textDocument: {
                uri
            },
            position
        });
    }

    const textDocumentReferences = new RequestType('textDocument/references');
    window.textDocumentReferences = (uri, position, context) => {
        return connection.sendRequest(textDocumentReferences, {
            textDocument: {
                uri
            },
            position,
            context
        });
    }

    const textDocumentHighlight = new RequestType('textDocument/documentHighlight');
    window.textDocumentHighlight = (uri, position) => {
        return connection.sendRequest(textDocumentHighlight, {
            textDocument: {
                uri
            },
            position
        });
    }

    const textDocumentSymbol = new RequestType('textDocument/documentSymbol');
    window.textDocumentSymbol = (uri) => {
        return connection.sendRequest(textDocumentSymbol, {
            textDocument: {
              uri
            }
        });
    }

    const textDocumentCodeAction = new RequestType('textDocument/codeAction');
    window.textDocumentCodeAction = async (uri, range, context) => {
        try {
            return await connection.sendRequest(textDocumentCodeAction, {
                textDocument: {
                    uri
                },
                range,
                context
            });
        } catch (e) {
            // if error code is not Content Modified
            if (e.code != -32801) {
                console.error('textDocumentCodeAction.catch', e);
            }
        }
    }

    const textDocumentCodeLens = new RequestType('textDocument/codeLens');
    window.textDocumentCodeLens = (uri) => {
        return connection.sendRequest(textDocumentCodeLens, {
            textDocument: {
              uri
            }
        });
    }

    const textDocumentFormatting = new RequestType('textDocument/formatting');
    window.textDocumentFormatting = (uri) => {
        return connection.sendRequest(textDocumentFormatting, {
            textDocument: {
              uri
            },
            options: {
              tabSize: 4,
              insertSpaces: true,
              trimTrailingWhitespace: true,
              insertFinalNewline: true,
              trimFinalNewlines: true,
            }
        });
    }

    const textDocumentRangeFormatting = new RequestType('textDocument/rangeFormatting');
    window.textDocumentRangeFormatting = (uri, range) => {
        return connection.sendRequest(textDocumentRangeFormatting, {
            textDocument: {
              uri
            },
            range,
            options: {
              tabSize: 4,
              insertSpaces: true,
              trimTrailingWhitespace: true,
              insertFinalNewline: true,
              trimFinalNewlines: true,
            }
        });
    }
    
    const textDocumentFoldingRange = new RequestType('textDocument/foldingRange');
    window.textDocumentFoldingRange = (uri) => {
        return connection.sendRequest(textDocumentFoldingRange, {
            textDocument: {
              uri
            },
        });
    }

    console.log('listening')
    connection.listen();
    console.log('listened')

    connection.sendRequest(InitializeRequest.type,{
        processId: process.pid,
        rootUri: null,
        capabilities: {
            textDocument: {
                synchronization: {
                    dynamicRegistration: false,
                },
                completion:{
                    dynamicRegistration: false,
                    completionItem: {
                        snippetSupport: true,
                        commitCharactersSupport: true,
                        deprecatedSupport: true,
                        preselectSupport: true,
                        tagSupport: null,
                        insertReplaceSupport: true,
                        resolveSupport: null,
                        insertTextModeSupport: null
                    }
                },
                hover:{
                    dynamicRegistration: false,
                    contentFormat: ['markdown']
                },
                signatureHelp:{
                    dynamicRegistration: false,
                    signatureInformation: {
                        documentationFormat: ['plaintext', 'markdown']
                    }
                },
                definition:{
                    dynamicRegistration: false,
                    linkSupport: false,
                },
                typeDefinition:{},
                references:{},
                documentHighlight:{},
                documentSymbol:{},
                codeAction:{},
                codeLens:{},
                formatting:{},
                rangeFormatting:{},
                foldingRange:{},
            }
        }
    }).then(initializeResult => {
        console.log('initialized', initializeResult);
        connection.sendNotification(InitializedNotification.type, {})
    })
}