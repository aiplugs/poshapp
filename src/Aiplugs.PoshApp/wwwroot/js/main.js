import store from './store/index.js';
import router from './router.js';

Vue.prototype.$signalr = new signalR.HubConnectionBuilder().withUrl("/poshapp").withAutomaticReconnect().build();
Vue.prototype.$signalr.onconnected = function (callback) {
    const f = () => {
        if (this.state === 'Connected') { callback(); }
        else { setTimeout(f, 10); }
    }; f();
};

const app = new Vue({
    el: '#app',
    store,
    router,
    vuetify: new Vuetify()
});