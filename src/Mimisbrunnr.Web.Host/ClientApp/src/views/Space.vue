<template>
  <b-container v-if="loaded" fluid class="text-left">
    <b-row v-if="error" class="h-100vh">
      <h2>Space not found or user has no permissions</h2>
    </b-row>
    <b-row v-else class="h-100vh">
      <Menu :space="space" :pageTree="pageTree" :userPermissions="userPermissions"/>
      <Page :page="page" :userPermissions="userPermissions"/>
    </b-row>
  </b-container>
</template>

<script>
import axios from "axios";
import Menu from "@/components/space/Menu.vue";
import Page from "@/components/space/Page.vue";
export default {
  name: "Space",
  components:{
    Menu,
    Page
  },
  data() {
    return {
      loaded: false,
      error: false,
      space: null,
      page: null,
      userPermissions: null,
      pageTree: []
    };
  },
  methods: {
    init: async function () {
      this.loaded = false;
      if(!await this.loadSpace()) return;
      if(!await this.loadPage()) return;
      if(!await this.loadPageTree()) return;
      // eslint-disable-next-line
      console.log("Space initialized");
      this.loaded = true;
    },
    loadSpace: async function () {
      var key = this.$route.params.key;
      if (key == null) {
        this.loaded = true;
        this.error = true;
        return false;
      }

      var spacesRequest = await axios.get("/api/space/" + key, {
        validateStatus: false,
      });
      if (spacesRequest.status != 200) {
        this.loaded = true;
        this.error = true;
        return false;
      }

      this.space = spacesRequest.data;

      var permissionsRequest = await axios.get("/api/space/" + key + "/permission", {
        validateStatus: false,
      });
      if (spacesRequest.status != 200) {
        this.loaded = true;
        this.error = true;
        return false;
      }
      this.userPermissions = permissionsRequest.data;

      return true;
    },
    loadPage: async function () {
      var pageId = this.$route.params.pageId;
      if (pageId == null) pageId = this.space.homePageId;

      var pageRequest = await axios.get("/api/page/" + pageId, {
        validateStatus: false,
      });
      if (pageRequest.status != 200 || pageRequest.data.spaceKey != this.space.key) {
        this.loaded = true;
        this.error = true;
        return false;
      }

      this.page = pageRequest.data;
      return true;
    },
    loadPageTree: async function () {
      var pageTreeRequest = await axios.get("/api/page/" + this.space.homePageId + "/tree", {
        validateStatus: false,
      });
      if (pageTreeRequest.status != 200) {
        this.loaded = true;
        this.error = true;
        return false;
      }

      this.pageTree = pageTreeRequest.data;
      return true;
    },
  },
  mounted: function () {
    this.init();
  },
  watch: {
    // eslint-disable-next-line
    "$route.params.key": function (to, from) {
      // eslint-disable-next-line
      console.log("Space key change from " + from + " to " + to);
      this.init();
    },
    // eslint-disable-next-line
    "$route.params.pageId": function (to, from) {
      // eslint-disable-next-line
      console.log("Page id change from " + from + " to " + to);
      this.loadPage();
    },
  },
};
</script>

<style>
</style>