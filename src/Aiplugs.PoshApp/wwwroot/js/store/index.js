import list from './list.js';
import toast from './toast.js';
import scripts from './scripts.js';
import repositories from './repositories.js';
import activation from './activation.js';
import signalr, { signalRPlugin } from './signalr.js';
import ipc, { ipcPlugin } from './ipc.js';

export default new Vuex.Store({
    modules: {
        //list,
        toast,
        scripts,
        repositories,
        activation,
        signalr,
        ipc
    },
    plugins: [signalRPlugin, ipcPlugin]
});
