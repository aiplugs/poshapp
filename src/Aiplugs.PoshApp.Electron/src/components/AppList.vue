<template>
    <div class="main-content d-flex flex-column">
        <header>
            <v-row class="pl-4 pr-4" style="width: 100%;">
                <v-col cols="12" class="pt-0 pb-0">
                    <ParametersForm ref="form"
                                     :script="scriptId"
                                     :page="page"
                                     :page-size="pageSize"
                                     :loading="invoking=='$default'"
                                     :disabled="disabled"
                                     v-on:run="run">
                    </ParametersForm>
                </v-col>
            </v-row>
            <v-row class="pl-4 pr-4">
                <v-btn text v-for="action in actions"
                       :key="action.name"
                       :loading="invoking==action.id"
                       :disabled="disabled"
                       v-on:click="invokeAction({ scriptId:action.id, input: selected.map(item => item.$clixml) })"
                       retain-focus-on-click>{{action.displayName || action.id}}</v-btn>
            </v-row>
        </header>
        <v-divider></v-divider>
        <div class="flex" ref="content" v-resize="handleResize" style="overflow: hidden;">
            <v-data-table v-model="selected"
                          :headers="view.headers"
                          :items="view.list"
                          :page.sync="page"
                          :items-per-page="pageSize"
                          :height="height"
                          item-key="$clixml"
                          hide-default-footer
                          show-select
                          fixed-header
                          v-if="view.list.length > 0">
                <template v-slot:item.$poshapp_action="{ item }">
                    <v-btn icon text v-on:click="openDetail(item)">
                        <v-icon>mdi-fullscreen</v-icon>
                    </v-btn>
                </template>
            </v-data-table>
        </div>
        <div class="text-center overline">Total {{view.total}}</div>
        <div class="ma-auto" style="width: 80%;">
            <v-pagination v-model="page" :length="view.pageCount"></v-pagination>
        </div>
        <transition name="slide-x-reverse-transition">
            <AppDetail :script="detailScriptId" :input="detailInput" v-if="detailScriptId" v-on:close="detailScriptId=null" style="z-index:2;"></AppDetail>
        </transition>
        <v-btn fab dark text color="grey" style="position: absolute; bottom: 8px; right: 8px;" v-on:click="gotoEditor">
            <v-icon>mdi-xml</v-icon>
        </v-btn>
    </div>
</template>
<script>
import { mapState, mapActions } from 'vuex'
import AppDetail from './AppDetail'
import ParametersForm from './UI/ParametersForm'
export default {
    components: { AppDetail, ParametersForm },
    data() {
        return {
            selected:[],
            detailScriptId: null,
            detailInput: null, 
            page: 1,
            pageSize: 10,
            height: 0
        }
    }, 
    computed: {
        ...mapState('ipc', ['invoking','defaultResult']),
        scriptId() {
            return `${this.$route.params.repo}:${this.$route.params.id}`;
        },
        detailId() {
            const { repo, id } = this.$route.params;
            return this.$store.getters['scripts/findDetail'](repo, id);
        },
        actions() {
            const { repo, id } = this.$route.params;
            return this.$store.getters['scripts/findActions'](repo, id);
        },
        disabled() {
            return !!this.invoking
        },
        view() {
            const data = this.defaultResult;
            if (data == null) {
                return { list: [], headers:[], total: 0 }
            }

            let index = 0;
            let total = data.length;
            if (data.length > 0 && typeof data[index].value === 'number') {
                total = data[index].value;
                index++;
            }

            const pageCount = ~~(total / this.pageSize) + Math.min(1, total % this.pageSize);

            const dataset = data.slice(index);

            const allKeys = Object.keys(dataset.reduce((o,d) => Object.assign(o,d.value), {}));
                
            const headers = allKeys.map(key => ({ text:key, value:key, sortable: false }));
                
            if (this.detailId) {
                headers.unshift({text: '', value: '$poshapp_action', width: 64, sortable: false });
            }

            const list = dataset.filter(d => typeof d.value == 'object').map(d => {
                return Object.assign({ $clixml: d.clixml }, d.value);
            });

            return { list, headers, total, pageCount };
        }
    },
    methods: {
        ...mapActions('ipc', ['invokeDefault', 'invokeAction', 'clearResult']),
        init() {
            this.clearResult();
            this.selected.splice(0);
        },
        run({ value, page, pageSize }) {
            this.invokeDefault({ scriptId: this.scriptId, input: value });
            this.page = page;
            this.pageSize = pageSize;
        },
        openDetail(item) {
            this.detailInput = item.$clixml;
            this.detailScriptId = this.detailId;
        },
        gotoEditor() {
            if (this.detailScriptId) {
                const [repo, id] = this.detailScriptId.split(':');
                this.$router.push(`/scripts/${repo}/${id}`)
            }
            else {
                this.$router.push(`/scripts/${this.$route.params.repo}/${this.$route.params.id}`)
            }
        },
        handleResize() {
            if (this.$refs.content)
                this.height = this.$refs.content.getClientRects()[0].height;
        }
    },
    mounted() {
        this.init();
    },
    beforeRouteUpdate (to, from, next) {
        this.init();  
        next();
    }
}
</script>