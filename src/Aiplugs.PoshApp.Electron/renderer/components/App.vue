<template>
    <div>
        <h1>Hello World!</h1>
        We are using Node.js {{nodeVersion}},
        Chromium {{chromeVersion}},
        and Electron {{electronVersion}}.
    </div>
</template>

<script>
export default {
    data() {
        return {
            nodeVersion: window.versions.node,
            chromeVersion: window.versions.chrome,
            electronVersion: window.versions.electron,
        }
    }
}
</script>