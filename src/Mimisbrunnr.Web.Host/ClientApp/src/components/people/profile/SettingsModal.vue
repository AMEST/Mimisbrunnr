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
      <b-tab :title="$t('profile.settings.profile.title')" disabled></b-tab>
      <b-tab :title="$t('profile.settings.other.title')" disabled></b-tab>
    </b-tabs>
  </b-modal>
</template>

<script>
import ApiTokens from '@/components/people/profile/settings/ApiTokens.vue';
export default {
  components: { 
    ApiTokens 
  },
  name: "SettingsModal",
};
</script>

<style>
#profile-settings-modal .modal-body {
    padding: unset;
}
</style>