<template>
  <b-modal
    @shown="init"
    id="page-versions-modal"
    centered
    :title="$t('page.versions.title')"
    ok-only
  >
    <b-alert v-if="this.versions.length == 0" show variant="light">{{
      $t("page.versions.empty")
    }}</b-alert>
    <b-list-group-item
      button
      v-for="version in this.versions"
      :key="version.version"
    >
      <span v-on:click="selectVersion(version)">
        <span class="pr-3">{{ $t("page.versions.version") }}: {{version.version}}</span> {{ $t("page.versions.updated") }} {{new Date(version.updated).toLocaleString()}}
      </span>
      <span class="text-muted" style="float: right">
        <b-icon-trash
          v-on:click="deleteVersion(version)"
          style="cursor: pointer"
        />
      </span>
    </b-list-group-item>
  </b-modal>
</template>

<script>
import { BIconTrash } from "bootstrap-vue";
import PageService from "@/services/pageService";
export default {
  name: "Versions",
  data() {
    return {
        versions: []
    };
  },
  components: {
    BIconTrash,
  },
  props: {
    page: Object,
    onVersionSelect: Function 
  },
  methods: {
    // eslint-disable-next-line
    init: async function (even) {
      var pageVersions = await PageService.getVersions(this.page.id);
      if(!pageVersions)
        return;
      this.versions = pageVersions.versions.sort((a,b) => b.version-a.version);
    },
    selectVersion: async function (selectedVersion) {
      console.log("[select]", selectedVersion);
      if (this.onVersionSelect != null) {
        this.onVersionSelect(selectedVersion);
        return;
      }
      window.open(
        `/space/${this.page.spaceKey}/${this.page.id}/version/${selectedVersion.version}`,
        "_blank"
      );
    },
    // eslint-disable-next-line
    deleteVersion: async function (selectedVersion) {
      console.log("[delete]", selectedVersion);
      await PageService.deleteVersion(this.page.id, selectedVersion.version);
      await this.init();
    },
  },
};
</script>

<style>
</style>