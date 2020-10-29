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
            window.quitAndInstall()
        },
        clearResult({ commit }) {
            commit('setDefaultResult', null);
            commit('setDetailResult', null);
        },
        async invokeDefault({ commit }, { scriptId, input }) {
            commit('setInvoking');
            const data = await window.invokeWithParameters(scriptId, input);
            commit('clearInvoking');
            commit('setDefaultResult', parsePSDataCollection(data));
        },
        async invokeDetail({ commit },{ scriptId, input }) {
            commit('setInvoking');
            const data = await window.invokeWithPipeline(scriptId, input);
            commit('clearInvoking');
            commit('setDetailResult', parsePSDataCollection(data));
        },
        async invokeAction({ commit, dispatch }, { scriptId, input }) {
            commit('setInvoking', scriptId);
            await window.invokeWithPipelines(scriptId, input);
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
            const parameters = await window.invokeGetParameters(scriptId);
            commit('clearLoadingParams');
            commit('setParameters', parameters);
        },
        async invokePrompt({ commit }, { input }) {
            commit('clearPrompt');
            await window.sendPrompt(input);
        },
        async invokePromptForChoice({ commit }, { index }) {
            commit('clearPromptForChoice');
            await window.sendPromptForChoice(index);
        },
        async invokePromptForCredential({ commit }, { username, password }) {
            commit('clearPromptForCredential');
            await window.sendPromptForCredential(username, password);
        },
        async invokePromptForGitCredential({ commit }, { username, password }) {
            commit('clearPromptForCredential');
            await window.sendPromptForGitCredential(username, password);
        },
        async invokeGitLog({ commit }, { path, name }) {
            commit('clearGitLog');
            const data = await window.getGitLog(path, name);
            if (data != null) {
                const { logs, origin, local } = data;
                commit('setGitLog', { name, logs, origin, local });
            }
            else {
                commit('setGitLogNotFound');
            }
        },
        async invokeGitStatus({ commit }, { path, name }) {
            commit('clearGitStatus');
            const status = await window.getGitStatus(path, name);
            if (status != null) {
                commit('setGitStatus', { status });
            }
        },
        async invokeGitFetch({ commit }, { path, name }) {
            await window.fetchGit(path, name);
        },
        async invokeGitForcePull({ dispatch }, payload) {
            await dispatch('invokeGitFetch', payload);
            await dispatch('invokeGitReset', payload);
            await dispatch('invokeGitLog', payload);
        },
        async invokeGitReset(_, { path, name }) {
            await window.resetGit(path);
        },
        async invokeGitClone({dispatch}, { path, origin, name }) {
            await window.cloneGit(origin, path, name);
            dispatch('invokeGitLog', { path, name });
            dispatch('invokeGitStatus',  { path, name });
        },
    }
};

export function ipcPlugin(store) {
    window.ipcOn('UpdateDownloading', (sender, progress) => {
        store.commit('ipc/setUpdateDownloading', progress);
        if (progress == 100) {
            store.commit('ipc/setUpdateAvailable');
        }
    });
    window.ipcOn('UpdateAvailable', sender => {
        store.commit('ipc/setUpdateAvailable');
    });
    window.ipcOn('WriteWithColor', (event, color, bgColor, text) => {
        store.commit('ipc/writeHost', { color, bgColor, text });
    });
    window.ipcOn('WriteWarningLine', (event, message) => {
        store.commit('ipc/writeWarning', { message });
    });
    window.ipcOn('WriteErrorLine', (event, message) => {
        store.commit('ipc/writeError', { message });
    });
    window.ipcOn('WriteDebugLine', (event, message) => {
        store.commit('ipc/writeDebug', { message });
    });
    window.ipcOn('WriteVerboseLine', (event, message) => {
        store.commit('ipc/writeVerbose', { message });
    });
    window.ipcOn('WriteProgress', (event, sourceId, progress) => {
        store.commit('ipc/writeProgress', { progress });
    });
    window.ipcOn('ParseError', (event, message) => {
        store.commit('ipc/writeError', { message });
    });
    window.ipcOn('Prompt', (event, caption, message, descriptions) => {
        store.commit('ipc/setPrompt', { caption, message, descriptions });
    });
    window.ipcOn('PromptForChoice', (event, caption, message, choices, defaultChoice) => {
        store.commit('ipc/setPromptForChoice', { caption, message, choices, defaultChoice });
    });
    window.ipcOn('PromptForCredential', (event, caption, message, userName, targetName) => {
        store.commit('ipc/setPromptForCredential', { caption, message, userName, targetName, allowedCredentialTypes: null, forGit: false });
    });
    window.ipcOn('PromptForCredentialWithType', (event, caption, message, userName, targetName, allowedCredentialTypes) => {
        store.commit('ipc/setPromptForCredential', { caption, message, userName, targetName, allowedCredentialTypes, forGit: false });
    });
    window.ipcOn('PromptForGitCredential', (event, url, userName) => {
        store.commit('ipc/setPromptForCredential', { caption: url, message: url, userName, targetName: null, allowedCredentialTypes: null, forGit: true });
    });
    window.ipcOn('GitProgress', (event, progress) => {
        store.commit('ipc/setGitProgress', { progress });
    });
}
