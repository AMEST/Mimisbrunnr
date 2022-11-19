import Vue from 'vue'
import App from './App.vue'
import router from './router'
import store from './services/store'
import language from './assets/lang.json'
import axios from 'axios'
import './registerServiceWorker'
// Bootstrap
import { BootstrapVue, IconsPlugin } from 'bootstrap-vue'
import 'bootstrap/dist/css/bootstrap.css'
import 'bootstrap-vue/dist/bootstrap-vue.css'
// Vue tree list
import VueTreeList from 'vue-tree-list'
// Vue touchscreen events
import Vue2TouchEvents from 'vue2-touch-events'
// i18n
import VueI18n from 'vue-i18n'
// Select plugin
import vSelect from 'vue-select'
import 'vue-select/dist/vue-select.css';

// Vue select
Vue.component('v-select', vSelect)
// Vue i18n plugin
Vue.use(VueI18n)
// Vue touchscreen events plugin
Vue.use(Vue2TouchEvents)
// TreeList plugin
Vue.use(VueTreeList)
// Make BootstrapVue available throughout your project
Vue.use(BootstrapVue)
// Optionally install the BootstrapVue icon components plugin
Vue.use(IconsPlugin)

Vue.config.productionTip = false

// Load bootstrap theme
let link = document.createElement('link');
link.rel = 'stylesheet';
link.href = '/css/bootstrap.cosmo.min.css';
document.head.appendChild(link);
// Load custom css
let customCss = document.createElement('link');
customCss.rel = 'stylesheet';
customCss.href = '/api/customization/css';
document.head.appendChild(customCss);

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
}else {
    store.commit('changeHomeMenuClose', window.innerWidth <= 860)
}
var applicationInfoTask = axios.get('/api/quickstart').then(result => {
  if (result.data == null || result.status !== 200) {
    return
  }
  store.commit('changeApplicationInfo', result.data)
  document.title = result.data.title
})

// Restore language settings
var currentLang = window.localStorage['lang']
if (currentLang === undefined || currentLang !== "ru" && currentLang !== "en" ) {
  var browserLang = navigator.language || navigator.userLanguage;
  currentLang = browserLang === "ru-RU" ? "ru" : "en";
  window.localStorage['lang'] = currentLang;
}
console.log(currentLang);
const messages =   {
  en: language["en"],
  ru: language["ru"],
}
const i18n = new VueI18n({
  locale: currentLang, // set locale
  messages, // set locale messages
})

window.addEventListener('dragover', function (e) {
  e.preventDefault()
}, false)
window.addEventListener('drop', function (e) {
  e.preventDefault()
}, false)

// eslint-disable-next-line
Promise.all([applicationInfoTask]).then(result => {
  new Vue({
    i18n,
    store,
    router,
    render: h => h(App)
  }).$mount('#app')
})
