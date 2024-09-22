<template>
  <b-modal
    @shown="onShow"
    id="page-copy-modal"
    centered
    :title="$t('page.copy.title')"
    hide-header-close
  >
    <div role="group">
        <label>{{$t("page.copy.parentSpace")}}:</label>
        <v-select
            v-model="selectedSpace" 
            :options="spaces" 
            @search="onSearch"
            :get-option-label="(option) => `${option.name} (${option.key})`"
            required
          ></v-select>
    </div>
    <br>
    <div role="group">
        <label>{{$t("page.copy.parentPage")}}:</label>
        <v-select
            v-model="selectedPage"  
            :options="pages" 
            :get-option-label="(option) => option.name"
            required
          ></v-select>
    </div>
    <template #modal-footer>
      <div align="right">
        <b-button variant="warning" class="mr-05em" @click="pageAction">{{$t("page.copy.copy")}}</b-button>
        <b-button variant="secondary" @click="close">{{$t("page.copy.cancel")}}</b-button>
      </div>
    </template>
  </b-modal>
</template>

<script>
import axios from 'axios';
import SpaceService from '@/services/spaceService';
import PageService from '@/services/pageService';
import SearchService from '@/services/searchService';
import FavoriteService from '@/services/favoriteService';
import { flattenJSON } from "@/services/Utils";
export default {
  name: "CopyPage",
  data() {
      return {
          selectedSpace: null,
          spaces: [],
          selectedPage: null,
          pages: []
      }
  },
  props: {
    actionCallBack: Function,
  },
  methods: {
    pageAction: async function() {
      var pageId = this.$route.params.pageId;
      var spaceKey = this.$route.params.key;
      if(pageId == null && spaceKey == null) return;
      if(pageId == null) {
        var space = await SpaceService.getSpace(spaceKey);
        pageId = space.homePageId;
      }
      if(this.selectedPage == null) return;
      var newPageRequest = await axios.post("/api/page/copy/" + pageId + '/' + this.selectedPage.id);
      if (this.actionCallBack !== null && newPageRequest.data.spaceKey == spaceKey)
        this.actionCallBack();

      this.$router.push("/space/" + newPageRequest.data.spaceKey + '/' + newPageRequest.data.id);
      this.$bvModal.hide("page-copy-modal");
    },
    onSearch: async function (query) {
      var searchResult = await SearchService.findSpaces(query);
      this.spaces = searchResult;
    },
    init: async function(){
      var spaceKey = this.$route.params.key;
      if (!spaceKey) 
        return;
      
      this.selectedSpace = await SpaceService.getSpace(spaceKey);
      await this.loadPages(this.selectedSpace.homePageId);
      await this.loadBaseSpaces();
    },
    loadPages: async function(homePageId){
      var spacePagesTree = await PageService.getPageTree(homePageId);
      this.pages = flattenJSON(spacePagesTree);
      this.selectedPage = this.pages.find(i => i.id == homePageId);
    },
    loadBaseSpaces: async function(){
      var favorite = await FavoriteService.getAll(5, 0, "space");
      var spaces = [];
      if (favorite.length > 0)
        spaces = favorite.map(i => i.space);
      if (spaces.length == 0)
        spaces = await SpaceService.getSpaces(5, 0);
      this.spaces = spaces;
    },
    close: function () {
      this.$bvModal.hide("page-copy-modal");
    },
    onShow: function () {
      this.selectedSpace = null;
      this.spaces = [];
      this.selectedPage = null;
      this.pages = [];

      this.init();
    },
  },
  watch: {
    // eslint-disable-next-line
    selectedSpace(newValue, oldValue) {
      if (newValue == null) return;
      this.loadPages(newValue.homePageId);
    },
  },
};
</script>

<style>
</style>