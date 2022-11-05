<template>
  <div>
    <b-container v-if="loaded" fluid class="full-size-container text-left">
      <div class="h-100vh">
        <b-form-input
          v-model="page.name"
          :placeholder="$t('pageEditor.placeholder')"
          class="page-edit-name"
          :state="nameState"
        ></b-form-input>
        <vue-simplemde
          :configs="mdeConfig"
          v-model="page.content"
          ref="markdownEditor"
        />
        <div style="float: right; padding-right: 1em">
          <b-form-checkbox
            class="side-by-side-switch"
            size="lg"
            switch
            @change="toggleSideBySide"
            >{{ $t("pageEditor.sideBySide") }}</b-form-checkbox
          >
          <b-button
            @click="save"
            variant="primary"
            style="margin-right: 0.5em"
            :disabled="!nameState"
          >
            {{ $t("pageEditor.update") }}
          </b-button>
          <b-button @click="cancel" variant="secondary">
            {{ $t("pageEditor.close") }}
          </b-button>
        </div>
      </div>
      <attachments :attachmentSelectAction="addAttachmentLink" :page="page" />
      <vue-markdown
        :html="this.$store.state.application.info.allowHtml"
        :source="this.page.content"
        :postrender="renderMarkdown"
        style="display: none"
      ></vue-markdown>
    </b-container>
    <draft-modal
      v-if="draft != null"
      :draft="draft"
      :continueCallBack="continueDraft"
      :resetCallBack="resetDraft"
    />
  </div>
</template>

