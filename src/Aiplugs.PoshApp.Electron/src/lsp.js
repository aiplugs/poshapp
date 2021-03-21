import { Uri } from 'monaco-editor'
export function toLSPPosition(monacoPosition) {
    const line = monacoPosition.lineNumber === undefined || monacoPosition.lineNumber === null ? undefined : monacoPosition.lineNumber - 1;
    const character = monacoPosition.column === undefined || monacoPosition.column === null ? undefined : monacoPosition.column - 1;
    return { line, character }
}
export function toLSPRange(monacoRange) {
    const start = { line: monacoRange.startLineNumber - 1, character: monacoRange.startColumn - 1 };
    const end = { line: monacoRange.endLineNumber - 1, character: monacoRange.endColumn - 1 };
    return { start, end }
}
export function toLSPLocation(monacoLocation) {
    const uri = monacoLocation.resource.toString()
    const range = toLSPRange(monacoLocation)
    return { uri, range }
}
export function fromLSPRange(lspRange) {
    const startLineNumber = lspRange.start.line + 1;
    const startColumn = lspRange.start.character + 1;
    const endLineNumber = lspRange.end.line + 1;
    const endColumn = lspRange.end.character + 1;
    return { startLineNumber, startColumn, endLineNumber, endColumn }
}
export function fromLSPLocation(lspLocation) {
    const resource = Uri.parse(lspLocation.uri);
    return Object.assign(fromLSPRange(lspLocation.range), { resource });
}
export function fromLSPDocumentSymbols(lspDocSymbols) {
    if (lspDocSymbols == null)
        return null;

    return lspDocSymbols.map(s => ({
        name: s.name,
        detail: s.detail,
        kind: s.kind + 1,
        tags: s.tags,
        range: fromLSPRange(s.range != null ? s.range : s.location.range),
        selectionRange: s.selectionRange != null ? fromLSPRange(s.selectionRange) : null,
        children: fromLSPDocumentSymbols(s.children)
    }));
}
export function fromLSPCommand(lspCommand) {
    if (!lspCommand)
        return null;
    return { id: lspCommand.command, title: lspCommand.title, arguments: lspCommand.arguments }
}

let registered = false;

