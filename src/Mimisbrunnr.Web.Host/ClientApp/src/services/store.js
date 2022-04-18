// Copyright 2022 Klabukov Erik.
// SPDX-License-Identifier: GPL-3.0-only

import Vue from 'vue'
import Vuex from 'vuex'

Vue.use(Vuex)

export default new Vuex.Store({
  state: {
    application: {
      profile: null,
      homeMenuClosed: false,
      info: {
        title: 'Mimisbrunnr',
        allowAnonymous: false,
        allowHtml: true,
        swaggerEnabled: true
      },
      history: [],
      theme: '',
      version: ''
    }
  },
  getters: {},
  mutations: {
    // eslint-disable-next-line
    changeProfile(state, payload) {
      state.application.profile = payload
    },
    // eslint-disable-next-line
    changeApplicationInfo(state, payload) {
      state.application.info = payload
    },
    // eslint-disable-next-line
    changeTheme(state, payload) {
      window.localStorage['theme'] = payload
      state.application.theme = payload
    },
    // eslint-disable-next-line
    changeHomeMenuClose(state, payload) {
      window.localStorage['homeMenuClosed'] = JSON.stringify(payload)
      state.application.homeMenuClosed = payload
    },
    // eslint-disable-next-line
    clearProfile(state) {
      state.application.profile = null
    },
    // eslint-disable-next-line
    addToHistory(state, payload) {
      for (let h of state.application.history) {
        if (payload.id === h.id) return
      }
      if (state.application.history.length > 15) {
        var sliceAt = state.application.history.length - 15
        state.application.history = state.application.history.slice(sliceAt, state.application.history.length)
      }
      state.application.history.push(payload)
      window.localStorage['history'] = JSON.stringify(state.application.history)
    },
    // eslint-disable-next-line
    changeVersion(state, value) {
      state.application.version = value
    }
  },
  actions: {}
})
