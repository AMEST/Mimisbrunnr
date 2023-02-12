<template>
  <b-tab class="max-tab-pane text-left" id="favorites-tab">
    <template #title>
      <b-icon icon="star" class="text-secondary"></b-icon>
      <strong class="text-secondary"> {{ $t("home.favorites.title") }}</strong>
    </template>
    <h2>{{ $t("home.favorites.title") }}</h2>
    <br />
    <b-tabs v-model="tabIndex" @activate-tab="tabChanged" content-class="mt-3">
      <b-tab
        :title="$t('home.favorites.tabs.space.title')"
        id="favoriteSpaces"
      >
        <div v-if="spaces.length == 0" align="center">
            <span class="text-muted">{{
            $t("home.favorites.tabs.space.empty")
            }}</span>
        </div>
        <b-card-group deck v-else>
          <FavoriteSpaceCard
            v-for="favorite in spaces"
            :key="favorite.id"
            :favorite="favorite"
          />
        </b-card-group>
      </b-tab>
      <b-tab
        :title="$t('home.favorites.tabs.page.title')"
        id="favoritePages"
      >
        <div v-if="pages.length == 0" align="center">
            <span class="text-muted">{{
            $t("home.favorites.tabs.page.empty")
            }}</span>
        </div>
        <b-card-group deck v-else>
          <FavoritePageCard
            v-for="favorite in pages"
            :key="favorite.id"
            :favorite="favorite"
          />
        </b-card-group>
      </b-tab>
    </b-tabs>
  </b-tab>
</template>

<script>
import FavoriteService from "@/services/favoriteService";
import FavoriteSpaceCard from "@/components/base/FavoriteSpaceCard.vue";
import FavoritePageCard from "@/components/base/FavoritePageCard.vue";
export default {
  name: "Favorites",
  components: {
    FavoriteSpaceCard,
    FavoritePageCard,
  },
  data() {
    return {
      spaces: [],
      spacesScrollMax: 0,
      pages: [],
      pagesScrollMax: 0,
      tabIndex: 0,
      availableTabs: ["space", "page"],
    };
  },
  methods: {
    // eslint-disable-next-line
    tabChanged: function (newTabIndex, oldTabIndex, event) {
      this.$router.push(
        `/dashboard/favorites/${this.availableTabs[newTabIndex]}`
      );
    },
    tabRouter: function () {
      if (this.$route.params.subsection != null) {
        var requestedIndex = this.availableTabs.indexOf(
          this.$route.params.subsection
        );
        this.tabIndex = requestedIndex >= 0 ? requestedIndex : 0;
      }
    },
    getFavoriteOffset: function (type) {
      switch (type) {
        case "page":
          return this.pages.length;
        case "space":
        default:
          return this.spaces.length;
      }
    },
    getScrollMax: function (type) {
      switch (type) {
        case "page":
          return this.pagesScrollMax;
        case "space":
        default:
          return this.spacesScrollMax;
      }
    },
    setScrollMax: function (type, value) {
      switch (type) {
        case "page":
          this.pagesScrollMax = value;
          break;
        case "space":
        default:
          this.spacesScrollMax = value;
          break;
      }
    },
    loadFavorites: async function (type) {
      if (this.availableTabs.indexOf(this.$route.params.subsection) < 0)
        type = "space";
      var self = this;
      var skip = this.getFavoriteOffset(type);
      var favorites = await FavoriteService.getAll(30, skip, type);
      favorites.forEach((element) => {
        switch (type) {
          case "page":
            self.pages.push(element);
            break;
          case "space":
          default:
            self.spaces.push(element);
            break;
        }
      });
    },
  },
  watch: {
    // eslint-disable-next-line
    "$route.params.subsection": function (to, from) {
      // eslint-disable-next-line
      this.tabRouter();
      if (this.spaces.length == 0 || this.pages.length == 0)
        if (this.availableTabs.indexOf(this.$route.params.subsection) >= 0)
          this.loadFavorites(this.$route.params.subsection);
    },
  },
  mounted() {
    var self = this;
    var favoritesTab = document.getElementById("favorites-tab");
    favoritesTab.onscroll = function () {
      var wh = window.innerHeight - 58.6;
      var scrollMax = self.getScrollMax(self.$route.params.subsection);
      if (
        favoritesTab.scrollTop + wh > favoritesTab.scrollHeight - wh / 2 &&
        scrollMax != favoritesTab.scrollHeight
      ) {
        // eslint-disable-next-line
        console.log(
          "Download next 30 favorites. scrollTop:" + favoritesTab.scrollTop
        );
        self.setScrollMax(
          self.$route.params.subsection,
          favoritesTab.scrollHeight
        );
        self.loadFavorites(self.$route.params.subsection);
      }
    };
  },
  created() {
    this.tabRouter();
    this.loadFavorites(this.$route.params.subsection);
  },
};
</script>

<style>
</style>