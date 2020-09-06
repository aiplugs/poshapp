<template id="powershell-ui-progress-for-git-component">
    <span class="d-flex align-center" v-if="gitClone != null">
        {{gitClone.progress}}
    </span>
</template>
<script>
import {mapState} from 'vuex'
export default {
    computed: {
        ...mapState('signalr', ['gitClone'])
    }
}
</script>