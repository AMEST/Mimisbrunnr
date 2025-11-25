<template>
  <b-container>
    <Menu activeMenuItem="Users" />
    <b-card :title="$t('admin.users.title')" class="admin-user-card">
      <b-button
        size="sm"
        class="user-add-button"
        variant="success"
        @click="$bvModal.show('user-modal')"
        >+</b-button
      >
      <b-form-input class="search-field" v-model="searchText" size="sm" :placeholder="$t('people.search.placeholder')"></b-form-input>
      <b-table
        :items="users"
        :fields="fields"
        striped
        responsive="md"
        class="text-left users-table"
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
    <user-modal :createAction="loadUsers" />
  </b-container>
</template>

<script>
import { BIconArrowClockwise } from "bootstrap-vue";
import Menu from "@/components/admin/Menu.vue";
import ProfileService from "@/services/profileService";
import UserService from "@/services/userService";
import SearchService from "@/services/searchService";
import { debounce } from "@/services/Utils";
import UserModal from "@/components/admin/modals/UserModal.vue";
export default {
  name: "UsersAdministration",
  components: {
    Menu,
    BIconArrowClockwise,
    UserModal,
  },
  data() {
    return {
      searchText: "",
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
    search: debounce(async function () {
      var searchResult = await SearchService.findUsers(this.searchText);
      if (searchResult != null) this.users = searchResult;
    }, 300),
    promote: async function (email) {
        var approve = await this.approve(this.$t("admin.users.approveModal.promote"));
        if(!approve) return;
        var promoted = await UserService.promote(email);
        if(!promoted) return;
        this.users.filter( x => x.email == email)[0].isAdmin = true;
    },
    demote: async function (email) {
        var approve = await this.approve(this.$t("admin.users.approveModal.demote"));
        if(!approve) return;
        var demoted = await UserService.demote(email);
        if(!demoted) return;
        this.users.filter( x => x.email == email)[0].isAdmin = false;
    },
    enable: async function (email) {
        var approve = await this.approve(this.$t("admin.users.approveModal.enable"));
        if(!approve) return;
        var enabled = await UserService.enable(email);
        if(!enabled) return;
        this.users.filter( x => x.email == email)[0].enable = true;
    },
    disable: async function (email) {
        var approve = await this.approve(this.$t("admin.users.approveModal.disable"));
        if(!approve) return;
        var disabled = await UserService.disable(email);
        if(!disabled) return;
        this.users.filter( x => x.email == email)[0].enable = false;
    },
    approve: async function(message) {
        return await this.$bvModal.msgBoxConfirm(message, {
            title: this.$t("admin.users.approveModal.title"),
            centered: true,
            size: 'sm',
            buttonSize: 'sm',
            cancelTitle: this.$t("admin.users.approveModal.cancel"),
            okTitle: this.$t("admin.users.approveModal.ok"),
            okVariant: 'danger',
            headerClass: 'p-2 border-bottom-0',
            footerClass: 'p-2 border-top-0',
        });
    }
  },
  watch: {
    // eslint-disable-next-line
    searchText(newValue, oldValue) {
      if (newValue.length > 2) this.search();
      if (newValue.length == 0 && oldValue.length > 0) this.users = [];
      if (newValue.length == 0) this.loadUsers();
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
.user-add-button {
  float: right;
  margin-top: -3em;
}
</style>

<style>
.users-table table thead tr th {
    border-top: 0!important;
}
</style>
