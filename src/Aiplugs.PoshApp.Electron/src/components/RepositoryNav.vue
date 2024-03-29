<template>
    <section>
        <v-btn text block v-on:click="openCreate"><v-icon dark>mdi-plus</v-icon>Add New Repository</v-btn>
        <v-list class="grow">
            <v-list-item :to="'/repositories/'+repo.name" v-for="repo in repositories" :key="repo.name">
                <v-list-item-content>   
                    <v-list-item-title class="pl-4">{{repo.name}}</v-list-item-title>
                </v-list-item-content>
                <v-list-item-action>
                    <v-menu>
                        <template v-slot:activator="{ on }">
                            <v-btn text icon v-on:click.stop.prevent="on.click($event)" 
                                             v-on:keydown.stop.prevent="on.keydown($event)">
                                <v-icon>mdi-dots-vertical</v-icon>
                            </v-btn>
                        </template>
                        <v-list>
                            <!--
                            <v-list-item v-on:click="openEdit(repo)">
                                <v-list-item-title>Properties</v-list-item-title>
                            </v-list-item> 
                            -->
                            <v-list-item v-on:click="openDelete(repo)">
                                <v-list-item-title>Delete</v-list-item-title>
                            </v-list-item>
                        </v-list>
                    </v-menu>
                </v-list-item-action>
            </v-list-item>
        </v-list>
        <v-dialog v-model="openNewDialog" max-width="600px" persistent>
            <v-card>
                <v-card-title class="headline">{{ !repositoryId ? "New Repository" : "Update Repository" }}</v-card-title>
                <v-card-text>
                    <v-container fluid>
                        <v-form ref="form" v-model="isValid">
                            <v-radio-group row v-model="repositoryType">
                                <v-radio label="Local" value="local"></v-radio>
                                <v-radio label="Remote" value="remote"></v-radio>
                            </v-radio-group>
                            <div v-if="repositoryType=='remote'">
                                <v-text-field label="Origin Url"
                                              v-model="origin"
                                              :rules="originRules"
                                              :readonly="readonlyOrigin"
                                              :disabled="readonlyOrigin"
                                              required>
                                </v-text-field>
                            </div>
                            <v-text-field label="Repository Name"
                                          v-model="name"
                                          :rules="nameRules"
                                          required>
                            </v-text-field>
                            <div class="d-flex align-center" v-if="desktop">
                                <v-text-field label="Directory Path"
                                              :value="path"
                                              :rules="pathRules"
                                              :disabled="readonlyPath"
                                              readonly
                                              required>
                                </v-text-field>
                                <v-btn :disabled="readonlyPath"
                                       v-on:click="openFileManager" text>
                                    <v-icon>mdi-folder-search-outline</v-icon>
                                </v-btn>
                            </div>
                        </v-form>
                    </v-container>
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="grey darken-1"
                           text
                           v-on:click="openNewDialog = false">
                        Cancel
                    </v-btn>
                    <v-btn color="blue darken-1"
                           text
                           :disabled="!isValid"
                           v-on:click="handleCreateOrUpdate">
                        {{ !repositoryId ? "Create" : "Update" }}
                    </v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
        <v-dialog :value="deleteDialog" max-width="600">
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
import {mapActions, mapGetters, mapMutations, mapState} from 'vuex'
export default {
    data() {
        return {
            openNewDialog: false,
            name: null,
            path: null,
            origin: null,
            readonlyOrigin: false,
            readonlyPath: false,
            isCreate: false,
            isValid: false,
            repositoryId: null,
            repositoryType: 'local',
            nameRules:[
                v => !!v || 'Repository Name is required',
                v => /^[a-zA-Z0-9_]*$/.test(v) || 'Valid characters are A-Z a-z 0-9 _'
            ],
            pathRules: [
                v => !!v || 'Repository Path is required'
            ],
            originRules: [
                v => this.repositoryType == 'local' || !!v || 'Origin Url is required'
            ],
            deleteTarget: null,
            desktop: window.electron === true
        }
    },
    computed: {
        ...mapState('ipc', ['selectedDirectory', 'selecting']),
        ...mapState('repositories', ['repositories']),
        deleteDialog() {
            return this.deleteTarget != null;
        }
    },
    methods: {
        ...mapActions('ipc', ['selectDirectory']), 
        ...mapActions('repositories', ['loadRepositories', 'createRepository', 'updateRepository', 'deleteRepository']),
        async handleDelete() {
            if (this.deleteTarget) {
                await this.deleteRepository(this.deleteTarget);
                this.deleteTarget = null;
            }
        },
        handleCreateOrUpdate() {
            const payload = {
                id: this.repositoryId,
                name: this.name,
                path: this.path,
                origin: this.origin
            };
            this.createOrUpdate(payload);
            this.openNewDialog = false;
        },
        async createOrUpdate(payload) {
            if (this.isCreate) {
                this.createRepository(payload)
            }
            else {
                if (await this.updateRepository(payload)) {
                    this.navigateIfChangeId();
                }
            }
        },
        navigateIfChangeId() {
            if (this.$route.params.id == this.repositoryId && this.repositoryId != this.name) {
                this.$router.replace('/repositories/' + this.scriptName);
            }
        },
        clearDialogModel() {
            this.repositoryId = null;
            this.name = null;
            this.origin = null;
            this.readonlyPath = false;
            this.readonlyOrigin = false;
        },
        loadDialogModel(repositoryName) {
            const data = this.$store.getters['repositories/find'](repositoryName);
            if (data) {
                this.repositoryId = data.name;
                this.name = data.name;
                this.origin = data.origin;
                this.readonlyPath = true;
                this.readonlyOrigin = !!this.origin;
            }
        },
        openCreate() {
            this.clearDialogModel();
            this.isCreate = true;
            this.openNewDialog = true;
        },
        openEdit(repository) {
            this.clearDialogModel();
            this.loadDialogModel(repository.name); 
            this.isCreate = false;
            this.openNewDialog = true;
        },
        openDelete(repository) {
            this.deleteTarget = repository;
        },
        async openFileManager() {
            this.path = await window.selectDirectory();
        }
    },
    mounted() {
        this.loadRepositories();
    }
}
</script>