<template>
  <b-modal
    @shown="onReset"
    id="space-create-modal"
    hide-footer
    title="Create space"
  >
    <b-overlay :show="processing">
      <b-form @submit="onSubmit" @reset="onReset" v-if="show">
        <b-form-group
          id="input-group-1"
          label="Space key:"
          description="Come up with a unique short space key that does not contain spaces"
        >
          <b-form-input
            id="create-space-key"
            v-model="form.key"
            placeholder="Enter space key"
            :state="keyValidation"
            aria-describedby="create-space-key-live-feedback"
            :disabled="form.type == 'Personal'"
          ></b-form-input>
          <b-form-invalid-feedback id="create-space-key-live-feedback">
            Space key length need more then 2 symbols and lower then 12 symbols
          </b-form-invalid-feedback>
        </b-form-group>

        <b-form-group label="Your space display name:">
          <b-form-input
            id="create-space-name"
            v-model="form.name"
            placeholder="Enter name"
            :disabled="form.type == 'Personal'"
            required
          ></b-form-input>
        </b-form-group>

        <b-form-group label="Space type:">
          <b-form-select
            id="create-space-types"
            class="form-select"
            v-model="form.type"
            :options="spaceTypes"
            required
          ></b-form-select>
        </b-form-group>

        <b-form-group
          label="Space description"
          description="Write description about space"
          class="mb-0"
        >
          <b-form-textarea
            id="create-space-description"
            v-model="form.description"
            placeholder="Enter description"
          ></b-form-textarea>
        </b-form-group>

        <b-form-group
          description="Create new space and import pages from export file"
          class="mb-0"
        >
          <b-form-checkbox v-model="importEnabled">
            &nbsp;Import from another wiki?
          </b-form-checkbox>

          <b-form-file
            v-if="importEnabled"
            v-model="importFile"
            placeholder="Choose a file or drop it here..."
            drop-placeholder="Drop file here..."
            accept=".zip"
          ></b-form-file>
        </b-form-group>

        <b-button type="submit" variant="primary">Create</b-button>
        <b-button type="reset" variant="danger">Reset</b-button>
      </b-form>
    </b-overlay>
  </b-modal>
</template>

<script>
import axios from "axios";
export default {
  name: "SpaceCreateModal",
  data() {
    return {
      form: {
        key: "",
        name: "",
        type: null,
        description: "",
      },
      importEnabled: false,
      importFile: null,
      spaceTypes: [
        { text: "Select One", value: null },
        "Personal",
        "Private",
        "Public",
      ],
      show: true,
      processing: false,
    };
  },
  computed: {
    keyValidation() {
      return this.form.key.length > 1 && this.form.key.length <= 24
        ? true
        : false;
    },
  },
  methods: {
    onSubmit: async function (event) {
      event.preventDefault();
      this.processing = true;
      var response = null;
      if (this.importEnabled) response = await this.createAndImportSpace();
      else response = await this.createSpace();

      if (response.status == 200) {
        var spaceKey = this.form.key;
        this.onReset();
        this.$bvModal.hide("space-create-modal");
        this.$router.push("/space/" + spaceKey);
        return;
      }
      alert(JSON.stringify(response.data));
    },
    createSpace: function () {
      return axios.post("/api/space", this.form, {
        validateStatus: false,
      });
    },
    createAndImportSpace: function () {
      var formData = new FormData();
      formData.append("model", JSON.stringify(this.form));
      formData.append("import", this.importFile);
      return axios({
        method: "post",
        url: "/api/space/import",
        data: formData,
        validateStatus: false,
      });
    },
    // eslint-disable-next-line
    onReset(event) {
      // Reset our form values
      this.processing = true;
      this.form.key = "";
      this.form.name = "";
      this.form.type = null;
      this.form.description = "";
      this.importEnabled = false;
      this.importFile = null;
      // Trick to reset/clear native browser form validation state
      this.show = false;
      this.$nextTick(() => {
        this.show = true;
      });
    },
  },
  watch: {
    // eslint-disable-next-line
    "form.type": function (to, from) {
      if (to === "Personal") {
        this.form.key = this.$store.state.application.profile.email;
        this.form.name = this.$store.state.application.profile.name;
      }
    },
  },
};
</script>
