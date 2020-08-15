import Vue from 'vue'
import App from './components/App'

window.Vue = Vue;
Vue.config.devtools = true

new Vue({
    render: h => h(App),
}).$mount('#app');