<template>
  <b-container>
    <Menu activeMenuItem="Groups" />
    <b-card :title="$t('admin.groups.title')" class="admin-group-card">
      <b-button
        size="sm"
        class="group-add-button"
        variant="success"
        @click="$bvModal.show('group-modal')"
        >+</b-button
      >
      <b-table
        :items="groups"
        :fields="fields"
        striped
        responsive="sm"
        class="text-left"
      >
        <template #cell(actions)="row">
          <div class="text-right">
            <b-button size="sm" v-on:click="row.toggleDetails()" class="mr-2">
              {{ $t("admin.groups.table.expand") }}
            </b-button>
            <b-button
              size="sm"
              variant="danger"
              v-on:click="removeGroup(row.item['name'])"
            >
              {{ $t("admin.groups.table.delete") }}
            </b-button>
          </div>
        </template>
        <template #row-details="row">
          <group-members :group="row.item['name']" />
        </template>
      </b-table>
      <b-button variant="light" class="load-more-button" @click="loadGroups">
        <b-icon-arrow-clockwise
          :animation="loading ? 'spin' : 'none'"
          font-scale="1"
        />
        {{ $t("admin.groups.loadMore") }}
      </b-button>
    </b-card>
    <group-modal :createAction="loadGroups" />
  </b-container>
</template>

<script>
import { BIconArrowClockwise } from "bootstrap-vue";
import Menu from "@/components/admin/Menu.vue";
import GroupMembers from "@/components/admin/GroupMembers.vue";
import GroupModal from "@/components/admin/modals/GroupModal.vue";
import ProfileService from "@/services/profileService";
import GroupService from "@/services/groupService";
export default {
  name: "GroupsAdministration",
  components: {
    Menu,
    GroupMembers,
    GroupModal,
    BIconArrowClockwise,
  },
  data() {
    return {
      groups: [],
      loading: false,
    };
  },
  computed: {
    fields() {
      return [
        {
          key: "name",
          label: this.$t("admin.groups.table.fields.name"),
        },
        {
          key: "description",
          label: this.$t("admin.groups.table.fields.description"),
        },
        {
          key: "actions",
          label: this.$t("admin.groups.table.fields.actions"),
        },
      ];
    },
  },
  methods: {
    loadGroups: async function () {
      this.loading = true;
      var groupsList = await GroupService.getGroups(this.groups.length);
      if (groupsList == null) {
        this.loading = false;
        return;
      }
      for (let group of groupsList) this.groups.push(group);
      this.loading = false;
    },
    removeGroup: async function (group) {
        var approve = await this.$bvModal.msgBoxConfirm(this.$t("admin.groups.approveModal.deleteGroup"), {
            title: this.$t("admin.groups.approveModal.title"),
            centered: true,
            size: 'sm',
            buttonSize: 'sm',
            cancelTitle: this.$t("admin.groups.approveModal.cancel"),
            okTitle: this.$t("admin.groups.approveModal.ok"),
            okVariant: 'danger',
        });
        if(!approve) return;
        await GroupService.deleteGroup(group);
        this.groups = [];
        this.loadGroups();
    },
  },
  mounted() {
    document.title = `${this.$store.state.application.info.title}`;
    if (!ProfileService.isAdmin()) {
      this.$router.push("/error/unauthorized");
      return;
    }
    this.loadGroups();
  },
};
</script>

<style scoped>
.admin-group-card {
  border-top: unset !important;
  border-top-left-radius: unset !important;
  border-top-right-radius: unset !important;
  text-align: right;
}

.admin-group-card .card-title {
  text-align: left;
}

.admin-group-card p {
  text-align: left;
}

@media (min-width: 440px) {
    .admin-group-card .card-body {
    margin: 2.25rem 2.25rem 2.25rem 2.25rem;
    }
}

.group-add-button {
  float: right;
  margin-top: -3em;
}
.load-more-button {
  width: 100%;
}
</style>