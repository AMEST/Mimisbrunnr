import Vue from 'vue'
import Router from 'vue-router'
import Dashboard from './views/Home/Dashboard.vue'
import CustomHome from './views/Home/CustomHome.vue'

Vue.use(Router)

export default new Router({
  mode: 'history',
  base: process.env.BASE_URL,
  routes: [
    {
      path: '/',
      name: 'home',
      component: CustomHome
    },
    {
        path: '/dashboard',
        name: 'dashboard',
        component: Dashboard
    },
    {
      path: '/spaces',
      name: 'spaces',
      component: () => import(/* webpackChunkName: "spaces" */ './views/SpaceDirectory.vue')
    },
    {
      path: '/space/:key',
      name: 'space',
      component: () => import(/* webpackChunkName: "space" */ './views/Wiki/Space.vue')
    },
    {
      path: '/space/:key/:pageId',
      name: 'page',
      component: () => import(/* webpackChunkName: "page" */ './views/Wiki/Space.vue')
    },
    {
      path: '/space/:key/:pageId/edit',
      name: 'pageEdit',
      component: () => import(/* webpackChunkName: "editor" */ './views/Wiki/PageEdit.vue')
    },
    {
      path: '/profile/:email',
      name: 'profile',
      component: () => import(/* webpackChunkName: "profile" */ './views/People/Profile.vue')
    },
    {
      path: '/error/:err',
      name: 'error',
      component: () => import(/* webpackChunkName: "error" */ './views/Error.vue')
    },
    {
      path: '/admin',
      name: 'GeneralConfiguration',
      component: () => import(/* webpackChunkName: "adminGeneral" */ './views/Admin/GeneralConfiguration.vue')
    }
  ]
})