<script>
import Attachments from "@/components/space/modal/Attachments.vue";
import VueSimplemde from "vue-simplemde";
import VueMarkdown from "vue-markdown";
import axios from "axios";
import { debounce } from "@/services/Utils.js";
import DraftModal from "@/components/pageEditor/DraftModal.vue";
import ProfileService from "@/services/profileService";
export default {
  name: "PageEdit",
  components: {
    VueSimplemde,
    VueMarkdown,
    Attachments,
    DraftModal,
  },
  data() {
    return {
      page: { content: "" },
      draft: null,
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
          "|",
          {
            name: "attachment",
            action: this.openAttachments,
            className: "fa fa-paperclip",
            title: "Add attachment",
          },
          "|",
          "guide",
        ],
        spellChecker: false,
        renderedMarkdown: "",
        previewRender: this.previewRender,
      },
    };
  },
  computed: {
    nameState() {
      return this.page.name.length > 0;
    },
    simplemde() {
      return this.$refs.markdownEditor.simplemde;
    },
  },
  methods: {
    init: async function () {
      if (ProfileService.isAnonymous()) {
        this.$router.push("/error/unauthorized");
        return;
      }

      await this.loadPage();
      await this.loadDraft();
      if (this.draft != null) {
        this.$bvModal.show("draft-modal");
      } else {
        this.loaded = true;
        this.initHandlers();
      }
    },
    continueDraft: async function () {
      this.page.name = this.draft.name;
      this.page.content = this.draft.content;
      this.$bvModal.hide("draft-modal");
      this.loaded = true;
      this.initHandlers();
    },
    resetDraft: async function () {
      this.$bvModal.hide("draft-modal");
      this.draft = null;
      await axios.delete(`/api/draft/${this.$route.params.pageId}`, {
        validateStatus: false,
      });
      this.loaded = true;
      this.initHandlers();
    },
    loadPage: async function () {
      var request = await axios.get(`/api/page/${this.$route.params.pageId}`, {
        validateStatus: false,
      });
      if (request.status != 200) {
        this.$router.push("/error/unauthorized");
        return;
      }

      this.page = request.data;
    },
    loadDraft: async function () {
      var request = await axios.get(`/api/draft/${this.$route.params.pageId}`, {
        validateStatus: false,
      });

      if (request.status == 404) return;
      if (request.status != 200) {
        this.$router.push("/error/unauthorized");
        return;
      }

      this.draft = request.data;
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
    saveDraft: debounce(async function () {
      await axios.put("/api/draft/" + this.page.id, this.page, {
        validateStatus: false,
      });
    }, 1000),
    cancel: function () {
      this.$router.push("/space/" + this.page.spaceKey + "/" + this.page.id);
    },
    // eslint-disable-next-line
    openAttachments: function (editor) {
      this.$bvModal.show("page-attachments-modal");
    },
    addAttachmentLink: function (attachment) {
      var linkToAttach = `/api/attachment/${this.page.id}/${encodeURIComponent(
        attachment.name
      )}`;
      var linkElement = `[${attachment.name}](${linkToAttach})`;

      if (this.isImageFile(attachment.name)) linkElement = `!${linkElement}`;

      var cursor = this.simplemde.codemirror.getCursor();
      this.simplemde.codemirror.setSelection(cursor, cursor);
      this.simplemde.codemirror.replaceSelection(linkElement);
      this.$bvModal.hide("page-attachments-modal");
    },
    initHandlers: function () {
      document.title = `${this.page.name} - ${this.$store.state.application.info.title}`;
      setTimeout(
        (self) => {
          self.simplemde.codemirror.on("drop", self.dragAndDrop);
          // eslint-disable-next-line
          self.simplemde.codemirror.on("change", (cm, ev) => self.saveDraft());
        },
        1000,
        this
      );
    },
    dragAndDrop: async function (codeMirror, dropEvent) {
      var self = this;
      if (dropEvent.dataTransfer.items.length == 0) return;
      var dropItem = dropEvent.dataTransfer.items[0];
      if (dropItem.kind == "string" && dropItem.type == "text/plain") {
        dropEvent.dataTransfer.items[0].getAsString((data) => {
          if (!data.startsWith("http://") && !data.startsWith("https://"))
            return;

          var link = `[${data}](${data})`;
          if (self.isImageFile(data)) link = `!${link}`;
          var cursor = codeMirror.getCursor();
          codeMirror.setSelection(cursor, cursor);
          codeMirror.replaceSelection(link);
        });
        return;
      }
      if (dropItem.kind == "file") {
        var droppedFile = dropItem.getAsFile();
        var formData = new FormData();
        formData.append("attachment", droppedFile);
        await axios({
          method: "post",
          url: "/api/attachment/" + this.page.id,
          data: formData,
          validateStatus: false,
        });
        this.addAttachmentLink({ name: droppedFile.name });
        return;
      }
    },
    isImageFile(data) {
      return (
        data.toLowerCase().endsWith(".png") ||
        data.toLowerCase().endsWith(".jpg") ||
        data.toLowerCase().endsWith(".jpeg") ||
        data.toLowerCase().endsWith(".gif") ||
        data.toLowerCase().endsWith(".svg")
      );
    },
    toggleSideBySide: function toggleSideBySide() {
      var editor = this.simplemde;
      var cm = this.simplemde.codemirror;
      var wrapper = cm.getWrapperElement();
      var preview = wrapper.nextSibling;
      var useSideBySideListener = false;
      if (/editor-preview-active-side/.test(preview.className)) {
        preview.className = preview.className.replace(
          /\s*editor-preview-active-side\s*/g,
          ""
        );
        wrapper.className = wrapper.className.replace(
          /\s*CodeMirror-sided\s*/g,
          " "
        );
      } else {
        // When the preview button is clicked for the first time,
        // give some time for the transition from editor.css to fire and the view to slide from right to left,
        // instead of just appearing.
        setTimeout(function () {
          preview.className += " editor-preview-active-side";
        }, 1);
        wrapper.className += " CodeMirror-sided";
        useSideBySideListener = true;
      }

      // Hide normal preview if active
      var previewNormal = wrapper.lastChild;
      if (/editor-preview-active/.test(previewNormal.className)) {
        previewNormal.className = previewNormal.className.replace(
          /\s*editor-preview-active\s*/g,
          ""
        );
        var toolbar = editor.toolbarElements.preview;
        var toolbar_div = wrapper.previousSibling;
        toolbar.className = toolbar.className.replace(/\s*active\s*/g, "");
        toolbar_div.className = toolbar_div.className.replace(
          /\s*disabled-for-preview*/g,
          ""
        );
      }

      var sideBySideRenderingFunction = function () {
        preview.innerHTML = editor.options.previewRender(
          editor.value(),
          preview
        );
      };

      if (!cm.sideBySideRenderingFunction) {
        cm.sideBySideRenderingFunction = sideBySideRenderingFunction;
      }

      if (useSideBySideListener) {
        preview.innerHTML = editor.options.previewRender(
          editor.value(),
          preview
        );
        cm.on("update", cm.sideBySideRenderingFunction);
      } else {
        cm.off("update", cm.sideBySideRenderingFunction);
      }

      // Refresh to fix selection being off (#309)
      cm.refresh();
    },
    // eslint-disable-next-line
    previewRender: function (plainText) {
      return this.renderedMarkdown;
    },
    renderMarkdown: function (html) {
      this.renderedMarkdown = html;
      return html;
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
  height: calc(100vh - var(--page-edit-height, 240px)) !important;
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
.editor-preview-side {
  height: calc(100vh - var(--page-edit-height, 240px));
  top: 152.75px;
}
.side-by-side-switch {
  display: inline-block;
  margin-right: 2em;
  position: relative;
  top: -3px;
}
@media (max-width: 575px) {
  .side-by-side-switch {
    display: none !important;
  }
}
</style>