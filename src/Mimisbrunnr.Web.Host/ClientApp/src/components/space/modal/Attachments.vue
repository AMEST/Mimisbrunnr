<template>
  <b-modal
    @shown="init"
    id="page-attachments-modal"
    centered
    :title="$t('page.attachments.title')"
  >
    <b-alert v-if="this.attachments.length == 0" show variant="light">{{
      $t("page.attachments.empty")
    }}</b-alert>
    <b-list-group-item
      button
      v-for="attachment in this.attachments"
      :key="attachment.name"
    >
      <span v-on:click="selectAttachment(attachment)">{{
        attachment.name
      }}</span>
      <span class="text-muted" style="float: right">
        <b-icon-trash
          v-on:click="deleteAttachment(attachment)"
          style="cursor: pointer"
        />
      </span>
    </b-list-group-item>
    <template #modal-footer>
      <b-input-group style="width: 100%">
        <b-form-file
          v-model="newAttachment"
          :placeholder="$t('page.attachments.placeholder')"
          drop-placeholder="Drop file here..."
          style="width: 80%"
        ></b-form-file>
        <b-input-group-append>
          <b-button
            @click="uploadAttachment"
            variant="primary"
            >{{ $t("page.attachments.upload") }}</b-button
          >
        </b-input-group-append>
      </b-input-group>
    </template>
  </b-modal>
</template>

<script>
import { BIconTrash } from "bootstrap-vue";
import axios from "axios";
export default {
  name: "Attachments",
  data() {
    return {
      newAttachment: null,
      attachments: [],
    };
  },
  components: {
    BIconTrash,
  },
  props: {
    page: Object,
    attachmentSelectAction: Function,
  },
  methods: {
    // eslint-disable-next-line
    init: async function (even) {
      if (this.page == null) return;
      var attachmentRequest = await axios.get(
        "/api/attachment/" + this.page.id,
        {
          validateStatus: false,
        }
      );
      if (attachmentRequest.status != 200) {
        alert(attachmentRequest.data + "\n" + attachmentRequest.status);
        return;
      }
      this.attachments = attachmentRequest.data;
    },
    selectAttachment: async function (attachment) {
      console.log("[select]", attachment);
      if (this.attachmentSelectAction != null) {
        this.attachmentSelectAction(attachment);
        return;
      }
      window.open(
        "/api/attachment/" + this.page.id + "/" + attachment.name,
        "_blank"
      );
    },
    // eslint-disable-next-line
    deleteAttachment: async function (attachment) {
      console.log("[delete]", attachment);
      await axios.delete(
        "/api/attachment/" + this.page.id + "/" + attachment.name,
        {
          validateStatus: false,
        }
      );
      await this.init();
    },
    uploadAttachment: async function () {
      var formData = new FormData();
      formData.append("attachment", this.newAttachment);
      await axios({
        method: "post",
        url: "/api/attachment/" + this.page.id,
        data: formData,
        validateStatus: false,
      });
      await this.init();
    },
  },
};
</script>

<style>
</style>