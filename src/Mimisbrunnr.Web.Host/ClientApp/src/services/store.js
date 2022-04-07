// Copyright 2022 Klabukov Erik.
// SPDX-License-Identifier: GPL-3.0-only

import Vue from 'vue'
import Vuex from 'vuex'

Vue.use(Vuex)

export default new Vuex.Store({
  state: {
    application: {
      profile: null,
      info: {
        title: 'Mimisbrunnr',
        allowAnonymous: false
      },
      history: [],
      theme: '',
      version: ''
    }
  },
  getters: {},
  mutations: {
    changeProfile (state, payload) {
      state.application.profile = payload
    },
    changeApplicationInfo (state, payload) {
      state.application.info = payload
    },
    changeTheme (state, payload) {
      window.localStorage['theme'] = payload
      state.application.theme = payload
    },
    // eslint-disable-next-line
    clearProfile (state) {
      state.application.profile = null
    },
    addToHistory (state, payload) {
      if (state.application.history.length > 10) {
        var sliceAt = state.application.history.length - 10
        state.application.history = state.application.history.slice(sliceAt, state.application.history.length)
      }
      state.application.history.push(payload)
      window.localStorage['history'] = JSON.stringify(state.application.history)
    },
    changeVersion (state, value) {
      state.application.version = value
    }
  },
  actions: {}
})
