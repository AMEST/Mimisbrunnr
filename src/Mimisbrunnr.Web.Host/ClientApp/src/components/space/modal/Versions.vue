<template>
  <b-modal
    @shown="init"
    id="page-versions-modal"
    centered
    :title="$t('page.versions.title')"
  >
    <b-alert v-if="this.attachments.length == 0" show variant="light">{{
      $t("page.versions.empty")
    }}</b-alert>
    <b-list-group-item
      button
      v-for="attachment in this.attachments"
      :key="attachment.name"
    >
      <span v-on:click="selectAttachment(attachment)">{{
        attachment.name
      }}</span>
      <span class="text-muted" style="float: right">
        <b-icon-trash
          v-on:click="deleteVersion(attachment)"
          style="cursor: pointer"
        />
      </span>
    </b-list-group-item>
    <template #modal-footer>
      <b-input-group style="width: 100%">
        <b-form-file
          v-model="newAttachment"
          :placeholder="$t('page.attachments.placeholder')"
          drop-placeholder="Drop file here..."
          style="width: 80%"
        ></b-form-file>
        <b-input-group-append>
          <b-button
            @click="uploadAttachment"
            variant="primary"
            >{{ $t("page.attachments.upload") }}</b-button
          >
        </b-input-group-append>
      </b-input-group>
    </template>
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
      if(pageVersions == null)
        return;
      this.versions = pageVersions.data;
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
      await PageService.deleteVersion(this.page.id, this.selectedVersion.version);
      await this.init();
    },
  },
};
</script>

<style>
</style>