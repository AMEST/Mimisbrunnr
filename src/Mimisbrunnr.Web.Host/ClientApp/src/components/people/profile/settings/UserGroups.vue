<template>
  <b-tab :title="$t('profile.settings.userGroups.title')" no-body>
    <b-card
      :title="$t('profile.settings.userGroups.title')"
      :sub-title="$t('profile.settings.userGroups.description')"
    >
      <b-alert v-if="this.groups.length == 0" show variant="light">{{$t('profile.settings.userGroups.empty')}}</b-alert>
      <b-table
          v-else
          :items="groups"
          :fields="fields"
          striped
          responsive="sm"
        >
      </b-table>
    </b-card>
  </b-tab>
</template>

<script>
export default {
  name: "UserGroups",
  data() {
    return {
      groups: [],
      fields: [
        {
          key: 'name',
          label: this.$t('profile.settings.userGroups.fields.name')
        },
        {
          key: 'description',
          label: this.$t('profile.settings.userGroups.fields.description')
        }
      ]
    };
  },
  methods: {
    load: async function () {
      var groupsRequest = await fetch(`/api/user/${this.$store.state.application.profile.email}/groups`);
      this.groups = await groupsRequest.json();
    },
  },
  mounted: function () {
    this.load();
  },
};
</script>