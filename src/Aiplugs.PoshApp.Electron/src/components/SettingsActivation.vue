<template id="activation-component">
    <div class="main-content overflow-y-auto no-scrollbar pa-8">
        <div v-if="status==='Valid'">
            <h1>Already activated!</h1>
            <p>Thank you for your support :)</p>
        </div>
        <div v-else-if="status==='NotMatch'">
            <h1>Computer Name or MAC Address is not match</h1>
            <p>Please reactivate machine. <a v-on:click="openEx"><v-icon x-small>mdi-open-in-new</v-icon>license page</a></p>
            <v-textarea label="Activation Request Code" v-model="requestCode" readonly auto-grow rows="3" v-on:click="copyRequestCode"></v-textarea>
            <v-form v-model="valid">
                <v-text-field label="Activation Code" v-model="activationCode" :rules="[v => !!v || 'Activation Code is required']"></v-text-field>
                <v-btn color="primary" block v-on:click="handleActivation" :disabled="!valid" :loading="loading">Register Activation Code</v-btn>
            </v-form>
        </div>
        <div v-else-if="status==='Expired'">
            <h1>Expired</h1>
            <p>Please reflesh or reactivate <a v-on:click="openEx"><v-icon x-small>mdi-open-in-new</v-icon>license page</a></p>
            <v-btn color="primary" v-on:click="handleReflesh" :loading="loading">Reflesh</v-btn>
            <v-btn text v-on:click="show=true">Reactivate</v-btn>
            <v-expand-transition>
                <div v-if="show">
                    <v-textarea label="Activation Request Code" v-model="requestCode" readonly auto-grow rows="1" v-on:click="copyRequestCode"></v-textarea>
                    <v-form v-model="valid">
                        <v-text-field label="Activation Code" v-model="activationCode" :rules="[v => !!v || 'Activation Code is required']"></v-text-field>
                        <v-btn color="primary" block v-on:click="handleActivation" :disabled="!valid" :loading="loading">Register Activation Code</v-btn>
                    </v-form>
                </div>
            </v-expand-transition>
        </div>
        <div v-else-if="status==='Illigal'">
            <h1>Activation code is illigal</h1>
            <p>Please try reactivation machine. <a v-on:click="openEx"><v-icon x-small>mdi-open-in-new</v-icon>license page</a></p>
            <v-textarea label="Activation Request Code" v-model="requestCode" readonly auto-grow rows="1" v-on:click="copyRequestCode"></v-textarea>
            <v-form v-model="valid">
                <v-text-field label="Activation Code" v-model="activationCode" :rules="[v => !!v || 'Activation Code is required']"></v-text-field>
                <v-btn color="primary" block v-on:click="handleActivation" :disabled="!valid" :loading="loading">Register Activation Code</v-btn>
            </v-form>
        </div>
        <div v-else-if="status==='None'">
            <v-card :max-width="400">
                <v-card-title>
                    PRO Plan
                </v-card-title>
                <v-card-text>
                    <v-row align="center">
                        <v-col class="display-3" cols="6">
                            $ 4.00
                        </v-col>
                        <v-col>
                            / month・machine
                        </v-col>
                    </v-row>
                    <ul>
                        <li>Unlimited repositories</li>
                        <li>Unlimites scripts</li>
                    </ul>
                </v-card-text>
                <v-card-actions>
                    <v-btn color="primary" block text v-on:click="show=true">
                        <v-icon>mdi-cart-outline</v-icon>
                        Activate
                    </v-btn>
                </v-card-actions>
                <v-expand-transition>
                    <div v-show="show">
                        <v-divider></v-divider>
                        <v-card-text>
                            <h5>Step 1 </h5>
                            <p><a v-on:click="openEx"><v-icon x-small>mdi-open-in-new</v-icon>Open license page</a> and signup &amp; checkout pro plan.</p>
                            <h5>Step 2</h5>
                            <p>Copy activation request code</p>
                            <v-textarea label="Activation Request Code" v-model="requestCode" readonly auto-grow rows="3" v-on:click="copyRequestCode"></v-textarea>
                            <h5>Step 3</h5>
                            <p>Register activation request code and get activation code on <a v-on:click="openEx"><v-icon x-small>mdi-open-in-new</v-icon>license page</a></p>
                            <h5>Final</h5>
                            <v-form v-model="valid">
                                <v-text-field label="Activation Code" v-model="activationCode" :rules="[v => !!v || 'Activation Code is required']"></v-text-field>
                                <v-btn color="primary" block v-on:click="handleActivation" :disabled="!valid" :loading="loading">Register Activation Code</v-btn>
                            </v-form>
                        </v-card-text>
                    </div>
                </v-expand-transition>
            </v-card>
        </div>
    </div>
</template>
<script>
import {mapState, mapActions} from 'vuex'
export default {
    data() {
        return {
            show: false,
            valid: false,
            loading: false,
            activationCode: null,
        }
    },
    computed: {
        ...mapState('activation', ['status', 'requestCode'])
    },
    methods: {
        ...mapActions('activation', ['activate', 'refleshActivationCode']),
        ...mapActions('ipc', ['copyTo']),
        openEx() {
            window.openExternal('https://poshapp.aiplugs.com/licenses');
        },
        async copyRequestCode() {
            await window.copyToClipboard(this.requestCode);
            alert("Copied!");
        },
        async handleReflesh() {
            try {
                this.loading = true;
                await this.refleshActivationCode();
            }
            finally {
                this.loading = false;
            }
        },
        async handleActivation() {
            try {
                this.loading = true;
                await this.activate(this.activationCode);
            }
            finally {
                this.loading = false;
            }
        }
    }
}
</script>