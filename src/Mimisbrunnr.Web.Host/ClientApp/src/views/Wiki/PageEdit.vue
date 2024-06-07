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
        :toc="true"
        style="display: none"
        v-if="sideBySide"
      ></vue-markdown>
    </b-container>
    <draft-modal
      v-if="draft != null"
      :draft="draft"
      :continueCallBack="continueDraft"
      :resetCallBack="resetDraft"
    />
    <GuideModal />
  </div>
</template>

<script>
import Attachments from "@/components/space/modal/Attachments.vue";
import VueSimplemde from "@/thirdparty/VueSimpleMde.vue";
const VueMarkdown = () =>
  import(
    /* webpackChunkName: "vue-markdown-component" */ "@/thirdparty/VueMarkdown"
  );
import axios from "axios";
import { debounce, isImageFile } from "@/services/Utils.js";
import { formatMarkdownTables, insertMarkdownTableColumn, insertMarkdownTableRow } from "@/services/markdown/tableUtils";
import DraftModal from "@/components/pageEditor/DraftModal.vue";
import GuideModal from "@/components/pageEditor/GuideModal.vue";
import ProfileService from "@/services/profileService";
export default {
  name: "PageEdit",
  components: {
    VueSimplemde,
    VueMarkdown,
    Attachments,
    DraftModal,
    GuideModal,
  },
  data() {
    return {
      page: { content: "" },
      draft: null,
      loaded: false,
      sideBySide: false,
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
          {
            name: "table1",
            action: this.insertTable,
            className: "fa fa-table",
            title: "Insert table",
          },
          {
            name: "table-format",
            action: this.formatTables,
            className: "table-format",
            title: "Format table",
          },
          {
            name: "table-add-column",
            action: this.insertTableColumn,
            className: "table-add-column",
            title: "Insert table column",
          },
          {
            name: "table-add-row",
            action: this.insertTableRow,
            className: "table-add-row",
            title: "Insert table row",
          },
          "|",
          {
            name: "attachment",
            action: this.openAttachments,
            className: "fa fa-paperclip",
            title: "Add attachment",
          },
          "|",
          {
            name: "guide",
            action: this.openGuide,
            className: "fa fa-question-circle",
            title: "Show guide",
          },
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
    // eslint-disable-next-line
    openGuide: function (editor) {
      this.$bvModal.show("guide-modal");
    },
    addAttachmentLink: function (attachment) {
      var linkToAttach = `/api/attachment/${this.page.id}/${encodeURIComponent(
        attachment.name
      )}`;
      var linkElement = `[${attachment.name}](${linkToAttach})`;

      if (isImageFile(attachment.name)) linkElement = `!${linkElement}`;

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
          self.simplemde.codemirror.on("paste", self.paste);
          // eslint-disable-next-line
          self.simplemde.codemirror.on("change", (cm, ev) => self.saveDraft());
          window.cm = this.simplemde.codemirror;
        },
        1000,
        this
      );
    },
    insertTable: function () {
      this.simplemde.drawTable();
    },
    formatTables: function() {
        var cursor = this.simplemde.codemirror.getCursor();
        var formatted = formatMarkdownTables(this.page.content)
        this.page.content = "";
        setTimeout(() => {
            this.page.content = formatted;
        },100);
        setTimeout(() => this.simplemde.codemirror.setCursor({ ch: cursor.ch, line: cursor.line }), 250);
    },
    insertTableColumn: function () {
        insertMarkdownTableColumn(this.simplemde.codemirror);
    },
    insertTableRow: function () {
        insertMarkdownTableRow(this.simplemde.codemirror);
        this.formatTables();
    },
    paste: async function (codeMirror, pasteEvent) {
        var data = (pasteEvent.clipboardData || window.clipboardData).items;
        var pasted = "";

        for (var i = 0; i < data.length; i++) {
            if (data[i].type.indexOf("image") !== -1) {
            var file = data[i].getAsFile();

            var formData = new FormData();
            formData.append("attachment", file);
            await axios({
                method: "post",
                url: "/api/attachment/" + this.page.id,
                data: formData,
                validateStatus: false,
            });
            this.addAttachmentLink({ name: file.name });

            } else if (data[i].type.indexOf("text/plain") !== -1) {
                try{
                    pasted += data[i].getAsString();
                }catch (e) {
                    //nothing
                }
            }
        }
        if (pasted.length === 0) 
            return;

        var cursor = codeMirror.getCursor();
        codeMirror.setSelection(cursor, cursor);
        codeMirror.replaceSelection(pasted);
    },
    dragAndDrop: async function (codeMirror, dropEvent) {
      if (dropEvent.dataTransfer.items.length == 0) return;
      var dropItem = dropEvent.dataTransfer.items[0];
      if (dropItem.kind == "string" && dropItem.type == "text/plain") {
        dropEvent.dataTransfer.items[0].getAsString((data) => {
          if (!data.startsWith("http://") && !data.startsWith("https://"))
            return;

          var link = `[${data}](${data})`;
          if (isImageFile(data)) link = `!${link}`;
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
        this.sideBySide = false;
      } else {
        // When the preview button is clicked for the first time,
        // give some time for the transition from editor.css to fire and the view to slide from right to left,
        // instead of just appearing.
        setTimeout(function () {
          preview.className += " editor-preview-active-side";
        }, 1);
        wrapper.className += " CodeMirror-sided";
        useSideBySideListener = true;
        this.sideBySide = true;
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
        setTimeout(() => {
          preview.innerHTML = editor.options.previewRender(
            editor.value(),
            preview
          );
        }, 100);
      };

      if (!cm.sideBySideRenderingFunction) {
        cm.sideBySideRenderingFunction = sideBySideRenderingFunction;
      }

      if (useSideBySideListener) {
        sideBySideRenderingFunction();
        cm.on("update", cm.sideBySideRenderingFunction);
      } else {
        cm.off("update", cm.sideBySideRenderingFunction);
      }

      // Refresh to fix selection being off (#309)
      cm.refresh();
    },
    // eslint-disable-next-line
    previewRender: function (plainText) {
      return this.renderedMarkdown || "";
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
@import "~easymde/dist/easymde.min.css";

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

/** Icons */

.table-format {
    background-position: center !important;
    background-repeat: no-repeat !important;
    background-size: 18px !important;
    position: relative;
    top: 0.55em;
    background-image: url("data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAyNCAyNCI+CiAgPHBhdGggZmlsbD0iIzIxMjEyMSIgZD0iTTE3LjUgMTJjLjE5NiAwIC4zODkuMDEuNTc5LjAzMmwuMjgyLjA0LjE3My43MTZhMiAyIDAgMCAwIDIuMzY4IDEuNDg2bC4xNDMtLjAzNy42MDItLjE3OGE1LjUgNS41IDAgMCAxIC43NDUgMS4yODdsLjEwOC4yODMtLjQ0Ny40M2EyIDIgMCAwIDAtLjExNyAyLjc2bC4xMTcuMTIyLjQ0Ny40M2E1LjU1IDUuNTUgMCAwIDEtLjY3OCAxLjMzMWwtLjE3NS4yMzktLjYwMi0uMTc4YTIgMiAwIDAgMC0yLjQ3MSAxLjMwN2wtLjA0LjE0Mi0uMTczLjcxNmE1LjE3NCA1LjE3NCAwIDAgMS0xLjQ0LjA0bC0uMjgyLS4wNC0uMTczLS43MTZhMiAyIDAgMCAwLTIuMzY4LTEuNDg2bC0uMTQzLjAzNy0uNjAyLjE3OGE1LjUzNyA1LjUzNyAwIDAgMS0uNzQ1LTEuMjg3bC0uMTA4LS4yODIuNDQ3LS40M2EyIDIgMCAwIDAgLjExNy0yLjc2MWwtLjExNy0uMTIyLS40NDctLjQzYTUuNTUgNS41NSAwIDAgMSAuNjc4LTEuMzMxbC4xNzUtLjIzOS42MDIuMTc4YTIgMiAwIDAgMCAyLjQ3MS0xLjMwN2wuMDQtLjE0Mi4xNzItLjcxNmMuMjgtLjA0Ny41NjktLjA3Mi44NjItLjA3MlptLTYuMzI2IDRhNi41MiA2LjUyIDAgMCAwIC43MDMgNC43NjRsLjE0NS4yMzZIOS41di01aDEuNjc0Wk04IDE2djVINi4yNWEzLjI1IDMuMjUgMCAwIDEtMy4yNDUtMy4wNjZMMyAxNy43NVYxNmg1Wm05LjUgMGMtLjggMC0xLjQ1LjY3Mi0xLjQ1IDEuNVMxNi43IDE5IDE3LjUgMTljLjggMCAxLjQ1LS42NzIgMS40NS0xLjVTMTguMyAxNiAxNy41IDE2Wm0tMy02LjV2Mi4yMzJhNi41MyA2LjUzIDAgMCAwLTIuNzY4IDIuNzY4SDkuNXYtNWg1Wk04IDkuNXY1SDN2LTVoNVptMTMgMHYyLjUyMmE2LjUyIDYuNTIgMCAwIDAtNS0uODQ4VjkuNWg1Wk0xNy43NSAzYTMuMjUgMy4yNSAwIDAgMSAzLjI0NSAzLjA2NkwyMSA2LjI1VjhoLTVWM2gxLjc1Wk0xNC41IDN2NWgtNVYzaDVaTTggM3Y1SDNWNi4yNWEzLjI1IDMuMjUgMCAwIDEgMy4wNjYtMy4yNDVMNi4yNSAzSDhaIi8+Cjwvc3ZnPgo=") !important;
}

.table-add-column {
    background-position: center !important;
    background-repeat: no-repeat !important;
    background-size: 16px !important;
    position: relative;
    top: 0.55em;
    background-image: url("data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAxOCAxOCI+CiAgPHBhdGggZmlsbD0iIzQ5NGM0ZSIgZD0iTTE4IDE1YTEgMSAwIDAgMS0xIDFoLTF2MWExIDEgMCAwIDEtMiAwdi0xaC0xYTEgMSAwIDAgMSAwLTJoMXYtMWExIDEgMCAwIDEgMiAwdjFoMWExIDEgMCAwIDEgMSAxek0xNiAydjZoLTJWMmgyem0xLTJoLTRhMSAxIDAgMCAwLTEgMXY4YTEgMSAwIDAgMCAxIDFoNGExIDEgMCAwIDAgMS0xVjFhMSAxIDAgMCAwLTEtMXpNOSAwSDFhMSAxIDAgMCAwLTEgMXYxNmExIDEgMCAwIDAgMSAxaDhhMSAxIDAgMCAwIDEtMVYxYTEgMSAwIDAgMC0xLTF6TTQgMTZIMlYyaDJ2MTR6bTQgMEg2VjJoMnYxNHoiLz4KPC9zdmc+Cg==") !important;
}

.table-add-row {
    background-position: center !important;
    background-repeat: no-repeat !important;
    background-size: 16px !important;
    position: relative;
    top: 0.55em;
    background-image: url("data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAxOCAxOCI+CiAgPHBhdGggZmlsbD0iIzQ5NGM0ZSIgZD0iTTMgMTJhMSAxIDAgMCAxIDEgMXYxaDFhMSAxIDAgMCAxIDAgMkg0djFhMSAxIDAgMCAxLTIgMHYtMUgxYTEgMSAwIDAgMSAwLTJoMXYtMWExIDEgMCAwIDEgMS0xem03IDJoNnYyaC02di0yem0tMi0xdjRhMSAxIDAgMCAwIDEgMWg4YTEgMSAwIDAgMCAxLTF2LTRhMSAxIDAgMCAwLTEtMUg5YTEgMSAwIDAgMC0xIDF6TTAgMXY4YTEgMSAwIDAgMCAxIDFoMTZhMSAxIDAgMCAwIDEtMVYxYTEgMSAwIDAgMC0xLTFIMWExIDEgMCAwIDAtMSAxem0xNiA1djJIMlY2aDE0em0wLTR2MkgyVjJoMTR6Ii8+Cjwvc3ZnPgo=") !important;
}
</style>