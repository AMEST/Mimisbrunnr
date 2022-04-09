<template>
  <b-col sm="9">
    <div align="right" class="page-head">
      <b-dropdown
        split
        split-variant="outline-secondary"
        :split-to="'/space/'+this.page.spaceKey+'/'+this.page.id+'/edit'"
        variant="secondary"
        text="Edit"
        size="sm"
        class="m-2"
      >
        <b-dropdown-item href="#" disabled>Attachments</b-dropdown-item>
        <b-dropdown-item href="#" disabled>Copy</b-dropdown-item>
        <b-dropdown-item href="#" disabled>Move</b-dropdown-item>
        <b-dropdown-item variant="danger" href="#" disabled>Delete</b-dropdown-item>
      </b-dropdown>
    </div>
    <div class="pb-2 page-title">
      <h2>{{ this.page.name }}</h2>
      <p class="text-muted page-title-dates">
        Created by <b-link :to="'/profile/'+this.page.createdBy.email">{{this.page.createdBy.name}}</b-link> at {{this.page.created}}.
        Updated by <b-link :to="'/profile/'+this.page.updatedBy.email">{{this.page.updatedBy.name}}</b-link> at {{this.page.updated}}.
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
</style>