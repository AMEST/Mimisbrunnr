import Vue from 'vue'
import App from './App.vue'
import router from './router'
import store from './services/store'
import axios from 'axios'
import './registerServiceWorker'
// Import Bootstrap an BootstrapVue CSS files (order is important)
import { BootstrapVue, IconsPlugin } from 'bootstrap-vue'
import 'bootstrap/dist/css/bootstrap.css'
import 'bootstrap-vue/dist/bootstrap-vue.css'
// Vue tree list
import VueTreeList from 'vue-tree-list'

// TreeList plugin
Vue.use(VueTreeList)
// Make BootstrapVue available throughout your project
Vue.use(BootstrapVue)
// Optionally install the BootstrapVue icon components plugin
Vue.use(IconsPlugin)

Vue.config.productionTip = false

// Restore history
var recentlyVisited = window.localStorage['history']
if (recentlyVisited !== undefined) {
  var parsedRecentlyVisited = JSON.parse(recentlyVisited)
  parsedRecentlyVisited.forEach(element => {
    store.commit('addToHistory', element)
  })
}
var applicationInfoTask = axios.get('/api/quickstart').then(result => {
  if (result.data == null || result.status !== 200) {
    return
  }
  store.commit('changeApplicationInfo', result.data)
  document.title = result.data.title
})

Promise.all([applicationInfoTask]).then(result => {
  new Vue({
    store,
    router,
    render: h => h(App)
  }).$mount('#app')
})