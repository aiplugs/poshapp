<template>
    <section>
        <v-btn text block v-on:click="openCreate"><v-icon dark>mdi-plus</v-icon>Add New Script</v-btn>
        <v-list class="grow">
            <v-list-group v-for="(repository, index) in repositories" :key="repository" :value="index == 0">
                <template v-slot:activator>
                    <v-list-item-content>
                        <v-list-item-title>📁 {{repository}}</v-list-item-title>
                    </v-list-item-content>
                </template>
                <v-list-item :to="`/scripts/${repository}/${script.id}`" v-for="script in repositoryScripts(repository)" :key="`${repository}:${script.id}`">
                    <v-list-item-content>
                        <v-list-item-title>📄 {{script.id}}</v-list-item-title>
                    </v-list-item-content>
                    <v-list-item-action>
                        <v-menu>
                            <template v-slot:activator="{ on }">
                                <v-btn text icon v-on:click.stop.prevent="on.click($event)" v-on:keydown.stop.prevent="on.keydown($event)">
                                    <v-icon>mdi-dots-vertical</v-icon>
                                </v-btn>
                            </template>
                            <v-list>
                                <v-list-item v-on:click="openEdit(script.repository, script.id)">
                                    <v-list-item-title>Properties</v-list-item-title>
                                </v-list-item>
                                <v-list-item v-on:click="openDelete(script.repository, script.id)">
                                    <v-list-item-title>Delete</v-list-item-title>
                                </v-list-item>
                            </v-list>
                        </v-menu>
                    </v-list-item-action>
                </v-list-item>
            </v-list-group>
        </v-list>
        <v-dialog v-model="openDialog" max-width="600px" persistent>
            <v-card>
                <v-card-title class="headline">{{!scriptId ? "New Script" : "Edit Script"}}</v-card-title>
                <v-card-text>
                    <v-container fluid>
                        <v-form ref="form" v-model="isValid">
                            <v-select label="Repository"
                                      v-model="repository"
                                      :disabled="!isCreate"
                                      :items="repositories"
                                      :rules="[v => !!v || 'Repository is required']">
                            </v-select>
                            <v-text-field label="Script Name"
                                          v-model="scriptName"
                                          :rules="[v => !!v || 'Script Name is required', v => /[a-zA-Z][a-zA-Z0-9_-]*/.test(v) || 'Script Name need [a-zA-Z][a-zA-Z0-9_-]* ']"
                                          required>
                            </v-text-field>
                            <v-radio-group row v-model="scriptType">
                                <v-radio label="List" value="List"></v-radio>
                                <v-radio label="Detail" value="Detail"></v-radio>
                                <v-radio label="Singleton" value="Singleton"></v-radio>
                                <v-radio label="Action" value="Action"></v-radio>
                            </v-radio-group>
                            <div v-if="scriptType == 'List'">
                                <v-text-field label="Display Name"
                                              v-model="displayName"
                                              required>
                                </v-text-field>
                                <v-text-field label="Group"
                                              v-model="group">
                                </v-text-field>
                                <v-select label="Detail"
                                          v-model="detail"
                                          :items="detailNames(repository)">
                                </v-select>
                                <v-select label="Actions"
                                          v-model="actions"
                                          :items="actionNames(repository)"
                                          multiple>
                                </v-select>
                            </div>
                            <div v-if="scriptType == 'Detail'">
                                <v-select label="Actions"
                                          v-model="actions"
                                          :items="actionNames(repository)"
                                          multiple>
                                </v-select>
                            </div>
                            <div v-if="scriptType == 'Singleton'">
                                <v-text-field label="Display Name"
                                              v-model="displayName"
                                              required>
                                </v-text-field>
                                <v-text-field label="Group"
                                              v-model="group">
                                </v-text-field>
                                <v-select label="Actions"
                                          v-model="actions"
                                          :items="actionNames(repository)"
                                          multiple>
                                </v-select>
                            </div>
                            <div v-if="scriptType == 'Action'">
                                <v-text-field label="Display Name"
                                              v-model="displayName"
                                              required>
                                </v-text-field>
                            </div>
                        </v-form>
                    </v-container>
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="grey darken-1"
                           text
                           v-on:click="openDialog = false">
                        Cancel
                    </v-btn>
                    <v-btn color="blue darken-1"
                           text
                           :disabled="!isValid"
                           v-on:click="handleCreateOrUpdate">
                        {{ !scriptId ? "Create" : "Update" }}
                    </v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
        <v-dialog :value="deleteTarget" max-width="600">
            <v-card>
                <v-card-title>Are you sure you want to delete?</v-card-title>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn text v-on:click="deleteTarget=null">No</v-btn>
                    <v-btn text color="error" v-on:click="handleDelete">Yes</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
    </section>
