<template>
  <b-container>
    <Menu activeMenuItem="General" />
    <b-card :title="$t('admin.general.title')" class="admin-general-card">
      <b-card-text>
        <br />
        <b-form-group
          :description="$t('admin.general.fields.title.description')"
          :label="$t('admin.general.fields.title.label')"
        >
          <b-form-input v-model="info.title" trim></b-form-input>
        </b-form-group>
        <br />
        <b-form-group
          :label="$t('admin.general.fields.anonymous.label')"
          :description="$t('admin.general.fields.anonymous.description')"
        >
          <b-form-checkbox v-model="info.allowAnonymous" switch>
            &nbsp;{{ $t("admin.general.fields.anonymous.content") }}
          </b-form-checkbox>
        </b-form-group>
        <b-form-group
          :label="$t('admin.general.fields.markdown.label')"
          :description="$t('admin.general.fields.markdown.description')"
        >
          <b-form-checkbox v-model="info.allowHtml" switch>
            &nbsp;{{ $t("admin.general.fields.markdown.content") }}
          </b-form-checkbox>
        </b-form-group>
        <b-form-group
          :label="$t('admin.general.fields.swagger.label')"
          :description="$t('admin.general.fields.swagger.description')"
        >
          <b-form-checkbox v-model="info.swaggerEnabled" switch>
            &nbsp;{{ $t("admin.general.fields.swagger.content") }}
          </b-form-checkbox>
        </b-form-group>
        <br />
        <b-form-group
          :label="$t('admin.general.fields.customCss.label')"
          :description="$t('admin.general.fields.customCss.description')"
        >
          <b-form-textarea
            size="sm"
            :placeholder="$t('admin.general.fields.customCss.placeholder')"
            disabled
          ></b-form-textarea>
        </b-form-group>
        <br />
        <b-form-group
          :label="$t('admin.general.fields.customHome.label')"
          :description="$t('admin.general.fields.customHome.description')"
        >
          <b-form-checkbox switch disabled>
            &nbsp;{{$t('admin.general.fields.customHome.switch')}}
          </b-form-checkbox>
          <b-form-textarea
            class="mt-1"
            size="sm"
            rows="3"
            placeholder="# Welcome to wiki 
                        <hr> 
                        <h3> allowed html code </h3>"
            disabled
          >
          </b-form-textarea>
        </b-form-group>
      </b-card-text>
      <br />
      <b-button @click="save" variant="primary" size="sm"> {{$t('admin.general.save')}} </b-button>
    </b-card>
  </b-container>
</template>

<script>
import Menu from "@/components/admin/Menu.vue";
import axios from "axios";
export default {
  name: "GeneralConfiguration",
  components: {
    Menu,
  },
  data: () => ({
    info: {
      title: "",
      allowAnonymous: false,
      allowHtml: true,
      swaggerEnabled: true,
    },
  }),
  methods: {
    save: async function () {
      await axios.put("/api/admin/applicationConfiguration", this.info);
      window.location.reload();
    },
  },
  mounted: async function () {
    if (
      !this.$store.state.application.profile ||
      !this.$store.state.application.profile.isAdmin
    ) {
      this.$router.push("/error/unauthorized");
      return;
    }
    var configurationRequest = await axios.get(
      "/api/admin/applicationConfiguration"
    );
    this.info = configurationRequest.data;
  },
};
</script>

<style scoped>
.admin-general-card {
  border-top: unset !important;
  border-top-left-radius: unset !important;
  border-top-right-radius: unset !important;
  text-align: right;
}

.admin-general-card .card-title {
  text-align: left;
}

.admin-general-card p {
  text-align: left;
}

.admin-general-card .card-body {
  margin: 2.25rem 2.25rem 2.25rem 2.25rem;
}
</style>