<template>
  <b-modal
    @shown="onReset"
    id="space-create-modal"
    hide-footer
    :title="$t('spaceCreate.title')"
  >
    <b-overlay :show="processing">
      <b-form @submit="onSubmit" @reset="onReset" v-if="show">
        <b-form-group
          id="input-group-1"
          :label="$t('spaceCreate.fields.key.label')"
          :description="$t('spaceCreate.fields.key.description')"
        >
          <b-form-input
            id="create-space-key"
            v-model="form.key"
            :placeholder="$t('spaceCreate.fields.key.placeholder')"
            :state="keyValidation"
            aria-describedby="create-space-key-live-feedback"
            :disabled="form.type == 'Personal'"
          ></b-form-input>
          <b-form-invalid-feedback id="create-space-key-live-feedback">
            {{$t("spaceCreate.fields.key.invalid")}}
          </b-form-invalid-feedback>
        </b-form-group>

        <b-form-group :label="$t('spaceCreate.fields.name.label')">
          <b-form-input
            id="create-space-name"
            v-model="form.name"
            :placeholder="$t('spaceCreate.fields.name.placeholder')"
            :disabled="form.type == 'Personal'"
            required
          ></b-form-input>
        </b-form-group>

        <b-form-group :label="$t('spaceCreate.fields.type.label')">
          <b-form-select
            id="create-space-types"
            class="form-select"
            v-model="form.type"
            :options="spaceTypes"
            required
          ></b-form-select>
        </b-form-group>

        <b-form-group
          :label="$t('spaceCreate.fields.description.label')"
          :description="$t('spaceCreate.fields.description.description')"
          class="mb-0"
        >
          <b-form-textarea
            id="create-space-description"
            v-model="form.description"
            :placeholder="$t('spaceCreate.fields.description.placeholder')"
            required
          ></b-form-textarea>
        </b-form-group>

        <b-form-group
          :description="$t('spaceCreate.fields.import.description')"
          class="mb-0"
        >
          <b-form-checkbox v-model="importEnabled">
            &nbsp;{{$t('spaceCreate.fields.import.content')}}
          </b-form-checkbox>

          <b-form-file
            v-if="importEnabled"
            v-model="importFile"
            :placeholder="$t('spaceCreate.fields.import.placeholder')"
            :drop-placeholder="$t('spaceCreate.fields.import.dropPlaceholder')"
            accept=".zip"
          ></b-form-file>
        </b-form-group>

        <b-button type="submit" variant="primary">{{$t('spaceCreate.create')}}</b-button>
        <b-button type="reset" variant="danger">{{$t('spaceCreate.reset')}}</b-button>
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
      return this.form.key.length > 1 && this.form.key.length <= 24;
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
        setTimeout((r) => r.push("/space/" + spaceKey), 1000, this.$router)
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
      this.processing = false;
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