</template>
<script>
import { mapGetters, mapActions, mapMutations } from 'vuex'
export default {
    data() {
        return {
            openDialog: false,
            scriptId: null,
            scriptType: 'List',
            scriptName: null,
            displayName: null,
            repository: null,
            group: null,
            detail: null,
            actions: [],
            isCreate: false,
            isValid: true,
            deleteTarget: null
        }
    },
    computed: {
        ...mapGetters('scripts', ['repositories', 'scripts', 'detailNames', 'actionNames']),
        ...mapGetters('activation', ['exceededFreePlanForScripts']),
        repositoryScripts() {
            return repositoryName => this.scripts.filter(script => script.repository === repositoryName);
        }
    },
    methods: {
        ...mapActions('scripts', ['loadScripts','createScript','updateScript', 'deleteScript']),   
        ...mapMutations('activation', ['showActivationNotice']),
        async handleDelete() {
            if (this.deleteTarget) {
                await this.deleteScript(this.deleteTarget);
                this.deleteTarget = null;
            }
        },
        async handleCreateOrUpdate() {
            const payload = {
                $id: this.scriptId,
                id: this.scriptName,
                displayName: this.displayName,
                type: this.scriptType,
                group: this.group,
                detail: this.detail,
                actions: this.actions,
                repository: this.repository,
            };
            await this.createOrUpdate(payload);
            this.openDialog = false;
        },
        async createOrUpdate(payload) {
            if (this.isCreate) {
                await this.createScript(payload)
            }
            else {
                if (await this.updateScript(payload)) {
                    this.navigateIfChangeId();
                }
            }
        },
        navigateIfChangeId() {
            const { repo, id } = this.$route.params;
            if (id == this.scriptId && this.scriptId != this.scriptName && repo == this.repository) {
                this.$router.replace(`/scripts/${repo}/${this.scriptName}`);
            }
        },
        clearDialogModel() {
            this.scriptId = null
            this.scriptType = 'List'
            this.scriptName = null
            this.displayName = null
            this.repository = null
            this.group = null
            this.detail = null
            this.actions.splice(0);
        },
        loadDialogModel(repository, scriptName) {
            const data = this.$store.getters['scripts/find'](repository, scriptName);
            if (data) {
                this.scriptId = data.id;
                this.scriptName = data.id;
                this.displayName = data.displayName;
                this.scriptType = data.type;
                this.repository = data.repository;
                this.group = data.group || null;
                this.detail = data.detail || null;
                if (Array.isArray(data.actions)) {
                    this.actions.splice(0, this.actions.length, ...data.actions);
                }
            } 
        },
        openCreate() {
            if (this.exceededFreePlanForScripts(this.scripts.length)) {
                this.showActivationNotice();
            }
            else {
                this.clearDialogModel();
                this.isCreate = true;
                this.openDialog = true;
            }
        },
        openEdit(repository, scriptName) {
            this.clearDialogModel();
            this.loadDialogModel(repository, scriptName); 
            this.isCreate = false;
            this.openDialog = true;
        },
        openDelete(repository, scriptName) {
            this.deleteTarget = { repository, id: scriptName };
        }
    },
    async mounted() {
        await this.loadScripts();
    }
}
</script>