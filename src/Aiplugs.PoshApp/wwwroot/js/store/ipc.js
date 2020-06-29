if (!window.ipcRenderer) {
    window.ipcRenderer = {
        on: function () { },
        send: function () { },
        removeAllListeners: function () { },
    };
}
export default {
    namespaced: true,
    state: {
        updateDownloading: null,
        updateAvailable: false,
        selecting: false,
        selectedDirectory: {},
        selectedFile: {}
    },
    mutations: {
        setUpdateDownloading(state, progress) {
            state.updateDownloading = progress;
        },
        setUpdateAvailable(state) {
            state.updateDownloading = null;
            state.updateAvailable = true;
        },
        clearUpdateAvailable(state) {
            state.updateAvailable = false;
        },
        setSelectedDirectory(state, { name, path }) {
            Vue.set(state.selectedDirectory, name, path);
            state.selecting = false;
        },
        clearSelectedDirectory(state) {
            state.selectedDirectory = {};
        },
        setSelectedFile(state, { name, path }) {
            Vue.set(state.selectedFile, name, path);
            state.selecting = false;
        },
        clearSelectedFile(state) {
            state.selectedFile = {};
        },
        setSelecting(state) {
            state.selecting = true;
        }
    },
    actions: {
        checkForUpdates() {
            ipcRenderer.send('check-for-updates');
        },
        quitAndInstall() {
            ipcRenderer.send('quit-and-install');
        },
        clearSelected({ commit }) {
            commit('clearSelectedDirectory');
            commit('clearSelectedFile');
        },
        selectDirectory({ commit }, { name }) {
            ipcRenderer.send('select-directory', name);
            commit('setSelecting');
        },
        selectFile({ commit }, { name }) {
            ipcRenderer.send('select-file', name);
            commit('clearSelectedFile');
            commit('setSelecting');
        },
        openActivation() {
            ipcRenderer.send('open-activation'); 
        },
        copyTo(_, text) {
            ipcRenderer.send('copy-to', text);
        },
        openRepositoryDir(_, { repositoryName }) {
            ipcRenderer.send('open-repository-dir', repositoryName);
        },
        reload() {
            ipcRenderer.send('reload');
        }
    }
};

export function ipcPlugin(store) {
    ipcRenderer.on('update-downloading', (sender, progress) => {
        store.commit('ipc/setUpdateDownloading', progress);
    });
    ipcRenderer.on('update-available', sender => {
        store.commit('ipc/setUpdateAvailable');
    });
    ipcRenderer.on('select-directory-reply', (sender, path, name) => {
        store.commit('ipc/setSelectedDirectory', { path, name });
    });
    ipcRenderer.on('select-file-reply', (sender, path, name) => {
        store.commit('ipc/setSelectedFile', { path, name });
    });
}
