
/**
 * metadata item
 * 
 * id: string
 * type: string := List | Detail | Singleton | Action
 * detail: string
 * actions: Array<string>
 * group: string
 */

function selectData(script) {
    const { id, type, displayName } = script;
    let data = null;
    if (type === 'List') {
        const { group, detail, actions } = script;
        data = { id, type, displayName, group, detail, actions };
    }
    else if (type === 'Detail') {
        const { actions } = script;
        data = { id, type, actions };
    }
    else if (type === 'Singleton') {
        const { group, actions } = script;
        data = { id, type, displayName, group, actions };
    }
    else if (type === 'Action') {
        data = { id, type, displayName };
    }
    return data;
}

export default {
    namespaced: true,
    state: {
        metadata: {}
    },
    getters: {
        navScriptTree(state) {
            return Object.keys(state.metadata).reduce((o, key) => {
                const tree = [];
                const groups = {};
                const navScripts = state.metadata[key].filter(d => d.type === 'List' || d.type === 'Singleton');
                for (let script of navScripts) {
                    if (script.group) {
                        if (!groups[script.group])
                            groups[script.group] = [];
                        groups[script.group].push({ type: "item", name: script.id });
                    }
                    else {
                        tree.push({ type: "item", name: script.id, displayName: script.displayName });
                    }
                }
                for (let key of Object.keys(groups)) {
                    tree.push({ type: 'group', name: key, children: groups[key] });
                }
                o[key] = tree;
                return o;
            }, {});
        },
        repositories(state) {
            return Object.keys(state.metadata);
        },
        scripts(state) {
            return Object.keys(state.metadata).map(key => state.metadata[key]).flat();
        },
        actionScripts(state) {
            return Object.keys(state.metadata).map(key => state.metadata[key].filter(script => script.type === 'Action')).flat();
        },
        detailScripts(state) {
            return Object.keys(state.metadata).map(key => state.metadata[key].filter(script => script.type === 'Detail')).flat();
        },
        find(state) {
            return (repository, id) => (state.metadata[repository] || []).find(d => d.repository === repository && d.id === id);
        },
        findActions(state) {
            return (repositoryName, scriptId) => {
                const script = (state.metadata[repositoryName] || []).find(d => d.id === scriptId);
                if (!script)
                    return [];

                return script.actions.map(id => {
                    const [repositoryName, scriptId] = id.split(':');
                    const action = (state.metadata[repositoryName] || []).find(script => script.repository === repositoryName && script.id === scriptId);
                    if (!action)
                        return null;
                    return { id: `${action.repository}:${action.id}`, displayName: action.displayName };
                });
            };
        }
    },
    mutations: {
        addScript(state, script) {
            const data = selectData(script);
            data.repository = script.repository;
            state.metadata[data.repository].push(data);
        },
        removeScript(state, script) {
            const { repository, id } = script;
            const index = state.metadata[repository].findIndex(d => d.id === id);
            state.metadata[script.repository].splice(index, 1);
        },
        replaceScript(state, script) {
            let { id } = script;
            if (script['@id']) {
                id = script['@id'];
            }
            const index = state.metadata[script.repository].findIndex(d => d.id === id);
            const data = selectData(script);
            data.repository = script.repository;
            state.metadata[data.repository].splice(index, 1, data);
        },
        replaceAllScripts(state, scripts) {
            for (let key of Object.keys(scripts)) {
                for (let script of scripts[key]) {
                    script.repository = key;
                }
            }
            Vue.set(state, 'metadata', scripts);
        }
    },
    actions: {
        async loadScripts(context) {
            const response = await fetch('/api/scripts/');
            if (response.ok) {
                const scripts = await response.json();
                context.commit('replaceAllScripts', scripts);
            }
        },
        async updateScript(context, script) {
            const type = script.type;
            const data = selectData(script);
            const url = `/api/repositories/${script.repository}/scripts@${type.toLowerCase()}/${script['@id']}`;
            const body = JSON.stringify(data);
            const response = await fetch(url, { method: 'put', headers: { 'Content-Type': 'application/json' }, body });
            if (response.ok) {
                context.commit('replaceScript', script);
                return true;
            }
            return false;
        },
        async createScript(context, script) {
            const type = script.type;
            const data = selectData(script);
            const url = `/api/repositories/${script.repository}/scripts@${type.toLowerCase()}`;
            const body = JSON.stringify(data);
            const response = await fetch(url, { method: 'post', headers: { 'Content-Type': 'application/json' }, body });
            if (response.ok) {
                context.commit('addScript', script);
                return true;
            }
            return false;
        },
        async deleteScript(context, script) {
            const url = `/api/repositories/${script.repository}/scripts/${script.id}`;
            const response = await fetch(url, { method: 'delete' });
            if (response.ok) {
                context.commit('removeScript', script);
                return true;
            }
            return false;
        }
    }
};