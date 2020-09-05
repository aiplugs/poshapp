<template>
    <div class="main-content d-flex flex-column pa-10">
        <h1 class="d-flex align-center mb-10 grey--text">
            <img src="@/assets/icon/poshapp-icon-grey.png" width="48" />
            <span class="ma-4">POSH App</span>
        </h1>

        <h4>Documentation</h4>
        <p class="mb-10">
            <a v-on:click="openExternal('https://github.com/aiplugs/poshapp/wiki')">https://github.com/aiplugs/poshapp/wiki</a>
        </p>

        <h4>Release notes</h4>
        <p class="mb-10">
            <a v-on:click="openExternal('https://github.com/aiplugs/poshapp/releases')">https://github.com/aiplugs/poshapp/releases</a>
        </p>

        <h4>Search Issues</h4>
        <p class="mb-10">
            <a v-on:click="openExternal('https://github.com/aiplugs/poshapp/issues')">https://github.com/aiplugs/poshapp/issues</a>
        </p>
    </div>
</template>

<script>
export default {
    methods: {
        openExternal(url) {
            window.openExternal(url)
        }
    }
}
</script>