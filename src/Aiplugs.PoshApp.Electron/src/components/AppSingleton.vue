<template>
    <div class="main-content d-flex flex-column">
        <header>
            <v-row class="pl-4 pr-4" style="width: 100%;">
                <v-col cols="12" class="pt-0 pb-0">
                    <ParametersForm ref="form" 
                                     :script="scriptId" 
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
                       v-on:click="invokeAction({scriptId:action.id, input: null})" 
                       retain-focus-on-click>{{action.displayName || action.id}}</v-btn>
            </v-row>
        </header>
        <v-divider></v-divider>
        <div class="flex scroll-content">
            <DataViewer :data="defaultResult"></DataViewer>
        </div>
        <v-btn fab dark text color="grey" style="position: absolute; bottom: 8px; right: 8px;" v-on:click="gotoEditor">
            <v-icon>mdi-xml</v-icon>
        </v-btn>
    </div>
</template>
<script>
import {mapState, mapActions} from 'vuex'
import ParametersForm from './UI/ParametersForm'
import DataViewer from './UI/DataViewer'
export default {
    components: { ParametersForm, DataViewer },
    computed: {
        ...mapState('signalr', ['invoking', 'defaultResult']),
        scriptId() {
            return `${this.$route.params.repo}:${this.$route.params.id}`;
        },
        actions() {
            const { repo, id } = this.$route.params;
            return this.$store.getters['scripts/findActions'](repo, id);
        },
        disabled() {
            return this.invoking != null
        }
    },
    methods: {
        ...mapActions('signalr', ['invokeDefault', 'invokeAction', 'clearResult']),
        init() {
            this.clearResult();
        },
        run({ value }) {
            this.invokeDefault({ scriptId: this.scriptId, input: value });
        },
        gotoEditor() {
            this.$router.push(`/scripts/${this.$route.params.repo}/${this.$route.params.id}`)
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