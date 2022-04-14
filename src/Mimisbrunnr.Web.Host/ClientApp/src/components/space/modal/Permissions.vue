<template>
  <b-modal
    id="space-permissions-modal"
    centered
    size="lg"
    title="Space Permissions"
  >
    <h6>Groups</h6>
    <permissions-table :permissions="permissions.filter( x=> x.group )" :type="'Group'" :actionCallBack="getPermissions"/>
    <h6>Individual user</h6>
    <permissions-table :permissions="permissions.filter( x=> x.user )" :type="'User'" :actionCallBack="getPermissions"/>
    <template #modal-footer >
        <div align="right">
            <b-button variant="secondary" @click="close">Cancel</b-button>
        </div>
    </template>
  </b-modal>
</template>

<script>
import PermissionsTable from "@/components/space/modal/permissions/PermissionsTable.vue";
import axios from 'axios';
export default {
  name: "Permissions",
  components: {
      PermissionsTable,
  },
  data() {
    return {
      permissions: [],
    };
  },
  methods: {
    getPermissions: async function () {
      var spaceKey = this.$route.params.key;
      if (spaceKey == null) return;

      var permissionsRequest = await axios.get("/api/space/"+spaceKey+"/permissions");
      this.permissions = permissionsRequest.data;
    },
    onShow: async function () {
      await this.getPermissions();
    },
    close: function(){
        this.$bvModal.hide("space-permissions-modal");
    },
  },
  mounted: async function(){
      await this.getPermissions();
  }
};
</script>

<style>
</style>