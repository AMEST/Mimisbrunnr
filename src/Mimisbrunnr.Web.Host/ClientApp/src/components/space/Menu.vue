<template>
  <b-col sm="4" md="auto" class="space-menu">
    <div class="space-menu-title space-menu-bottom-line">
      <b-avatar
        class="space-avatar-bg"
        :text="getSpaceNameInitials()"
        :src="this.space.avatarUrl"
        :style="this.space.avatarUrl ? 'background-color: transparent' : ''"
      ></b-avatar>
      <b-link :to="'/space/' + this.space.key">{{ this.space.name }}</b-link>
    </div>
    <div class="mt-3 pb-3 space-menu-bottom-line" v-if="this.space.description">
      <i class="text-muted">
        {{ $t("space.description") }}: {{ this.space.description }}
      </i>
    </div>
    <div
      class="mt-3 pb-3 space-actions-menu space-menu-bottom-line"
      v-if="this.space.type == 'Personal' || userPermissions.isAdmin"
    >
      <i class="text-muted">{{ $t("space.actions.title") }}</i>
      <b-list-group class="mt-1">
        <b-list-group-item
          v-if="this.homePage && this.space.type == 'Personal'"
          :to="'/profile/' + this.homePage.createdBy.email"
        >
          <b-icon-person-fill/>&nbsp; {{ $t("space.actions.profile") }}
        </b-list-group-item>
        <b-list-group-item
          v-b-modal.space-permissions-modal
          v-if="userPermissions.isAdmin"
        >
          <b-icon-shield-lock-fill />&nbsp;
          {{ $t("space.actions.permissions") }}
        </b-list-group-item>
        <b-list-group-item
          v-b-modal.space-settings-modal
          v-if="userPermissions.isAdmin"
        >
          <b-icon-gear-fill/>&nbsp; {{ $t("space.actions.settings") }}
        </b-list-group-item>
      </b-list-group>
    </div>
    <div class="mt-3 space-menu-page-tree">
      <i class="text-muted">{{ $t("space.tree") }}</i>
      <i
        class="text-center text-muted"
        v-if="this.pageTree && this.pageTree.childs.length == 0"
        >: {{ $t("space.emptyTree") }}</i
      >
      <div class="text-center" v-if="this.pageTree == undefined">
        <b-spinner variant="secondary"></b-spinner>
      </div>
      <vue-tree-list
        :model="pageTreeList"
        default-tree-node-name="Home page"
        default-leaf-node-name="Child page"
        v-bind:default-expanded="false"
      >
        <template v-slot:leafNameDisplay="slotProps">
          <span>
            <b-link
              :title="slotProps.model.name"
              :to="'/space/' + space.key + '/' + slotProps.model.id"
              >{{ slotProps.model.name }}</b-link
            >
          </span>
        </template>
      </vue-tree-list>
    </div>
  </b-col>
</template>

