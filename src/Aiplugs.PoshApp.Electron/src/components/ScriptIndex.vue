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
import { mapActions } from 'vuex'
import * as monaco from 'monaco-editor'
export default {
    data() {
        return {
            confirm: null,
            editor: null,
            content: null
        }
    },
    methods: {
        ...mapActions('scripts', ['loadScriptContent','saveScriptContent']),
        ...mapActions('toast', ['toast']),
        async loadScriptContent(repo, id) {
            return await window.ipcRenderer.invoke('GetScriptContent', repo, id)
        },
        async saveScriptContent(repo, id, content) {
            await window.ipcRenderer.invoke('PutScriptContent', repo, id, content);
        },
        async createEditor() {
            this.content = await this.loadScriptContent(this.$route.params.repo, this.$route.params.id);
            this.editor = monaco.editor.create(this.$refs.editor, {
                value: this.content,
                language: 'powershell',
                fontSize: 16,
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
        }, 
        async changeContent(repository, id) {
            if (this.editor) {
                const content = await this.loadScriptContent(repository, id);
                this.editor.getModel().setValue(content);
                this.content = content;
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
    mounted() {
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