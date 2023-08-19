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
      <span 
        v-if="userPermissions && userPermissions.canEdit && userPermissions.canRemove"
        class="text-muted version-action" 
        style="float: right"
       >
        <b-icon-trash
          v-on:click="deleteVersion(version)"
          style="cursor: pointer"
        />
      </span>
      <span 
        v-if="userPermissions && userPermissions.canEdit && userPermissions.canRemove"
        class="text-muted version-action" 
        style="float: right" 
        v-on:click="restoreVersion(version)"
        >
        <svg width="16px" fill="#868e96" version="1.1" viewBox="0 0 512 512" xml:space="preserve" xmlns="http://www.w3.org/2000/svg"> 
            <path d="m287.88 438.64-72.599-72.634v48.715h-159.4c-3.995 0-6.034-2.488-6.895-3.971-0.859-1.483-2.009-4.487-0.027-7.955l27.597-48.292-41.522-23.728-27.596 48.295c-9.982 17.468-9.912 38.284 0.185 55.686 10.099 17.401 28.138 27.79 48.257 27.79h159.4v48.711l72.599-72.617z"/>
            <path d="m504.44 378.6-79.703-138.05 42.185-24.357-99.188-26.564-26.604 99.189 42.19-24.358 79.703 138.05c1.996 3.459 0.862 6.471 8e-3 7.956-0.853 1.486-2.882 3.982-6.875 4l-55.62 0.247 0.212 47.823 55.62-0.247c20.119-0.089 38.112-10.559 48.133-28.004 10.02-17.445 9.998-38.259-0.061-55.684z"/>
            <path d="m302.05 28.427c-10.115-17.341-28.12-27.682-48.187-27.682h-0.132c-20.118 0.045-38.134 10.473-48.192 27.897l-79.702 138.05-42.185-24.357 26.588 99.181 99.202-26.555-42.19-24.358 79.703-138.05c1.997-3.458 5.17-3.98 6.883-3.985 1.695 0.011 4.889 0.505 6.903 3.953l28.025 48.045 41.31-24.095-28.026-48.045z"/>
        </svg>
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
    BIconTrash
  },
  props: {
    page: Object,
    userPermissions: Object,
    onVersionSelect: Function ,
    onVersionRestore: Function
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
    restoreVersion: async function(selectedVersion){
        console.log("[restore]", selectedVersion);
        await PageService.restoreVersion(this.page.id, selectedVersion.version);
        await this.init();
        if(this.onVersionRestore && !this.$route.params.versionId)
            this.onVersionRestore();
        else
            this.$router.push(`/space/${this.page.spaceKey}/${this.page.id}`)
    }
  },
};
</script>

<style>
.version-action{
    width: 24px;
    height: 24px;
    border-radius: 2px;
    text-align: center;
    line-height: 24px;
    cursor: pointer;
}
.version-action:hover{
    background-color: rgba(0, 0, 0, 0.2);
    transition-duration: 250ms;
}
</style>