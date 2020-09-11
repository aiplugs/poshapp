<template>
    <v-dialog :value="progressesLength > 0" max-width="600px">
        <v-card>
            <v-card-text>
                <v-list>
                    <v-list-item v-for="(progress, activity) in progresses" :key="activity">
                        <v-list-item-content>
                            <v-list-item-title>{{progress.activity}}</v-list-item-title>
                            <v-progress-linear color="light-blue"
                                               height="10"
                                               striped
                                               :value="progress.percent">
                            </v-progress-linear>
                            <v-list-item-subtitle>{{progress.status}}: {{progress.currentOperation}}</v-list-item-subtitle>
                        </v-list-item-content>
                    </v-list-item>
                </v-list>
            </v-card-text>
        </v-card>
    </v-dialog>
</template>
<script>
import {mapState} from 'vuex'
export default {
    computed: {
        ...mapState('ipc', ['progresses']),
        progressesLength() {
            return Object.keys(this.progresses).filter(name => !name.startsWith('_')).length;
        }
    }
}
</script>