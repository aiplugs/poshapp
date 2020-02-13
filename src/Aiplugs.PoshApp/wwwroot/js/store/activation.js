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
            const response = await fetch('/api/activation');
            if (response.ok) {
                var data = await response.json();
                commit('setStatus', data.status);
                commit('setRequestCode', data.requestCode);
            }
        },
        async refleshActivationCode({ commit, dispatch }) {
            const response = await fetch('/api/reflesh', {
                method: 'post'
            });
            if (response.ok) {
                const data = await response.json();
                if (data.status !== 'Illigal') {
                    commit('setStatus', data.status);
                    commit('setRequestCode', data.requestCode);
                }
                else {
                    dispatch('toast/toast', {
                        text: "Invalid activation code",
                        color: "error",
                        bottom: true
                    }, { root: true });
                }
            }
            else {
                dispatch('toast/toast', {
                    text: "Invalid activation code",
                    color: "error",
                    bottom: true
                }, { root: true });
            }
        },
        async activate({ commit, dispatch }, activationCode) {s
            const response = await fetch('/api/activation', {
                method: 'post',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ activationCode })
            }); 
            if (response.ok) {
                var data = await response.json();
                if (data.status !== 'Illigal') {
                    commit('setStatus', data.status);
                    commit('setRequestCode', data.requestCode);
                }
                else {
                    dispatch('toast/toast', {
                        text: "Invalid activation code",
                        color: "error",
                        bottom: true
                    }, { root: true });
                }
            }
            else {
                dispatch('toast/toast', {
                    text: "Activation Code is required",
                    color: "error",
                    bottom: true
                }, { root: true });
            }
        }
    }
}