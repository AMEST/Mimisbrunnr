<template>
  <b-card title="Quickstart" class="init-card">
    <b-card-text>
      <p>
        Welcome to Mimisbrunnr wiki. For use own instance, you will need to
        initialize with some configurations
      </p>
      <br />
      <b-form-group
        description="The title of your wiki instance."
        label="Enter title"
      >
        <b-form-input v-model="info.title" trim></b-form-input>
      </b-form-group>
      <br />
      <b-form-group
        label="Publish mode"
        description="Allow unauthorized (anonymous) users to read public wiki information"
      >
        <b-form-checkbox v-model="info.allowAnonymous" switch>
          &nbsp;Allow anonymous access
        </b-form-checkbox>
      </b-form-group>
      <b-form-group
        label="Markdown settings"
        description="Allow using html inside markdown on pages"
      >
        <b-form-checkbox v-model="info.allowHtml" switch>
          &nbsp;Allow html
        </b-form-checkbox>
      </b-form-group>
      <b-form-group
        label="Application swagger"
        description="Enable swagger for view application api"
      >
        <b-form-checkbox v-model="info.swaggerEnabled" switch>
          &nbsp;Enable swagger
        </b-form-checkbox>
      </b-form-group>
    </b-card-text>
    <br />
    <b-button @click="init" variant="primary" size="lg"> Initialize </b-button>
  </b-card>
</template>

<script>
import axios from "axios";
export default {
  name: "Quickstart",
  data: () => ({
    info: {
      title: "",
      allowAnonymous: false,
      allowHtml: true,
      swaggerEnabled: true
    },
  }),
  methods: {
    init: async function () {
      await axios.post("/api/quickstart/initialize", this.info);
      window.location.reload();
    },
  },
  created: function () {
    this.info.title = this.$store.state.application.info.title;
    this.info.allowAnonymous = this.$store.state.application.info.allowAnonymous;
    this.info.allowHtml = this.$store.state.application.info.allowHtml;
    this.info.swaggerEnabled = this.$store.state.application.info.swaggerEnabled;
  },
};
</script>

<style scoped>
.init-card {
  margin-top: 3em;
  margin-left: auto;
  margin-right: auto;
  width: 460px;
}
.init-card p {
  text-align: left;
}
.init-card .card-body {
  margin: 2.25rem 2.25rem 2.25rem 2.25rem;
}
</style>