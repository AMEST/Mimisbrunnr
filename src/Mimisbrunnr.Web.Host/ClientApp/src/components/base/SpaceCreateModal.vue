<template>
  <b-modal
    @shown="onReset"
    id="space-create-modal"
    hide-footer
    title="Create space"
  >
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

      <b-button type="submit" variant="primary">Create</b-button>
      <b-button type="reset" variant="danger">Reset</b-button>
    </b-form>
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
      spaceTypes: [
        { text: "Select One", value: null },
        "Personal",
        "Private",
        "Public",
      ],
      show: true,
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
      var response = await axios.post("/api/space", this.form, {
        validateStatus: false,
      });

      if (response.status == 200) {
        var spaceKey = this.form.key;
        this.onReset();
        this.$bvModal.hide("space-create-modal");
        this.$router.push("/space/" + spaceKey);
        return;
      }
      alert(JSON.stringify(response.data));
    },
    // eslint-disable-next-line
    onReset(event) {
      // Reset our form values
      this.form.key = "";
      this.form.name = "";
      this.form.type = null;
      this.form.description = "";
      // Trick to reset/clear native browser form validation state
      this.show = false;
      this.$nextTick(() => {
        this.show = true;
      });
    },
  },
  watch: {
    // eslint-disable-next-line
    "form.type": function(to, from) {
      if ( to === 'Personal' )
        this.form.key = this.$store.state.application.profile.email
        this.form.name = this.$store.state.application.profile.name
    },
  },
};
</script>
