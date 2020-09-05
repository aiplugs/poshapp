export default {
    namespaced: true,
    state: {
        display: false,
        text: null,
        color: 'info',
        top: true,
        bottom: false,
        left: false,
        right: true
    },
    mutations: {
        replaceToast(state, payload) {
            state.text = payload.text;
            state.color = payload.color;
            state.display = payload.display;
            state.top = payload.top;
            state.bottom = payload.bottomv;
            state.left = payload.left;
            state.right = payload.right;
        }
    },
    actions: {
        toast(context, payload) {
            context.commit('replaceToast', {
                ...payload,
                display: true
            });
            setTimeout(() => {
                context.commit('replaceToast', {
                    ...payload,
                    display: false
                });
            }, payload.timeout || 800);
        }
    }
}