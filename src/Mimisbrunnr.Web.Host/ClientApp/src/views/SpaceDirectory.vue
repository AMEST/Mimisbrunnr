<template>
  <b-container fluid class="spaces-container h-100vh" id="space-container">
    <b-container class="text-left">
      <br />
      <div v-if="!this.isAnonymous">
        <div class="mb-3">
          <h3>
            <b>{{ $t("spaceDirectory.favorites.title") }}</b>
          </h3>
        </div>
        <div v-if="favoriteSpaces.length == 0" align="center">
          <span class="text-muted">{{
            $t("spaceDirectory.favorites.empty")
          }}</span>
        </div>
        <b-card-group align="center" class="favorite-spaces-deck" deck v-else>
          <FavoriteSpaceCard
            v-for="favorite in favoriteSpaces"
            :key="favorite.id"
            :favorite="favorite"
          />
        </b-card-group>
      </div>
      <br />
      <div>
        <h3>
          <b>{{ $t("spaceDirectory.all") }}</b>
        </h3>
        <b-form-input
          class="search-field space-search"
          v-model="searchText"
          size="sm"
        ></b-form-input>
      </div>
      <b-list-group class="spaces-list">
        <SpaceListItem
          v-for="space in spaces"
          :key="space.key"
          :space="space"
          :callBack="loadFavorites"
          button
        />
      </b-list-group>
    </b-container>
  </b-container>
</template>
<script>
import SearchService from "@/services/searchService";
import FavoriteService from "@/services/favoriteService";
import SpaceService from "@/services/spaceService";
import { getNameInitials, debounce } from "@/services/Utils";
import FavoriteSpaceCard from "@/components/base/FavoriteSpaceCard.vue";
import SpaceListItem from "@/components/spaceDirectory/SpaceListItem.vue";
export default {
  name: "SpaceDirectory",
  data: () => ({
    spaces: [],
    favoriteSpaces: [],
    searchText: "",
    scrollMax: 0,
    offset: 0,
    batchSize: 12
  }),
  components: {
    FavoriteSpaceCard,
    SpaceListItem,
  },
  computed: {
    isAnonymous() {
      return this.$store.state.application.profile == undefined;
    },
  },
  methods: {
    getSpaceNameInitials(name) {
      return getNameInitials(name);
    },
    goToSpace(spaceKey) {
      this.$router.push("/space/" + spaceKey);
    },
    search: debounce(async function () {
      var searchResult = await SearchService.findSpaces(this.searchText);
      if (searchResult != null) this.spaces = searchResult;
    }, 300),
    loadSpaces: async function () {
      var spacesBatch = await SpaceService.getSpaces(this.batchSize, this.offset);
      for (const space of spacesBatch)
        this.spaces.push(space);
      if(spacesBatch.length > 2)
        this.scrollMax = this.scrollMax - 100;
    },
    loadFavorites: async function () {
      this.favoriteSpaces = await FavoriteService.getAll(15, 0, "space");
    },
  },
  watch: {
    // eslint-disable-next-line
    searchText(newValue, oldValue) {
      if (newValue.length > 2) this.search();
      if (newValue.length == 0) {
        this.spaces = [];
        this.scrollMax = 0;
        this.offset = 0;
        this.loadSpaces();
      }
    },
  },
  mounted: function (){
    var self = this;
    var spaceContainer = window.document.getElementById("space-container");
    spaceContainer.onscroll = function () {
        var headerNavHeight = window.document.getElementById("header-nav").clientHeight;
        var wh = window.innerHeight-headerNavHeight;
        if ((spaceContainer.scrollTop + wh > spaceContainer.scrollHeight - wh / 2) && (self.scrollMax != spaceContainer.scrollHeight)) {
            // eslint-disable-next-line
            console.log(`Download next ${self.batchSize} spaces. scrollTop: ${spaceContainer.scrollTop}`);
            self.scrollMax = spaceContainer.scrollHeight
            self.offset += self.batchSize;
            self.loadSpaces();
        }
    }
  },
  created: async function () {
    document.title = `${this.$t("spaceDirectory.title")} - ${
      this.$store.state.application.info.title
    }`;
    this.loadSpaces();
    if(!this.isAnonymous)
        this.loadFavorites();
  },
};
</script>
<style scoped>
.spaces-container {
  background-color: #fff;
  overflow-x: hidden !important;
}
.spaces-list {
  height: 52px;
  border: unset;
  border-radius: unset;
}
.spaces-list .list-group-item {
  border: unset !important;
}
.space-search {
  width: 150px;
  margin-left: auto;
}
.favorite-spaces-deck {
  justify-content: center;
}
</style>