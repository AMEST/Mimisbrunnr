<template>
  <b-container>
    <Menu activeMenuItem="Users" />
    <b-card :title="$t('admin.users.title')" class="admin-user-card">
      <b-table
        :items="users"
        :fields="fields"
        striped
        responsive="sm"
        class="text-left"
      >
        <template #cell(name)="row">
          <s v-if="!row.item['enable']">{{ row.item["name"] }}</s>
          <span v-if="row.item['enable']">{{ row.item["name"] }}</span>
        </template>
        <template #cell(email)="row">
          <s v-if="!row.item['enable']">{{ row.item["email"] }}</s>
          <span v-if="row.item['enable']">{{ row.item["email"] }}</span>
        </template>
        <template #cell(actions)="row">
          <div class="text-right">
            <!--Enable or disable user-->
            <b-button
              v-if="!row.item['enable']"
              size="sm"
              variant="warning"
              class="mr-2"
              v-on:click="enable(row.item['email'])"
              >{{ $t("admin.users.table.enable") }}</b-button
            >
            <b-button
              v-if="row.item['enable']"
              size="sm"
              variant="warning"
              class="mr-2"
              v-on:click="disable(row.item['email'])"
              >{{ $t("admin.users.table.disable") }}</b-button
            >
            <!--Set administrator privileges or revoke-->
            <b-button
              v-if="!row.item['isAdmin']"
              size="sm"
              variant="danger"
              class="mr-2"
              v-on:click="promote(row.item['email'])"
              >{{ $t("admin.users.table.promote") }}</b-button
            >
            <b-button
              v-if="row.item['isAdmin']"
              size="sm"
              variant="danger"
              class="mr-2"
              v-on:click="demote(row.item['email'])"
              >{{ $t("admin.users.table.demote") }}</b-button
            >
          </div>
        </template>
      </b-table>
      <b-button variant="light" class="load-more-button" @click="loadUsers">
        <b-icon-arrow-clockwise
          :animation="loading ? 'spin' : 'none'"
          font-scale="1"
        />
        {{ $t("admin.users.loadMore") }}
      </b-button>
    </b-card>
  </b-container>
</template>

<script>
import { BIconArrowClockwise } from "bootstrap-vue";
import Menu from "@/components/admin/Menu.vue";
import ProfileService from "@/services/profileService";
import UserService from "@/services/userService";
export default {
  name: "UsersAdministration",
  components: {
    Menu,
    BIconArrowClockwise,
  },
  data() {
    return {
      users: [],
      loading: false,
    };
  },
  computed: {
    fields() {
      return [
        {
          key: "name",
          label: this.$t("admin.users.table.fields.name"),
        },
        {
          key: "email",
          label: this.$t("admin.users.table.fields.email"),
        },
        {
          key: "actions",
          label: this.$t("admin.users.table.fields.actions"),
        },
      ];
    },
  },
  methods: {
    loadUsers: async function () {
      this.loading = true;
      var usersList = await UserService.getUsers(this.users.length);
      if(usersList == null) {
        this.loading = false;
        return;
      }
      for (let user of usersList) this.users.push(user);
      this.loading = false;
    },
    promote: async function (email) {
        var promoted = await UserService.promote(email);
        if(!promoted) return;
        this.users.filter( x => x.email == email)[0].isAdmin = true;
    },
    demote: async function (email) {
        var demoted = await UserService.demote(email);
        if(!demoted) return;
        this.users.filter( x => x.email == email)[0].isAdmin = false;
    },
    enable: async function (email) {
        var enabled = await UserService.enable(email);
        if(!enabled) return;
        this.users.filter( x => x.email == email)[0].enable = true;
    },
    disable: async function (email) {
        var disabled = await UserService.disable(email);
        if(!disabled) return;
        this.users.filter( x => x.email == email)[0].enable = false;
    },
  },
  mounted() {
    document.title = `${this.$store.state.application.info.title}`;
    if (!ProfileService.isAdmin()) {
      this.$router.push("/error/unauthorized");
      return;
    }
    this.loadUsers();
  },
};
</script>

<style scoped>
.admin-user-card {
  border-top: unset !important;
  border-top-left-radius: unset !important;
  border-top-right-radius: unset !important;
  text-align: right;
}

.admin-user-card .card-title {
  text-align: left;
}

.admin-user-card p {
  text-align: left;
}

@media (min-width: 440px) {
    .admin-user-card .card-body {
    margin: 2.25rem 2.25rem 2.25rem 2.25rem;
    }
}

.load-more-button {
  width: 100%;
}
</style>