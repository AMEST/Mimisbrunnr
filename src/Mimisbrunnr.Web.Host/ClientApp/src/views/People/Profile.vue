<template>
  <b-container v-if="profile">
    <b-card overlay img-src="" text-variant="white" class="profile-img-header">
    </b-card>
    <b-row class="h-100vh profile-content">
      <b-col sm="3" class="text-left">
        <b-avatar
          class="profile-avatar-bg"
          :text="getInitials()"
          :src="this.profile.avatarUrl"
          :style="this.profile.avatarUrl ? 'background-color: #f4f5f7' : ''"
        ></b-avatar>
        <div class="profile-title">
          <h4>{{ this.profile.name }}</h4>
          <span class="text-muted"> {{ this.profile.email }} </span>
        </div>
        <div class="profile-actions" v-if="this.itsMe">
          <b-button class="profile-actions-button" block variant="light" v-b-modal.profile-settings-modal>{{
            $t("profile.settings.title")
          }}</b-button>
          <b-button class="profile-actions-button" block variant="light">{{
            $t("profile.favorites")
          }}</b-button>
        </div>
        <additional-info :itsMe="this.itsMe" />
      </b-col>
      <worked-on />
    </b-row>
    <settings-modal />
  </b-container>
</template>

<script>
import WorkedOn from "@/components/people/profile/WorkedOn.vue";
import AdditionalInfo from "@/components/people/profile/AdditionalInfo.vue";
import SettingsModal from "@/components/people/profile/SettingsModal.vue";
import UserService from "@/services/userService";
import ProfileService from "@/services/profileService";
import { getInitials } from "@/services/Utils";
export default {
  components: { 
    WorkedOn,
    AdditionalInfo, 
    SettingsModal 
  },
  name: "Profile",
  data() {
    return {
      profile: null,
    };
  },
  computed: {
    itsMe() {
      return this.$store.state.application.profile.email == this.profile.email;
    },
  },
  methods: {
    getInitials: function () {
      return getInitials(this.profile);
    },
    loadProfile: async function () {
      if (!this.$route.params.email) return;
      var profile = await UserService.getUser(this.$route.params.email);
      if (profile == null) {
        this.$router.push("/error/notfound");
        return;
      }
      this.profile = profile;
      document.title = `${profile.name} - ${this.$store.state.application.info.title}`;
    },
    ensureAnonymous: function () {
      if (ProfileService.isAnonymous()) {
        this.$router.push("/error/unauthorized");
        return true;
      }
      return false;
    },
  },
  watch: {
    // eslint-disable-next-line
    "$route.params.email": function (to, from) {
      // eslint-disable-next-line
      if(this.ensureAnonymous()) return;
      this.loadProfile();
    },
  },
  mounted: function () {
    if(this.ensureAnonymous()) return;
    this.loadProfile();
  },
};
</script>

<style>
.profile-img-header {
  height: 192px;
  width: 100vw;
  max-width: 100%;
  position: absolute;
  left: 0;
  background: -webkit-linear-gradient(
    left,
    rgb(76, 154, 255) 0%,
    rgb(134, 186, 255) 40%,
    rgb(193, 219, 255) 80%,
    rgb(222, 235, 255) 100%
  );
  background: -o-linear-gradient(
    left,
    rgb(76, 154, 255) 0%,
    rgb(134, 186, 255) 40%,
    rgb(193, 219, 255) 80%,
    rgb(222, 235, 255) 100%
  );
  background: -ms-linear-gradient(
    left,
    rgb(76, 154, 255) 0%,
    rgb(134, 186, 255) 40%,
    rgb(193, 219, 255) 80%,
    rgb(222, 235, 255) 100%
  );
  background: -moz-linear-gradient(
    left,
    rgb(76, 154, 255) 0%,
    rgb(134, 186, 255) 40%,
    rgb(193, 219, 255) 80%,
    rgb(222, 235, 255) 100%
  );
  background: linear-gradient(
    to right,
    rgb(76, 154, 255) 0%,
    rgb(134, 186, 255) 40%,
    rgb(193, 219, 255) 80%,
    rgb(222, 235, 255) 100%
  );
  border: 0;
  border-radius: unset !important;
}

.profile-content {
  position: relative;
  top: 192px;
  height: calc(100vh - 56px - 192px) !important;
}

.profile-title {
  padding-top: 1em;
  padding-bottom: 2em;
}

.profile-avatar-bg {
  width: 8em;
  height: 8em;
  margin-top: -4em;
  box-shadow: 0 0 0 5px #f4f5f7;
}

.profile-avatar-bg .b-avatar-text {
  font-size: 28px;
}

.profile-actions {
  padding-bottom: 2em;
}

.profile-actions-button {
  width: 100%;
  border-color: var(--ds-background-neutral, rgba(9, 30, 66, 0.04));
  background: var(--ds-background-neutral, rgba(9, 30, 66, 0.04));
  border: unset;
  border-radius: 2px;
}

.profile-actions-button:hover {
  background: var(--ds-background-neutral-hovered, rgba(9, 30, 66, 0.08));
  text-decoration: inherit;
  transition-duration: 0s, 0.15s;
  color: var(--ds-text, #42526e) !important;
  border: unset;
  border-radius: 2px;
}

.profile-info {
  background-color: transparent;
}

.profile-info .form-control {
  background-color: transparent;
  border: unset;
}

.profile-info .input-group-text {
  background-color: transparent;
  border: unset;
}

.profile-worked-on {
  padding-top: 3em;
}
</style>