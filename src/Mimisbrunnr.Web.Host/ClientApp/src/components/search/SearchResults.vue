<template>
  <b-overlay :show="overlayState" rounded="sm">
    <div class="text-left search-results">
      <h2 v-if="searchResults.length == 0">
        {{ $t("search.results.noResults") }}
      </h2>
      <h2 v-else>
        {{ $t("search.results.hasResults") }} {{ searchResults.length }}
      </h2>
      <b-card
        class="updates-card"
        v-for="result in searchResults"
        :key="searchType == 'space' ? result.key : result.id"
      >
        <b-icon-folder
          class="search-result-icon"
          v-if="searchType == 'space'"
        />
        <b-icon-file-earmark-text class="search-result-icon" v-else />
        <b-link
          class="search-result-page-name"
          :to="
            searchType == 'space'
              ? '/space/' + result.key
              : '/space/' + result.spaceKey + '/' + result.id
          "
          >{{ result.name }}</b-link
        ><br />
        <b-link
          class="search-result-content"
          :to="
            searchType == 'space'
              ? '/space/' + result.key
              : '/space/' + result.spaceKey + '/' + result.id
          "
        >
          {{
            searchType == "space"
              ? result.description
              : getFoundedContent(result)
          }}
        </b-link>
      </b-card>
    </div>
  </b-overlay>
</template>

<script>
import { BIconFolder, BIconFileEarmarkText } from "bootstrap-vue";
import SearchService from "@/services/searchService";
export default {
  name: "SearchResults",
  data() {
    return {
      searchResults: [],
      searchDelayTimeout: null,
      overlayState: false,
    };
  },
  components: {
    BIconFolder,
    BIconFileEarmarkText,
  },
  props: {
    textForSearch: String,
    searchType: String,
  },
  methods: {
    initSearch() {
      this.overlayState = true;
      if (this.searchDelayTimeout != null)
        clearTimeout(this.searchDelayTimeout);

      this.searchDelayTimeout = setTimeout(this.search, 1000);
    },
    search: async function () {
      this.searchDelayTimeout = null;
      var searchRequestResult =
        this.searchType == "space"
          ? await SearchService.findSpaces(this.textForSearch)
          : await SearchService.findPages(this.textForSearch);

      if (searchRequestResult != null) this.searchResults = searchRequestResult;
      this.overlayState = false;
    },
    go: function (result) {
      var route = "";
      if (this.searchType == "space") {
        route = "/space/" + result.key;
      } else {
        route = "/space/" + result.spaceKey + "/" + result.id;
      }
      this.$router.push(route);
    },
    getFoundedContent: function (result) {
      var searchTextPosition = result.content
        .toLowerCase()
        .indexOf(this.textForSearch.toLowerCase());
      if (searchTextPosition <= 25) return result.content.substring(0, 128);
      return result.content.substring(
        searchTextPosition - 20,
        searchTextPosition + 108
      );
    },
  },
  watch: {
    // eslint-disable-next-line
    textForSearch(newValue, oldValue) {
      this.initSearch();
    },
    // eslint-disable-next-line
    searchType(newValue, oldValue) {
      this.initSearch();
    },
  },
  mounted() {
    this.initSearch();
  },
};
</script>

<style>
.search-results {
  min-height: calc(100vh - 50px);
}
.search-result-icon {
  float: left;
  width: 3.5em !important;
  height: 3.5em !important;
  margin-right: 1em;
  background-color: transparent;
}

.search-result-page-name {
  text-decoration: none;
  font-weight: bold;
  font-size: 14px;
}

.search-result-content {
  text-decoration: none;
  color: black;
}
</style>