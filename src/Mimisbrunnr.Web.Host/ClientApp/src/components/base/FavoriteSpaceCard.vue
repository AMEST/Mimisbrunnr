<template>
  <b-card
    class="favorite-space-card"
    header-bg-variant="light"
    header="|"
    header-text-variant="light"
    v-on:click="goToSpace()"
    align="left"
  >
    <b-avatar
      size="48"
      :text="getSpaceNameInitials()"
      :src="favorite.space.avatarUrl"
      :style="
        this.favorite.space.avatarUrl
          ? 'background-color: white; margin-top: -4em'
          : 'margin-top: -4em'
      "
    ></b-avatar>
    <b-icon-star-fill
      font-scale="1.5"
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
import { BIconStarFill } from "bootstrap-vue";
import { getNameInitials } from "@/services/Utils";
export default {
  name: "FavoriteSpaceCard",
  props: {
    favorite: Object,
  },
  components: {
    BIconStarFill,
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