<template>
  <b-container v-if="loaded" fluid class="text-left">
    <b-row class="h-100vh">
      <Menu
        :space="space"
        :pageTree="pageTree"
        :userPermissions="userPermissions"
      />
      <Page
        :space="space"
        :page="page"
        :userPermissions="userPermissions"
        :versions="pageVersions"
        :historyMode="pageVersions.length > 0"
      />
    </b-row>
    <delete-page
      v-if="userPermissions && userPermissions.canRemove"
      :pageDeletedCallback="loadPageTree"
    />
    <copy-page v-if="userPermissions && userPermissions.canEdit" />
    <move-page
      v-if="
        userPermissions && userPermissions.canEdit && userPermissions.canRemove
      "
    />
    <permissions v-if="userPermissions && userPermissions.isAdmin" />
    <settings
      v-if="userPermissions && userPermissions.isAdmin"
      :spaceUpdateCallback="init"
      :space="space"
    />
    <attachments :page="page" />
  </b-container>
</template>

<script>
import axios from "axios";
import Menu from "@/components/space/Menu.vue";
import Page from "@/components/space/Page.vue";
const DeletePage = () =>
  import(
    /* webpackChunkName: "page-modals-component" */ "@/components/space/modal/DeletePage.vue"
  );
const CopyPage = () =>
  import(
    /* webpackChunkName: "page-modals-component" */ "@/components/space/modal/CopyPage.vue"
  );
const MovePage = () =>
  import(
    /* webpackChunkName: "page-modals-component" */ "@/components/space/modal/MovePage.vue"
  );
const Settings = () =>
  import(
    /* webpackChunkName: "space-modals-component" */ "@/components/space/modal/Settings.vue"
  );
const Attachments = () =>
  import(
    /* webpackChunkName: "page-modals-component" */ "@/components/space/modal/Attachments.vue"
  );
const Permissions = () =>
  import(
    /* webpackChunkName: "space-modals-component" */ "@/components/space/modal/Permissions.vue"
  );
import { showToast } from "@/services/Utils";
import SpaceService from "@/services/spaceService";
import PageService from "../../services/pageService";
export default {
  name: "Space",
  components: {
    Menu,
    Page,
    DeletePage,
    CopyPage,
    MovePage,
    Permissions,
    Settings,
    Attachments,
  },
  data() {
    return {
      loaded: false,
      space: null,
      page: null,
      userPermissions: null,
      pageTree: null,
      pageVersions: [],
    };
  },
  methods: {
    init: async function () {
      this.loaded = false;
      try {
        if (!(await this.loadSpace())) return;
        if (!(await this.loadPage())) return;
      } catch (error) {
        if (error == 401 || error == 403)
          this.$router.push("/error/unauthorized");
        else this.$router.push("/error/unknown");
        return;
      }
      // eslint-disable-next-line
      console.log("Space initialized");
      this.loaded = true;
      await this.loadPageTree();
      console.log("Page tree loaded");
    },
    loadSpace: async function () {
      var key = this.$route.params.key;
      if (key == null) {
        this.$router.push("/error/notfound");
        return false;
      }

      var space = await SpaceService.getSpace(key);
      if (space == null) {
        this.$router.push("/error/notfound");
        return false;
      }
      this.space = space;

      var spacePermissions = await SpaceService.getSpacePermissions(key);
      if (spacePermissions == null) {
        this.$router.push("/error/notfound");
        return false;
      }
      this.userPermissions = spacePermissions;

      this.$store.commit("addToHistory", {
        id: this.space.key,
        key: this.space.key,
        name: this.space.name,
        date: Date.now(),
        type: "Space",
      });

      return true;
    },
    loadPage: async function () {
      var pageId = this.$route.params.pageId;
      if (pageId == null) pageId = this.space.homePageId;
      if (pageId == null) return;

      var pageRequest = await axios.get("/api/page/" + pageId, {
        validateStatus: false,
      });
      if (
        pageRequest.status != 200 ||
        pageRequest.data.spaceKey != this.space.key
      ) {
        this.$router.push("/error/notfound");
        return false;
      }

      this.page = pageRequest.data;
      this.$store.commit("addToHistory", {
        id: this.page.id,
        name: this.page.name,
        key: this.page.spaceKey,
        date: Date.now(),
        type: "Page",
      });

      if (this.$route.params.versionId) {
        await this.loadHistoryVersion();
      }

      this.changePageTitle();
      return true;
    },
    loadHistoryVersion: async function () {
      var versions = await PageService.getVersions(this.page.id);
      if (!versions || !versions.data) return;
      this.pageVersions = versions.data;
      var selectedVersion = this.pageVersions.find(
        (x) => x.version == parseInt(this.$route.params.versionId)
      );
      if (!selectedVersion) {
        this.pageVersions = [];
        this.$router.push(`/space/${this.space.key}/${this.page.id}`);
        return;
      }
      this.page.version = selectedVersion.version;
      this.page.name = selectedVersion.name;
      this.page.content = selectedVersion.content;
      this.page.updated = selectedVersion.updated;
      this.page.updatedBy = selectedVersion.updatedBy;
    },
    loadPageTree: async function () {
      var pageTreeRequest = await axios.get(
        "/api/page/" + this.space.homePageId + "/tree",
        {
          validateStatus: false,
        }
      );
      if (pageTreeRequest.status != 200) {
        showToast(
          `${pageTreeRequest.statusText} (${pageTreeRequest.status})`,
          "Can't load page tree for this space",
          "danger"
        );
        return false;
      }

      this.pageTree = pageTreeRequest.data;
      return true;
    },
    changePageTitle: function () {
      if (!this.space && !this.page) return;
      if (this.space.homePageId == this.page.id)
        document.title = `${this.space.name} - ${this.$store.state.application.info.title}`;
      else
        document.title = `${this.page.name} - ${this.$store.state.application.info.title}`;
    },
  },
  mounted: function () {
    document.title = `Space - ${this.$store.state.application.info.title}`;
    this.pageVersions = [];
    this.init();
  },
  watch: {
    // eslint-disable-next-line
    "$route.params.key": function (to, from) {
      // eslint-disable-next-line
      console.log("Space key change from " + from + " to " + to);
      this.pageVersions = [];
      this.init();
    },
    // eslint-disable-next-line
    "$route.params.pageId": function (to, from) {
      // eslint-disable-next-line
      console.log("Page id change from " + from + " to " + to);
      if (this.loaded) {
        this.pageVersions = [];
        this.loadPage();
      }
    },
    "$route.params.versionId": function (to, from) {
      // eslint-disable-next-line
      console.log("Page history changed from " + from + " to " + to);
      if (this.loaded) {
        this.pageVersions = [];
        this.loadPage();
      }
    },
  },
};
</script>

<style>
</style>