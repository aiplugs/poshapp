import "vuetify/dist/vuetify.min.css";
import '@mdi/font/css/materialdesignicons.css'
import Vue from 'vue'
import Vuetify from 'vuetify'
import VueRouter from 'vue-router'
import App from './App.vue'
import Index from './components/Index.vue'
import AppNav from './components/AppNav.vue'
import AppList from './components/AppList.vue'
import AppSingleton from './components/AppSingleton.vue'
import ScriptNav from './components/ScriptNav.vue'
import ScriptIndex from './components/ScriptIndex.vue'
import RepositoryNav from './components/RepositoryNav.vue'
import RepositoryIndex from './components/RepositoryIndex.vue'
import SettingsNav from './components/SettingsNav.vue'
import SettingsVersion from './components/SettingsVersion.vue'
import createStore from './store'

Vue.use(Vuetify)
Vue.use(VueRouter)

Vue.config.productionTip = false

const router = new VueRouter({
  routes: [
    { path: '/', components: { default: Index, nav: AppNav } },
    { path: '/list/:repo/:id', components: { default: AppList, nav: AppNav } },
    { path: '/singleton/:repo/:id', components: { default: AppSingleton, nav: AppNav } },
    { path: '/scripts/', components: { nav: ScriptNav } },
    { path: '/scripts/:repo/:id', components: { default: ScriptIndex, nav: ScriptNav } },
    { path: '/repositories/', components: { nav: RepositoryNav } },
    { path: '/repositories/:id', components: { default: RepositoryIndex, nav: RepositoryNav } },
    { path: '/settings/', components: { nav: SettingsNav } },
    { path: '/settings/version', components: { default: SettingsVersion, nav: SettingsNav } },
  ]
})

new Vue({
  store: createStore(),
  router,
  vuetify: new Vuetify({theme: { dark: window.matchMedia('(prefers-color-scheme: dark)').matches }}),
  render: h => h(App),
}).$mount('#app')

Vue.config.errorHandler = (err, vm, info) => {
  console.error(err, vm, info);
};
window.addEventListener("error", event => {
  console.error(event.error);
});
