export default new VueRouter({
    routes: [
        { path: '/', components: { nav: Vue.component('PageNav') } },
        { path: '/list/:repo/:id', components: { default: Vue.component('ListPage'), nav: Vue.component('PageNav') } },
        { path: '/singleton/:repo/:id', components: { default: Vue.component('SingletonPage'), nav: Vue.component('PageNav') } },
        { path: '/scripts/', components: { nav: Vue.component('ScriptsNav') } },
        { path: '/scripts/:repo/:id', components: { default: Vue.component('Scripts'), nav: Vue.component('ScriptsNav') } },
        { path: '/repositories/', components: { nav: Vue.component('RepositoryNav') } },
        { path: '/repositories/:id', components: { default: Vue.component('Repository'), nav: Vue.component('RepositoryNav') } },
    ]
})