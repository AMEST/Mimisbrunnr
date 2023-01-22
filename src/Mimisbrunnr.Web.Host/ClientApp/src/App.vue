<template>
  <div id="app">
    <div v-if="this.loaded && this.initialized">
      <Header />
      <router-view />
    </div>
    <div v-if="this.loaded && !this.initialized">
      <Quickstart />
    </div>
    <SpaceCreateModal />
    <search-bar />
  </div>
</template>

<script>
import Header from "@/components/base/Header.vue";
import SpaceCreateModal from "@/components/base/SpaceCreateModal.vue";
import Quickstart from "@/components/quickstart/Quickstart.vue";
import axios from "axios";
import SearchBar from "@/components/search/SearchBar.vue";
import ProfileService from "@/services/profileService";
export default {
  components: {
    Header,
    SpaceCreateModal,
    Quickstart,
    SearchBar,
  },
  data: () => ({
    loaded: false,
    initialized: false,
    headerNavHeight: 56,
  }),
  created: async function () {
    window.addEventListener("resize", this.updateHeaderHeightVar);
    var initializedRequest = await axios.get("/api/quickstart/initialize");
    var currentAccount = await ProfileService.getCurrentUser();
    if (
      currentAccount == null &&
      (!initializedRequest.data.isInitialized ||
        !this.$store.state.application.info.allowAnonymous)
    ) {
      window.location.href =
        "/api/account/login?redirectUri=" + window.location.pathname;
      return;
    }

    this.$store.commit("changeProfile", currentAccount);

    this.loaded = true;
    this.updateHeaderHeightVar();
    this.initialized = initializedRequest.data.isInitialized;
  },
  destroyed() {
    window.removeEventListener("resize", this.updateHeaderHeightVar);
  },
  methods: {
    updateHeaderHeightVar: function () {
      this.headerNavHeight =
        window.document.getElementById("header-nav").clientHeight;
      window.document
        .getElementById("app")
        .style.setProperty("--header-height", `${this.headerNavHeight}px`);
    },
  },
};
</script>

<style>
#app {
  font-family: "Avenir", Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  --header-height: 56px;
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
.h-100vh {
  height: calc(100vh - var(--header-height)) !important;
}
.max-tab-pane {
  overflow: auto !important;
  max-height: calc(100vh - 57px) !important;
}
.text-left {
  text-align: left !important;
}
.text-right {
  text-align: right !important;
}
.full-size-container {
  padding: 0 !important;
  margin: 0 !important;
  height: 100% !important;
  overflow: hidden !important;
}
.badge-primary {
  color: #fff;
  background-color: #007bff;
}
.search-field {
  font-weight: bold;
  background-color: transparent;
  border-top: unset !important;
  border-left: unset !important;
  border-right: unset !important;
  border-bottom: 2px solid #b3bac5 !important;
  background-image: url("data:image/svg+xml;base64,PD94bWwgdmVyc2lvbj0iMS4wIiA/PjwhRE9DVFlQRSBzdmcgIFBVQkxJQyAnLS8vVzNDLy9EVEQgU1ZHIDEuMS8vRU4nICAnaHR0cDovL3d3dy53My5vcmcvR3JhcGhpY3MvU1ZHLzEuMS9EVEQvc3ZnMTEuZHRkJz48c3ZnIGVuYWJsZS1iYWNrZ3JvdW5kPSJuZXcgMCAwIDUwIDUwIiBoZWlnaHQ9IjUwcHgiIGlkPSJMYXllcl8xIiB2ZXJzaW9uPSIxLjEiIHZpZXdCb3g9IjAgMCA1MCA1MCIgd2lkdGg9IjUwcHgiIHhtbDpzcGFjZT0icHJlc2VydmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyIgeG1sbnM6eGxpbms9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkveGxpbmsiPjxyZWN0IGZpbGw9Im5vbmUiIGhlaWdodD0iNTAiIHdpZHRoPSI1MCIvPjxjaXJjbGUgY3g9IjIxIiBjeT0iMjAiIGZpbGw9Im5vbmUiIHI9IjE2IiBzdHJva2U9IiMwMDAwMDAiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIgc3Ryb2tlLW1pdGVybGltaXQ9IjEwIiBzdHJva2Utd2lkdGg9IjIiLz48bGluZSBmaWxsPSJub25lIiBzdHJva2U9IiMwMDAwMDAiIHN0cm9rZS1taXRlcmxpbWl0PSIxMCIgc3Ryb2tlLXdpZHRoPSI0IiB4MT0iMzIuMjI5IiB4Mj0iNDUuNSIgeTE9IjMyLjIyOSIgeTI9IjQ1LjUiLz48L3N2Zz4=");
  background-repeat: no-repeat;
  background-position: right calc(0.375em + 0.1875rem) center;
  background-size: calc(0.75em + 0.375rem) calc(0.75em + 0.375rem);
  margin-bottom: 5px;
}
</style>
