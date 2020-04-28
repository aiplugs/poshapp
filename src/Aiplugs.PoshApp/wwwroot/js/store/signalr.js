import { parsePSDataCollection } from '/js/clixml.js';
const signalr = new signalR.HubConnectionBuilder().withUrl("/poshapp").withAutomaticReconnect().build();
let init = signalr.start();

export default {
    namespaced: true,
    state: {
        connectionId: null,
        status: 'connecting',
        invoking: null,
        loadingParams: false,
        defaultResult: null,
        detailResult: null,
        parameters: [],
        logMessages: [],
        logErrors: null,
        progresses: {},
        prompt: null,
        promptForChoice: null,
        promptForCredential: null,
        gitClone: null,
        gitLog: null,
        gitLogMessage: null,
        gitStatus: []
    },
    mutations: {
        setInvoking(state, name = '$default') {
            state.invoking = name;
        },
        clearInvoking(state) {
            state.invoking = null;
        },
        setLoadingParams(state) {
            state.loadingParams = true;
        },
        clearLoadingParams(state) {
            state.loadingParams = false;
        },
        setDefaultResult(state, result) {
            state.defaultResult = result;
        },
        setDetailResult(state, result) {
            state.detailResult = result;
        },
        setParameters(state, parameters) {
            state.parameters.splice(0, state.parameters.length, ...parameters);
        },
        writeHost(state, { color, bgColor, text }) {
            if (state.logMessages.length > 30) {
                state.logMessages.shift();
            }
            state.logMessages.push({ color, bgColor, text });
        },
        writeWarning(state, { message }) {
            state.logMessages.push({
                color: 6,
                bgColor: 0,
                text: message
            });
        },
        writeError(state, { message }) {
            const text = message.replace(/\r\n/g, '\n').split('\n').filter(line => !line.match(/^\s*$/)).join('\n');
            if (state.logErrors !== null) {
                state.logErrors += '\n\n' + text;
            }
            else {
                state.logErrors = text;
            }
        },
        clearError(state) {
            state.logErrors = null;
        },
        writeDebug(state, { message }) {
            state.logMessages.push({
                color: 15,
                bgColor: 0,
                text: message
            });
        },
        writeVerbose(state, { message }) {
            state.logMessages.push({
                color: 15,
                bgColor: 0,
                text: message
            });
        },
        writeInformation(state, { }) {
            state.logMessages.push({
                color: 15,
                bgColor: 0,
                text: message
            });
        },
        writeProgress(state, { progress }) {
            Vue.set(state.progresses, progress.activityId.toString(), progress);
            if (progress.percentComplete === 100) {
                Vue.delete(state.progresses, progress.activityId);
            }
        },
        setPrompt(state, { caption, message, descriptions }) {
            state.prompt = { caption, message, descriptions };
        },
        clearPrompt(state) {
            state.prompt = null;
        },
        setPromptForChoice(state, { caption, message, choices, defaultChoice }) {
            state.promptForChoice = { caption, message, choices, defaultChoice };
        },
        clearPromptForChoice(state) {
            state.promptForChoice = null;
        },
        setPromptForCredential(state, { caption, message, userName, targetName, allowedCredentialTypes, forGit }) {
            state.promptForCredential = { caption, message, userName, targetName, allowedCredentialTypes, forGit };
        },
        clearPromptForCredential(state) {
            state.promptForCredential = null;
        },
        setGitProgress(state, { name, progress }) {
            state.gitClone = { name, progress };
        },
        setGitClone(state, { name }) {
            state.gitClone = null;
        },
        setGitCloneFaild(state, { name }) {
            state.gitClone = null;
        },
        setGitLog(state, { name, logs, origin, local }) {
            state.gitLog = { name, logs, origin, local };
            state.gitLogMessage = !origin ? 'This repository does not have remote origin/master branch.' : null;
        },
        clearGitLog(state) {
            state.gitLog = null;
            state.gitLogMessage = null;
        },
        setGitLogNotFound(state, { name }) {
            state.gitLogMessage = 'This repository have not initialized git.';
        },
        setGitFetchProgress(state, { name, progress }) {
            state.gitLogMessage = progress;
        },
        setGitStatus(state, { name, status }) {
            state.gitStatus.splice(0, state.gitStatus.length, ...status);
        },
        setStatus(state, { status, connectionId }) {
            state.status = status;
            state.connectionId = connectionId;
        }
    },
    actions: {
        clearResult({ commit }) {
            commit('setDefaultResult', null);
            commit('setDetailResult', null);
        },
        async invokeDefault({ commit }, { scriptId, input }) {
            commit('setInvoking');
            await signalr.invoke('Invoke', scriptId, input);
        },
        async invokeDetail({ commit },{ scriptId, input }) {
            commit('setInvoking');
            await signalr.invoke('InvokeDetail', scriptId, input);
        },
        async invokeAction({ commit }, { scriptId, input }) {
            commit('setInvoking', scriptId);
            await signalr.invoke('InvokeAction', scriptId, input);
        },
        async invokeGetParameters({ commit }, { scriptId }) {
            commit('setParameters', []);
            commit('setLoadingParams', []);
            await init;
            await signalr.invoke('GetParameters', scriptId);
        },
        async invokePrompt({ commit }, { input }) {
            commit('clearPrompt');
            await signalr.invoke('Prompt', input);
        },
        async invokePromptForChoice({ commit }, { index }) {
            commit('clearPromptForChoice');
            await signalr.invoke('PromptForChoice', index);
        },
        async invokePromptForCredential({ commit }, { username, password }) {
            commit('clearPromptForCredential');
            await signalr.invoke('PromptForCredential', username, password);
        },
        async invokePromptForGitCredential({ commit }, { username, password }) {
            commit('clearPromptForCredential');
            await signalr.invoke('PromptForGitCredential', username, password);
        },
        async invokeGitLog({ commit }, { repositoryName }) {
            commit('clearGitLog');
            await init;
            await signalr.invoke('GitLog', repositoryName);
        },
        async invokeGitForcePull(_, { repositoryName }) {
            await signalr.invoke('GitForcePull', repositoryName);
        },
        async invokeGitReset(_, { repositoryName }) {
            await signalr.invoke('GitReset', repositoryName);
        },
        async reconnect({ commit }) {
            if (signalr.state === 'Disconnected') {
                init = signalr.start();
            }
            else if (signalr.state === 'Connected') {
                commit('signalr/setStatus', { status: 'connected', connectionId: signalr.connectionId });
            }
        }
    }
};

