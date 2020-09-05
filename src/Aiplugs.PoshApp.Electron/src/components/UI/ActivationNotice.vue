<template>
    <v-dialog v-model="display" max-width="600">
        <v-card>
            <v-card-title>FREE Plan limit exceeded</v-card-title>
            <v-card-text>
                <v-row align="center">
                    <v-col>
                        <span class="display-2">$0.00</span>
                        forever
                    </v-col>
                    <v-col>
                        <span class="display-2">$4.00</span> / month・machine
                    </v-col>
                </v-row>
                <v-row>
                    <v-col>
                        <ul>
                            <li><span class="font-weight-bold">2</span> repositories</li>
                            <li><span class="font-weight-bold">20</span> scripts</li>
                        </ul>
                    </v-col>
                    <v-col>
                        <ul>
                            <li>Unlimited repositories</li>
                            <li>Unlimites scripts</li>
                        </ul>
                    </v-col>
                </v-row>
            </v-card-text>
            <v-divider></v-divider>
            <v-card-actions>
                <v-spacer></v-spacer>
                <v-btn text v-on:click="hideActivationNotice">No thank you</v-btn>
                <v-btn color="primary" v-on:click="moveToActivation">Activate</v-btn>
            </v-card-actions>
        </v-card>
    </v-dialog>
</template>
<script>
import { mapState, mapMutations, mapActions } from 'vuex'
export default {
    computed: {
        ...mapState('activation', ['display'])
    }, 
    methods: {
        ...mapMutations('activation', ['hideActivationNotice']),
        ...mapActions('activation', ['loadActivationStatus']),
        moveToActivation() {
            this.hideActivationNotice();
            this.$router.push('/settings/activation');
        }
    },
    mounted() {
        this.loadActivationStatus();
    }
}
</script>