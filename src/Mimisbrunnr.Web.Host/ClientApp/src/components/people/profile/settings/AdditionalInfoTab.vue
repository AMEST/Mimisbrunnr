<template>
  <b-tab :title="$t('profile.settings.profile.title')" no-body>
    <b-card
      :title="$t('profile.settings.profile.title')"
      :sub-title="$t('profile.settings.profile.description')"
    >
      <b-input-group-prepend class="info-input-block">
        <b-icon-globe-2 />
        <b-form-input
          v-model="profile.website"
          v-on:change="save"
          :placeholder="$t('profile.additional.website')"
        ></b-form-input>
      </b-input-group-prepend>
      <b-input-group-prepend class="info-input-block">
        <b-icon-bag-fill />
        <b-form-input
          v-model="profile.post"
          v-on:change="save"
          :placeholder="$t('profile.additional.post')"
        ></b-form-input>
      </b-input-group-prepend>
      <b-input-group-prepend class="info-input-block">
        <b-icon-diagram-2 />
        <b-form-input
          v-model="profile.department"
          v-on:change="save"
          :placeholder="$t('profile.additional.department')"
        ></b-form-input>
      </b-input-group-prepend>
      <b-input-group-prepend class="info-input-block">
        <b-icon-building />
        <b-form-input
          v-model="profile.organization"
          v-on:change="save"
          :placeholder="$t('profile.additional.organization')"
        ></b-form-input>
      </b-input-group-prepend>
      <b-input-group-prepend class="info-input-block">
        <b-icon-geo-alt />
        <b-form-input
          v-model="profile.location"
          v-on:change="save"
          :placeholder="$t('profile.additional.location')"
        ></b-form-input>
      </b-input-group-prepend>
    </b-card>
  </b-tab>
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
  name: "AdditionalInfoTab",
  props: {
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
    save: debounce(async function () {
      await ProfileService.updateProfileInfo(this.profile);
    }, 1000),
  },
};
</script>

<style>
.info-input-block {
    padding-bottom: 1em;
}

.info-input-block svg {
    margin-top: auto;
    margin-bottom: auto;
    margin-right: 0.5em;
}
</style>