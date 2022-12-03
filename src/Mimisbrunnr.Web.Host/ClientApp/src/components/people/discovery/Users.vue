<template>
  <div>
    <h5 class="pt-5 pb-3">{{ $t("people.users.title") }}</h5>
    <div class="people-card-group">
      <b-card
        class="people-card"
        v-for="user in users"
        :key="user.email"
        v-on:click="$router.push(`/profile/${user.email}`)"
      >
        <b-avatar
          class="avatar-bg"
          :text="getInitials(user.name)"
          :src="user.avatarUrl"
          :style="user.avatarUrl ? 'background-color: transparent' : ''"
        ></b-avatar>
        <b-card-text class="pt-2">
          <b-link :to="`/profile/${user.email}`">{{ user.name }}</b-link>
        </b-card-text>
      </b-card>
      <b-card class="people-card" @click="loadUsers" v-if="!textForSearch">
        <b-icon
          icon="arrow-clockwise"
          :animation="loading ? 'spin' : 'none'"
          font-scale="6"
          class="mt-3"
        ></b-icon>
        <b-card-text>
          {{ $t("people.users.loadMore") }}
        </b-card-text>
      </b-card>
    </div>
  </div>
</template>

<script>
import UserService from "@/services/userService";
import SearchService from "@/services/searchService";
import { getNameInitials, debounce } from "@/services/Utils";
export default {
  name: "Users",
  data() {
    return {
      users: [],
      loading: false,
    };
  },
  props: {
    textForSearch: String,
  },
  methods: {
    getInitials: function (username) {
      return getNameInitials(username);
    },
    search: debounce(async function () {
      var searchResult = await SearchService.findUsers(this.textForSearch);
      if (searchResult != null) this.users = searchResult;
    }, 300),
    loadUsers: async function () {
      this.loading = true;
      var usersList = await UserService.getUsers(this.users.length);
      if (usersList == null) return;
      for (let user of usersList) this.users.push(user);
      this.loading = false;
    },
  },
  watch: {
    // eslint-disable-next-line
    textForSearch(newValue, oldValue) {
      if (newValue.length > 2) this.search();
      if (newValue.length == 0 && oldValue.length > 0) this.users = [];
      if (newValue.length == 0) this.loadUsers();
    },
  },
  mounted() {
    this.loadUsers();
  },
};
</script>

<style>
.people-card-group {
  flex: auto;
  grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
  display: grid;
}
.people-card {
  text-align: center;
  max-width: 146px !important;
  height: 180px !important;
  margin-right: 1em;
  margin-top: 1em;
  cursor: pointer;
  overflow: hidden;
}
.people-card .avatar-bg {
  width: 72px;
  height: 72px;
}
</style>