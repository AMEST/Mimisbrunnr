<template>
  <div>
    <b-container v-if="loaded" class="text-left editor-block">
      <b-form-input
        v-model="page.name"
        :placeholder="$t('pageEditor.placeholder')"
        class="page-edit-name"
        :state="nameState"
      ></b-form-input>
      <editor ref="editor" :config="config" :initialized="initEditor" />
    </b-container>
    <b-container class="buttons-block pb-3">
      <div align="right">
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
    </b-container>
    <attachments :attachmentSelectAction="addAttachmentLink" :page="page" />
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
import { debounce } from "@/services/Utils.js";
import DraftModal from "@/components/pageEditor/DraftModal.vue";
import ProfileService from "@/services/profileService";
import PageService from "@/services/pageService";
// EditorJs
import Header from "@editorjs/header";
import List from "@editorjs/list";
import CodeTool from "@editorjs/code";
import Paragraph from "@editorjs/paragraph";
import Embed from "@editorjs/embed";
import Table from "@editorjs/table";
import Checklist from "@editorjs/checklist";
import Marker from "@editorjs/marker";
import Warning from "@editorjs/warning";
import Quote from "@editorjs/quote";
import InlineCode from "@editorjs/inline-code";
import Delimiter from "@editorjs/delimiter";
// OwnExtensions
import {
  parseEditorJsToMarkdown,
  parseMarkdownToEditorJs,
} from "@/services/editorJs/Parser";
//===End===
export default {
  name: "PageEditEditorJs",
  components: {
    Attachments,
    DraftModal,
  },
  data() {
    return {
      page: { content: "", name: "" },
      config: {
        tools: {
          header: {
            class: Header,
            config: {
              placeholder: "Enter a header",
              levels: [2, 3, 4],
              defaultLevel: 3,
            },
          },
          list: {
            class: List,
            inlineToolbar: true,
          },
          code: {
            class: CodeTool,
          },
          paragraph: {
            class: Paragraph,
          },
          embed: {
            class: Embed,
            config: {
              services: {
                youtube: true,
                coub: true,
                imgur: true,
              },
            },
          },
          table: {
            class: Table,
            inlineToolbar: true,
            config: {
              rows: 2,
              cols: 3,
            },
          },
          checklist: {
            class: Checklist,
          },
          Marker: {
            class: Marker,
            shortcut: "CMD+SHIFT+M",
          },
          warning: {
            class: Warning,
            inlineToolbar: true,
            shortcut: "CMD+SHIFT+W",
            config: {
              titlePlaceholder: "Title",
              messagePlaceholder: "Message",
            },
          },
          quote: {
            class: Quote,
            inlineToolbar: true,
            shortcut: "CMD+SHIFT+O",
            config: {
              quotePlaceholder: "Enter a quote",
              captionPlaceholder: "Quote's author",
            },
          },
          inlineCode: {
            class: InlineCode,
            shortcut: "CMD+SHIFT+M",
          },
          delimiter: Delimiter,
        },
        onReady: () => {},
        // eslint-disable-next-line
        onChange: (args) => {},
      },
      draft: null,
      loaded: false,
    };
  },
  computed: {
    nameState() {
      return this.page.name.length > 0;
    },
    editorJs() {
      return this.$refs.editor._data.state.editor;
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
        document.title = `${this.page.name} - ${this.$store.state.application.info.title}`;
      }
    },
    continueDraft: async function () {
      this.page.name = this.draft.name;
      this.page.content = this.draft.content;
      this.$bvModal.hide("draft-modal");
      this.loaded = true;
      document.title = `${this.page.name} - ${this.$store.state.application.info.title}`;
    },
    resetDraft: async function () {
      this.$bvModal.hide("draft-modal");
      this.draft = null;
      await PageService.deleteDraft(this.$route.params.pageId);
      this.loaded = true;
    },
    loadPage: async function () {
      try {
        var page = await PageService.getPage(this.$route.params.pageId);
        if (page == null) return;
        this.page = page;
      } catch (e) {
        if (e == 401) this.$router.push("/error/unauthorized");
      }
    },
    loadDraft: async function () {
      try {
        var draft = await PageService.getDraft(this.$route.params.pageId);
        if (draft == null) return;
        this.draft = draft;
      } catch (e) {
        if (e == 401) this.$router.push("/error/unauthorized");
      }
    },
    save: async function () {
      var data = await this.editorJs.save();
      console.log(this.editorJs);
      console.log(data);
      console.log(parseEditorJsToMarkdown(data));
      return;
      var isPageSaved = await PageService.savePage(this.page);
      if (isPageSaved)
        this.$router.push(`/space/${this.page.spaceKey}/${this.page.id}`);
    },
    saveDraft: debounce(async function () {
      await PageService.saveDraft(this.page.id, this.page);
    }, 1000),
    cancel: function () {
      this.$router.push(`/space/${this.page.spaceKey}/${this.page.id}`);
    },
    // eslint-disable-next-line
    openAttachments: function (editor) {
      this.$bvModal.show("page-attachments-modal");
    },
    // eslint-disable-next-line
    addAttachmentLink: function (attachment) {},
    // eslint-disable-next-line
    initEditor: function (editor) {
      var convertedMarkdown = parseMarkdownToEditorJs(this.page.content);
      console.log(editor);
      console.log(convertedMarkdown);
      setTimeout(
        (ed, data) => {
          ed.render({
            blocks: data.filter(
              (value) => Object.keys(value).length !== 0
            ), // filter through array and remove empty objects
          });
        },
        1000,
        editor,
        convertedMarkdown
      );
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
.editor-block {
  background-color: #fff;
  box-shadow: 0 -3px 29px -5px rgba(34, 39, 47, 0.14);
  max-width: 850px;
  min-height: calc(100vh - var(--header-height) - 52.5px) !important;
}
.buttons-block {
  background-color: #fff;
  max-width: 850px;
}
.page-edit-name {
  background-color: transparent;
  border: unset;
  font-size: 1.5em;
  margin-left: 2.5em;
  width: calc(100% - 2.5em);
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