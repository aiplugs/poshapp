
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
        data = { id, type, displayName, group, detail, actions: [...actions] };
    }
    else if (type === 'Detail') {
        const { actions } = script;
        data = { id, type, actions: [...actions] };
    }
    else if (type === 'Singleton') {
        const { group, actions } = script;
        data = { id, type, displayName, group, actions: [...actions] };
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
                        groups[script.group].push({ type: "item", name: script.id, displayName: script.displayName });
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
        actionNames(state, getters) {
            return repositoryName => getters.actionScripts.map(script => script.repository === repositoryName ? script.id : `${script.repository}:${script.id}`);
        },
        detailNames(state, getters) {
            return repositoryName => getters.detailScripts.map(script => script.repository === repositoryName ? script.id : `${script.repository}:${script.id}`);
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
                    const [_repositoryName, _scriptId] = id.indexOf(":") >= 0 ? id.split(':') : [repositoryName, id];
                    const action = (state.metadata[_repositoryName] || []).find(script => script.repository === _repositoryName && script.id === _scriptId);
                    if (!action)
                        return null;
                    return { id: `${action.repository}:${action.id}`, displayName: action.displayName };
                });
            };
        },
        findDetail(state) {
            return (repositoryName, scriptId) => {
                const script = (state.metadata[repositoryName] || []).find(d => d.id === scriptId);
                if (!script || !script.detail)
                    return null;

                let repo = repositoryName, id = script.detail;
                if (script.detail.indexOf(':') >= 0) {
                    [repo, id] = script.detail.split(':');
                }
                return `${repo}:${id}`;
            };
        },
        findPageByDetail(state) {
            return (repo, id) => {
                const key = `${repo}:${id}`;
                const domestics = state.metadata[repo].filter(s => s.type === 'List' && s.detail === id);
                const others = Object.keys(state.metadata).map(key => state.metadata[key]).flat().filter(s => s.type === 'List' && s.detail === key);
                return domestics.concat(others)[0];
            };
        },
        findPageByAction(state) {
            return (repo, id) => {
                const key = `${repo}:${id}`;
                const domestics = state.metadata[repo].filter(s => s.actions && s.actions.includes(id));
                const others = Object.keys(state.metadata).map(key => state.metadata[key]).flat().filter(s => s.actions && s.actions.includes(key));
                return domestics.concat(others)[0];
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