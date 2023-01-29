<template>
  <b-card>
    <b-card-title>
      {{ favorite.page.name }}
    </b-card-title>
    <b-card-text>
      {{ favorite.page.content.substring(0, 128)}}
      {{ favorite.page.content.length > 128 ? "..." : "" }}
    </b-card-text>
    <b-card-text class="small text-muted">Last updated 3 mins ago</b-card-text>
  </b-card>
</template>

<script>
export default {
  name: "FavoritePageCard",
  props: {
    space: Object,
    favorite: Object,
  },
};
</script>

<style>
</style>