<template>
    <span class="d-flex align-center" v-if="updateDownloading">
        Downloading update... {{progress}}%
    </span>
</template>
<script>
import {mapState} from 'vuex'
export default {
    computed: {
        ...mapState('ipc', ['updateDownloading']),
        progress() {
            if (!this.updateDownloading)
                return null;
            
            const percent = this.updateDownloading.toString();

            const [head, tail] = percent.split('.');

            return head + '.' + (tail || '0').slice(0, 1);
        }
    }
}
</script>