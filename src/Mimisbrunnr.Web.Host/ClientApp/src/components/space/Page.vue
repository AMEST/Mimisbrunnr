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
        <b-icon-pencil-fill class="edit-icon" font-scale="0.9" />
        {{ $t("page.edit") }}
      </b-button>
      <b-button
        v-if="!this.isSpaceHomePage"
        variant="outline-secondary"
        @click="star"
        size="sm"
        class="m-2"
      >
        <b-icon-star-fill v-if="inFavorite" variant="warning" />
        <b-icon-star v-else />
      </b-button>
      <b-dropdown variant="secondary" size="sm" class="m-2 no-arrow-dropdown">
        <template #button-content>
          <b-icon-three-dots />
        </template>
        <b-dropdown-item
          href="#"
          v-b-modal.page-attachments-modal
          :disabled="!this.$store.state.application.profile"
          >{{ $t("page.attachments.title") }}</b-dropdown-item
        >
        <b-dropdown-divider></b-dropdown-divider>
        <b-dropdown-item
          v-b-modal.page-copy-modal
          :disabled="!userPermissions.canEdit"
          >{{ $t("page.copy.title") }}</b-dropdown-item
        >
        <b-dropdown-item
          v-b-modal.page-move-modal
          :disabled="
            isSpaceHomePage ||
            (!userPermissions.canEdit && !userPermissions.canRemove) ||
            isArchived
          "
          >{{ $t("page.move.title") }}</b-dropdown-item
        >
        <b-dropdown-item
          variant="danger"
          v-b-modal.page-delete-modal
          :disabled="
            isSpaceHomePage || !userPermissions.canRemove || isArchived
          "
          >{{ $t("page.delete.button") }}</b-dropdown-item
        >
      </b-dropdown>
      <b-button
        v-if="isSpaceHomePage"
        variant="secondary"
        size="sm"
        class="m-2"
        @click="star"
      >
        {{ inFavorite ? $t("page.unstar") : $t("page.star") }}
      </b-button>
    </div>
    <div class="pb-2 page-title">
      <h2>{{ this.page.name }}</h2>
      <p class="text-muted page-title-dates">
        {{ $t("page.date.created") }}
        <b-link :to="'/profile/' + this.page.createdBy.email">{{
          this.page.createdBy.name
        }}</b-link>
        {{ $t("page.date.at") }}
        {{ new Date(this.page.created).toLocaleString() }}.
        {{ $t("page.date.updated") }}
        <b-link :to="'/profile/' + this.page.updatedBy.email">{{
          this.page.updatedBy.name
        }}</b-link>
        {{ $t("page.date.at") }}
        {{ new Date(this.page.updated).toLocaleString() }}.
      </p>
    </div>
    <div>
      <vue-markdown
        :toc="true"
        :html="this.$store.state.application.info.allowHtml"
        :source="this.page.content"
        id="page-content"
      ></vue-markdown>
    </div>
    <br />
    <h3 v-if="comments.length > 0">
      <b>{{ $t("page.comments.title") }}: {{ this.comments.length }}</b>
    </h3>
    <hr v-if="comments.length > 0" />
    <comment
      v-for="comment in comments"
      :key="comment.id"
      :comment="comment"
      :deleteAction="deleteComment"
    />
    <CommentCreate v-if="!isAnonymous" :createAction="addComment" />
  </b-col>
</template>

<script>
// eslint-disable-next-line
import hljs from "highlight.js/lib/common";
import "highlight.js/styles/github.css";
import {
  BIconPencilFill,
  BIconStarFill,
  BIconStar,
  BIconThreeDots,
} from "bootstrap-vue";
import { replaceRelativeLinksToRoute } from "@/services/Utils";
const VueMarkdown = () => import(/* webpackChunkName: "vue-markdown-component" */"@/thirdparty/VueMarkdown");
import FavoriteService from "@/services/favoriteService";
import PageService from "@/services/pageService";
import ProfileService from "@/services/profileService";
import CommentCreate from "@/components/space/components/CommentCreate.vue";
import Comment from "@/components/space/components/Comment.vue";
export default {
  name: "Page",
  data() {
    return {
      breadcrumbs: [],
      comments: [],
      inFavorite: false,
    };
  },
  components: {
    VueMarkdown,
    CommentCreate,
    Comment,
    BIconPencilFill,
    BIconStarFill,
    BIconStar,
    BIconThreeDots,
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
    isSpaceHomePage() {
      return this.space.homePageId == this.page.id;
    },
    isAnonymous() {
      return ProfileService.isAnonymous();
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
      if (this.isSpaceHomePage) {
        spaceBreadcrumb.active = true;
        return;
      }
      this.breadcrumbs.push({
        text: this.page.name,
        active: true,
      });
    },
    loadComments: async function () {
      this.comments = [];
      var comments = await PageService.getComments(this.page.id);
      if (comments != null) this.comments = comments;
    },
    addComment: async function (comment) {
      var createdComment = await PageService.createComment(
        this.page.id,
        comment
      );
      if (createdComment != null) this.comments.push(createdComment);
    },
    deleteComment: async function (comment) {
      await PageService.deleteComment(this.page.id, comment.id);
      var newComments = [];
      this.comments.forEach((c) => {
        if (c.id != comment.id) newComments.push(c);
      });
      this.comments = newComments;
    },
    checkInFavorites: async function () {
      if (this.isSpaceHomePage)
        this.inFavorite = await FavoriteService.existsSpace(this.space.key);
      else this.inFavorite = await FavoriteService.existsPage(this.page.id);
    },
    star: async function () {
      if (this.inFavorite) {
        await this.unStar();
      } else {
        if (this.isSpaceHomePage)
          await FavoriteService.addSpace(this.space.key);
        else await FavoriteService.addPage(this.page.id);
      }
      this.inFavorite = !this.inFavorite;
    },
    unStar: async function () {
      var favorite = null;
      if (this.isSpaceHomePage)
        favorite = await FavoriteService.getSpace(this.space.key);
      else favorite = await FavoriteService.getPage(this.page.id);
      await FavoriteService.delete(favorite.id);
    },
    scrollToAnchor() {
      if (!window.location.hash) return;
      var hash = decodeURI(window.location.hash);
      if (hash.length == 1) return;
      var anchor = document.getElementById(hash.substring(1, hash.length));
      if (!anchor) return;
      anchor.scrollIntoView();
    },
  },
  watch: {
    // eslint-disable-next-line
    page: function (newValue, oldValue) {
      this.initBreadcrumbs();
      this.checkInFavorites();
      this.loadComments();
      setTimeout(() => hljs.highlightAll(), 100);
      setTimeout(this.scrollToAnchor, 100);
      setTimeout(replaceRelativeLinksToRoute, 250, "page-content");
    },
  },
  mounted: function () {
    this.initBreadcrumbs();
    this.checkInFavorites();
    this.loadComments();
    setTimeout(() => hljs.highlightAll(), 100);
    setTimeout(this.scrollToAnchor, 100);
    setTimeout(replaceRelativeLinksToRoute, 250, "page-content");
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
.page-head .edit-icon {
  margin-bottom: 0.25em;
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
  background-color: white;
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