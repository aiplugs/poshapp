<template>
    <div class="flex-grow-1">
        <v-btn text color="primary" v-on:click="handleOpen">Open Directory</v-btn>
        <v-btn text color="primary" v-on:click="handleRefresh">Refresh</v-btn>
        <v-btn text color="primary" :disabled="!updatable" v-on:click="handleUpdate">Update</v-btn>
        <v-dialog v-model="resetDialog" width="600" v-if="modified">
            <template v-slot:activator="{on}">
                <v-btn text color="primary" v-on="on">
                    Remove Modified <v-chip x-small color="primary">{{gitStatus.length}}</v-chip>
                </v-btn>
            </template>
            <v-card>
                <v-card-title>
                    Remove modified
                </v-card-title>
                <v-card-text>
                    <table>
                        <tbody>
                            <tr v-for="(item, index) in gitStatus" :key="index">
                                <td>{{item.labels.join(',')}}</td>
                                <td>{{item.file}}</td>
                            </tr>
                        </tbody>
                    </table>
                </v-card-text>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn text v-on:click="resetDialog=false">Cancel</v-btn>
                    <v-btn text color="error" v-on:click="handleReset">Remove Modified</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
        <span v-if="gitLogMessage">{{gitLogMessage}}</span>
        <v-data-table :headers="headers" :items="gitLog.logs" item-key="commit" single-expand show-expand hide-default-footer v-if="gitLog != null">
            <template v-slot:item.tag="{item}">
                <v-chip v-if="item.commit == gitLog.origin" color="primary">ORIGIN</v-chip>
                <v-chip v-if="item.commit == gitLog.local">LOCAL</v-chip>
            </template>
            <template v-slot:item.commit="{item}">
                {{item.commit.substring(0,7)}}
            </template>
            <template v-slot:item.author="{item}">
                {{item.name}} &lt;{{item.email}}&gt;
            </template>
            <template v-slot:item.when="{item}">
                {{new Date(item.when).toLocaleString()}}
            </template>
            <template v-slot:expanded-item="{ headers, item }">
                <td :colspan="headers.length">
                    <pre>{{item.message}}</pre>
                </td>
            </template>
        </v-data-table>
    </div>
</template>
<script>
import {mapState, mapActions, mapGetters} from 'vuex'
export default {
    data() {
        return {
            expanded: [],
            headers: [
                { text: '', value: 'tag', sortable: false },
                { text: 'Commit', value: 'commit', sortable: false },
                { text: 'Message', value: 'messageShort', sortable: false },
                { text: '', value: 'data-table-expand' },
                { text: 'Author', value: 'author', sortable: false },
                { text: 'Time', value: 'when', sortable: false },
            ],
            resetDialog: false,
        }
    },
    computed: {
        ...mapState('signalr', ['gitLog', 'gitLogMessage', 'gitStatus']),
        updatable() {
            return this.gitLog
                && this.gitLog.name == this.$route.params.id
                && this.gitLog.origin != this.gitLog.local
                && this.gitLog.logs.findIndex(item => item.commit == this.gitLog.origin) < this.gitLog.logs.findIndex(item => item.commit == this.gitLog.local)
        },
        modified() {
            return this.gitStatus.length > 0;
        }
    },
    methods: {
        ...mapActions('signalr', ['invokeGitLog', 'invokeGitStatus', 'invokeGitFetch', 'invokeGitForcePull', 'invokeGitReset']),
        ...mapActions('ipc', ['openRepositoryDir']),
        ...mapGetters('repositories',['find']),
        async getLog(repositoryName) {
            const repo = this.find()(repositoryName);
            if (repo) {
                await this.invokeGitFetch(repo);
                await this.invokeGitLog(repo);
                await this.invokeGitStatus(repo);
            }
        },
        handleOpen() {
            const repo = this.find()(this.$route.params.id);
            if (repo) {
                window.openDirectory(window.repositoryConfigFilePath(repo.path));
            }
        },
        handleRefresh() {
            this.getLog(this.$route.params.id);
        },
        handleUpdate() {
            const repo = this.find()(this.$route.params.id);
            if (repo) {
                this.invokeGitForcePull(repo);
            }
        },
        async handleReset() {
            const repo = this.find()(this.$route.params.id);
            if (repo) {
                await this.invokeGitReset(repo);
                await this.invokeGitStatus(repo);
                this.resetDialog = false;
            }
        }
    },
    mounted() {
        this.getLog(this.$route.params.id);
    },
    beforeRouteUpdate (to, from, next) {
        this.getLog(to.params.id);
        next();
    },
}
</script>