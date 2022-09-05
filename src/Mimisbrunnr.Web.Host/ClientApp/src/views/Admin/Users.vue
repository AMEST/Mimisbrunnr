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
        <b-icon
          icon="arrow-clockwise"
          :animation="loading ? 'spin' : 'none'"
          font-scale="1"
        ></b-icon>
        {{ $t("admin.users.loadMore") }}
      </b-button>
    </b-card>
  </b-container>
</template>

<script>
import Menu from "@/components/admin/Menu.vue";
import axios from "axios";
export default {
  name: "UsersAdministration",
  components: {
    Menu,
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
      var request = await axios.get(`/api/user?offset=${this.users.length}`, {
        validateStatus: false,
      });
      if (request.status != 200) {
        this.$bvToast.toast(
          `status:${request.status}.${JSON.stringify(request.data)}`,
          {
            title: "Error when getting users.",
            variant: "warning",
            solid: true,
          }
        );
        this.loading = false;
        return;
      }
      for (let user of request.data) {
        this.users.push(user);
      }
      this.loading = false;
    },
    promote: async function (email) {
        var request = await axios.post(`/api/user/${email}/promote`, {
            validateStatus: false,
        });
        if(request.status == 200){
            this.users.filter( x => x.email == email)[0].isAdmin = true;
            return;
        }
        this.$bvToast.toast(
          `status:${request.status}.${JSON.stringify(request.data)}`,
          {
            title: "Error when promote user.",
            variant: "warning",
            solid: true,
          }
        );
    },
    demote: async function (email) {
        var request = await axios.post(`/api/user/${email}/demote`, {
            validateStatus: false,
        });
        if(request.status == 200){
            this.users.filter( x => x.email == email)[0].isAdmin = false;
            return;
        }
        this.$bvToast.toast(
          `status:${request.status}.${JSON.stringify(request.data)}`,
          {
            title: "Error when demote user.",
            variant: "warning",
            solid: true,
          }
        );
    },
    enable: async function (email) {
        var request = await axios.post(`/api/user/${email}/enable`, {
            validateStatus: false,
        });
        if(request.status == 200){
            this.users.filter( x => x.email == email)[0].enable = true;
            return;
        }
        this.$bvToast.toast(
          `status:${request.status}.${JSON.stringify(request.data)}`,
          {
            title: "Error when enable user.",
            variant: "warning",
            solid: true,
          }
        );
    },
    disable: async function (email) {
        var request = await axios.post(`/api/user/${email}/disable`, {
            validateStatus: false,
        });
        if(request.status == 200){
            this.users.filter( x => x.email == email)[0].enable = false;
            return;
        }
        this.$bvToast.toast(
          `status:${request.status}.${JSON.stringify(request.data)}`,
          {
            title: "Error when disable user.",
            variant: "warning",
            solid: true,
          }
        );
    },
  },
  mounted() {
    if (
      !this.$store.state.application.profile ||
      !this.$store.state.application.profile.isAdmin
    ) {
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

.admin-user-card .card-body {
  margin: 2.25rem 2.25rem 2.25rem 2.25rem;
}
.load-more-button {
  width: 100%;
}
</style>