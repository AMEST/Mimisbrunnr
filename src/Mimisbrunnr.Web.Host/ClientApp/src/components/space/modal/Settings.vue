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
      <b-form-checkbox v-model="this.public" switch
        >&nbsp;Is public space?</b-form-checkbox
      >
      <b-form-text>Allow visible space to all users </b-form-text>
    </div>
    <b-form-group
        label="Space status"
        description="Archive space for disable creating/updating/deleting pages"
      >
        <b-form-checkbox v-model="isArchive" switch>
          &nbsp;Archive
        </b-form-checkbox>
      </b-form-group>
    <template #modal-footer>
      <div align="left">
        <b-button v-if="space.status == 'Archived'" variant="danger" style="float:left" @click="remove">
          Remove
        </b-button>
      </div>
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
    spaceUpdateCallback: Function
  },
  data() {
    return {
      public: false,
      isArchive: false
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
      await axios.put(`/api/space/${this.space.key}`, spaceUpdateModel);
      if(this.isArchive)
        await axios.post(`/api/space/${this.space.key}/archive`);
      else
        await axios.post(`/api/space/${this.space.key}/unarchive`);

      if(this.spaceUpdateCallback != null ) await this.spaceUpdateCallback();
    },
    remove: async function() {
      await axios.delete(`/api/space/${this.space.key}`);
      this.$router.push("/");
    },
    close: function () {
      this.$bvModal.hide("space-settings-modal");
    },
  },
  watch: {
    // eslint-disable-next-line
    space: function (newValue, oldValue) {
      this.public = this.space.type == "Public";
      this.isArchive = this.space.status == "Archived";
    },
  },
  mounted: function () {
    this.public = this.space.type == "Public";
    this.isArchive = this.space.status == "Archived";
  },
};
</script>

<style>
#space-settings-modal .modal-footer {
  justify-content: space-between !important;
}
</style>