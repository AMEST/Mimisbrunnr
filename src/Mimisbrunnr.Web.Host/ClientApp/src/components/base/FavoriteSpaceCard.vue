<template>
  <b-card
    class="favorite-space-card"
    header-bg-variant="light"
    header="|"
    header-text-variant="light"
    v-on:click="goToSpace()"
  >
    <b-avatar
      size="48"
      :text="getSpaceNameInitials()"
      :src="favorite.space.avatarUrl"
      :style="
        favorite.space.avatarUrl
          ? 'background-color: white; margin-top: -4em'
          : 'margin-top: -4em'
      "
    ></b-avatar>
    <b-icon
      font-scale="1.5"
      icon="star-fill"
      variant="warning"
      style="border: unset; float: right; margin-top: -0.5em"
    />
    <b-card-text :title="favorite.space.name">
      {{ favorite.space.name.substring(0, 16) }}
      {{ favorite.space.name.length > 16 ? "..." : "" }}
    </b-card-text>
  </b-card>
</template>

<script>
import { getNameInitials } from "@/services/Utils";
export default {
  name: "FavoriteSpaceCard",
  props: {
    favorite: Object,
  },
  methods: {
    getSpaceNameInitials(name) {
      return getNameInitials(name);
    },
    goToSpace() {
      this.$router.push("/space/" + this.favorite.space.key);
    },
  },
};
</script>

<style>
.favorite-space-card {
  max-width: 200px !important;
  min-width: 200px !important;
  cursor: pointer;
  margin-bottom: 1em!important;
}
.favorite-space-card .card-header {
  border-bottom: unset !important;
}
</style>