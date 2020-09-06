<template>
    <v-dialog :value="updateAvailable" max-width="600" persistent>
        <v-card>
            <v-card-title>New version is released!</v-card-title>
            <v-divider></v-divider>
            <v-card-actions>
                <v-spacer></v-spacer>
                <v-btn text v-on:click="clearUpdateAvailable">At later</v-btn>
                <v-btn color="primary" v-on:click="quitAndInstall">Quit and Install now</v-btn>
            </v-card-actions>
        </v-card>
    </v-dialog>
</template>
<script>
import {mapState, mapMutations, mapActions} from 'vuex'
export default{
    computed: {
        ...mapState('ipc', ['updateAvailable'])
    },
    methods: {
        ...mapMutations('ipc', ['clearUpdateAvailable']),
        ...mapActions('ipc', ['quitAndInstall'])
    }
}
</script>