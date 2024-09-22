<template>
  <b-modal
    @shown="onShow"
    id="page-move-modal"
    centered
    :title="$t('page.move.title')"
    hide-header-close
  >
  <div role="group">
        <label>{{$t("page.move.parentSpace")}}:</label>
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
        <label>{{$t("page.move.parentPage")}}:</label>
        <v-select
            v-model="selectedPage"  
            :options="pages" 
            :get-option-label="(option) => option.name"
            required
          ></v-select>
    </div>
    <b-form-checkbox v-model="withChilds">&nbsp;{{$t("page.move.withChilds")}}</b-form-checkbox>
    <template #modal-footer>
      <div align="right">
        <b-button variant="warning" class="mr-05em" @click="pageAction">{{$t("page.move.move")}}</b-button>
        <b-button variant="secondary" @click="close">{{$t("page.move.cancel")}}</b-button>
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
  name: "MovePage",
  data() {
      return {
          selectedSpace: null,
          spaces: [],
          selectedPage: null,
          pages: [],
          withChilds: false,
      }
  },
  props: {
    actionCallBack: Function,
  },
  methods: {
    pageAction: async function() {
      var pageId = this.$route.params.pageId;
      if(pageId == null) return;
      if(this.selectedPage == null) return;

      var newPageRequest = await axios.post(`/api/page/move/${pageId}/${this.selectedPage.id}?withChilds=${this.withChilds}`);

      if (this.actionCallBack !== null && newPageRequest.data.spaceKey == this.$route.params.key)
        this.actionCallBack();

      if( newPageRequest.data.spaceKey != this.$route.params.key )
        this.$router.push("/space/" + newPageRequest.data.spaceKey + '/' + newPageRequest.data.id);
      this.$bvModal.hide("page-move-modal");
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
      if (spaces.length == 0){
        var allSpaces = await SpaceService.getSpaces();
        spaces = allSpaces.slice(0, 5);
      }
      this.spaces = spaces;
    },
    close: function () {
      this.$bvModal.hide("page-move-modal");
    },
    onShow: function () {
      this.selectedSpace = null;
      this.spaces = [];
      this.selectedPage = null;
      this.pages = [];
      this.withChilds = false;

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