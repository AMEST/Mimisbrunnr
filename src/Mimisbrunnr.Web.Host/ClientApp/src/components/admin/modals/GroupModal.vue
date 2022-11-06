<template>
  <b-modal
    id="group-modal"
    :title="$t('admin.groups.modal.title')"
    @shown="onShow"
  >
    <b-form @submit.stop.prevent>
      <b-form-group :label="$t('admin.groups.modal.name')">
        <b-form-input v-model="name" required></b-form-input>
      </b-form-group>
      <b-form-group :label="$t('admin.groups.modal.description')">
        <b-form-textarea v-model="description" required></b-form-textarea>
      </b-form-group>
    </b-form>
    <template #modal-footer="{ cancel }">
      <b-button size="sm" variant="success" @click="ok()"> OK </b-button>
      <b-button size="sm" @click="cancel()"> Cancel </b-button>
    </template>
  </b-modal>
</template>

<script>
import GroupService from "@/services/groupService";
export default {
  name: "GroupModal",
  data() {
    return {
      name: "",
      description: "",
    };
  },
  methods: {
    ok: async function () {
      var created = await GroupService.createGroup(this.name, this.description);
      if (created)
        this.$bvModal.hide("group-modal");
    },
    onShow: function () {
      this.name = "";
      this.description = "";
    },
  },
};
</script>

<style>
</style>