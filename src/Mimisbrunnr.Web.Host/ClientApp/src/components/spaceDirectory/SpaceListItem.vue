<template>
  <b-list-group-item class="space-list-item" :title="space.description" button>
    <div class="space-title" v-on:click="goToSpace()">
      <b-avatar
        class="mr-2"
        :text="getSpaceNameInitials(space.name)"
        :src="space.avatarUrl"
        :style="space.avatarUrl ? 'background-color: transparent' : ''"
      ></b-avatar>
      <b>{{ space.name }}</b>
    </div>
    <div class="space-actions">
      <b-button variant="light" @click="star">
        <b-icon v-if="inFavorite" icon="star-fill" variant="warning" />
        <b-icon v-else icon="star"
      /></b-button>
    </div>
  </b-list-group-item>
</template>

<script>
import FavoriteService from "@/services/favoriteService";
import { getNameInitials } from "@/services/Utils";
export default {
  name: "SpaceListItem",
  props: {
    space: Object,
  },
  data() {
    return {
      inFavorite: false,
    };
  },
  methods: {
    getSpaceNameInitials(name) {
      return getNameInitials(name);
    },
    goToSpace() {
      this.$router.push("/space/" + this.space.key);
    },
    star: async function(){
        if(this.inFavorite){
            await this.unStar();
        }else{
            await FavoriteService.addSpace(this.space.key);
        }
        this.inFavorite = !this.inFavorite;
    },
    unStar: async function(){
        var favorite = await FavoriteService.getSpace(this.space.key);
        if(favorite == null) return;
        await FavoriteService.delete(favorite.id);
    }
  },
  created: async function () {
    this.inFavorite = await FavoriteService.existsSpace(this.space.key);
  },
};
</script>

<style>
.space-list-item .space-actions {
  margin-left: auto;
  display: inline-block;
  float: right;
}
.space-list-item .list-group-item {
  border: unset !important;
}
.space-list-item .space-title {
  width: 85%;
  display: inline-block;
}
</style>