import Vue from 'vue'
import Router from 'vue-router'
import Home from './views/Home.vue'

Vue.use(Router)

export default new Router({
  mode: 'history',
  base: process.env.BASE_URL,
  routes: [
    {
      path: '/',
      name: 'home',
      component: Home
    },
    {
      path: '/spaces',
      name: 'spaces',
      component: () => import(/* webpackChunkName: "about" */ './views/SpaceDirectory.vue')
    },
    {
      path: '/space/:key',
      name: 'space',
      component: () => import(/* webpackChunkName: "about" */ './views/Space.vue')
    },
    {
      path: '/space/:key/:pageId',
      name: 'page',
      component: () => import(/* webpackChunkName: "about" */ './views/Space.vue')
    },
    {
      path: '/space/:key/:pageId/edit',
      name: 'pageEdit',
      component: () => import(/* webpackChunkName: "about" */ './views/PageEdit.vue')
    },
    {
      path: '/profile/:email',
      name: 'profile',
      component: () => import(/* webpackChunkName: "about" */ './views/Profile.vue')
    }
    ,
    {
      path: '/error/:err',
      name: 'error',
      component: () => import(/* webpackChunkName: "about" */ './views/Error.vue')
    }
  ]
})
