<template>
  <b-card class="favorite-page-card" v-on:click="goToPage()">
    <b-icon-star-fill
      font-scale="1.5"
      variant="warning"
      style="float: right"
    />
    <b-card-title>
      <b-icon-file-earmark-text scale="1" />
      {{ favorite.page.name.substring(0, 24) }}
      {{ favorite.page.name.length > 24 ? "..." : "" }}
    </b-card-title>
    <b-card-text class="text-muted">
      <b>
        {{ favorite.page.spaceKey }}
      </b>
    </b-card-text>
    <b-card-text>
      <i>
        {{ favoritePageContent.substring(0, 80) }}
        {{ favoritePageContent.length > 80 ? "..." : "" }}
      </i>
    </b-card-text>
  </b-card>
</template>

<script>
import { BIconStarFill, BIconFileEarmarkText } from 'bootstrap-vue';
export default {
  name: "FavoritePageCard",
  props: {
    favorite: Object,
  },
  components: {
    BIconStarFill,
    BIconFileEarmarkText
  },
  computed: {
    favoritePageContent() {
        var favoriteContent = this.favorite.page.content;
        favoriteContent = favoriteContent.replaceAll(/__|\*|\#|(?:\[([^\]]*)\]\([^)]*\))/g, "");
        return favoriteContent;
    }
  },
  methods: {
    goToPage() {
      this.$router.push(
        `/space/${this.favorite.page.spaceKey}/${this.favorite.page.id}`
      );
    },
  },
};
</script>

<style>
.favorite-page-card {
  max-width: 300px !important;
  min-width: 300px !important;
  cursor: pointer;
  margin-bottom: 1em !important;
}
.favorite-page-card .card-title {
  margin-bottom: unset !important;
  padding-bottom: unset !important;
}
.favorite-page-card .card-text {
  margin-bottom: unset !important;
  padding-bottom: unset !important;
}
</style>