<template>
    <v-menu :value="dialog || readLine != null" ref="main"
            :nudge-top="600"
            :close-on-click="false"
            :close-on-content-click="false"
            transition="slide-y-reverse-transition">
        <template v-slot:activator="{on}">
            <v-btn x-small text icon v-on="on" title="Write-Host Log" :height="24">
                <v-icon>mdi-timeline-text-outline</v-icon>
            </v-btn>
        </template>
        <div class="black d-flex flex-column overflow-y-auto no-scrollbar" style="width: 600px; height: 600px; scroll-behavior: smooth;">
            <div style="flex: 1;"></div>
            <pre v-for="(message, index) in logMessages" :key="index"
                 v-text="message.text"
                 :class="resolveColor(message)" class="pl-4 pr-4" style="white-space:pre-wrap;"></pre>
            <div v-if="readLine!=null" class="pl-4 pr-4">
                <v-text-field v-model="input"
                            :type="readLine.secure ? 'password':'text'"
                            append-outer-icon="mdi-keyboard-return"
                            placeholder="Input here"
                            dark
                            @click:append-outer="handlePrompt"></v-text-field>
            </div>
            <div class="black text-right" ref="bottom" style="position:sticky;bottom:0;width:100%;">
                <v-btn dark text icon v-on:click="dialog=false">
                    <v-icon>mdi-chevron-down</v-icon>
                </v-btn>
            </div>
        </div>
    </v-menu>
</template>
<script>
import Vue from 'vue'
import { mapState, mapActions } from 'vuex'
export default {
    data() {
        return {
            dialog: false,
            input: null,
        }
    },
    computed: {
        ...mapState('ipc', ['logMessages', 'readLine'])
    },
    watch: {
        logMessages() {
            this.dialog = true;
            Vue.nextTick()
                .then(() => {
                    this.scrollDown();
                })
        }
    },
    methods: {
        ...mapActions('ipc', ['invokeReadLine']),
        resolveColor(message) {
            const { color, bgColor } = message.color == 0 && message.bgColor == 0 ? { color: 15, bgColor: 0 } : message;
            const foregroundColors = [
                'black--text',
                'blue--text text--darken-4',
                'green--text text--darken-4',
                'cyan--text text--darken-4',
                'red--text text--darken-4',
                'pink--text text--darken-4',
                'yellow--text text--darken-4',
                'grey--text',
                'grey--text text--darken-4',
                'blue--text',
                'green--text',
                'cyan--text',
                'red--text',
                'pink--text',
                'yellow--text',
                'white--text']
            const backgroudColors = [
                'black',
                'blue darken-4',
                'green darken-4',
                'cyan darken-4',
                'red darken-4',
                'pink darken-4',
                'yellow darken-4',
                'grey',
                'grey darken-4',
                'blue',
                'green',
                'cyan',
                'red',
                'pink',
                'yellow',
                'white']
            return [...foregroundColors[color].split(' '), ...backgroudColors[bgColor].split(' ')]
        },
        scrollDown() {
            this.$refs.bottom.parentElement.scrollTo(0, this.$refs.bottom.parentElement.scrollHeight)
        },
        handlePrompt() {
            this.invokeReadLine({ input: this.input, secure: this.readLine.secure });
            this.input = null;
        }
    }
}
</script>