<template>
  <b-container>
    <Menu activeMenuItem="Groups" />
    <b-table :items="groups" :fields="fields" striped responsive="sm">
        <template #cell(actions)="row">
            <b-button size="sm" @click="row.toggleDetails" class="mr-2">
                {{ $t("admin.groups.table.expand") }}
            </b-button>
            <b-button size="sm" variant="danger">
                {{ $t("admin.groups.table.delete") }}
            </b-button>
        </template>
        <template #row-details="row">
            <span>more</span>
        </template>
    </b-table>
    <b-button variant="light" class="load-more-button" @click="loadGroups">
        <b-icon
          icon="arrow-clockwise"
          :animation="loading ? 'spin' : 'none'"
          font-scale="1"
        ></b-icon>
        {{ $t("admin.groups.loadMore") }}
    </b-button>
  </b-container>
</template>

<script>
import Menu from "@/components/admin/Menu.vue";
import axios from "axios";
export default {
    name: "GroupsAdministration",
    components: {
        Menu
    },
    data() {
        return {
            groups: [],
            loading: false
        }
    },
    computed: {
        fields() {
            return [{
                key: "name",
                label: this.$t("admin.groups.table.fields.name")
            },
            {
                key: "description",
                label: this.$t("admin.groups.table.fields.description")
            },
            {
                key: "actions",
                label: this.$t("admin.groups.table.fields.actions")
            }] 
        }
    },
    methods: {
        loadGroups: async function () {
            this.loading = true;
            var groupRequest = await axios.get(
                `/api/group?offset=${this.groups.length}`,
                { validateStatus: false }
            );
            if (groupRequest.status != 200) {
                this.$bvToast.toast(
                `status:${groupRequest.status}.${JSON.stringify(groupRequest.data)}`,
                {
                    title: "Error when getting groups.",
                    variant: "warning",
                    solid: true,
                }
                );
                this.loading = false;
                return;
            }
            for (let group of groupRequest.data) this.groups.push(group);
            this.loading = false;
        },
    },
    mounted () {
        if (
            !this.$store.state.application.profile ||
            !this.$store.state.application.profile.isAdmin
        ) {
            this.$router.push("/error/unauthorized");
            return;
        }
        this.loadGroups();
    },
}
</script>

<style>
.load-more-button {
    width: 100%;
}
</style>