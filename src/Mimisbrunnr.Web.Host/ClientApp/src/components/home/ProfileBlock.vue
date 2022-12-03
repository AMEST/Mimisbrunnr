<template>
  <div>
    <b-card
      tag="article"
      img-src="/img/gradient-1.svg"
      img-height="64px"
      img-width="100%"
      img-top
      style="max-width: 20rem"
      class="profile-card"
      v-if="this.$store.state.application.profile"
    >
      <b-card-title>
        <b-avatar
          :text="getUserInitials()"
          :src="$store.state.application.profile.avatarUrl"
          :style="$store.state.application.profile.avatarUrl ? 'background-color: white' : ''"
        ></b-avatar>
        <br />
        {{ $store.state.application.profile.name }}
      </b-card-title>
      <b-button
        class="profile-action-go"
        @click="goToPersonalSpace"
        variant="secondary"
        >{{ $t("header.profileDropdown.personalSpace") }}</b-button
      >
    </b-card>
    <b-card
      tag="article"
      style="max-width: 20rem"
      class="profile-card anonymous-block-card"
      v-else
    >
      <b-button class="profile-action-go" @click="auth" variant="secondary">{{
        $t("header.login")
      }}</b-button>
    </b-card>
  </div>
</template>

<script>
import ProfileService from "@/services/profileService";
import { getInitials } from "@/services/Utils";
export default {
  name: "ProfileBlock",
  methods: {
    auth: function () {
      window.location.href =
        "/api/account/login?redirectUri=" + window.location.pathname;
    },
    getUserInitials: function () {
      return getInitials(this.$store.state.application.profile);
    },
    goToPersonalSpace: async function () {
      var personalSpaceKey = await ProfileService.getOrCreatePersonalSpace(
        this.$store.state.application.profile
      );
      this.$router.push("/space/" + personalSpaceKey);
    },
  },
};
</script>

<style>
.profile-card {
  text-align: center;
  border-bottom: unset !important;
  border-bottom-left-radius: unset !important;
  border-bottom-right-radius: unset !important;
}
.profile-card img {
  object-fit: cover;
}
.profile-card .card-title {
  text-align: center;
}
.profile-card .b-avatar {
  border-radius: 50%;
  box-sizing: content-box;
  cursor: inherit;
  outline: none;
  overflow: hidden;
  transition: transform 200ms ease 0s, opacity 200ms ease 0s;
  box-shadow: 0 0 0 2px rgb(247 247 247);
  width: 3em;
  height: 3em;
  margin-top: -2.2em;
}
.profile-card .profile-action-go {
  font-size: smaller;
}
.anonymous-block-card .profile-action-go {
  width: 100%;
}
.anonymous-block-card .card-body {
  padding: 0.5em !important;
}
</style>