export const tryRegisterLSP = monaco => {
    if (registered)
        return;

    if (window.textDocumentCompletion) {
        monaco.languages.registerCompletionItemProvider('powershell', {
            triggerCharacters: [ '.', '-', ':', '\\' ],
            provideCompletionItems(model, position, context, token) {
                return window.textDocumentCompletion(model.uri.toString(), toLSPPosition(position), context).then(result => {
                    return {
                        incomplete: false,
                        suggestions: result
                    }
                })
            }
        })
        registered = true;
    }
    if (window.textDocumentHover) {
        monaco.languages.registerHoverProvider('powershell', {
            provideHover(model, position, token) {
                return window.textDocumentHover(model.uri.toString(), toLSPPosition(position)).then(result => {
                    if (result == null)
                        return null;
                    return {
                        contents: result.contents,
                        range: result.range ? fromLSPRange(result.range) : null
                    }
                })
            }
        })
        registered = true;
    }
    if (window.textDocumentSignatureHelp) {
        monaco.languages.registerSignatureHelpProvider('powershell', {
            triggerCharacters: [' '],
            provideSignatureHelp(model, position, token, context) {
                return window.textDocumentSignatureHelp(model.uri.toString(), toLSPPosition(position), context).then(result => {
                    return result;
                })
            }
        })
        registered = true;
    }
    if (window.textDocumentDefinition) {
        monaco.languages.registerDefinitionProvider('powershell', {
            provideDefinition(model, position, token) {
                return window.textDocumentDefinition(model.uri.toString(), toLSPPosition(position)).then(result => {
                    if (result == null)
                        return null;
    
                    if (Array.isArray(result)) {
                        return result.map(r => ({
                            uri: monaco.Uri.parse(r.uri),
                            range: fromLSPRange(r.range)
                        }))
                    }
                    return {
                        uri: monaco.Uri.parse(result.uri),
                        range: fromLSPRange(result.range)
                    };
                })
            }
        })
        registered = true;
    }
    if (window.textDocumentReferences) {
        monaco.languages.registerReferenceProvider('powershell', {
            provideReferences(model, position, context, token) {
                return window.textDocumentReferences(model.uri.toString(), toLSPPosition(position), context).then(result => {
                    if (result == null)
                        return null;
    
                    return result.map(r => ({
                        uri: monaco.Uri.parse(r.uri),
                        range: fromLSPRange(r.range)
                    }))
                })
            }
        })
        registered = true;
    }
    if (window.textDocumentHighlight) {
        monaco.languages.registerDocumentHighlightProvider('powershell', {
            provideDocumentHighlights(model, position, token) {
                return window.textDocumentHighlight(model.uri.toString(), toLSPPosition(position)).then(result => {
                    if (result == null)
                        return null;
                    return result.map(h => ({
                        kind: h.kind+1,
                        range: fromLSPRange(h.range)
                    }))
                })
            }
        })
        registered = true;
    }
    if (window.textDocumentSymbol) {
        monaco.languages.registerDocumentSymbolProvider('powershell', {
            provideDocumentSymbols(model, token) {
                return window.textDocumentSymbol(model.uri.toString()).then(result => {
                    return fromLSPDocumentSymbols(result);
                })
            }
        })
        registered = true;
    }
    if (window.textDocumentCodeAction) {
        monaco.languages.registerCodeActionProvider('powershell', {
            provideCodeActions(model, range, context, token) {
                return window.textDocumentCodeAction(model.uri.toString(), toLSPRange(range), {
                    diagnostics: context.markers.map(m => ({
                        range: toLSPRange(m),
                        severity: m.severity == 8 ? 1
                                : m.severity == 4 ? 2
                                : m.severity == 2 ? 3
                                : m.severity == 1 ? 4
                                : 0,
                        code: typeof m.code == 'object' ? m.code.value :m.code,
                        codeDescription: typeof m.code == 'object' ? m.code.target.toString() : null,
                        source: m.source,
                        message: m.message,
                        tags: m.tags,
                        relatedInformation: m.relatedInformation ? { message: m.relatedInformation.message, location: toLSPLocation(m.relatedInformation) } : null,
                    })),
                    only: context.only ? [context.only] : null
                }).then(result => {
                    if (result == null)
                        return null;
    
                    const actions = [];
    
                    if (Array.isArray(result)) {
                        actions.push(...result.map(a => ({
                            command: a.command ?  { id: a.command, title: a.title, arguments: a.arguments } : null,
                            diagnostics: a.diagnostics ?  a.diagnostics.map(d => Object.assign(fromLSPRange(d.range), {
                                severity: d.severity == 1 ? 8
                                        : d.severity == 2 ? 4
                                        : d.severity == 3 ? 2
                                        : d.severity == 4 ? 1
                                        : 0,
                                code: d.codeDescription ?  { target: monaco.Uri.parse(d.codeDescription), value: d.code } : d.code,
                                source: d.source,
                                message: d.message,
                                tags: d.tags,
                                relatedInformation: d.relatedInformation ? Object.assign(fromLSPLocation(d.relatedInformation), { message: d.relatedInformation.message }) : null,
                            })) : null,
                            edit: a.edit ? { edits: Object.keys(a.edit.changes).map(uri => ({ 
                                resource: monaco.Uri.parse(uri).toString(),
                                edit: { text: a.edit.changes[uri].newText, range: fromLSPRange(a.edit.changes[uri].range) }
                            })) } : null,
                            disabled: a.disabled,
                            isPreferred: a.isPreferred,
                            kind: a.kind,
                            title: a.title,
                        })));
                    }
                    else {
                        actions.push({ command: { id: result.command, title: result.title, arguments: result.arguments } })
                    }
    
                    return { actions, dispose: function() {} }
                })
            }
        })
        registered = true;
    }
    if (window.textDocumentCodeLens) {
        monaco.languages.registerCodeLensProvider('powershell', {
            provideCodeLenses(model, token) {
                return window.textDocumentCodeLens(model.uri.toString()).then(result => {
                    if (result == null)
                        return null;
    
                    return { 
                        lenses: result.map(l => ({
                            range: fromLSPRange(l.range),
                            command: fromLSPCommand(l.command)
                        })),
                        dispose: function() {}
                    };
                })
            }
        })
        registered = true;
    }
    if (window.textDocumentFormatting) {
        monaco.languages.registerDocumentFormattingEditProvider('powershell', {
            provideDocumentFormattingEdits(model, options, token) {
                return window.textDocumentFormatting(model.uri.toString()).then(result => {
                    if (result == null)
                        return null;
    
                    return result.map(edit => ({
                        range: fromLSPRange(edit.range),
                        text: edit.newText
                    }))
                })
            }
        })
        registered = true;
    }
    if (window.textDocumentRangeFormatting) {
        monaco.languages.registerDocumentRangeFormattingEditProvider('powershell', {
            provideDocumentRangeFormattingEdits(model, range, options, token) {
                return window.textDocumentRangeFormatting(model.uri.toString(), toLSPRange(range)).then(result => {
                    if (result == null)
                        return null;
    
                    return result.map(edit => ({
                        range: fromLSPRange(edit.range),
                        text: edit.newText
                    }))
                })
            }
        })
        registered = true;
    }
}