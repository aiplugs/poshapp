function replaceData(state, data) {
    state.data.splice(0);
    for (const d of data) {
        if (typeof d.value === 'object') {
            d.value.$clixml = d.clixml;
            state.data.push(d.value);
        }
    }
}

export default {
    namespaced: true,
    state: {
        title: '',
        selected: [],
        headers: [],
        data: [],
        page: 0,
        pageCount: 1,
        pageSize: 20,
        detail: null
    },
    mutations: {
        replaceData,

        updateList(state, payload) {
            const {headers, title, page, pageCount, pageSize, data} = payload;
            state.title = title
            state.page = page
            state.pageCount = pageCount
            state.pageSize = pageSize
            state.headers.splice(0, state.headers.length, ...headers)
            replaceData(state, data)
        },

        setDetail(state, detail) {
            state.detail = detail;
        } 
    }
}