<template>
  <b-container fluid class="spaces-container h-100vh">
    <b-container class="text-left">
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
        <b-list-group-item
          v-for="space in spaces"
          :key="space.key"
          :title="space.description"
          button
        >
          <div class="space-title" v-on:click="goToSpace(space.key)">
            <b-avatar
              class="mr-2"
              :text="getSpaceNameInitials(space.name)"
              :src="space.avatarUrl"
              :style="space.avatarUrl ? 'background-color: transparent' : ''"
            ></b-avatar>
            <b>{{ space.name }}</b>
          </div>
          <div class="space-actions">
            <b-button variant="light"> <b-icon icon="star"></b-icon></b-button>
          </div>
        </b-list-group-item>
      </b-list-group>
    </b-container>
  </b-container>
</template>
<script>
import axios from "axios";
import SearchService from "@/services/searchService";
import { getNameInitials, debounce } from "@/services/Utils";
export default {
  name: "SpaceDirectory",
  data: () => ({
    spaces: [],
    searchText: "",
  }),
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
      var spacesRequest = await axios.get("/api/space", {
        validateStatus: false,
      });
      if (spacesRequest.status == 200) this.spaces = spacesRequest.data;
    },
  },
  watch: {
    // eslint-disable-next-line
    searchText(newValue, oldValue) {
      if (newValue.length > 2) this.search();
      if (newValue.length == 0) this.loadSpaces();
    },
  },
  created: async function () {
    document.title = `${this.$t("spaceDirectory.title")} - ${
      this.$store.state.application.info.title
    }`;
    this.loadSpaces();
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
  border: unset;
}
.spaces-list .space-title {
  width: 85%;
  display: inline-block;
}
.spaces-list .space-actions {
  margin-left: auto;
  display: inline-block;
  float: right;
}
.space-search {
  width: 150px;
  margin-left: auto;
}
</style>