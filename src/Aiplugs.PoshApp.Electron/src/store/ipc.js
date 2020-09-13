import Vue from 'vue'
import { parsePSDataCollection } from '../clixml.js';
export default {
    namespaced: true,
    state: {
        updateDownloading: null,
        updateAvailable: false,
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
        writeInformation(state, { message }) {
            state.logMessages.push({
                color: 15,
                bgColor: 0,
                text: message
            });
        },
        writeProgress(state, { progress }) {
            Vue.set(state.progresses, progress.activity.toString(), progress);
            if (progress.percent === 100) {
                Vue.delete(state.progresses, progress.activity);
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
        setGitProgress(state, { progress }) {
            state.gitClone = { progress };
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
        setGitLogNotFound(state) {
            state.gitLogMessage = 'This repository have not initialized git.';
        },
        setGitFetchProgress(state, { progress }) {
            state.gitLogMessage = progress;
        },
        setGitStatus(state, { status }) {
            state.gitStatus.splice(0, state.gitStatus.length, ...status);
        },
        clearGitStatus(state) {
            state.gitStatus.splice(0);
        }
    },
    actions: {
        quitAndInstall() {
            window.ipcRenderer.send('QuitAndInstall');
        },
        clearResult({ commit }) {
            commit('setDefaultResult', null);
            commit('setDetailResult', null);
        },
        async invokeDefault({ commit }, { scriptId, input }) {
            commit('setInvoking');
            const data = await window.ipcRenderer.invoke('InvokeWithParameters', scriptId, input);
            commit('clearInvoking');
            commit('setDefaultResult', parsePSDataCollection(data));
        },
        async invokeDetail({ commit },{ scriptId, input }) {
            commit('setInvoking');
            const data = await window.ipcRenderer.invoke('InvokeWithPipeline', scriptId, input);
            commit('clearInvoking');
            commit('setDetailResult', parsePSDataCollection(data));
        },
        async invokeAction({ commit, dispatch }, { scriptId, input }) {
            commit('setInvoking', scriptId);
            await window.ipcRenderer.invoke('InvokeWithPipelines', scriptId, input);
            commit('clearInvoking');
            dispatch('toast/toast', {
                text: `${scriptId} is succeeded`,
                color: "info",
                top: true,
                right: true
            }, { root: true });
        },
        async invokeGetParameters({ commit }, { scriptId }) {
            commit('setParameters', []);
            commit('setLoadingParams', []);
            const parameters = await window.ipcRenderer.invoke('InvokeGetParameters', scriptId);
            commit('clearLoadingParams');
            commit('setParameters', parameters);
        },
        async invokePrompt({ commit }, { input }) {
            commit('clearPrompt');
            await window.ipcRenderer.send('Prompt', input);
        },
        async invokePromptForChoice({ commit }, { index }) {
            commit('clearPromptForChoice');
            await window.ipcRenderer.send('PromptForChoice', index);
        },
        async invokePromptForCredential({ commit }, { username, password }) {
            commit('clearPromptForCredential');
            await window.ipcRenderer.send('PromptForCredential', username, password);
        },
        async invokePromptForGitCredential({ commit }, { username, password }) {
            commit('clearPromptForCredential');
            await window.ipcRenderer.send('PromptForGitCredential', username, password);
        },
        async invokeGitLog({ commit }, { name, path }) {
            commit('clearGitLog');
            const data = await window.ipcRenderer.invoke('GitLog', path);
            if (data != null) {
                const { logs, origin, local } = data;
                commit('setGitLog', { name, logs, origin, local });
            }
            else {
                commit('setGitLogNotFound');
            }
        },
        async invokeGitStatus({ commit }, { path }) {
            commit('clearGitStatus');
            const status = await window.ipcRenderer.invoke('GitStatus', path);
            if (status != null) {
                commit('setGitStatus', { status });
            }
        },
        async invokeGitFetch({ commit }, { path }) {
            await window.ipcRenderer.invoke('GitFetch', path);
        },
        async invokeGitForcePull({ dispatch }, payload) {
            await dispatch('invokeGitFetch', payload);
            await dispatch('invokeGitReset', payload);
            await dispatch('invokeGitLog', payload);
        },
        async invokeGitReset(_, { path }) {
            await window.ipcRenderer.invoke('GitReset', path);
        },
        async invokeGitClone(_, { path, origin }) {
            await window.ipcRenderer.invoke('GitClone', origin, path);
        },
    }
};

export function ipcPlugin(store) {
    window.ipcRenderer.on('UpdateDownloading', (sender, progress) => {
        store.commit('ipc/setUpdateDownloading', progress);
        if (progress == 100) {
            store.commit('ipc/setUpdateAvailable');
        }
    });
    window.ipcRenderer.on('UpdateAvailable', sender => {
        store.commit('ipc/setUpdateAvailable');
    });
    window.ipcRenderer.on('WriteWithColor', (event, color, bgColor, text) => {
        store.commit('ipc/writeHost', { color, bgColor, text });
    });
    window.ipcRenderer.on('WriteWarningLine', (event, message) => {
        store.commit('ipc/writeWarning', { message });
    });
    window.ipcRenderer.on('WriteErrorLine', (event, message) => {
        store.commit('ipc/writeError', { message });
    });
    window.ipcRenderer.on('WriteDebugLine', (event, message) => {
        store.commit('ipc/writeDebug', { message });
    });
    window.ipcRenderer.on('WriteVerboseLine', (event, message) => {
        store.commit('ipc/writeVerbose', { message });
    });
    window.ipcRenderer.on('WriteProgress', (event, sourceId, progress) => {
        store.commit('ipc/writeProgress', { progress });
    });
    window.ipcRenderer.on('ParseError', (event, message) => {
        store.commit('ipc/writeError', { message });
    });
    window.ipcRenderer.on('Prompt', (event, caption, message, descriptions) => {
        store.commit('ipc/setPrompt', { caption, message, descriptions });
    });
    window.ipcRenderer.on('PromptForChoice', (event, caption, message, choices, defaultChoice) => {
        store.commit('ipc/setPromptForChoice', { caption, message, choices, defaultChoice });
    });
    window.ipcRenderer.on('PromptForCredential', (event, caption, message, userName, targetName) => {
        store.commit('ipc/setPromptForCredential', { caption, message, userName, targetName, allowedCredentialTypes: null, forGit: false });
    });
    window.ipcRenderer.on('PromptForCredentialWithType', (event, caption, message, userName, targetName, allowedCredentialTypes) => {
        store.commit('ipc/setPromptForCredential', { caption, message, userName, targetName, allowedCredentialTypes, forGit: false });
    });
    window.ipcRenderer.on('PromptForGitCredential', (event, url, userName) => {
        store.commit('ipc/setPromptForCredential', { caption: url, message: url, userName, targetName: null, allowedCredentialTypes: null, forGit: true });
    });
    window.ipcRenderer.on('GitProgress', (event, progress) => {
        store.commit('ipc/setGitProgress', { progress });
    });
}
