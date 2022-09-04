<template>
  <b-modal
    id="group-modal"
    :title="$t('admin.groups.modal.title')"
    @shown="onShow"
  >
    <b-form @submit.stop.prevent>
      <b-form-group :label="$t('admin.groups.modal.name')">
        <b-form-input
          v-model="name"
          required
        ></b-form-input>
      </b-form-group>
      <b-form-group :label="$t('admin.groups.modal.description')">
        <b-form-textarea
          v-model="description"
          required
        ></b-form-textarea>
      </b-form-group>
    </b-form>
    <template #modal-footer="{ cancel }">
      <b-button size="sm" variant="success" @click="ok()"> OK </b-button>
      <b-button size="sm" @click="cancel()"> Cancel </b-button>
    </template>
  </b-modal>
</template>

<script>
import axios from "axios";
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
      var request = await axios.post(
        `/api/group`,
        {
          name: this.name,
          description: this.description,
        },
        { validateStatus: false }
      );
      if (request.status == 200) {
        this.$bvModal.hide("group-modal");
        return;
      }
      this.$bvToast.toast(
        `status:${request.status}.${JSON.stringify(
            request.data
        )}`,
        {
          title: "Error when adding group.",
          variant: "warning",
          solid: true,
        }
      );
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