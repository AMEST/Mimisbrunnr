<template>
  <b-modal
    id="space-settings-modal"
    centered
    :title="$t('space.settings.title')"
    hide-header-close
  >
    <b-overlay :show="processing">
      <div role="group" v-if="space.type != 'Personal'">
        <label>{{ $t("space.settings.avatar.label") }}:</label>
        <br />
        <div align="center">
          <b-avatar style="" :src="this.space.avatarUrl" size="7rem"></b-avatar>
        </div>
        <b-input-group class="pt-2" style="width: 100%">
          <b-form-file
            v-model="newAttachment"
            :placeholder="$t('space.settings.avatar.placeholder')"
            drop-placeholder="Drop file here..."
            style="width: 80%"
          ></b-form-file>
          <b-input-group-append>
            <b-button @click="uploadAvatar" variant="primary"
              >{{ $t("space.settings.avatar.upload") }}
            </b-button>
          </b-input-group-append>
        </b-input-group>
        <b-form-text>{{ $t("space.settings.avatar.description") }}</b-form-text>
      </div>
      <div role="group">
        <label>{{ $t("space.settings.name.label") }}:</label>
        <b-form-input
          v-model="space.name"
          required
          :placeholder="$t('space.settings.name.placeholder')"
          :state="spaceNameState"
          trim
        ></b-form-input>
        <b-form-text>{{ $t("space.settings.name.description") }}</b-form-text>
      </div>
      <div role="group">
        <label>{{ $t("space.settings.description.label") }}:</label>
        <b-form-textarea
          v-model="space.description"
          required
          :placeholder="$t('space.settings.description.placeholder')"
          :state="spaceDescriptionState"
        ></b-form-textarea>
        <b-form-text>{{
          $t("space.settings.description.description")
        }}</b-form-text>
      </div>
      <div role="group" v-if="space.type != 'Personal'">
        <label>{{ $t("space.settings.privateType.label") }}:</label>
        <b-form-checkbox v-model="isPublic" switch
          >&nbsp;{{ $t("space.settings.privateType.content") }}</b-form-checkbox
        >
        <b-form-text>{{
          $t("space.settings.privateType.description")
        }}</b-form-text>
      </div>
      <b-form-group
        :label="$t('space.settings.status.label')"
        :description="$t('space.settings.status.description')"
      >
        <b-form-checkbox v-model="isArchive" switch>
          &nbsp;{{ $t("space.settings.status.content") }}
        </b-form-checkbox>
      </b-form-group>
    </b-overlay>
    <template #modal-footer>
      <div align="left">
        <b-button
          v-if="space.status == 'Archived'"
          variant="danger"
          style="float: left"
          @click="remove"
        >
          {{ $t("space.settings.remove") }}
        </b-button>
      </div>
      <div align="right">
        <b-button variant="warning" class="mr-05em" @click="save">
          {{ $t("space.settings.save") }}
        </b-button>
        <b-button variant="secondary" @click="close">{{
          $t("space.settings.cancel")
        }}</b-button>
      </div>
    </template>
  </b-modal>
</template>

<script>
import axios from "axios";

import { showToast, isImageFile } from "@/services/Utils.js";
export default {
  name: "Settings",
  props: {
    space: Object,
    spaceUpdateCallback: Function,
  },
  data() {
    return {
      newAttachment: null,
      isPublic: false,
      isArchive: false,
      processing: false,
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
        avatarUrl: this.space.avatarUrl
      };
      if (this.space.type != "Personal")
        spaceUpdateModel.public = this.isPublic;
      await axios.put(`/api/space/${this.space.key}`, spaceUpdateModel);
      if (this.isArchive)
        await axios.post(`/api/space/${this.space.key}/archive`);
      else await axios.post(`/api/space/${this.space.key}/unarchive`);

      if (this.spaceUpdateCallback != null) await this.spaceUpdateCallback();
    },
    remove: async function () {
      this.processing = true;
      await axios.delete(`/api/space/${this.space.key}`);
      this.processing = false;
      this.$router.push("/");
    },
    close: function () {
      this.$bvModal.hide("space-settings-modal");
    },
    uploadAvatar: async function () {
      if(!isImageFile(this.newAttachment.name)) {
        showToast("Selected file is not image.", "Upload error", "danger");
        return;
      }
      var avatarPath = `/api/attachment/${this.space.homePageId}/${this.newAttachment.name}`;
      var formData = new FormData();
      formData.append("attachment", this.newAttachment);
      await axios({
        method: "post",
        url: `/api/attachment/${this.space.homePageId}`,
        data: formData,
        validateStatus: false,
      });
      this.space.avatarUrl = avatarPath;
      await this.save();
    },
  },
  watch: {
    // eslint-disable-next-line
    space: function (newValue, oldValue) {
      this.isPublic = this.space.type == "Public";
      this.isArchive = this.space.status == "Archived";
    },
  },
  mounted: function () {
    this.isPublic = this.space.type == "Public";
    this.isArchive = this.space.status == "Archived";
  },
};
</script>

<style>
#space-settings-modal .modal-footer {
  justify-content: space-between !important;
}
</style>