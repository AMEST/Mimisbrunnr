<template>
  <div class="text-left">
    <hr />
    <h5>{{ $t("admin.groups.table.details.members") }}</h5>
    <b-input-group prepend="Email" class="mt-3">
      <b-form-input v-model="email" type="email"></b-form-input>
      <b-input-group-append>
        <b-button variant="primary" @click="addUserToGroup">{{
          $t("admin.groups.table.details.add")
        }}</b-button>
        <b-button variant="danger" @click="removeUserFromGroup">{{
          $t("admin.groups.table.details.remove")
        }}</b-button>
      </b-input-group-append>
    </b-input-group>
    <b-table
      v-if="!loading"
      :items="members"
      :fields="groupMembersFields"
      striped
      responsive="sm"
    >
    </b-table>
  </div>
</template>

<script>
import axios from "axios";
export default {
  name: "GroupMembers",
  props: {
    group: String,
  },
  data() {
    return {
      email: "",
      members: [],
      loading: false,
    };
  },
  computed: {
    groupMembersFields() {
      return [
        {
          key: "name",
          label: this.$t("admin.groups.table.details.fields.name"),
        },
        {
          key: "email",
          label: this.$t("admin.groups.table.details.fields.email"),
        },
      ];
    },
  },
  methods: {
    loadMembers: async function () {
      var memberRequest = await axios.get(`/api/Group/${this.group}/users`, {
        validateStatus: false,
      });
      if (memberRequest.status != 200) {
        this.$bvToast.toast(
          `status:${memberRequest.status}.${JSON.stringify(
            memberRequest.data
          )}`,
          {
            title: "Error when getting group members.",
            variant: "warning",
            solid: true,
          }
        );
        return;
      }
      this.loading = true;
      this.members = memberRequest.data;
      this.$forceUpdate();
      this.loading = false;
    },
    addUserToGroup: async function () {
      var request = await axios.post(`/api/Group/${this.group}/${this.email}`, {
        validateStatus: false,
      });
      this.email = "";
      if (request.status != 200) {
        this.$bvToast.toast(
          `status:${request.status}.${JSON.stringify(request.data)}`,
          {
            title: "Error when adding member to group.",
            variant: "warning",
            solid: true,
          }
        );
        return;
      }
      await this.loadMembers();
    },
    removeUserFromGroup: async function () {
      var request = await axios.delete(
        `/api/Group/${this.group}/${this.email}`,
        {
          validateStatus: false,
        }
      );
      this.email = "";
      if (request.status != 200) {
        this.$bvToast.toast(
          `status:${request.status}.${JSON.stringify(request.data)}`,
          {
            title: "Error when deleting member from group.",
            variant: "warning",
            solid: true,
          }
        );
        return;
      }
      await this.loadMembers();
    },
  },
  mounted() {
    this.loadMembers();
  },
};
</script>

<style>
</style>