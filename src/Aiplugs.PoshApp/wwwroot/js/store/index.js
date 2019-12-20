import list from './list.js'
import toast from './toast.js'
import scripts from './scripts.js'
import repositories from './repositories.js'

export default new Vuex.Store({
    modules: {
        list,
        toast,
        scripts,
        repositories,
    }
})
