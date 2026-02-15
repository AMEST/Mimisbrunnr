<template>
  <b-modal 
    id="profile-settings-modal" 
    :title="$t('profile.settings.title')"
    hide-footer
    centered 
    size="lg"
  >
<b-tabs :pills="isVertical" :card="isVertical" :vertical="isVertical">
      <api-tokens/>
      <user-groups/>
      <additional-info-tab :profile="profile"/>
      <b-tab :title="$t('profile.settings.other.title')" disabled></b-tab>
    </b-tabs>
  </b-modal>
</template>

<script>
import ApiTokens from '@/components/people/profile/settings/ApiTokens.vue';
import UserGroups from '@/components/people/profile/settings/UserGroups.vue';
import AdditionalInfoTab from '@/components/people/profile/settings/AdditionalInfoTab.vue';
export default {
  components: { 
    ApiTokens,
    UserGroups,
    AdditionalInfoTab
  },
  name: "SettingsModal",
  props: {
    profile: Object,
  },
  data() {
    return {
      windowWidth: window.innerWidth
    };
  },
  computed: {
    isVertical() {
      return this.windowWidth > 440;
    }
  },
  created() {
    window.addEventListener('resize', this.updateWindowWidth);
  },
  destroyed() {
    window.removeEventListener('resize', this.updateWindowWidth);
  },
  methods: {
    updateWindowWidth() {
      this.windowWidth = window.innerWidth;
    }
  }
};
</script>

<style>
#profile-settings-modal .modal-body {
    padding: unset;
}
</style>