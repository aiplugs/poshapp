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
            const data = await window.getRepositories();
            commit('replaceRepositories', data);
        },
        async createRepository({ commit, dispatch }, repository) {
            const {name, path, origin} = repository;
            const result = await window.createRepository(name, path);
            if (result == 200) {
                commit('addRepository', repository);
                if (origin) {
                    await dispatch('ipc/invokeGitClone', repository, { root: true });
                }
                return true;
            }
            return false;
        },
        async updateRepository({ commit }, payload) {
            return false;
        },
        async deleteRepository({ commit }, repository) {
            const result = await window.deleteRepository(repository.name);
            
            if (result == 204) {
                commit('removeRepository', repository);
                return true;
            }

            return false;
        }
    }
};