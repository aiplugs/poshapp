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
        replaceRepository(state, repository) {
            const { id, path, origin } = repository;
            let { name } = repository;
            if (id) {
                name = id;
            }
            const index = state.metadata.findIndex(d => d.name === name);
            state.repositories.splice(index, 1, { name, path, origin });
        },
    },
    actions: {
        async loadRepositories({commit}) {
            const response = await fetch('/api/repositories');
            if (response.ok)  {
                const data = await response.json();
                commit('replaceRepositories', data);
            }
        },
        async createRepository({commit}, payload) {
            const { name, path, origin, connectionId } = payload;
            const data = { name, path, origin, connectionId };
            const url = `/api/repositories`;
            const body = JSON.stringify(data);
            const response = await fetch(url, { method: 'post', headers: { 'Content-Type': 'application/json' }, body });
            
            if (response.ok) {
                commit('addRepository', payload);
                return true;
            }

            return false;
        },
        async updateRepository({commit}, payload) {
            const { id, name, path, origin, connectionId } = payload;
            const data = { name, path, origin, connectionId };
            const url = `/api/repositories/${id}`;
            const body = JSON.stringify(data);
            const response = await fetch(url, { method: 'post', headers: { 'Content-Type': 'application/json' }, body });
            
            if (response.ok) {
                commit('replaceScript', payload);
                return true;
            }

            return false;
        }
    }
}