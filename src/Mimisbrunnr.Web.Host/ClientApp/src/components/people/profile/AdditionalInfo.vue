<template>
  <b-card class="profile-info">
    <span class="text-muted"> {{ $t("profile.additional.title") }} </span>
    <b-card-text>
      <b-input-group-prepend is-text v-on:click="goToWebSite">
        <b-icon-globe-2 /> &nbsp;
        <b-form-input
          :disabled="!this.itsMe"
          v-model="profile.website"
          v-on:change="save"
          :placeholder="$t('profile.additional.website')"
        ></b-form-input>
      </b-input-group-prepend>
      <b-input-group-prepend is-text>
        <b-icon-bag-fill /> &nbsp;
        <b-form-input
          :disabled="!this.itsMe"
          v-model="profile.post"
          v-on:change="save"
          :placeholder="$t('profile.additional.post')"
        ></b-form-input>
      </b-input-group-prepend>
      <b-input-group-prepend is-text>
        <b-icon-diagram-2 /> &nbsp;
        <b-form-input
          :disabled="!this.itsMe"
          v-model="profile.department"
          v-on:change="save"
          :placeholder="$t('profile.additional.department')"
        ></b-form-input>
      </b-input-group-prepend>
      <b-input-group-prepend is-text>
        <b-icon-building /> &nbsp;
        <b-form-input
          :disabled="!this.itsMe"
          v-model="profile.organization"
          v-on:change="save"
          :placeholder="$t('profile.additional.organization')"
        ></b-form-input>
      </b-input-group-prepend>
      <b-input-group-prepend is-text>
        <b-icon-geo-alt /> &nbsp;
        <b-form-input
          :disabled="!this.itsMe"
          v-model="profile.location"
          v-on:change="save"
          :placeholder="$t('profile.additional.location')"
        ></b-form-input>
      </b-input-group-prepend>
    </b-card-text>
  </b-card>
</template>

<script>
import {
  BIconGlobe2,
  BIconBagFill,
  BIconDiagram2,
  BIconBuilding,
  BIconGeoAlt,
} from "bootstrap-vue";
import { debounce } from "@/services/Utils.js";
import ProfileService from "@/services/profileService";
export default {
  name: "AdditionalInfo",
  props: {
    itsMe: Boolean,
    profile: Object,
  },
  components: {
    BIconGlobe2,
    BIconBagFill,
    BIconDiagram2,
    BIconBuilding,
    BIconGeoAlt,
  },
  methods: {
    goToWebSite() {
      if (this.itsMe) return;
      if (
        this.profile == null ||
        this.profile.website == null ||
        (!this.profile.website.startsWith("http://") &&
          !this.profile.website.startsWith("https://"))
      ) {
        return;
      }

      window.open(this.profile.website, "_blank");
    },
    save: debounce(async function () {
      await ProfileService.updateProfileInfo(this.profile);
    }, 1000),
  },
};
</script>

<style>
</style>