<template>
  <b-col sm="4" md="auto" class="space-menu">
    <div class="space-menu-title space-menu-bottom-line">
      <b-avatar class="space-avatar-bg" :text="getInitials()"></b-avatar>
      <b-link :to="'/space/' + this.space.key">{{ this.space.name }}</b-link>
    </div>
    <div class="mt-3 pb-3 space-menu-bottom-line">
      <i class="text-muted">Description: {{ this.space.description }}</i>
    </div>
    <div class="mt-3 pb-3 space-actions-menu space-menu-bottom-line">
      <i class="text-muted">Space actions</i>
      <b-list-group class="mt-1">
        <b-list-group-item
          v-if="this.homePage && this.space.type == 'Personal'"
          :to="'/profile/' + this.homePage.createdBy.email"
        >
          <b-icon icon="person-fill" />&nbsp; Profile
        </b-list-group-item>
        <b-list-group-item v-b-modal.space-permissions-modal v-if="userPermissions.isAdmin">
          <b-icon icon="shield-lock-fill" />&nbsp; Permissions
        </b-list-group-item>
        <b-list-group-item v-b-modal.space-settings-modal v-if="userPermissions.isAdmin">
          <b-icon icon="gear-fill" />&nbsp; Settings
        </b-list-group-item>
      </b-list-group>
    </div>
    <div class="mt-3 space-menu-page-tree">
      <i class="text-muted">Page tree</i>
      <vue-tree-list
        :model="pageTreeList"
        default-tree-node-name="Home page"
        default-leaf-node-name="Child page"
        v-bind:default-expanded="false"
      >
        <template v-slot:leafNameDisplay="slotProps">
          <span>
            <b-link :to="'/space/' + space.key + '/' + slotProps.model.id">{{ slotProps.model.name }}</b-link>
          </span>
        </template>
      </vue-tree-list>
    </div>
  </b-col>
</template>

<script>
import axios from "axios";
import { VueTreeList, Tree } from "vue-tree-list";

export default {
  name: "Menu",
  components: {
    VueTreeList,
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
    userPermissions: Object
  },
  methods: {
    getInitials: function () {
      if (!this.space) return "";
      var splited = this.space.name.split(" ");
      if (splited.length > 1) return splited[0][0] + splited[1][0];
      return splited[0][0];
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
      // eslint-disable-next-line
      pages.forEach(function(page, i, arr) {
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
      return tree;
    },
    expandTree: function(){
      var pageId = this.$route.params.pageId;
      if(pageId == null)
        return;
      var flatPageList = this.findPageTree(this.pageTree.childs, pageId);
      for(var i = 0; i < flatPageList.length; i++){
        if(flatPageList[i] == pageId) continue;
        var pageTreeItem = document.getElementById(flatPageList[i]);
        if(pageTreeItem == null) continue;
        var expandIcon = pageTreeItem.getElementsByClassName("vtl-icon");
        if(expandIcon != null && expandIcon.length > 0)
          expandIcon[0].click();
      }
    },
    findPageTree: function(pages, neededPageId){
      var pagesFlat = []
      for(var i = 0; i < pages.length; i++){
        if(pages[i].page.id == neededPageId){
          pagesFlat.push(pages[i].page.id);
          return pagesFlat;
        }
        var inner = this.findPageTree(pages[i].childs, neededPageId);
        if(inner.length != 0){
          inner.push(pages[i].page.id);
          return inner;
        }
      }
      return pagesFlat;
    }
  },
  watch: {
    // eslint-disable-next-line
    space: async function (newValue, oldValue) {
      await this.loadHomePage();
      this.loadPageTree();
      setTimeout(this.expandTree, 1000);
    },
  },
  mounted: async function () {
    await this.loadHomePage();
    this.loadPageTree();
    setTimeout(this.expandTree, 1000);
  },
};
</script>

<style>
.space-menu {
  background-color: rgba(0, 0, 0, 0.03);
  overflow-x: hidden;
  overflow-y: auto;
  box-shadow: inset 0 0rem .5em rgba(0,0,0,.15)!important;
}
@media (min-width: 575px) {
  .space-menu {
    max-height: calc( 100vh - 57px);
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
.space-avatar-bg .b-avatar-img img {
  background-color: white;
}
.space-avatar-bg .b-avatar-text {
  background-color: white;
}
.space-menu-page-tree .vtl-node-content {
  height: 24px;
  overflow: hidden;
  word-break: break-all;
}
</style>