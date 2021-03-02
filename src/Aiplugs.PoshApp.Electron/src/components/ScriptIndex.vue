<template>
    <div style="position: absolute; width: 100%; height: 100%;" v-resize="handleResize">
        <div>
            <v-btn text v-on:click="save" :disabled="!editor">Save</v-btn>
            <v-btn text v-on:click="gotoApp">Goto App</v-btn>
        </div>
        <v-dialog :value="confirm" :max-width="600">
            <v-card>
                <v-card-title>Are you sure you want to leave?</v-card-title>
                <v-divider></v-divider>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn text v-on:click="cancelLeave">No</v-btn>
                    <v-btn text color="error" v-on:click="leave">Yes</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
        <div ref="editor" class="fill-height"></div>
    </div>
</template>
<script>
import { mapActions, mapGetters } from 'vuex'
import * as monaco from 'monaco-editor'
function toLSPPosition(monacoPosition) {
    const line = monacoPosition.lineNumber === undefined || monacoPosition.lineNumber === null ? undefined : monacoPosition.lineNumber - 1;
    const character = monacoPosition.column === undefined || monacoPosition.column === null ? undefined : monacoPosition.column - 1;
    return { line, character }
}
function toLSPRange(monacoRange) {
    const start = { line: monacoRange.startLineNumber - 1, character: monacoRange.startColumn - 1 };
    const end = { line: monacoRange.endLineNumber - 1, character: monacoRange.endColumn - 1 };
    return { start, end }
}
function toLSPLocation(monacoLocation) {
    const uri = monacoLocation.resource.toString()
    const range = toLSPRange(monacoLocation)
    return { uri, range }
}
function fromLSPRange(lspRange) {
    const startLineNumber = lspRange.start.line + 1;
    const startColumn = lspRange.start.character + 1;
    const endLineNumber = lspRange.end.line + 1;
    const endColumn = lspRange.end.character + 1;
    return { startLineNumber, startColumn, endLineNumber, endColumn }
}
function fromLSPLocation(lspLocation) {
    const resource = monaco.Uri.parse(lspLocation.uri);
    return Object.assign(fromLSPRange(lspLocation.range), { resource });
}
function fromLSPDocumentSymbols(lspDocSymbols) {
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
function fromLSPCommand(lspCommand) {
    if (!lspCommand)
        return null;
    return { id: lspCommand.command, title: lspCommand.title, arguments: lspCommand.arguments }
}
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
}
if (window.textDocumentSymbol) {
    monaco.languages.registerDocumentSymbolProvider('powershell', {
        provideDocumentSymbols(model, token) {
            return window.textDocumentSymbol(model.uri.toString()).then(result => {
                return fromLSPDocumentSymbols(result);
            })
        }
    })
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
}
export default {
    data() {
        return {
            confirm: null,
            editor: null,
            content: null,
            version: 1
        }
    },
    methods: {
        ...mapActions('scripts', ['loadScriptContent','saveScriptContent']),
        ...mapActions('repositories', ['loadRepositories']),
        ...mapActions('toast', ['toast']),
        getFullPath(repo, id) {
            const script = this.$store.getters['scripts/find'](repo || this.$route.params.repo, id || this.$route.params.id);
            const repository = this.$store.getters['repositories/find']( repo || this.$route.params.repo);
            return window.combinePath(repository.path, script.path).replace(/\\/g, '/');
        },
        async loadScriptContent(repo, id) {
            return await window.getScriptContent(repo, id)
        },
        async saveScriptContent(repo, id, content) {
            await window.putScriptContent(repo, id, content);
        },
        async createEditor() {
            this.content = await this.loadScriptContent(this.$route.params.repo, this.$route.params.id);
            this.editor = monaco.editor.create(this.$refs.editor, {
                fontSize: 16,
                theme: window.matchMedia('(prefers-color-scheme: dark)').matches ? 'vs-dark' : 'vs-light'
            });
            this.editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KEY_S, () => {
                const value = this.editor.getModel().getValue();
                this.saveScriptContent(this.$route.params.repo, this.$route.params.id, value).then(_ => {
                    this.content = value;
                    this.toast({
                        text: "Saved",
                        color: "success",
                        top: true,
                        right: true
                    });
                })
            })
            await this.changeContent(this.$route.params.repo, this.$route.params.id);
        }, 
        async changeContent(repository, id) {
            if (this.editor) {
                const path = this.getFullPath(repository, id);
                const uri = monaco.Uri.from({scheme: 'inmemory', path });
                const content = await this.loadScriptContent(repository, id);
                let model = monaco.editor.getModel(uri);
                if (!model) {
                    model = monaco.editor.createModel(content, 'powershell', uri);
                    model.onDidChangeContent(evt => {
                        const changes = evt.changes.map(c => ({
                            text: c.text,
                            range: toLSPRange(c.range)
                        }))
                        if (window.textDocumentDidChange)
                            window.textDocumentDidChange(uri.toString(), evt.versionId, changes);
                    })
                    if (window.textDocumentDidOpen)
                        window.textDocumentDidOpen(uri.toString(), 'powershell', this.version, content);
                }
                this.editor.setModel(model);
                this.content = model.getValue();
            }
        },
        confirmLeave(callback) {
            if (this.editor == null) {
                callback();
                return;
            }
            
            const value = this.editor.getModel().getValue();

            if (this.content == value) {
                callback();
                return;
            }

            this.confirm = callback;
        },
        handleResize() {
            if (this.editor) {
                this.editor.layout();
            }
        },
        async save() {
            const value = this.editor.getModel().getValue();
            await this.saveScriptContent(this.$route.params.repo, this.$route.params.id, value);
            this.content = value;
            this.toast({
                text: "Saved",
                color: "success",
                top: true,
                right: true
            });
        },
        gotoListFromDetail(repo, id) {
            const page = this.$store.getters['scripts/findPageByDetail'](repo, id);
            if (page) {
                this.$router.push(`/${page.type.toLowerCase()}/${page.repository}/${page.id}`);
            }
        },
        gotoPageFromAction(repo, id) {
            const page = this.$store.getters['scripts/findPageByAction'](repo, id);
            if (page) {
                if (page.type === 'Detail') {
                    this.gotoListFromDetail(page.repository, page.id);
                }
                else {
                    this.$router.push(`/${page.type.toLowerCase()}/${page.repository}/${page.id}`);
                }
            }
        },
        gotoApp() {
            const { repo, id } = this.$route.params;
            const script = this.$store.getters['scripts/find'](repo, id);

            if (['List', 'Singleton'].includes(script.type)) {
                this.$router.push(`/${script.type.toLowerCase()}/${repo}/${id}`);
            }
            else if (script.type === 'Detail') {
                this.gotoListFromDetail(repo, id);
            }
            else if (script.type === 'Action') {
                this.gotoPageFromAction(repo, id);
            }
        },
        cancelLeave() {
            this.confirm = null;
        },
        leave() {
            this.confirm();
        }
    },
    async mounted() {
        await this.loadRepositories();
        this.createEditor();
    },
    beforeRouteUpdate(to, from, next) {
        this.confirmLeave(() => {
            next();
            this.confirm = null;
            this.changeContent(to.params.repo, to.params.id);
        })
    },
    beforeRouteLeave (to, from , next) {
        this.confirmLeave(() => {
            next();
        })
    }
}
</script>