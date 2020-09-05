export default {
    namespaced: true,
    state: {
        repositories: []
    },
    getters: {
        find(state) {
            return name => state.repositories.find(repo => repo.name === name);
        }
    },
    mutations: {
        replaceRepositories(state, repositories) {
            state.repositories.splice(0, state.repositories.length, ...repositories);
        },
        addRepository(state, repository) {
            state.repositories.push(repository);
        },
        removeRepository(state, repository) {
            const index = state.repositories.findIndex(repo => repo.name === repository.name);
            state.repositories.splice(index, 1);
        },
        replaceRepository(state, repository) {
            const { id, path, origin } = repository;
            let { name } = repository;
            if (id) {
                name = id;
            }
            const index = state.repositories.findIndex(repo => repo.name === name);
            state.repositories.splice(index, 1, { name, path, origin });
        }
    },
    actions: {
        async loadRepositories({ commit }) {
            const data = await window.ipcRenderer.invoke('GetRepositories');
            commit('replaceRepositories', data);
        },
        async createRepository({ commit, dispatch }, repository) {
            const {name, path, origin} = repository;
            const result = await window.ipcRenderer.invoke('CreateRepository', name, path);
            if (result == 200) {
                commit('addRepository', repository);
                if (origin) {
                    await dispatch('signalr/invokeGitClone', repository, { root: true });
                }
                return true;
            }
            return false;
        },
        async updateRepository({ commit }, payload) {
            // const { id, name, path, origin, connectionId } = payload;
            // const data = { name, path, origin, connectionId };
            // const url = `/api/repositories/${id}`;
            // const body = JSON.stringify(data);
            // const response = await fetch(url, { method: 'post', headers: { 'Content-Type': 'application/json' }, body });

            // if (response.ok) {
            //     commit('replaceScript', payload);
            //     return true;
            // }

            return false;
        },
        async deleteRepository({ commit }, repository) {
            const result = await window.ipcRenderer.invoke('DeleteRepository', repository.name);
            
            if (result == 204) {
                commit('removeRepository', repository);
                return true;
            }

            return false;
        }
    }
};