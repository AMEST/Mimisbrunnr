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
          v-for="space in spaces.filter(this.spaceNameFilter)"
          :key="space.key"
          :title="space.description"
          button
        >
          <div class="space-title" v-on:click="goToSpace(space.key)">
            <b-avatar
              class="mr-2"
              :text="getSpaceNameInitials(space.name)"
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
import { getNameInitials } from "@/services/Utils";
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
    spaceNameFilter(item) {
      return (
        item.name.toLowerCase().indexOf(this.searchText.toLowerCase()) != -1
      );
    },
  },
  created: async function () {
    document.title = `${this.$t("spaceDirectory.title")} - ${
      this.$store.state.application.info.title
    }`;
    var spacesRequest = await axios.get("/api/space", {
        validateStatus: false,
    });
    if (spacesRequest.status == 200) this.spaces = spacesRequest.data;
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