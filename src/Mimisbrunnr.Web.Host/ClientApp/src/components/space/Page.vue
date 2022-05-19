<template>
  <b-col class="page-view">
    <b-breadcrumb
      class="page-view-breadcrumbs"
      :items="breadcrumbs"
    ></b-breadcrumb>
    <div align="right" class="page-head">
      <b-button
        :to="'/space/' + this.page.spaceKey + '/' + this.page.id + '/edit'"
        variant="outline-secondary"
        size="sm"
        class="m-2"
        :disabled="!userPermissions.canEdit || isArchived"
      >
        <b-icon icon="pencil-fill" font-scale="0.9"/>
        Edit
      </b-button>
      <b-dropdown
        variant="secondary"
        size="sm"
        class="m-2 no-arrow-dropdown"
      >
        <template #button-content>
          <b-icon icon="three-dots" />
        </template>
        <b-dropdown-item
         href="#"
         v-b-modal.page-attachments-modal
         :disabled="!this.$store.state.application.profile"
        >Attachments</b-dropdown-item>
        <b-dropdown-divider></b-dropdown-divider>
        <b-dropdown-item
          v-b-modal.page-copy-modal
          :disabled="!userPermissions.canEdit || isArchived"
          >Copy</b-dropdown-item
        >
        <b-dropdown-item
          v-b-modal.page-move-modal
          :disabled="(!userPermissions.canEdit && !userPermissions.canRemove) || isArchived"
          >Move</b-dropdown-item
        >
        <b-dropdown-item
          variant="danger"
          v-b-modal.page-delete-modal
          :disabled="!userPermissions.canRemove || isArchived"
          >Delete</b-dropdown-item
        >
      </b-dropdown>
    </div>
    <div class="pb-2 page-title">
      <h2>{{ this.page.name }}</h2>
      <p class="text-muted page-title-dates">
        Created by
        <b-link :to="'/profile/' + this.page.createdBy.email">{{
          this.page.createdBy.name
        }}</b-link>
        at {{ new Date(this.page.created).toLocaleString() }}. Updated by
        <b-link :to="'/profile/' + this.page.updatedBy.email">{{
          this.page.updatedBy.name
        }}</b-link>
        at {{ new Date(this.page.updated).toLocaleString() }}.
      </p>
    </div>
    <div>
      <vue-markdown
        :html="this.$store.state.application.info.allowHtml"
        :source="this.page.content"
      ></vue-markdown>
    </div>
  </b-col>
</template>

<script>
// eslint-disable-next-line
import hljs from "highlight.js";
import "highlight.js/styles/github.css";
import VueMarkdown from "vue-markdown";
export default {
  name: "Page",
  data() {
    return {
      breadcrumbs: [],
    };
  },
  components: {
    VueMarkdown,
  },
  props: {
    space: Object,
    page: Object,
    userPermissions: Object,
  },
  computed: {
    isArchived() {
      return this.space.status == "Archived";
    },
  },
  methods: {
    initBreadcrumbs() {
      this.breadcrumbs = [
        {
          text: "Home",
          to: { name: "home" },
        },
      ];
      if (this.space == undefined || this.page == undefined) return;
      var spaceBreadcrumb = {
        text: this.space.name,
        to: "/space/" + this.space.key,
      };
      this.breadcrumbs.push(spaceBreadcrumb);
      if (this.space.homePageId == this.page.id) {
        spaceBreadcrumb.active = true;
        return;
      }
      this.breadcrumbs.push({
        text: this.page.name,
        active: true,
      });
    },
  },
  watch: {
    // eslint-disable-next-line
    page: function (newValue, oldValue) {
      this.initBreadcrumbs();
      setTimeout(() => hljs.highlightAll(), 100);
    },
  },
  mounted: function () {
    this.initBreadcrumbs();
    setTimeout(() => hljs.highlightAll(), 100);
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
.page-title {
  padding-top: 5px;
}
.page-title h2 {
  margin-bottom: 0 !important;
}
.page-title-dates {
  font-size: 12px !important;
}
.page-title-dates a {
  color: #6c757d !important;
  text-decoration-color: #6c757d !important;
}
.page-view {
  overflow: auto;
}
.page-view img {
  max-width: 100%;
}
@media (min-width: 575px) {
  .page-view {
    max-height: calc(100vh - 57px);
  }
}
.page-view-breadcrumbs {
  float: left !important;
  margin-bottom: 0 !important;
  background-color: transparent !important;
}
.no-arrow-dropdown .dropdown-toggle::after {
  content: unset !important;
}
</style>