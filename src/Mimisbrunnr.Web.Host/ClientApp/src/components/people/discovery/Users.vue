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
        ></b-avatar>
        <b-card-text class="pt-2">
          <b-link :to="`/profile/${user.email}`">{{ user.name }}</b-link>
        </b-card-text>
      </b-card>
      <b-card class="people-card" @click="loadUsers">
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
import axios from "axios";
export default {
  name: "Users",
  data() {
    return {
      users: [],
      loading: false,
    };
  },
  methods: {
    getInitials: function (username) {
      if (!username) return "";
      var splited = username.split(" ");
      if (splited.length > 1) return splited[0][0] + splited[1][0];
      return splited[0][0];
    },
    loadUsers: async function () {
      this.loading = true;
      var usersRequest = await axios.get(
        `/api/user?offset=${this.users.length}`,
        { validateStatus: false }
      );
      if (usersRequest.status != 200) {
        this.$bvToast.toast(
          `status:${usersRequest.status}.${JSON.stringify(usersRequest.data)}`,
          {
            title: "Error when getting users.",
            variant: "warning",
            solid: true,
          }
        );
        this.loading = false;
        return;
      }
      for (let user of usersRequest.data) this.users.push(user);
      this.loading = false;
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
}
.people-card .avatar-bg {
  width: 72px;
  height: 72px;
}
</style>