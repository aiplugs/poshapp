<template>
    <v-dialog :value="promptForChoice != null" max-width="600px" persistent>
        <v-card v-if="promptForChoice != null">
            <v-card-title class="headline" primary-title>
                {{promptForChoice.caption}}
            </v-card-title>
            <v-card-text>
                <p style="white-space: pre-wrap">{{promptForChoice.message}}</p>
                <table>
                    <tr v-for="(choice, index) in promptForChoice.choices" :key="index">
                        <td>{{format(choice.Label)}}</td>
                        <td>{{choice.HelpMessage}}</td>
                    </tr>
                </table>
            </v-card-text>
            <v-divider></v-divider>
            <v-card-actions style="flex-wrap: wrap; justify-content: flex-end;">
                <v-spacer></v-spacer>
                <v-btn text v-for="(choice, index) in promptForChoice.choices"
                       :key="index"
                       :color="index==promptForChoice.defaultChoice?'primary':null"
                       v-on:click="select(index)">
                    {{format(choice.Label)}}
                </v-btn>
            </v-card-actions>
        </v-card>
    </v-dialog>
</template>
<script>
import {mapState, mapActions} from 'vuex'
export default {
    computed: {
        ...mapState('ipc', ['promptForChoice'])
    },
    methods: {
        ...mapActions('ipc', ['invokePromptForChoice']),
        format(label) {
            return (label||'').replace(/&/, '');
        },
        select(index) {
            this.invokePromptForChoice({ index });
        }
    }
}
</script>