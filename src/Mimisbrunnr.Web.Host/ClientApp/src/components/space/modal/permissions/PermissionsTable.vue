<template>
  <b-table-simple style="text-align: left">
    <b-thead>
      <b-tr>
        <b-th>{{$t("space.permissions.table.name")}}</b-th>
        <b-th>{{$t("space.permissions.table.canView")}}</b-th>
        <b-th>{{$t("space.permissions.table.canEdit")}}</b-th>
        <b-th>{{$t("space.permissions.table.canDelete")}}</b-th>
        <b-th>{{$t("space.permissions.table.admin")}}</b-th>
        <b-th class="text-right">{{$t("space.permissions.table.action")}}</b-th>
      </b-tr>
    </b-thead>
    <b-tbody>
      <b-tr v-for="(permission, index) in permissions" :key="index">
        <b-td>{{
          type == "Group"
            ? permission.group.name
            : permission.user.name + " (" + permission.user.email + ")"
        }}</b-td>
        <b-td
          ><b-form-checkbox v-model="permission.canView"></b-form-checkbox
        ></b-td>
        <b-td
          ><b-form-checkbox v-model="permission.canEdit"></b-form-checkbox
        ></b-td>
        <b-td
          ><b-form-checkbox v-model="permission.canRemove"></b-form-checkbox
        ></b-td>
        <b-td
          ><b-form-checkbox v-model="permission.isAdmin"></b-form-checkbox
        ></b-td>
        <b-td class="text-right">
          <b-icon
            v-on:click="save(permission)"
            icon="disc"
            style="cursor: pointer"
          ></b-icon>
          &nbsp;
          <b-icon
            v-on:click="deletePermission(permission)"
            icon="trash"
            style="cursor: pointer"
          ></b-icon>
        </b-td>
      </b-tr>
      <b-tr><b-td>{{$t("space.permissions.table.add")}}</b-td></b-tr>
      <b-tr>
        <b-td
          ><b-form-input
            v-model="newPermission.name"
            :placeholder="type == 'Group' ? 'Group name' : 'User Email'"
          ></b-form-input
        ></b-td>
        <b-td
          ><b-form-checkbox v-model="newPermission.canView"></b-form-checkbox
        ></b-td>
        <b-td
          ><b-form-checkbox v-model="newPermission.canEdit"></b-form-checkbox
        ></b-td>
        <b-td
          ><b-form-checkbox v-model="newPermission.canRemove"></b-form-checkbox
        ></b-td>
        <b-td
          ><b-form-checkbox v-model="newPermission.isAdmin"></b-form-checkbox
        ></b-td>
        <b-td class="text-right">
          <b-icon @click="add" icon="disc" style="cursor: pointer"></b-icon>
        </b-td>
      </b-tr>
    </b-tbody>
  </b-table-simple>
</template>

<script>
import axios from "axios";
export default {
  name: "PermissionsTable",
  props: {
    permissions: Array,
    type: String,
    actionCallBack: Function,
  },
  data() {
    return {
      newPermission: {
        name: "",
        canView: true,
        canEdit: false,
        canRemove: false,
        isAdmin: false,
      },
    };
  },
  methods: {
    save: async function (permission) {
      var spaceKey = this.$route.params.key;
      if (spaceKey == null) return;
      if (permission == null) return;
      var savePermissionRequest = await axios.put(`/api/space/${spaceKey}/permissions`, permission, { validateStatus: false });
      if (savePermissionRequest.status == 200 ) await this.actionCallBack();
      else this.$bvToast.toast(
          savePermissionRequest.data.message != undefined
            ? savePermissionRequest.data.message
            : JSON.stringify(savePermissionRequest.data),
          {
            title: "Error when saving permission.",
            variant: "danger",
            solid: true,
          }
        );
    },
    deletePermission: async function (permission) {
      var spaceKey = this.$route.params.key;
      if (spaceKey == null) return;
      if (permission == null) return;
      await axios.delete(`/api/space/${spaceKey}/permissions`, {
        data: permission,
      });
      await this.actionCallBack();
    },
    add: async function () {
      var spaceKey = this.$route.params.key;
      if (spaceKey == null) return;
      var permission = {
        canView: this.newPermission.canView,
        canEdit: this.newPermission.canEdit,
        canRemove: this.newPermission.canRemove,
        isAdmin: this.newPermission.isAdmin,
      };
      if (this.type == "Group") {
        alert("Add group permissions not implemented");
        return;
      } else {
        var profileRequest = await axios.get(
          "/api/user/" + this.newPermission.name,
          { validateStatus: false }
        );
        if (profileRequest.status != 200) {
          this.$bvToast.toast(
            `status:${profileRequest.status}.${JSON.stringify(
              profileRequest.data
            )}`,
            {
              title: "Error when search user.",
              variant: "warning",
              solid: true,
            }
          );
          return;
        }
        permission.user = profileRequest.data;
      }
      var addPermissionRequest = await axios.put(
        `/api/space/${spaceKey}/permissions`,
        permission,
        { validateStatus: false }
      );
      if (addPermissionRequest.status == 200) await this.actionCallBack();
      else
        this.$bvToast.toast(
          addPermissionRequest.data.message != undefined
            ? addPermissionRequest.data.message
            : JSON.stringify(addPermissionRequest.data),
          {
            title: "Error when adding permission.",
            variant: "danger",
            solid: true,
          }
        );
    },
  },
};
</script>

<style>
</style>