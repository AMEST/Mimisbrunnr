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
            v-model="info.customCss"
            rows="5"
            max-rows="8"
          ></b-form-textarea>
        </b-form-group>
        <br />
        <b-form-group
          :label="$t('admin.general.fields.customHome.label')"
          :description="$t('admin.general.fields.customHome.description')"
        >
          <b-form-checkbox switch v-model="info.customHomepageEnabled">
            &nbsp;{{ $t("admin.general.fields.customHome.switch") }}
          </b-form-checkbox>
          <b-form-select
            v-model="info.customHomepageSpaceKey"
            :options="spaces"
            class="mr-3 mt-2"
            :disabled="!info.customHomepageEnabled"
          >
            <b-form-select-option :value="null" disabled>
            -- {{ $t("admin.general.fields.customHome.select") }} --
            </b-form-select-option>
          </b-form-select>
        </b-form-group>
      </b-card-text>
      <br />
      <b-button @click="save" variant="primary" size="sm">
        {{ $t("admin.general.save") }}
      </b-button>
    </b-card>
  </b-container>
</template>

<script>
import Menu from "@/components/admin/Menu.vue";
import axios from "axios";
import ProfileService from "@/services/profileService";
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
      customCss: "",
      customHomepageEnabled: false,
      customHomepageSpaceKey: null,
    },
    spaces: [],
  }),
  methods: {
    save: async function () {
      await axios.put("/api/admin/applicationConfiguration", this.info);
      window.location.reload();
    },
  },
  mounted: async function () {
    document.title = `${this.$store.state.application.info.title}`;
    if (!ProfileService.isAdmin()) {
      this.$router.push("/error/unauthorized");
      return;
    }
    var spacesRequest = await axios.get("/api/space");
    for (let spaceIndex in spacesRequest.data) {
      if (spacesRequest.data[spaceIndex].type != "Public") continue;
      this.spaces.push({
        value: spacesRequest.data[spaceIndex].key,
        text: `${spacesRequest.data[spaceIndex].name} (${spacesRequest.data[spaceIndex].key})`,
      });
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