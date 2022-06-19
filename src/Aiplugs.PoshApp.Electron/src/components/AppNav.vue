<template>
    <section>
        <v-list class="grow">
            <v-list-group v-for="(repository,index) in repositories" :key="repository" :value="index===0">
                <template v-slot:activator>
                    <v-list-item-content>
                        <v-list-item-title>📁 {{repository}}</v-list-item-title>
                    </v-list-item-content>
                </template>
                <component :is="item.type=='item' ? 'v-list-item':'v-list-group'"
                        v-bind="item.type=='item' ? {to:href(repository, item.name)} :{'sub-group':true}"
                        v-for="item in navScriptTree[repository]" :key="item.name">
                    <v-list-item-content v-if="item.type=='item'">   
                        <v-list-item-title>{{item.displayName||item.name}}</v-list-item-title>
                    </v-list-item-content>
                    <template v-slot:activator v-if="item.type=='group'">
                        <v-list-item-content>
                            <v-list-item-title>{{item.name}}</v-list-item-title>
                        </v-list-item-content>
                    </template>
                    <template v-if="item.type=='group'">
                    <v-list-item :to="href(repository, child.name)" 
                                v-for="child in item.children" :key="child.name"
                               >
                        <v-list-item-content>
                            <v-list-item-title class="pl-4">{{child.displayName||child.name}}</v-list-item-title>
                        </v-list-item-content>
                    </v-list-item>
                    </template>
                </component>
            </v-list-group>
        </v-list>
    </section>
</template>
<script>
import {mapGetters, mapActions} from 'vuex'
export default {
    computed: {
        ...mapGetters('scripts', ['repositories', 'navScriptTree'])
    },
    methods: {
        ...mapActions('scripts', ['loadScripts']),
        href(repository, scriptName) {
            const data = this.$store.getters['scripts/find'](repository, scriptName);
            return `/${data.type.toLowerCase()}/${repository}/${scriptName}`;
        }
    },
    mounted() {
        this.loadScripts();    
    }
}
</script>