<template>
    <section v-if="detailResult" class="d-flex flex-column white" style="position: absolute; top:0; width:calc(100vw - 56px - 256px); height: calc(100vh - 24px); overflow:hidden;">
        <header>
            <v-btn text v-on:click="$emit('close')">
                <v-icon>mdi-arrow-right</v-icon>
            </v-btn>
            <v-btn text v-for="action in actions" :key="action.id" v-on:click="invokeAction({scriptId:action.id, input:[input]})">{{action.displayName || action.id}}</v-btn>
        </header>
        <div class="flex scroll-content">
            <DataViewer :data="detailResult"></DataViewer>
        </div>
    </section>
</template>

<script>
import { mapState, mapActions } from 'vuex'
import DataViewer from './UI/DataViewer'
export default {
    props: ['script', 'input'],
    components: { DataViewer },
    computed: {
        ...mapState('signalr', ['detailResult']),
        actions() {
            const [repo, id] = this.script.split(':');
            return this.$store.getters['scripts/findActions'](repo, id);
        }
    },
    methods: {
        ...mapActions('signalr', ['invokeDetail', 'invokeAction'])
    },
    mounted() {
        this.invokeDetail({ scriptId: this.script, input: this.input });
    }
}
</script>