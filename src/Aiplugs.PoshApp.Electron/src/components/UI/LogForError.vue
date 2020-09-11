<template>
    <v-dialog :value="logErrors != null" max-width="600px">
        <v-card>
            <v-card-title class="headline red--text">
                Error
            </v-card-title>
            <v-card-text class="overflow-y-auto"
                         style="white-space: pre-wrap; max-height:70vh;">{{logErrors}}</v-card-text>
            <v-divider></v-divider>
            <v-card-actions>
                <v-spacer></v-spacer>
                <v-btn text v-on:click="clearError">
                    CLOSE
                </v-btn>
            </v-card-actions>
        </v-card>
    </v-dialog>
</template>
<script>
import {mapState, mapMutations} from 'vuex'
export default {
    data() {
        return {
            dialog: false,
            message: null
        }
    },
    computed: {
        ...mapState('ipc', ['logErrors'])
    },
    methods: {
        ...mapMutations('ipc', ['clearError'])
    },
}
</script>