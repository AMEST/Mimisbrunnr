<template>
    <b-sidebar class="search-sidebar" id="search-sidebar" title="Search..." right backdrop-variant="dark" no-header backdrop shadow>
      <b-form-input class="search-field" id="search-input" v-model="searchText" size="lg" placeholder="Search" autofocus></b-form-input>
      <b-form-checkbox class="search-type-switch" switch size="sm" v-model="searchSpaces">Search space</b-form-checkbox>
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
.search-field {
    font-weight: bold;
    background-color: transparent;
    border: unset;
    background-image: url("data:image/svg+xml;base64,PD94bWwgdmVyc2lvbj0iMS4wIiA/PjwhRE9DVFlQRSBzdmcgIFBVQkxJQyAnLS8vVzNDLy9EVEQgU1ZHIDEuMS8vRU4nICAnaHR0cDovL3d3dy53My5vcmcvR3JhcGhpY3MvU1ZHLzEuMS9EVEQvc3ZnMTEuZHRkJz48c3ZnIGVuYWJsZS1iYWNrZ3JvdW5kPSJuZXcgMCAwIDUwIDUwIiBoZWlnaHQ9IjUwcHgiIGlkPSJMYXllcl8xIiB2ZXJzaW9uPSIxLjEiIHZpZXdCb3g9IjAgMCA1MCA1MCIgd2lkdGg9IjUwcHgiIHhtbDpzcGFjZT0icHJlc2VydmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyIgeG1sbnM6eGxpbms9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkveGxpbmsiPjxyZWN0IGZpbGw9Im5vbmUiIGhlaWdodD0iNTAiIHdpZHRoPSI1MCIvPjxjaXJjbGUgY3g9IjIxIiBjeT0iMjAiIGZpbGw9Im5vbmUiIHI9IjE2IiBzdHJva2U9IiMwMDAwMDAiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIgc3Ryb2tlLW1pdGVybGltaXQ9IjEwIiBzdHJva2Utd2lkdGg9IjIiLz48bGluZSBmaWxsPSJub25lIiBzdHJva2U9IiMwMDAwMDAiIHN0cm9rZS1taXRlcmxpbWl0PSIxMCIgc3Ryb2tlLXdpZHRoPSI0IiB4MT0iMzIuMjI5IiB4Mj0iNDUuNSIgeTE9IjMyLjIyOSIgeTI9IjQ1LjUiLz48L3N2Zz4=");
    background-repeat: no-repeat;
    background-position: right calc(0.375em + 0.1875rem) center;
    background-size: calc(0.75em + 0.375rem) calc(0.75em + 0.375rem);
    margin-bottom: 5px;
}

.search-field:focus {
    border-bottom: 3px var(--primary) solid !important;
}
</style>