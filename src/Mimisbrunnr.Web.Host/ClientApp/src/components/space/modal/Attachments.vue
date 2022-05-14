<template>
  <b-modal
    @shown="init"
    id="page-attachments-modal"
    centered
    title="Attachment manager"
  >
    <b-list-group-item
      button
      v-for="attachment in this.attachments"
      :key="attachment.name"
    >
      <span v-on:click="selectAttachment(attachment)">{{
        attachment.name
      }}</span>
      <span class="text-muted" style="float: right">
        <b-icon
          v-on:click="deleteAttachment(attachment)"
          icon="trash"
          style="cursor: pointer"
        ></b-icon>
      </span>
    </b-list-group-item>
    <template #modal-footer>
      <div style="width: 100%">
        <b-form-file
          v-model="newAttachment"
          placeholder="Choose a file"
          drop-placeholder="Drop file here..."
          style="width: 80%"
        ></b-form-file>
        <b-button
          @click="uploadAttachment"
          style="margin-top: 0.5em"
          variant="primary"
          >Upload</b-button
        >
      </div>
    </template>
  </b-modal>
</template>

<script>
import axios from "axios";
export default {
  name: "Attachments",
  data() {
    return {
      newAttachment: null,
      attachments: [],
    };
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
      await axios.delete("/api/attachment/" + this.page.id + "/" + attachment.name, {
        validateStatus: false,
      });
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