export function signalRPlugin(store) {
    signalr.on('UnitResult', () => {
        store.commit('signalr/clearInvoking');
    });
    signalr.on('DefaultResult', json => {
        store.commit('signalr/clearInvoking');
        store.commit('signalr/setDefaultResult', parsePSDataCollection(JSON.parse(json)));
    });
    signalr.on('DetailResult', json => {
        store.commit('signalr/clearInvoking');
        store.commit('signalr/setDetailResult', parsePSDataCollection(JSON.parse(json)));
    });
    signalr.on('ActionResult', (id, json) => {
        store.commit('signalr/clearInvoking');
        store.dispatch('toast/toast', {
            text: `${id} is succeeded`,
            color: "info",
            top: true,
            right: true
        });
    });
    signalr.on('GetParameters', parameters => {
        store.commit('signalr/clearLoadingParams');
        store.commit('signalr/setParameters', parameters);
    });
    signalr.on('WriteWithColor', (color, bgColor, text) => {
        store.commit('signalr/writeHost', { color, bgColor, text });
    });
    signalr.on('WriteWarningLine', message => {
        store.commit('signalr/writeWarning', { message });
    });
    signalr.on('WriteErrorLine', message => {
        store.commit('signalr/writeError', { message });
    });
    signalr.on('WriteDebugLine', message => {
        store.commit('signalr/writeDebug', { message });
    });
    signalr.on('WriteVerboseLine', message => {
        store.commit('signalr/writeVerbose', { message });
    });
    signalr.on('WriteProgress', (sourceId, progress) => {
        store.commit('signalr/writeProgress', { progress });
    });
    signalr.on('ParseError', message => {
        store.commit('signalr/writeError', { message });
    });
    signalr.on('Prompt', (caption, message, descriptions) => {
        store.commit('signalr/setPrompt', { caption, message, descriptions });
    });
    signalr.on('PromptForChoice', (caption, message, choices, defaultChoice) => {
        store.commit('signalr/setPromptForChoice', { caption, message, choices, defaultChoice });
    });
    signalr.on('PromptForCredential', (caption, message, userName, targetName) => {
        store.commit('signalr/setPromptForCredential', { caption, message, userName, targetName, allowedCredentialTypes: null, forGit: false });
    });
    signalr.on('PromptForCredentialWithType', (caption, message, userName, targetName, allowedCredentialTypes) => {
        store.commit('signalr/setPromptForCredential', { caption, message, userName, targetName, allowedCredentialTypes, forGit: false });
    });
    signalr.on('PromptForGitCredential', (url, usernameFromUrl) => {
        store.commit('signalr/setPromptForCredential', { caption: url, message: url, userName: usernameFromUrl, targetName: null, allowedCredentialTypes: null, forGit: true });
    });
    signalr.on('GitProgress', (name, progress) => {
        store.commit('signalr/setGitProgress', { name, progress });
    });
    signalr.on('GitClone', name => {
        store.commit('signalr/setGitClone', { name });
    });
    signalr.on('GitCloneFaild', name => {
        store.commit('signalr/setGitCloneFaild', { name });
    });
    signalr.on('GitLog', (name, logs, origin, local) => {
        store.commit('signalr/setGitLog', { name, logs, origin, local });
    });
    signalr.on('GitLogNotFound', name => {
        store.commit('signalr/setGitLogNotFound', { name });
    });
    signalr.on('GitFetchProgress', (name, progress) => {
        store.commit('signalr/setGitFetchProgress', { name, progress });
    });
    signalr.on('GitStatus', (name, status) => {
        store.commit('signalr/setGitStatus', { name, status });
    });

    signalr.onclose(err => {
        store.commit('signalr/setStatus', { status: 'close' });
    });
    signalr.onreconnecting(err => {
        store.commit('signalr/setStatus', { status: 'reconnecting' });
    });
    signalr.onreconnected(connectedId => {
        store.commit('signalr/setStatus', { status: 'connected', connectionId: signalr.connectionId });
    });
    init.then(() => {
        store.commit('signalr/setStatus', { status: 'connected', connectionId: signalr.connectionId });
    }).catch(() => {
        store.commit('signalr/setStatus', { status: 'faild' });
    });
}