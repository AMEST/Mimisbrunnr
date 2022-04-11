<template>
  <div id="app" >
    <div v-if="this.loaded && this.initialized">
      <Header/>
      <router-view/>
    </div>
    <div v-if="this.loaded && !this.initialized">
        <Quickstart/>
    </div>
    <SpaceCreateModal/>
  </div>
</template>

<script>
import Header from '@/components/base/Header.vue'
import SpaceCreateModal from '@/components/base/SpaceCreateModal.vue'
import Quickstart from '@/components/quickstart/Quickstart.vue'
import axios from 'axios'
export default {
  components: {
    Header,
    SpaceCreateModal,
    Quickstart
  },
  data: () => ({
    loaded: false,
    initialized: false
  }),
  created: async function(){
    var initializedRequest = await axios.get("/api/quickstart/initialize");
    var currentAccountRequest = await axios.get("/api/user/current", { validateStatus: false });

    if(currentAccountRequest.status == 401 || currentAccountRequest.status == 404
      && (!initializedRequest.data.isInitialized || !this.$store.state.application.info.allowAnonymous)){
      window.location.href = "/api/account/login"
      return
    }
    
    if (currentAccountRequest.status != 404)
      this.$store.commit("changeProfile", currentAccountRequest.data)

    this.loaded = true;
    this.initialized = initializedRequest.data.isInitialized;
  }
}
</script>

<style>
#app {
  font-family: 'Avenir', Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
}
#nav {
  padding: 30px;
}

#nav a {
  font-weight: bold;
  color: #2c3e50;
}

#nav a.router-link-exact-active {
  color: #42b983;
}
body {
  background-color: #f4f5f7 !important;
}
.h-100vh{
    height: calc(100vh - 56px) !important;
}
.max-tab-pane {
  overflow: auto !important;
  max-height: calc(100vh - 57px) !important;
}
.text-left {
  text-align: left  !important;
}
.text-right {
  text-align: right  !important;
}
.full-size-container {
  padding: 0 !important;
  margin: 0 !important;
  height: 100% !important;
  overflow: hidden !important;
}
</style>
