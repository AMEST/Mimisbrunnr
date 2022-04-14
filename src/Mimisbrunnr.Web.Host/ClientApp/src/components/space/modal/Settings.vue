<template>
  <b-modal
    id="space-settings-modal"
    centered
    title="Space settings"
    hide-header-close
  >
    <div role="group">
      <label>Space name:</label>
      <b-form-input
        v-model="space.name"
        required
        placeholder="Enter space name"
        :state="spaceNameState"
        trim
      ></b-form-input>
      <b-form-text>Display space name</b-form-text>
    </div>
    <div role="group">
      <label>Space description:</label>
      <b-form-textarea
        v-model="space.description"
        required
        placeholder="Enter space description"
        :state="spaceDescriptionState"
      ></b-form-textarea>
      <b-form-text>Display space name</b-form-text>
    </div>
    <div role="group" v-if="space.type != 'Personal'">
      <label>Space private type:</label>
      <b-form-checkbox v-model="public"
        >&nbsp;Is public space?</b-form-checkbox
      >
      <b-form-text>Allow visible space to all users </b-form-text>
    </div>
    <template #modal-footer>
      <div align="right">
        <b-button variant="warning" class="mr-05em" @click="save">
          Save
        </b-button>
        <b-button variant="secondary" @click="close">Cancel</b-button>
      </div>
    </template>
  </b-modal>
</template>

<script>
import axios from "axios";
export default {
  name: "Settings",
  props: {
    space: Object,
  },
  data() {
    return {
      public: false,
    };
  },
  computed: {
    spaceNameState() {
      return this.space.name.length > 1;
    },
    spaceDescriptionState() {
      return this.space.description.length > 1;
    },
  },
  methods: {
    save: async function () {
      var spaceUpdateModel = {
        name: this.space.name,
        description: this.space.description,
      };
      if (this.space.type != "Personal") spaceUpdateModel.public = this.public;
      await axios.put("/api/space/" + this.space.key, spaceUpdateModel);
    },
    close: function () {
      this.$bvModal.hide("space-settings-modal");
    },
  },
  watch: {
    // eslint-disable-next-line
    space: function (newValue, oldValue) {
      this.public = this.space.type == "Public";
    },
  },
  mounted: function () {
    this.public = this.space.type == "Public";
  },
};
</script>

<style>
</style>