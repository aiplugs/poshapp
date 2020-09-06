const MAX_FREE_PLAN_REPOSITORIES = 2;
const MAX_FREE_PLAN_SCRIPTS = 20;
export default {
    namespaced: true,
    state: {
        display: false,
        status: 'None',
        requestCode: null
    },
    getters: {
        exceededFreePlanForRepositories(state) {
            return len => state.status !== 'Valid' && len >= MAX_FREE_PLAN_REPOSITORIES;
        },
        exceededFreePlanForScripts(state) {
            return len => state.status !== 'Valid' && len >= MAX_FREE_PLAN_SCRIPTS;
        }
    },
    mutations: {
        showActivationNotice(state) {
            state.display = true;
        },
        hideActivationNotice(state) {
            state.display = false;
        },
        setStatus(state, status) {
            state.status = status;
        },
        setRequestCode(state, code) {
            state.requestCode = code;
        }
    },
    actions: {
        async loadActivationStatus({ commit }) {
            const { status, requestCode } = await window.ipcRenderer.invoke("GetActivation")
            commit('setStatus', status);
            commit('setRequestCode', requestCode);
        },
        async refleshActivationCode({ commit, dispatch }) {
            try {
                const { status, requestCode } = await window.ipcRenderer.invoke('RefleshActivation')
                if (status !== 'Illigal') {
                    commit('setStatus', status);
                    commit('setRequestCode', requestCode);
                }
                else {
                    dispatch('toast/toast', {
                        text: "Invalid activation code",
                        color: "error",
                        bottom: true
                    }, { root: true });
                }
            }
            catch {
                dispatch('toast/toast', {
                    text: "Invalid activation code",
                    color: "error",
                    bottom: true
                }, { root: true });
            }
        },
        async activate({ commit, dispatch }, activationCode) {
            try {
                const { status, requestCode } = await window.ipcRenderer.invoke('PostActivation', activationCode)
                if (status !== 'Illigal') {
                    commit('setStatus', status);
                    commit('setRequestCode', requestCode);
                }
                else {
                    dispatch('toast/toast', {
                        text: "Invalid activation code",
                        color: "error",
                        bottom: true
                    }, { root: true });
                }
            }
            catch {
                dispatch('toast/toast', {
                    text: "Invalid activation code",
                    color: "error",
                    bottom: true
                }, { root: true });
            }
        }
    }
}