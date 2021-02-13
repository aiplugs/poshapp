import { ipcMain } from 'electron'
import path from 'path'
import cp  from 'child_process'
const lsp = require('vscode-languageserver-protocol')
const isDevelopment = process.env.NODE_ENV !== 'production'

// let connection;
export async function startPSES () {
  const binary = process.platform == 'win32' ? 'Aiplugs.PoshApp.Pses.exe' : 'Aiplugs.PoshApp.Pses'
  const deamon = isDevelopment ? `bin/pses/bin/Common/${binary}` : path.join(__dirname, '../bin/pses/bin/Common/', binary)
  const childProcess = cp.spawn(deamon);
  const connection = lsp.createProtocolConnection(
      new lsp.StreamMessageReader(childProcess.stdout),
      new lsp.StreamMessageWriter(childProcess.stdin));

  connection.onError((e, message, number) => {
    console.error('onError', message);
  });

  connection.onNotification(lsp.LogMessageNotification.type, ({type,message}) => {
    console.log('logMessage', message)
  })
  connection.onNotification(lsp.ShowMessageNotification.type, (params) => {
    console.log('showMessage',params)
  })
  connection.onNotification(lsp.TelemetryEventNotification.type, (params) => {
    console.log('telemetryEvent',params)
  })
  connection.onNotification(lsp.PublishDiagnosticsNotification.type, (params) => {
    console.log('publishDiagnostic',params)
  })

  const textDocumentDidOpen = new lsp.RequestType('textDocument/didOpen');
  ipcMain.handle('textDocumentDidOpen', async (event, uri, languageId, version, text) => {
    return await connection.sendRequest(textDocumentDidOpen, {
      textDocument: {
        uri,
        languageId,
        version,
        text
      }
    })
  })

  const textDocumentDidChange = new lsp.RequestType('textDocument/didChange');
  ipcMain.handle('textDocumentDidChange', async (event, uri, version, changes) => {
    return await connection.sendRequest(textDocumentDidChange, {
      textDocument: {
        uri,
        version
      },
      contentChanges: changes
    })
  })

  const textDocumentDidSave = new lsp.RequestType('textDocument/didSave');
  ipcMain.handle('textDocumentDidSave', async (event, uri, text) => {
    return await connection.sendRequest(textDocumentDidSave, {
      textDocument: {
        uri
      },
      text
    })
  })

  const textDocumentDidClose = new lsp.RequestType('textDocument/didClose');
  ipcMain.handle('textDocumentDidClose', async (event, uri) => {
    return await connection.sendRequest(textDocumentDidClose, {
      textDocument: {
        uri
      }
    })
  })

  const textDocumentCompletion = new lsp.RequestType('textDocument/completion');
  ipcMain.handle("textDocumentCompletion", async (event, uri, position, context) => {
    console.log("textDocumentCompletion", uri)
    return await connection.sendRequest(textDocumentCompletion, {
        textDocument: {
          uri
        },
        position,
        context
    });
  });

  const textDocumentHover = new lsp.RequestType('textDocument/hover');
  ipcMain.handle("textDocumentHover", async (event, uri, position) => {
    return await connection.sendRequest(textDocumentHover, {
        textDocument: {
          uri
        },
        position
    });
  });

  const textDocumentSignatureHelp = new lsp.RequestType('textDocument/signatureHelp');
  ipcMain.handle("textDocumentSignatureHelp", async (event, uri, position, context) => {
    return await connection.sendRequest(textDocumentSignatureHelp, {
        textDocument: {
          uri
        },
        position,
        context
    });
  });

  const textDocumentDefinition = new lsp.RequestType('textDocument/definition');
  ipcMain.handle("textDocumentDefinition", async (event, uri, position) => {
    return await connection.sendRequest(textDocumentDefinition, {
        textDocument: {
          uri
        },
        position
    });
  });

  const textDocumentReferences = new lsp.RequestType('textDocument/references');
  ipcMain.handle("textDocumentReferences", async (event, uri, position, context) => {
    return await connection.sendRequest(textDocumentReferences, {
        textDocument: {
          uri
        },
        position,
        context
    });
  });

  const textDocumentHighlight = new lsp.RequestType('textDocument/documentHighlight');
  ipcMain.handle("textDocumentHighlight", async (event, uri, position) => {
    return await connection.sendRequest(textDocumentHighlight, {
        textDocument: {
          uri
        },
        position
    });
  });

  const textDocumentSymbol = new lsp.RequestType('textDocument/documentSymbol');
  ipcMain.handle("textDocumentSymbol", async (event, uri) => {
    return await connection.sendRequest(textDocumentSymbol, {
        textDocument: {
          uri
        }
    });
  });

  const textDocumentCodeAction = new lsp.RequestType('textDocument/codeAction');
  ipcMain.handle("textDocumentCodeAction", async (event, uri, range, context) => {
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
  });

  const textDocumentCodeLens = new lsp.RequestType('textDocument/codeLens');
  ipcMain.handle("textDocumentCodeLens", async (event, uri) => {
    return await connection.sendRequest(textDocumentCodeLens, {
        textDocument: {
          uri
        }
    });
  });

  const textDocumentFormatting = new lsp.RequestType('textDocument/formatting');
  ipcMain.handle("textDocumentFormatting", async (event, uri) => {
    return await connection.sendRequest(textDocumentFormatting, {
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
  });

  const textDocumentRangeFormatting = new lsp.RequestType('textDocument/rangeFormatting');
  ipcMain.handle("textDocumentRangeFormatting", async (event, uri, range) => {
    return await connection.sendRequest(textDocumentRangeFormatting, {
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
  });

  const textDocumentFoldingRange = new lsp.RequestType('textDocument/foldingRange');
  ipcMain.handle("textDocumentFoldingRange", async (event, uri) => {
    return await connection.sendRequest(textDocumentFoldingRange, {
        textDocument: {
          uri
        },
    });
  });

  connection.listen();
  const initializeResult = await connection.sendRequest(lsp.InitializeRequest.type,{
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
  })
  await connection.sendNotification(lsp.InitializedNotification.type, {})
}
