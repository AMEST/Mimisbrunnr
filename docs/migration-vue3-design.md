# Design Document: Migration to Vue 3 via @vue/compat

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Current State Analysis](#2-current-state-analysis)
3. [Migration Strategy](#3-migration-strategy)
4. [Phase 0: Infrastructure Preparation](#4-phase-0-infrastructure-preparation)
5. [Phase 1: Bootstrap & Core Migration](#5-phase-1-bootstrap--core-migration)
6. [Phase 2: Thirdparty Components](#6-phase-2-thirdparty-components)
7. [Phase 3: Application Components](#7-phase-3-application-components)
8. [Phase 4: Dependency Upgrades](#8-phase-4-dependency-upgrades)
9. [Phase 5: Remove @vue/compat](#9-phase-5-remove-vuecompat)
10. [Testing Strategy](#10-testing-strategy)
11. [Rollback Plan](#11-rollback-plan)
12. [Risk Register](#12-risk-register)
13. [Appendix: Full Change Manifest](#13-appendix-full-change-manifest)

---

## 1. Executive Summary

**Goal:** Migrate the Mimisbrunnr Wiki frontend from Vue 2.6 to Vue 3 without breaking functionality.

**Approach:** Incremental migration using `@vue/compat` (Vue 3 Compatibility Build) with `MODE: 2` as the global default, allowing BootstrapVue 2.23+ to operate in legacy mode while gradually converting components to Vue 3 semantics.

**Duration:** 2–3 weeks (1 developer)

**Key Constraints:**
- BootstrapVue 2.23+ supports `@vue/compat` with `{ MODE: 2 }` globally — this is the enabler for incremental migration
- 67 `.vue` components, all Options API, no mixins/filters/event-bus — codebase is clean
- 49 `$bvModal`/`$bvToast` calls across 18 files — these work under compat mode

---

## 2. Current State Analysis

### 2.1 Technology Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| Framework | Vue | 2.6.14 |
| State Management | Vuex | 3.6.2 |
| Router | Vue Router | 3.0.3 |
| UI Library | BootstrapVue | 2.21.2 |
| CSS Framework | Bootstrap | 4.6.1 |
| i18n | vue-i18n | 8.27.1 |
| Markdown Editor | EasyMDE | 2.18.0 |
| Markdown Parser | markdown-it + 14 plugins | various |
| Build Tool | @vue/cli-service | 3.0.5 |
| Transpiler | @vue/cli-plugin-babel | 3.0.5 |
| Linter | eslint + eslint-plugin-vue | 5.x / 5.x |
| SFC Compiler | vue-template-compiler | 2.6.10 |

### 2.2 Codebase Metrics

| Metric | Count |
|--------|-------|
| `.vue` components | 67 |
| `.js` thirdparty components | 1 (VueMarkdown.js) |
| Views (routes) | 18 |
| Lazy-loaded routes | 16 |
| `$bvModal` / `$bvToast` calls | 49 across 18 files |
| `$store.state` accesses | 67 across 23 files |
| `$store.commit()` calls | 8 across 4 files |
| Lifecycle hooks (destroy*) | 6 across 5 files |
| `$forceUpdate()` calls | 4 across 3 files |
| `$parent` access | 1 file (2 occurrences) |
| `$nextTick` calls | 2 files |
| Mixins | 0 |
| Filters | 0 |
| Event bus | 0 |
| `$listeners` | 0 |
| `$children` | 0 |
| Functional components | 0 |
| Render functions | 1 (VueMarkdown.js) |
| `.native` modifier | 0 |
| `slot-scope` (legacy) | 0 |

### 2.3 Vue 2-specific Patterns Found

| Pattern | Severity | Files |
|---------|----------|-------|
| `new Vue()` bootstrap | Must fix | `main.js` |
| `new Vuex.Store()` | Must fix | `services/store.js` |
| `new Router()` | Must fix | `router.js` |
| `new VueI18n()` | Must fix | `main.js` |
| `Vue.use()` / `Vue.component()` | Must fix | `main.js`, `router.js`, `store.js` |
| `Vue.config.productionTip` | Must remove | `main.js` |
| `destroyed()` hook | Must rename | `App.vue`, `VueSimpleMde.vue`, `SettingsModal.vue`, `PageEdit.vue` |
| `beforeDestroy()` hook | Must rename | `PreviewOverlay.vue`, `EmbeddedPage.vue` |
| `render(createElement)` | Must refactor | `VueMarkdown.js` |
| `domProps: { innerHTML }` | Must refactor | `VueMarkdown.js` |
| `this.$slots.default` as array | Must refactor | `VueMarkdown.js` |
| `model:` option (custom v-model) | Must remove | `VueSimpleMde.vue` |
| `$emit("input")` for v-model | Must change | `VueSimpleMde.vue` |
| `this.$parent.type / .spaceKey` | Must refactor | `PageTemplateModal.vue` |

---

## 3. Migration Strategy

### 3.1 Why @vue/compat

`@vue/compat` is a Vue 3 build that enables Deprecated Vue 2 APIs. It allows:

- Running Vue 2-style plugins (BootstrapVue 2.23+) on Vue 3 core
- Per-component opt-in to `{ compatConfig: { MODE: 3 } }` for gradual migration
- The app runs on Vue 3 runtime from day one, with Vue 2 API surface available

### 3.2 Global Compat Configuration

```
createApp(App).config.globalProperties.compatConfig = { MODE: 2 }
```

This is the **global default**. BootstrapVue requires `MODE: 2` to function. Individual first-party components can be upgraded to `MODE: 3` at any time by adding:

```js
export default {
  compatConfig: { MODE: 3 },
  // ...
}
```

### 3.3 Migration Order

```
Phase 0: Infrastructure (package.json, config files, build tooling)
    ↓
Phase 1: Bootstrap (main.js, router.js, store.js) — app starts on Vue 3
    ↓
Phase 2: Thirdparty (VueMarkdown.js, VueSimpleMde.vue)
    ↓
Phase 3: Application components (lifecycle hooks, $parent, $bvModal patterns)
    ↓
Phase 4: Dependency upgrades (vue-select, vue-i18n, etc.)
    ↓
Phase 5: Remove @vue/compat — pure Vue 3 (future, optional)
```

Each phase is independently testable and deployable.

---

## 4. Phase 0: Infrastructure Preparation

### 4.1 Upgrade bootstrap-vue

**Why:** v2.23.0+ adds `@vue/compat` support.

```diff
# package.json
- "bootstrap-vue": "2.21.2"
+ "bootstrap-vue": "^2.23.1"
```

Also remove the duplicate in devDependencies:
```diff
# devDependencies
- "bootstrap-vue": "2.21.2"
```

### 4.2 Replace vue-template-compiler with @vue/compiler-sfc

**Why:** `vue-template-compiler` is Vue 2-only. `@vue/compiler-sfc` is the Vue 3 SFC compiler.

```diff
# devDependencies
- "vue-template-compiler": "^2.6.10"
+ "@vue/compiler-sfc": "^3.4.0"
```

### 4.3 Add @vue/compat

```diff
# dependencies
+ "@vue/compat": "^3.4.0"
```

### 4.4 Upgrade Vue

```diff
# dependencies
- "vue": "^2.6.14"
+ "vue": "^3.4.0"
```

### 4.5 Upgrade @vue/cli-service and plugins

```diff
# devDependencies
- "@vue/cli-plugin-babel": "^3.0.5"
- "@vue/cli-plugin-eslint": "^3.0.5"
- "@vue/cli-plugin-pwa": "^3.0.5"
- "@vue/cli-service": "^3.0.5"
+ "@vue/cli-plugin-babel": "^5.0.0"
+ "@vue/cli-plugin-eslint": "^5.0.0"
+ "@vue/cli-plugin-pwa": "^5.0.0"
+ "@vue/cli-service": "^5.0.0"
```

### 4.6 Upgrade ESLint toolchain

```diff
# devDependencies
- "babel-eslint": "^10.0.1"
- "eslint": "^5.16.0"
- "eslint-plugin-vue": "^5.0.0"
+ "@babel/eslint-parser": "^7.23.0"
+ "eslint": "^8.56.0"
+ "eslint-plugin-vue": "^9.20.0"
```

Update `.eslintrc.js`:
```diff
  parserOptions: {
-   parser: 'babel-eslint'
+   parser: '@babel/eslint-parser'
  },
  'extends': [
-   'plugin:vue/essential',
+   'plugin:vue/vue3-essential',
    'eslint:recommended'
  ],
```

### 4.7 Upgrade core-js

```diff
# dependencies
- "core-js": "^2.6.5"
+ "core-js": "^3.35.0"
```

Update `babel.config.js`:
```diff
  presets: [
-   '@vue/app'
+   '@vue/cli-plugin-babel/preset'
  ]
```

### 4.8 Update vue.config.js for compat mode

```js
// vue.config.js
const { defineConfig } = require('@vue/cli-service')

module.exports = defineConfig({
  productionSourceMap: process.env.NODE_ENV !== 'production',
  transpileDependencies: ['bootstrap-vue'],
  chainWebpack: (config) => {
    config.plugins.delete('prefetch')
  },
})
```

Key addition: `transpileDependencies: ['bootstrap-vue']` ensures BootstrapVue is compiled through the Vue 3 Babel pipeline.

### 4.9 Remove openssl-legacy-provider hack

```diff
# package.json scripts
- "serve": "export NODE_OPTIONS=--openssl-legacy-provider && vue-cli-service serve"
- "build": "export NODE_OPTIONS=--openssl-legacy-provider && vue-cli-service build"
- "lint": "export NODE_OPTIONS=--openssl-legacy-provider && vue-cli-service lint"
+ "serve": "vue-cli-service serve"
+ "build": "vue-cli-service build"
+ "lint": "vue-cli-service lint"
```

### 4.10 Upgrade vue-i18n to v9

```diff
# dependencies
- "vue-i18n": "^8.27.1"
+ "vue-i18n": "^9.9.0"
```

### 4.11 Upgrade vue-router to v4

```diff
# dependencies
- "vue-router": "^3.0.3"
+ "vue-router": "^4.3.0"
```

### 4.12 Upgrade Vuex to v4

```diff
# dependencies
- "vuex": "^3.6.2"
+ "vuex": "^4.1.0"
```

### 4.13 Replace vue2-touch-events

```diff
# dependencies
- "vue2-touch-events": "^3.2.2"
+ "vue3-touch-events": "^3.2.0"
```

### 4.14 Upgrade vue-select

```diff
# dependencies
- "vue-select": "^3.20.0"
+ "vue-select": "^4.0.0"
```

### 4.15 Upgrade axios (optional, not Vue-related but current version is old)

```diff
# dependencies
- "axios": "^0.26.1"
+ "axios": "^1.6.0"
```

### 4.16 Final package.json state after Phase 0

```json
{
  "dependencies": {
    "@vue/compat": "^3.4.0",
    "axios": "^1.6.0",
    "bootstrap": "4.6.1",
    "bootstrap-vue": "^2.23.1",
    "codemirror": "^5.65.13",
    "codemirror-spell-checker": "^1.1.2",
    "core-js": "^3.35.0",
    "easymde": "^2.18.0",
    "highlight.js": "^11.5.1",
    "markdown-it": "^13.0.1",
    "markdown-it-abbr": "^1.0.4",
    "markdown-it-collapsible": "^2.0.2",
    "markdown-it-container": "^3.0.0",
    "markdown-it-deflist": "^2.1.0",
    "markdown-it-emoji": "^2.0.2",
    "markdown-it-footnote": "^3.0.3",
    "markdown-it-imsize": "^2.0.1",
    "markdown-it-ins": "^3.0.1",
    "markdown-it-mark": "^3.0.1",
    "markdown-it-sub": "^1.0.0",
    "markdown-it-sup": "^1.0.0",
    "markdown-it-task-lists": "^2.1.1",
    "markdown-it-toc-and-anchor": "^4.2.0",
    "marked": "^4.0.13",
    "register-service-worker": "^1.6.2",
    "turndown": "^7.1.3",
    "vue": "^3.4.0",
    "vue-i18n": "^9.9.0",
    "vue-router": "^4.3.0",
    "vue-select": "^4.0.0",
    "vue-tree-list": "^1.5.0",
    "vue3-touch-events": "^3.2.0",
    "vuex": "^4.1.0"
  },
  "devDependencies": {
    "@babel/eslint-parser": "^7.23.0",
    "@vue/cli-plugin-babel": "^5.0.0",
    "@vue/cli-plugin-eslint": "^5.0.0",
    "@vue/cli-plugin-pwa": "^5.0.0",
    "@vue/cli-service": "^5.0.0",
    "@vue/compiler-sfc": "^3.4.0",
    "eslint": "^8.56.0",
    "eslint-plugin-vue": "^9.20.0"
  }
}
```

---

## 5. Phase 1: Bootstrap & Core Migration

### 5.1 Entry point: main.js

**File:** `src/main.js`

```js
import { createApp } from 'vue'
import { createI18n } from 'vue-i18n'
import App from './App.vue'
import router from './router'
import store from './services/store'
import language from './assets/lang.json'
import axios from 'axios'
import './registerServiceWorker'
// Bootstrap
import { BootstrapVue, BIcon } from 'bootstrap-vue'
import 'bootstrap/dist/css/bootstrap.css'
import 'bootstrap-vue/dist/bootstrap-vue.css'
// Vue touchscreen events
import Vue3TouchEvents from 'vue3-touch-events'
// i18n
// Select plugin
import vSelect from 'vue-select'
import 'vue-select/dist/vue-select.css'
// Vue Markdown
import './thirdparty/VueMarkdown.css'

const app = createApp(App)

// Compat config — global MODE:2 for BootstrapVue compatibility
app.config.globalProperties.compatConfig = { MODE: 2 }

// Plugins
app.use(store)
app.use(router)
app.use(BootstrapVue)
app.use(Vue3TouchEvents)

// i18n
const currentLang = (function () {
  var stored = window.localStorage['lang']
  if (stored === 'ru' || stored === 'en') return stored
  var browserLang = navigator.language || navigator.userLanguage
  return browserLang === 'ru-RU' ? 'ru' : 'en'
})()
window.localStorage['lang'] = currentLang

const i18n = createI18n({
  locale: currentLang,
  messages: {
    en: language['en'],
    ru: language['ru'],
  },
  legacy: true,  // Options API mode — required for compat MODE:2
})
app.use(i18n)

// Global components
app.component('v-select', vSelect)
app.component('b-icon', BIcon)

// Load bootstrap theme
var link = document.createElement('link')
link.rel = 'stylesheet'
link.href = '/css/bootstrap.cosmo.min.css'
document.head.appendChild(link)
// Load custom css
var customCss = document.createElement('link')
customCss.rel = 'stylesheet'
customCss.href = '/api/customization/css'
document.head.appendChild(customCss)

// Restore history
var recentlyVisited = window.localStorage['history']
if (recentlyVisited !== undefined) {
  JSON.parse(recentlyVisited).forEach(function (element) {
    store.commit('addToHistory', element)
  })
}
// Restore Home menu close state
var homeMenuClosed = window.localStorage['homeMenuClosed']
if (homeMenuClosed !== undefined) {
  store.commit('changeHomeMenuClose', JSON.parse(homeMenuClosed))
} else {
  store.commit('changeHomeMenuClose', window.innerWidth <= 860)
}

// Fetch app info then mount
var applicationInfoTask = axios.get('/api/quickstart').then(function (result) {
  if (result.data == null || result.status !== 200) return
  store.commit('changeApplicationInfo', result.data)
  document.title = result.data.title
})

window.addEventListener('dragover', function (e) { e.preventDefault() }, false)
window.addEventListener('drop', function (e) { e.preventDefault() }, false)

Promise.all([applicationInfoTask]).then(function () {
  app.mount('#app')
})
```

**Changes from original:**
- `import { createApp } from 'vue'` instead of `import Vue from 'vue'`
- `createI18n({ legacy: true })` instead of `new VueI18n()`
- `app.use()` / `app.component()` instead of `Vue.use()` / `Vue.component()`
- `app.mount('#app')` instead of `new Vue({...}).$mount('#app')`
- `import Vue3TouchEvents from 'vue3-touch-events'` instead of `vue2-touch-events`
- Removed `Vue.config.productionTip = false`
- Added `app.config.globalProperties.compatConfig = { MODE: 2 }`

### 5.2 Router: router.js

**File:** `src/router.js`

```js
import { createRouter, createWebHistory } from 'vue-router'
import Dashboard from './views/Home/Dashboard.vue'
import CustomHome from './views/Home/CustomHome.vue'

const routes = [
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
    component: () => import(/* webpackChunkName: "editor" */ './views/Wiki/PageEdit.vue')
  },
  {
    path: '/space/:key/:pageId/embedded',
    name: 'embeddedPage',
    component: () => import(/* webpackChunkName: "space" */ './views/Wiki/EmbeddedPage.vue')
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
  },
  {
    path: '/admin/plugins',
    name: 'PluginsAdministration',
    component: () => import(/* webpackChunkName: "admin" */ './views/Admin/Plugins.vue')
  },
  {
    path: '/admin/templates',
    name: 'PageTemplates',
    component: () => import(/* webpackChunkName: "admin" */ './views/Admin/PageTemplates.vue')
  }
]

const router = createRouter({
  history: createWebHistory(process.env.BASE_URL),
  routes,
})

export default router
```

**Changes from original:**
- `import { createRouter, createWebHistory } from 'vue-router'` instead of `import Router from 'vue-router'` + `Vue.use(Router)`
- `createRouter({ history: createWebHistory(), routes })` instead of `new Router({ mode: 'history', ... })`
- Routes extracted to a named `routes` array (cleaner, same structure)

### 5.3 Store: services/store.js

**File:** `src/services/store.js`

```js
import { createStore } from 'vuex'

export default createStore({
  state: {
    application: {
      profile: null,
      homeMenuClosed: false,
      info: {
        title: 'Mimisbrunnr',
        allowAnonymous: false,
        allowHtml: true,
        swaggerEnabled: true,
        customHomePageEnabled: false
      },
      history: [],
      theme: '',
      version: ''
    },
    templates: {
      system: [],
      user: [],
      space: [],
    }
  },
  getters: {},
  mutations: {
    changeProfile(state, payload) {
      state.application.profile = payload
    },
    changeApplicationInfo(state, payload) {
      state.application.info = payload
    },
    changeTheme(state, payload) {
      window.localStorage['theme'] = payload
      state.application.theme = payload
    },
    changeHomeMenuClose(state, payload) {
      window.localStorage['homeMenuClosed'] = JSON.stringify(payload)
      state.application.homeMenuClosed = payload
    },
    clearProfile(state) {
      state.application.profile = null
    },
    addToHistory(state, payload) {
      for (let h of state.application.history) {
        if (payload.id === h.id) return
      }
      if (state.application.history.length > 31) {
        var sliceAt = state.application.history.length - 31
        state.application.history = state.application.history.slice(sliceAt)
      }
      state.application.history.push(payload)
      window.localStorage['history'] = JSON.stringify(state.application.history)
    },
    changeVersion(state, value) {
      state.application.version = value
    },
    setSystemTemplates(state, payload) {
      state.templates.system = payload
    },
    setUserTemplates(state, payload) {
      state.templates.user = payload
    },
    setSpaceTemplates(state, payload) {
      state.templates.space = payload
    }
  },
  actions: {}
})
```

**Changes from original:**
- `import { createStore } from 'vuex'` instead of `import Vue from 'vue'` + `import Vuex from 'vuex'` + `Vue.use(Vuex)`
- `createStore({...})` instead of `new Vuex.Store({...})`
- Removed `import Vue` and `Vue.use(Vuex)`

### 5.4 service-worker.js — no changes needed

`registerServiceWorker.js` is Vue-agnostic (uses `register-service-worker` package). No migration required.

---

## 6. Phase 2: Thirdparty Components

### 6.1 VueMarkdown.js

**File:** `src/thirdparty/VueMarkdown.js`

This component uses a render function, `domProps`, and iterates `this.$slots.default` as an array. Three changes needed:

#### 6.1.1 Render function: `createElement` → `h`

```diff
+ import { h } from 'vue'

  export default {
    // ... props, computed, data unchanged ...

    render(createElement) {
      // ... markdown-it setup unchanged ...

      return createElement(
        'div', {
-         domProps: {
-           innerHTML: outHtml,
-         },
+         innerHTML: outHtml,
        },
      )
    },
  }
```

The `domProps` option is removed in Vue 3. `innerHTML` is now a direct prop.

Note: `render(createElement)` still works in compat MODE:2. The `h` import is only needed if switching to MODE:3. However, this change is safe to apply immediately in MODE:2.

#### 6.1.2 Slots: array → function

```diff
  beforeMount() {
    if (this.$slots.default) {
      this.sourceData = ''
-     for (let slot of this.$slots.default) {
+     for (let slot of this.$slots.default()) {
        this.sourceData += slot.text
      }
    }
```

In Vue 3, `this.$slots.default` is a function returning VNode[], not a VNode[] directly. However, **in compat MODE:2, `$slots.default` is still an array**. This change can be deferred to Phase 5. If applied early, it must be wrapped:

```js
// Safe wrapper for both modes:
const slotNodes = typeof this.$slots.default === 'function'
  ? this.$slots.default()
  : this.$slots.default
for (let slot of slotNodes) {
  this.sourceData += slot.text
}
```

**Recommendation:** Apply the wrapper now, clean up in Phase 5.

#### 6.1.3 No other changes needed

`$watch`, `$forceUpdate`, `$emit` all work in MODE:2 and MODE:3.

### 6.2 VueSimpleMde.vue

**File:** `src/thirdparty/VueSimpleMde.vue`

#### 6.2.1 Remove `model:` option

```diff
  export default {
    name: "vue-simplemde",
-   model: {
-     prop: "modelValue",
-     event: "update:modelValue",
-   },
    props: {
```

In Vue 3, `v-model` always uses `modelValue` prop + `update:modelValue` event by default. The component already declares `modelValue` as a prop, so this block is redundant and invalid in Vue 3.

#### 6.2.2 Remove duplicate `$emit("input")`

```diff
  handleInput(val) {
    this.isValueUpdateFromInner = true
    this.$emit("update:modelValue", val)
-   this.$emit("input", val)
  },
```

In Vue 3, v-model uses `update:modelValue` exclusively. The `input` event is no longer used for v-model binding.

#### 6.2.3 Rename `destroyed` → `unmounted`

```diff
- destroyed() {
+ unmounted() {
    this.simplemde = null
  },
```

Or, since the component runs in compat MODE:2 globally, this rename is optional until Phase 5. **However, applying it now is safe and recommended** — `unmounted` is recognized in both MODE:2 and MODE:3.

#### 6.2.4 `$el` usage — no change needed

```js
element: this.$el.firstElementChild,  // line 80
```

`this.$el` works identically in Vue 3 compat mode.

---

## 7. Phase 3: Application Components

### 7.1 Lifecycle Hooks

**6 occurrences across 5 files.** Simple rename, works in both MODE:2 and MODE:3.

| File | Old Hook | New Hook |
|------|----------|----------|
| `App.vue:60` | `destroyed()` | `unmounted()` |
| `components/people/profile/SettingsModal.vue:48` | `destroyed()` | `unmounted()` |
| `views/Wiki/PageEdit.vue:586` | `destroyed: function()` | `unmounted: function()` |
| `components/PreviewOverlay.vue:39` | `beforeDestroy()` | `beforeUnmount()` |
| `views/Wiki/EmbeddedPage.vue:54` | `beforeDestroy: function()` | `beforeUnmount: function()` |

**Note:** `VueSimpleMde.vue:141` already handled in Phase 2.

The old hook names (`beforeDestroy`, `destroyed`) work in compat MODE:2 but emit console warnings. Renaming eliminates warnings and prepares for Phase 5.

### 7.2 `$parent` Access in PageTemplateModal.vue

**File:** `src/components/templates/PageTemplateModal.vue:76-77`

```js
// Current code:
type: this.$parent.type,
spaceKey: this.$parent.spaceKey || "",
```

This component is a child of `PageTemplateManager.vue` which already passes `type` and `spaceKey` as props to itself. The fix: pass them through to `PageTemplateModal`.

**Step 1:** Add props to `PageTemplateModal.vue`:

```diff
  props: {
    modalId: { type: String, default: "page-template-modal" },
    template: { type: Object, default: null },
+   type: { type: String, default: "System" },
+   spaceKey: { type: String, default: "" },
  },
```

**Step 2:** Update `onSubmit` to use props:

```diff
  async onSubmit() {
    // ...
    await pageTemplateService.create({
      name: this.form.name,
      description: this.form.description,
      content: this.form.content,
-     type: this.$parent.type,
-     spaceKey: this.$parent.spaceKey || "",
+     type: this.type,
+     spaceKey: this.spaceKey || "",
    });
  },
```

**Step 3:** Pass props in `PageTemplateManager.vue`:

```diff
  <page-template-modal
    :modal-id="'page-template-modal-' + type"
    :template="editingTemplate"
+   :type="type"
+   :space-key="spaceKey"
    @saved="loadTemplates"
  />
```

`$parent` still works in compat MODE:2 but should be removed — it's a code smell and won't survive Phase 5 cleanly.

### 7.3 `$bvModal` and `$bvToast` — No Changes Needed

All 38 `$bvModal` and 11 `$bvToast` calls work unchanged under BootstrapVue 2.23+ with compat MODE:2. These are BootstrapVue's injected instance methods and are not affected by the Vue 2→3 migration at the compat layer.

**Verified usage patterns:**
- `this.$bvModal.show(id)` — 14 calls across 10 files
- `this.$bvModal.hide(id)` — 14 calls across 10 files
- `this.$bvModal.msgBoxConfirm(...)` — 5 calls across 5 files
- `this.$bvToast.toast(...)` — 11 calls across 6 files

### 7.4 `$store`, `$router`, `$route`, `$i18n` — No Changes Needed

All these instance properties are available in Options API under both MODE:2 and MODE:3. The 67 `$store.state` accesses and 8 `$store.commit()` calls require no modification.

### 7.5 Watch Definitions — No Changes Needed

24 files with `watch:` blocks. All use standard Options API syntax, fully compatible.

### 7.6 `$nextTick` — No Changes Needed

2 occurrences (`VueSimpleMde.vue:105`, `SpaceCreateModal.vue:159`). `this.$nextTick()` works identically in Options API under compat.

### 7.7 `$forceUpdate` — No Changes Needed

4 occurrences across 3 files. `$forceUpdate()` works identically in Options API under compat.

### 7.8 `$emit` (Component Events) — No Changes Needed

14 occurrences across 6 files. All are standard `this.$emit('event-name', data)` calls, identical in Vue 2 and 3.

### 7.9 Event Modifiers in Templates — No Changes Needed

`.prevent`, `.stop`, `.self` modifiers are unchanged in Vue 3.

---

## 8. Phase 4: Dependency Upgrades

### 8.1 vue-tree-list

**File:** Used in `components/space/Menu.vue`

`vue-tree-list@1.5.0` was written for Vue 2. Check status:

- If compatible with Vue 3 / compat MODE:2 → no action
- If not → replace with [vue-tree-list-next](https://github.com/rafaelklauden/vue-tree-list-next) or equivalent

**Action:** Test after Phase 1. If runtime errors occur, investigate alternatives.

### 8.2 EasyMDE / CodeMirror

`easymde@2.18.0` and `codemirror@5.65.13` are vanilla JS libraries — no Vue dependency. **No changes needed.**

### 8.3 highlight.js

`highlight.js@11.5.1` is a vanilla JS library. **No changes needed.**

### 8.4 marked

`marked@4.0.13` is a vanilla JS library used in `VueSimpleMde.vue`. **No changes needed.**

### 8.5 turndown

`turndown@7.1.3` is a vanilla JS library. **No changes needed.**

### 8.6 markdown-it ecosystem

All 14 markdown-it plugins are vanilla JS. **No changes needed.**

### 8.7 register-service-worker

Vanilla JS, Vue-agnostic. **No changes needed.**

---

## 9. Phase 5: Remove @vue/compat

**This phase is optional and can be done weeks/months after the migration.**

Goal: Remove the compat layer so the app runs on pure Vue 3.

### 5.1 Remove compat configuration

```diff
# main.js
- app.config.globalProperties.compatConfig = { MODE: 2 }
```

### 5.2 Verify BootstrapVue-Next availability

At this point, either:
- **bootstrap-vue-next** is mature enough to replace BootstrapVue 2.23 → migrate UI components
- Or: keep BootstrapVue 2.23 with `compatConfig: { MODE: 2 }` on a per-component basis as needed

### 5.3 Clean up VueMarkdown.js slots

```diff
- const slotNodes = typeof this.$slots.default === 'function'
-   ? this.$slots.default()
-   : this.$slots.default
+ for (let slot of this.$slots.default()) {
```

### 5.4 Clean up any remaining MODE:2 patterns

Remove any `compatConfig: { MODE: 2 }` annotations left on individual components.

---

## 10. Testing Strategy

### 10.1 Pre-migration baseline

Before starting Phase 0:
1. Run `npm run build` — ensure production build succeeds
2. Run `npm run lint` — record current lint state
3. Manual smoke test of all major flows:
   - Login / logout
   - Home dashboard
   - Space directory
   - Create / edit / view page
   - Markdown editor (EasyMDE)
   - Search
   - Admin panels (Users, Groups, Plugins, Templates)
   - User profile

### 10.2 Per-phase testing

After each phase:
1. `npm run build` — production build succeeds
2. `npm run lint` — no new lint errors
3. Manual smoke test of the flows above
4. Check browser console for Vue deprecation warnings

### 10.3 Specific regression tests

| Test | Phase | Why |
|------|-------|-----|
| Modal open/close | 1 | BootstrapVue compat |
| `$bvToast` notification | 1 | BootstrapVue compat |
| Markdown page rendering | 2 | VueMarkdown.js render function |
| Markdown editor save | 2 | VueSimpleMde v-model |
| Page template CRUD | 3 | $parent → props refactor |
| Navigate between routes | 1 | Router v4 migration |
| i18n language switch | 1 | vue-i18n v9 migration |
| Vuex state restore from localStorage | 1 | Store migration |
| Touch events on mobile | 1 | vue3-touch-events |

### 10.4 Console warning monitoring

In compat MODE:2, Vue 3 emits console warnings for deprecated features still in use. After migration, search console for:
- `[Vue compat]` warnings
- `[DEPRECATION]` tags

These indicate code paths that need attention before Phase 5.

---

## 11. Rollback Plan

Each phase can be independently rolled back:

| Phase | Rollback |
|-------|----------|
| Phase 0 | Revert `package.json`, `npm install`, revert config files |
| Phase 1 | Revert `main.js`, `router.js`, `store.js` — app runs on Vue 2 again |
| Phase 2 | Revert `VueMarkdown.js`, `VueSimpleMde.vue` |
| Phase 3 | Revert lifecycle renames, `$parent` refactor |
| Phase 4 | Revert dependency versions |
| Phase 5 | Re-add `@vue/compat` and compat config |

**Critical:** Do not mix Vue 2 and Vue 3 npm packages. A full `npm install` after reverting `package.json` is mandatory.

---

## 12. Risk Register

| # | Risk | Impact | Probability | Mitigation |
|---|------|--------|-------------|------------|
| 1 | BootstrapVue 2.23+ has undiscovered compat bugs | High | Medium | Test thoroughly in Phase 1. Fallback: stay on Vue 2.7 |
| 2 | `vue-tree-list@1.5.0` incompatible with Vue 3 | Medium | Medium | Test in Phase 4. Fallback: fork or find alternative |
| 3 | `marked@4` deprecated API usage | Low | Low | `marked` is vanilla JS, not Vue-coupled |
| 4 | VueMarkdown.js `$slots` iteration breaks | Medium | Low | Use function-call wrapper from Phase 2 |
| 5 | `this.$parent` refactor introduces bug | Medium | Low | Pass props explicitly, write test |
| 6 | ESLint plugin version conflict | Low | Medium | Pin versions, test lint after Phase 0 |
| 7 | Bootstrap CSS theme incompatibility | Low | Low | Bootstrap 4.6.1 stays unchanged |

---

## 13. Appendix: Full Change Manifest

### Files to modify

| # | File | Phase | Changes |
|---|------|-------|---------|
| 1 | `package.json` | 0 | Update all dependency versions |
| 2 | `vue.config.js` | 0 | Add `defineConfig`, `transpileDependencies` |
| 3 | `babel.config.js` | 0 | Change preset to `@vue/cli-plugin-babel/preset` |
| 4 | `.eslintrc.js` | 0 | Update parser, extends |
| 5 | `src/main.js` | 1 | Full rewrite: `createApp`, `createI18n`, `app.use()` |
| 6 | `src/router.js` | 1 | `createRouter` + `createWebHistory` |
| 7 | `src/services/store.js` | 1 | `createStore` from vuex |
| 8 | `src/thirdparty/VueMarkdown.js` | 2 | `domProps` → prop, `$slots` wrapper |
| 9 | `src/thirdparty/VueSimpleMde.vue` | 2 | Remove `model:`, remove `$emit("input")`, rename hook |
| 10 | `src/App.vue` | 3 | `destroyed` → `unmounted` |
| 11 | `src/components/PreviewOverlay.vue` | 3 | `beforeDestroy` → `beforeUnmount` |
| 12 | `src/views/Wiki/EmbeddedPage.vue` | 3 | `beforeDestroy` → `beforeUnmount` |
| 13 | `src/views/Wiki/PageEdit.vue` | 3 | `destroyed` → `unmounted` |
| 14 | `src/components/people/profile/SettingsModal.vue` | 3 | `destroyed` → `unmounted` |
| 15 | `src/components/templates/PageTemplateModal.vue` | 3 | Add props, remove `$parent` access |
| 16 | `src/components/templates/PageTemplateManager.vue` | 3 | Pass new props to child |

### Files NOT modified (confirmed compatible)

All 51 remaining `.vue` components, all service files (`profileService.js`, `pageTemplateService.js`, etc.), all markdown-it plugins, `registerServiceWorker.js`, CSS files, and all vanilla JS thirdparty code.

### Summary by phase

| Phase | Files Modified | Estimated Time |
|-------|---------------|----------------|
| Phase 0 | 4 config files | 0.5 day |
| Phase 1 | 3 core files | 1 day (including testing) |
| Phase 2 | 2 thirdparty files | 1 day (including testing) |
| Phase 3 | 6 component files | 1 day (including testing) |
| Phase 4 | 0–1 files | 0.5 day |
| Phase 5 | 2–3 files (future) | 0.5 day |
| **Total** | **16 files** | **~4 days + testing** |
