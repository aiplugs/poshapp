<template>
    <v-dialog :value="promptForCredential != null" max-width="600px" persistent>
        <v-card v-if="promptForCredential != null">
            <v-card-title class="headline grey lighten-2" primary-title>
                {{promptForCredential.caption}}
            </v-card-title>
            <v-card-text>
                <v-container>
                    <p style="white-space: pre-wrap">{{promptForCredential.message}}</p>
                    <v-form ref="form" v-model="valid">
                        <v-text-field label="Username"
                                      v-model="username"
                                      :rules="[v => !!v || 'Username is required']"
                                      required></v-text-field>
                        <v-text-field label="Password"
                                      type="password"
                                      v-model="password"
                                      :rules="[v => !!v || 'Password is required']"
                                      required></v-text-field>
                    </v-form>
                </v-container>
            </v-card-text>

            <v-divider></v-divider>

            <v-card-actions>
                <v-spacer></v-spacer>
                <v-btn text color="primary"
                       v-on:click="handlePromptForCredential"
                       :disabled="!valid">
                    OK
                </v-btn>
            </v-card-actions>
        </v-card>
    </v-dialog>
</template>
<script>
import { mapState, mapActions } from 'vuex'
export default {
    data() {
        return {
            username: null,
            password: null,
            valid: false,
        }
    },
    computed: {
        ...mapState('ipc', ['promptForCredential']),
    },
    methods: {
        ...mapActions('ipc', ['invokePromptForCredential', 'invokePromptForGitCredential']),
        async handlePromptForCredential() {
            const payload = { username: this.username, password: this.password };
            if (this.promptForCredential.forGit) {
                await this.invokePromptForGitCredential(payload)
            }
            else {
                await this.invokePromptForCredential(payload)
            }
            this.username = null;
            this.password = null;
        }
    }
}
</script>