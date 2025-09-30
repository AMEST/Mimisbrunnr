<template>
  <b-modal 
    id="profile-settings-modal" 
    :title="$t('profile.settings.title')"
    hide-footer
    centered 
    size="lg"
  >
    <b-tabs pills card vertical>
      <api-tokens/>
      <user-groups/>
      <b-tab :title="$t('profile.settings.profile.title')" disabled></b-tab>
      <b-tab :title="$t('profile.settings.other.title')" disabled></b-tab>
    </b-tabs>
  </b-modal>
</template>

<script>
import ApiTokens from '@/components/people/profile/settings/ApiTokens.vue';
import UserGroups from '@/components/people/profile/settings/UserGroups.vue';
export default {
  components: { 
    ApiTokens,
    UserGroups
  },
  name: "SettingsModal",
};
</script>

<style>
#profile-settings-modal .modal-body {
    padding: unset;
}
</style>