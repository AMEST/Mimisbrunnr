<template>
    <b-sidebar class="search-sidebar" id="search-sidebar" :title="$t('search.title')" right backdrop-variant="dark" no-header backdrop shadow>
      <b-form-input class="search-field" id="search-input" v-model="searchText" size="lg" :placeholder="$t('search.placeholder')" autofocus></b-form-input>
      <b-form-checkbox class="search-type-switch" switch size="sm" v-model="searchSpaces">{{$t("search.searchInSpaces")}}</b-form-checkbox>
      <recently-visited class="pt-3" v-if="searchText == null || searchText == ''"/>
      <search-results v-else :textForSearch="searchText" :searchType="searchSpaces ? 'space' : 'page'"/>
    </b-sidebar>
</template>

<script>
import RecentlyVisited from "@/components/home/RecentlyVisited.vue";
import SearchResults from "@/components/search/SearchResults.vue";
export default { 
    name: "SearchBar",
    components: {
        RecentlyVisited,
        SearchResults,
    },
    data() {
        return {
            searchText: null,
            searchSpaces: false
        }
    },
}
</script>

<style>
#search-sidebar {
    width: 40em;
    background-color: white!important;
}
#search-sidebar h2 {
    color: #6b778c !important;
    font-size: 11px !important;
    font-weight: 600 !important;
    line-height: 1.45454545 !important;
    letter-spacing: 0 !important;
    text-transform: uppercase !important;
    line-height: 1.81818182 !important;
    padding: 0 10px !important;
}
</style>

<style scoped>
.search-type-switch {
    float: right;
}
.search-sidebar .tab-pane {
    display: unset !important;
}
.search-field:focus {
    border-bottom: 3px var(--primary) solid !important;
}
</style>