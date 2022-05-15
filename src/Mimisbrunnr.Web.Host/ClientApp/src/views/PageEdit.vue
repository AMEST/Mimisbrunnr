<template>
  <b-container v-if="loaded" fluid class="full-size-container text-left">
    <div class="h-100vh">
      <b-form-input
        v-model="page.name"
        placeholder="PageName"
        class="page-edit-name"
        :state="nameState"
      ></b-form-input>
      <vue-simplemde
        :configs="mdeConfig"
        v-model="page.content"
        ref="markdownEditor"
      />
      <div style="float: right; padding-right: 1em">
        <b-button
          @click="save"
          variant="primary"
          style="margin-right: 0.5em"
          :disabled="!nameState"
        >
          Update
        </b-button>
        <b-button @click="cancel" variant="secondary"> Close </b-button>
      </div>
    </div>
    <attachments :attachmentSelectAction="addAttachmentLink" :page="page" />
  </b-container>
</template>

<script>
import Attachments from "@/components/space/modal/Attachments.vue";
import VueSimplemde from "vue-simplemde";
import axios from "axios";
export default {
  name: "PageEdit",
  components: {
    VueSimplemde,
    Attachments
  },
  data() {
    return {
      page: { content: "" },
      loaded: false,
      mdeConfig: {
        toolbar: [
          "bold",
          "italic",
          "heading",
          "strikethrough",
          "|",
          "code",
          "quote",
          "unordered-list",
          "ordered-list",
          "clean-block",
          "|",
          "link",
          "image",
          "table",
          "fullscreen",
          "|",
          {
            name: 'attachment',
            action: this.openAttachments,
            className: 'fa fa-paperclip',
            title: 'Add attachment'
          },
          "|",
          "guide",
        ],
        spellChecker: false,
      },
    };
  },
  computed: {
    nameState() {
      return this.page.name.length > 0;
    },
    isAnonymous() {
      return this.$store.state.application.profile == undefined;
    },
    simplemde() {
      return this.$refs.markdownEditor.simplemde;
    },
  },
  methods: {
    init: async function () {
      if (this.isAnonymous) {
        this.$router.push("/error/unauthorized");
        return;
      }

      var pageId = this.$route.params.pageId;

      var pageRequest = await axios.get("/api/page/" + pageId, {
        validateStatus: false,
      });
      if (pageRequest.status != 200) {
        this.$router.push("/error/unauthorized");
        return;
      }

      this.page = pageRequest.data;
      this.loaded = true;
    },
    save: async function () {
      var pageSaveRequest = await axios.put(
        "/api/page/" + this.page.id,
        this.page,
        {
          validateStatus: false,
        }
      );
      if (pageSaveRequest.status != 200) {
        alert("Cannot save page");
        return;
      }
      this.$router.push("/space/" + this.page.spaceKey + "/" + this.page.id);
    },
    cancel: function () {
      this.$router.push("/space/" + this.page.spaceKey + "/" + this.page.id);
    },
    // eslint-disable-next-line
    openAttachments: function(editor){
      this.$bvModal.show("page-attachments-modal");
    },
    addAttachmentLink: function (attachment) {
      var linkToAttach =
        `/api/attachment/${this.page.id}/${encodeURIComponent(attachment.name)}`;
      var linkElement = `[${attachment.name}](${linkToAttach})`;

      if (
        attachment.name.toLowerCase().endsWith(".png") ||
        attachment.name.toLowerCase().endsWith(".jpg") ||
        attachment.name.toLowerCase().endsWith(".jpeg") ||
        attachment.name.toLowerCase().endsWith(".gif") ||
        attachment.name.toLowerCase().endsWith(".svg")
      )
        linkElement = `!${linkElement}`;

      var cursor = this.simplemde.codemirror.getCursor();
      this.simplemde.codemirror.setSelection(cursor, cursor);
      this.simplemde.codemirror.replaceSelection(linkElement);
      this.$bvModal.hide("page-attachments-modal");
    },
  },
  mounted: function () {
    this.init();
  },
  watch: {
    // eslint-disable-next-line
    "$route.params.pageId": function (to, from) {
      this.init();
    },
  },
};
</script>

<style>
@import "~simplemde/dist/simplemde.min.css";
.vue-simplemde .CodeMirror {
  height: calc(100vh - var(--page-edit-height,240px)) !important;
}
.page-edit-name {
  background-color: transparent;
  border: unset;
  font-size: 1.5em;
}
@media (max-width: 575px) {
  #app {
    --page-edit-height: 278px;
  }
}
@media (max-width: 497px) {
  #app {
    --page-edit-height: 298px;
  }
}
@media (max-width: 314px) {
  #app {
    --page-edit-height: 318px;
  }
}
</style>