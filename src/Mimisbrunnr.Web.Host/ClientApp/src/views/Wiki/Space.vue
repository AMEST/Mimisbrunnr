<template>
  <b-container v-if="loaded" fluid class="text-left">
    <b-row class="h-100vh">
      <Menu :space="space" :pageTree="pageTree" :userPermissions="userPermissions"/>
      <Page :space="space" :page="page" :userPermissions="userPermissions"/>
    </b-row>
    <delete-page v-if="userPermissions && userPermissions.canRemove"/>
    <copy-page v-if="userPermissions && userPermissions.canEdit"/>
    <move-page v-if="userPermissions && userPermissions.canEdit && userPermissions.canRemove"/>
    <permissions v-if="userPermissions && userPermissions.isAdmin"/>
    <settings v-if="userPermissions && userPermissions.isAdmin" :spaceUpdateCallback="init" :space="space"/>
    <attachments :page="page"/>
  </b-container>
</template>

<script>
import axios from "axios";
import Menu from "@/components/space/Menu.vue";
import Page from "@/components/space/Page.vue";
import DeletePage from "@/components/space/modal/DeletePage.vue";
import CopyPage from "@/components/space/modal/CopyPage.vue";
import MovePage from "@/components/space/modal/MovePage.vue";
import Permissions from "@/components/space/modal/Permissions.vue";
import Settings from "@/components/space/modal/Settings.vue";
import Attachments from "@/components/space/modal/Attachments.vue";
export default {
  name: "Space",
  components:{
    Menu,
    Page,
    DeletePage,
    CopyPage,
    MovePage,
    Permissions,
    Settings,
    Attachments
  },
  data() {
    return {
      loaded: false,
      space: null,
      page: null,
      userPermissions: null,
      pageTree: null
    };
  },
  methods: {
    init: async function () {
      this.loaded = false;
      if(!await this.loadSpace()) return;
      if(!await this.loadPage()) return;
      // eslint-disable-next-line
      console.log("Space initialized");
      this.loaded = true;
      await this.loadPageTree();
      console.log("Page tree loaded")
    },
    loadSpace: async function () {
      var key = this.$route.params.key;
      if (key == null) {
        this.$router.push("/error/notfound");
        return false;
      }

      var spacesRequest = await axios.get("/api/space/" + key, {
        validateStatus: false,
      });
      if (spacesRequest.status != 200) {
        this.$router.push("/error/unauthorized");
        return false;
      }

      this.space = spacesRequest.data;

      var permissionsRequest = await axios.get("/api/space/" + key + "/permission", {
        validateStatus: false,
      });
      if (spacesRequest.status != 200) {
        this.$router.push("/error/unauthorized");
        return false;
      }
      this.userPermissions = permissionsRequest.data;
      this.$store.commit('addToHistory', {
        id: this.space.key,
        key: this.space.key,
        name: this.space.name,
        date: Date.now(),
        type: "Space"
      })

      return true;
    },
    loadPage: async function () {
      var pageId = this.$route.params.pageId;
      if (pageId == null) pageId = this.space.homePageId;
      if (pageId == null) return;

      var pageRequest = await axios.get("/api/page/" + pageId, {
        validateStatus: false,
      });
      if (pageRequest.status != 200 || pageRequest.data.spaceKey != this.space.key) {
        this.$router.push("/error/unknown");
        return false;
      }

      this.page = pageRequest.data;
      this.$store.commit('addToHistory', {
        id: this.page.id,
        name: this.page.name,
        key: this.page.spaceKey,
        date: Date.now(),
        type: "Page"
      })
      return true;
    },
    loadPageTree: async function () {
      var pageTreeRequest = await axios.get("/api/page/" + this.space.homePageId + "/tree", {
        validateStatus: false,
      });
      if (pageTreeRequest.status != 200) {
        this.$router.push("/error/unknown");
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