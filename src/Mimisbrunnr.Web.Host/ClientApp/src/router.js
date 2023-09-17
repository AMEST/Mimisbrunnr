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
            path: '/dashboard/:section',
            name: 'dashboard-sections',
            component: Dashboard
        },
        {
            path: '/dashboard/:section/:subsection',
            name: 'dashboard-subsections',
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
            component: () => import(/* webpackChunkName: "space" */ './views/Wiki/Space.vue')
        },
        {
            path: '/space/:key/:pageId/version/:versionId',
            name: 'historicalPage',
            component: () => import(/* webpackChunkName: "space" */ './views/Wiki/Space.vue')
        },
        {
            path: '/space/:key/:pageId/edit',
            name: 'pageEdit',
            component: () => import(/* webpackChunkName: "editor-simplemde" */ './views/Wiki/PageEditor/SimpleMde.vue')
        },
        {
            path: '/space/:key/:pageId/edit-editorjs',
            name: 'pageEditEditorJs',
            component: () => import(/* webpackChunkName: "editor-js-md" */ './views/Wiki/PageEditor/EditorJsMD.vue')
        },
        {
            path: '/profile/:email',
            name: 'profile',
            component: () => import(/* webpackChunkName: "profile" */ './views/People/Profile.vue')
        },
        {
            path: '/people',
            name: 'Discovery',
            component: () => import(/* webpackChunkName: "profiles" */ './views/People/Discovery.vue')
        },
        {
            path: '/error/:err',
            name: 'error',
            component: () => import(/* webpackChunkName: "error" */ './views/Error.vue')
        },
        {
            path: '/admin',
            name: 'GeneralConfiguration',
            component: () => import(/* webpackChunkName: "admin" */ './views/Admin/GeneralConfiguration.vue')
        },
        {
            path: '/admin/groups',
            name: 'GroupsAdministration',
            component: () => import(/* webpackChunkName: "admin" */ './views/Admin/Groups.vue')
        },
        {
            path: '/admin/users',
            name: 'UsersAdministration',
            component: () => import(/* webpackChunkName: "admin" */ './views/Admin/Users.vue')
        }
    ]
})
