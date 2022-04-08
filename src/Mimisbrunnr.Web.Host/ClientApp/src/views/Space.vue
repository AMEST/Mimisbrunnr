<template>
  <b-container v-if="loaded" fluid class="text-left">
    <div v-if="error">Space not found or user has no permissions</div>
    <div v-else>
      <ul>
        <li>Key: {{ this.$route.params.key }}</li>
        <li>PageId: {{ this.$route.params.pageId }}</li>
        <li>Space: {{JSON.stringify(this.space)}}</li>
        <li>Page: {{JSON.stringify(this.page)}}</li>
      </ul>
    </div>
  </b-container>
</template>

<script>
import axios from "axios";
export default {
  name: "Space",
  data() {
    return {
      loaded: false,
      error: false,
      space: null,
      page: null,
      pageTree: []
    };
  },
  methods: {
    init: async function () {
      this.loaded = false;
      if(!await this.loadSpace()) return;
      if(!await this.loadPage()) return;
      if(!await this.loadPageTree()) return;
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
      console.log("Space key change from " + from + " to " + to);
      this.init();
    },
    // eslint-disable-next-line
    "$route.params.pageId": function (to, from) {
      console.log("Page id change from " + from + " to " + to);
      this.init();
    },
  },
};
</script>

<style>
</style>