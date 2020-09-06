<template>
    <div class="main-content overflow-y-auto no-scrollbar pa-8">
        version {{app}} <br>
        <small>- electron {{electron}}</small> <br>
        <small>- chrome {{chrome}}</small> <br>
        <small>- node {{node}}</small>
    </div>
</template>
<script>
export default {
    data() {
        return {
            app: window.versions.app,
            node: window.versions.node,
            chrome: window.versions.chrome,
            electron: window.versions.electron,
        }
    }
}
</script>