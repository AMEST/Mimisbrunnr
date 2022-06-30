<template>
  <b-card title="Quickstart" class="init-card">
    <b-card-text>
      <p>
        {{$t("quickStart.title")}}
      </p>
      <br />
      <b-form-group
        :description="$t('quickStart.fields.title.description')"
        :label="$t('quickStart.fields.title.label')"
      >
        <b-form-input v-model="info.title" trim></b-form-input>
      </b-form-group>
      <br />
      <b-form-group
        :label="$t('quickStart.fields.anonymous.label')"
        :description="$t('quickStart.fields.anonymous.description')"
      >
        <b-form-checkbox v-model="info.allowAnonymous" switch>
          &nbsp;{{$t('quickStart.fields.anonymous.content')}}
        </b-form-checkbox>
      </b-form-group>
      <b-form-group
        :label="$t('quickStart.fields.markdown.label')"
        :description="$t('quickStart.fields.markdown.description')"
      >
        <b-form-checkbox v-model="info.allowHtml" switch>
          &nbsp;{{$t('quickStart.fields.markdown.content')}}
        </b-form-checkbox>
      </b-form-group>
      <b-form-group
        :label="$t('quickStart.fields.swagger.label')"
        :description="$t('quickStart.fields.swagger.description')"
      >
        <b-form-checkbox v-model="info.swaggerEnabled" switch>
          &nbsp;{{$t('quickStart.fields.swagger.content')}}
        </b-form-checkbox>
      </b-form-group>
    </b-card-text>
    <br />
    <b-button @click="init" variant="primary" size="lg"> {{$t('quickStart.init')}} </b-button>
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