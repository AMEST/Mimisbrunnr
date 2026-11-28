<template>
  <b-overlay :show="overlayState" rounded="sm">
    <div class="text-left search-results">
      <b-alert v-if="searchFailed" variant="warning" show>
        {{ $t("search.results.failed") }}
      </b-alert>
      <h2 v-if="!searchFailed && !overlayState && searchResults.length === 0 && nextPageOffset === null">
        {{ $t("search.results.noResults") }}
      </h2>
      <h2 v-else-if="searchResults.length > 0">
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
      <b-button
        v-if="searchType === 'page' && nextPageOffset !== null"
        class="m-3"
        variant="outline-primary"
        :disabled="overlayState"
        @click="search(true)"
      >
        {{ $t("search.results.loadMore") }}
      </b-button>
    </div>
  </b-overlay>
</template>

<script>
import { BIconFolder, BIconFileEarmarkText } from "bootstrap-vue";
import SearchService from "@/services/searchService";

const pageSize = 20;

export default {
  name: "SearchResults",
  data() {
    return {
      searchResults: [],
      searchDelayTimeout: null,
      searchRequestId: 0,
      nextPageOffset: null,
      searchFailed: false,
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
      clearTimeout(this.searchDelayTimeout);
      this.searchRequestId++;
      this.searchResults = [];
      this.nextPageOffset = null;
      this.searchFailed = false;
      this.overlayState = true;
      this.searchDelayTimeout = setTimeout(() => this.search(), 1000);
    },
    async search(append = false) {
      this.searchDelayTimeout = null;
      const requestId = ++this.searchRequestId;
      const query = this.textForSearch.trim();
      const searchPages = this.searchType === "page";
      const offset = append ? this.nextPageOffset : 0;
      this.overlayState = true;
      this.searchFailed = false;

      try {
        if (!query) return;

        const matches = searchPages
          ? await SearchService.findPages(query, pageSize, offset)
          : await SearchService.findSpaces(query);

        if (requestId !== this.searchRequestId) return;
        if (matches == null) {
          this.searchFailed = true;
          return;
        }

        if (!searchPages) {
          this.searchResults = matches;
          return;
        }

        this.nextPageOffset = matches.nextOffset;
        if (append) {
          const seen = new Set(this.searchResults.map(result => result.id));
          this.searchResults.push(...matches.pages.filter(result => !seen.has(result.id)));
        } else {
          this.searchResults = matches.pages;
        }
      } catch {
        if (requestId === this.searchRequestId) this.searchFailed = true;
      } finally {
        if (requestId === this.searchRequestId) this.overlayState = false;
      }
    },
    getFoundedContent: function (result) {
      const content = result.content || "";
      const searchTextPosition = content
        .toLowerCase()
        .indexOf(this.textForSearch.trim().toLowerCase());
      if (searchTextPosition <= 25) return content.substring(0, 128);
      return content.substring(
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
  beforeDestroy() {
    clearTimeout(this.searchDelayTimeout);
    this.searchRequestId++;
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