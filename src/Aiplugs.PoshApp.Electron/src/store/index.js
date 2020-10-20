import Vue from 'vue';
import Vuex from 'vuex';
import toast from './toast.js';
import scripts from './scripts.js';
import repositories from './repositories.js';
import activation from './activation.js';
import ipc, {ipcPlugin} from './ipc.js';

Vue.use(Vuex)

export default function() {
    return new Vuex.Store({
        modules: {
            toast,
            scripts,
            repositories,
            activation,
            ipc
        },
        plugins: [ipcPlugin]
    });
}
