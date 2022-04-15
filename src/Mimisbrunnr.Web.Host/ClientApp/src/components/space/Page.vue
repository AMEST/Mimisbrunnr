<template>
  <b-col sm="8" md="9" xl="10" class="page-view">
    <div align="right" class="page-head">
      <b-dropdown
        split
        split-variant="outline-secondary"
        :split-to="'/space/'+this.page.spaceKey+'/'+this.page.id+'/edit'"
        variant="secondary"
        text="Edit"
        size="sm"
        class="m-2"
        :disabled="!userPermissions.canEdit"
      >
        <b-dropdown-item href="#" disabled>Attachments</b-dropdown-item>
        <b-dropdown-item v-b-modal.page-copy-modal :disabled="!userPermissions.canEdit">Copy</b-dropdown-item>
        <b-dropdown-item v-b-modal.page-move-modal :disabled="!userPermissions.canEdit && !userPermissions.canRemove">Move</b-dropdown-item>
        <b-dropdown-item variant="danger" v-b-modal.page-delete-modal :disabled="!userPermissions.canRemove">Delete</b-dropdown-item>
      </b-dropdown>
    </div>
    <div class="pb-2 page-title">
      <h2>{{ this.page.name }}</h2>
      <p class="text-muted page-title-dates">
        Created by <b-link :to="'/profile/'+this.page.createdBy.email">{{this.page.createdBy.name}}</b-link> at {{new Date(this.page.created).toLocaleString()}}.
        Updated by <b-link :to="'/profile/'+this.page.updatedBy.email">{{this.page.updatedBy.name}}</b-link> at {{new Date(this.page.updated).toLocaleString()}}.
      </p>
    </div>
    <div>
      <vue-markdown :source="this.page.content"></vue-markdown>
    </div>
  </b-col>
</template>

<script>
import VueMarkdown from 'vue-markdown'
export default {
  name: "Page",
  components:{
    VueMarkdown
  },
  props: {
    page: Object,
    userPermissions: Object
  },
};
</script>

<style>
.page-head {
  padding-right: 1.5em;
}
.page-head .sr-only {
  display: none;
}
.page-title h2{
  margin-bottom: 0!important;
}
.page-title-dates {
  font-size: 12px!important;
}
.page-title-dates a{
  color: #6c757d !important;
  text-decoration-color: #6c757d !important;
}
.page-view{
  overflow: auto;
  max-height: calc( 100vh - 57px);
}
</style>