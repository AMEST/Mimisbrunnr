<template>
  <b-table-simple style="text-align: left">
    <b-thead>
      <b-tr>
        <b-th>{{ $t("space.permissions.table.name") }}</b-th>
        <b-th>{{ $t("space.permissions.table.canView") }}</b-th>
        <b-th>{{ $t("space.permissions.table.canEdit") }}</b-th>
        <b-th>{{ $t("space.permissions.table.canDelete") }}</b-th>
        <b-th>{{ $t("space.permissions.table.admin") }}</b-th>
        <b-th class="text-right">{{
          $t("space.permissions.table.action")
        }}</b-th>
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
      <b-tr
        ><b-td>{{ $t("space.permissions.table.add") }}</b-td></b-tr
      >
      <b-tr>
        <b-td style="min-width: 200px; max-width: 350px;">
          <v-select
            v-model="permissionTarget"
            :options="permissionTargetList"
            @search="onSearch"
            :filter="permissionTargetListFilter"
            :get-option-label="(option) => option.name"
          ></v-select>
        </b-td>
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
import UserService from "@/services/userService";
import GroupService from "@/services/groupService";
import SearchService from "@/services/searchService";
import { showToast } from "@/services/Utils";
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
      permissionTarget: null,
      permissionTargetList: [],
    };
  },
  methods: {
    onSearch: async function (query) {
      console.log(query);
      if (this.type == "Group") return;
      if (query.length < 1) {
        this.permissionTargetList = [];
        this.load();
        return;
      }
      var searchResult = await SearchService.findUsers(query);
      this.permissionTargetList = searchResult;
    },
    permissionTargetListFilter: function (options, search) {
      if (this.type != "Group") return options;
      return options.filter(
        (item) => item.name.toLowerCase().indexOf(search.toLowerCase()) != -1
      );
    },
    load: async function () {
      var targets = null;
      if (this.type == "Group") {
        targets = await GroupService.getGroups();
      } else {
        targets = await UserService.getUsers(this.permissionTargetList.length);
      }
      if (targets == null) return;
      for (let target of targets) this.permissionTargetList.push(target);
    },
    save: async function (permission) {
      var spaceKey = this.$route.params.key;
      if (spaceKey == null) return;
      if (permission == null) return;
      var savePermissionRequest = await axios.put(
        `/api/space/${spaceKey}/permissions`,
        permission,
        { validateStatus: false }
      );
      if (savePermissionRequest.status == 200) await this.actionCallBack();
      else
        showToast(
          savePermissionRequest.data.message != undefined
            ? savePermissionRequest.data.message
            : JSON.stringify(savePermissionRequest.data),
          "Error when saving permission.",
          "danger"
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
        var group = await GroupService.getGroup(this.newPermission.name);
        if (group == null) {
          showToast(
            `Group with name ${this.newPermission.name} not found`,
            "Error when search group.",
            "warning"
          );
          return;
        }
        permission.group = group;
      } else {
        var profileRequest = await UserService.getUser(this.newPermission.name);
        if (profileRequest == null) {
          showToast(
            `Profile with email ${this.newPermission.name} not found.`,
            "Error when search user.",
            "warning"
          );
          return;
        }
        permission.user = profileRequest;
      }
      var addPermissionRequest = await axios.post(
        `/api/space/${spaceKey}/permissions`,
        permission,
        { validateStatus: false }
      );
      if (addPermissionRequest.status == 200) await this.actionCallBack();
      else
        showToast(
          addPermissionRequest.data.message != undefined
            ? addPermissionRequest.data.message
            : JSON.stringify(addPermissionRequest.data),
          "Error when adding permission.",
          "danger"
        );
    },
  },
  mounted() {
    this.load();
  },
  watch: {
    // eslint-disable-next-line
    permissionTarget(newValue, oldValue) {
      if (newValue == null) return;
      this.newPermission.name =
        this.type == "Group" ? newValue.name : newValue.email;
    },
  },
};
</script>

<style>
</style>