export default {
    namespaced: true,
    state: {
        updateDownloading: null,
        updateAvailable: false,
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
        }
    },
    actions: {
        quitAndInstall() {
            window.ipcRenderer.send('QuitAndInstall');
        }
    }
};

export function ipcPlugin(store) {
    window.ipcRenderer.on('UpdateDownloading', (sender, progress) => {
        store.commit('ipc/setUpdateDownloading', progress);
    });
    window.ipcRenderer.on('UpdateAvailable', sender => {
        store.commit('ipc/setUpdateAvailable');
    });
}
