<template>
    <v-snackbar :value="display" 
                :color="color" 
                :top="top" 
                :bottom="bottom" 
                :left="left" 
                :right="right">
        {{ text }}
    </v-snackbar>
</template>

<script>
import { mapState } from 'vuex'
export default {
    computed: {
        ...mapState('toast', ['display', 'text', 'color', 'top', 'bottom', 'left', 'right']),            
    },
}
</script>