import Vue from 'vue'
import App from './App.vue'
import router from './router'
import store from './services/store'
import axios from 'axios'
import './registerServiceWorker'
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

// Load bootstrap theme
let link = document.createElement('link');
link.rel = "stylesheet";
link.href = "/css/bootstrap.cosmo.min.css";
document.head.appendChild(link);

// Restore history
var recentlyVisited = window.localStorage['history']
if (recentlyVisited !== undefined) {
  var parsedRecentlyVisited = JSON.parse(recentlyVisited)
  parsedRecentlyVisited.forEach(element => {
    store.commit('addToHistory', element)
  })
}
// Restore Home menu close state
var homeMenuClosed = window.localStorage['homeMenuClosed']
if (homeMenuClosed !== undefined) {
  var parsedHomeMenuClosed = JSON.parse(homeMenuClosed)
  store.commit('changeHomeMenuClose', parsedHomeMenuClosed)
}
var applicationInfoTask = axios.get('/api/quickstart').then(result => {
  if (result.data == null || result.status !== 200) {
    return
  }
  store.commit('changeApplicationInfo', result.data)
  document.title = result.data.title
})

// eslint-disable-next-line
Promise.all([applicationInfoTask]).then(result => {
  new Vue({
    store,
    router,
    render: h => h(App)
  }).$mount('#app')
})