<script>
import {
  BIconPersonFill,
  BIconShieldLockFill,
  BIconGearFill,
} from "bootstrap-vue";
import axios from "axios";
import { VueTreeList, Tree } from "vue-tree-list";
import { getNameInitials } from "@/services/Utils";
export default {
  name: "Menu",
  components: {
    VueTreeList,
    BIconPersonFill,
    BIconShieldLockFill,
    BIconGearFill,
  },
  data() {
    return {
      homePage: null,
      pageTreeList: new Tree([]),
    };
  },
  props: {
    space: Object,
    pageTree: Object,
    userPermissions: Object,
  },
  methods: {
    getSpaceNameInitials: function () {
      if (!this.space) return "";
      return getNameInitials(this.space.name);
    },
    loadHomePage: async function () {
      var homePageRequest = await axios.get(
        "/api/page/" + this.space.homePageId,
        {
          validateStatus: false,
        }
      );
      if (homePageRequest.status != 200) return;
      this.homePage = homePageRequest.data;
    },
    loadPageTree: function () {
      var tree = this.convertTree(this.pageTree.childs);
      this.pageTreeList = new Tree(tree);
    },
    convertTree: function (pages) {
      var self = this;
      var tree = [];
      /*eslint-disable*/
      pages
        .sort(function (a, b) {
          return a.page.name.localeCompare(b.page.name, undefined, {
            sensitivity: "base",
            ignorePunctuation: true,
          });
        })
        .forEach(function (page, i, arr) {
          var treeNode = {
            name: page.page.name,
            id: page.page.id,
            dragDisabled: true,
            addTreeNodeDisabled: true,
            addLeafNodeDisabled: true,
            editNodeDisabled: true,
            delNodeDisabled: true,
          };
          treeNode.children = self.convertTree(page.childs);
          tree.push(treeNode);
        });
      /* eslint-enable*/
      return tree;
    },
    expandTree: function () {
      var pageId = this.$route.params.pageId;
      if (pageId == null) return;
      var flatPageList = this.findPageTree(this.pageTree.childs, pageId);
      for (var i = 0; i < flatPageList.length; i++) {
        //if (flatPageList[i] == pageId) continue;
        var pageTreeItem = document.getElementById(flatPageList[i]);
        if (pageTreeItem == null) continue;
        var expandIcon = pageTreeItem.getElementsByClassName("vtl-icon");
        if (expandIcon != null && expandIcon.length > 0 && !expandIcon[0].classList.contains("vtl-icon-caret-down")) expandIcon[0].click();
      }
    },
    findPageTree: function (pages, neededPageId) {
      var pagesFlat = [];
      for (var i = 0; i < pages.length; i++) {
        if (pages[i].page.id == neededPageId) {
          pagesFlat.push(pages[i].page.id);
          return pagesFlat;
        }
        var inner = this.findPageTree(pages[i].childs, neededPageId);
        if (inner.length != 0) {
          inner.push(pages[i].page.id);
          return inner;
        }
      }
      return pagesFlat;
    },
  },
  watch: {
    // eslint-disable-next-line
    space: async function (newValue, oldValue) {
      await this.loadHomePage();
    },
    "$route.params.pageId": async function(newValue, oldValue){
        if( newValue === oldValue) return;
        setTimeout(this.expandTree, 500);
    },
    pageTree: function (newValue, oldValue) {
      if (newValue == undefined && oldValue != undefined) {
        this.pageTreeList = new Tree([]);
        return;
      }
      if (newValue == undefined) return;
      this.loadPageTree();
      setTimeout(this.expandTree, 1000);
    },
  },
  mounted: async function () {
    await this.loadHomePage();
    if (this.pageTree != undefined) {
      this.loadPageTree();
    }
  },
};
</script>

<style>
.space-menu {
  background-color: rgb(247 247 247);
  overflow-x: hidden;
  overflow-y: auto;
  box-shadow: inset 0 0rem 0.5em rgba(0, 0, 0, 0.15) !important;
}
@media (min-width: 575px) {
  .space-menu {
    max-height: calc(100vh - 57px);
    max-width: 350px;
  }
}
@media (min-width: 699px) {
  .space-menu {
    min-width: 350px;
  }
}
.space-menu-title {
  padding-top: 3em;
  padding-bottom: 2em;
  padding-left: 1.5em;
  width: 100%;
  text-align: left;
}
.space-menu-bottom-line {
  border-bottom: 1px white solid;
}
.space-menu-title a {
  text-decoration: none;
  white-space: nowrap;
  text-overflow: ellipsis;
  overflow: hidden;
  display: inline-block;
  max-width: 70%;
  text-transform: none;
  margin-top: 0;
}

.space-actions-menu .list-group-item {
  background-color: transparent !important;
  border-width: 0px;
}
.space-actions-menu .list-group-item:hover {
  background-color: rgba(0, 0, 0, 0.03) !important;
}

.space-avatar-bg {
  width: 4em;
  height: 4em;
  margin-right: 10px;
  margin-top: -15px;
}
.space-menu-page-tree .vtl-node-content {
  height: 24px;
  overflow: hidden;
  word-break: break-all;
}
.space-menu-page-tree .vtl-icon-folder:before {
  content: unset !important;
}

.space-menu-page-tree .vtl-operation {
  display: none;
}
</style>