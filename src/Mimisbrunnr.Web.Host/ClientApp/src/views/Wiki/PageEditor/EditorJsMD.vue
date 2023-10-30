<template>
  <div>
    <b-container v-if="loaded" class="text-left editor-block">
      <b-form-input
        v-model="page.name"
        :placeholder="$t('pageEditor.placeholder')"
        class="page-edit-name"
        :state="nameState"
      ></b-form-input>
      <EditorJsComponent
        ref="editor"
        :imageUpload="uploadByFile"
        :onChange="onEditorChange"
        :initialized="initEditor"
      />
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
import axios from "axios";
// EditorJs
import EditorJsComponent from "@/thirdparty/EditorJsComponent.vue";
// OwnExtensions
import {
  parseEditorJsToMarkdown,
  parseMarkdownToEditorJs,
} from "@/services/editorJs/Parser";
//===End===
export default {
  name: "EditorJsMD",
  components: {
    Attachments,
    DraftModal,
    EditorJsComponent
  },
  data() {
    return {
      page: { content: "", name: "" },
      cursor: {
        offset: 0,
        block: 0
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
      return this.$refs.editor.editor;
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
      /* eslint-disable */
      var isPageSaved = await PageService.savePage(this.page);
      if (isPageSaved)
        this.$router.push(`/space/${this.page.spaceKey}/${this.page.id}`);
        /* eslint-disable */
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
    uploadByFile: async function(file){
        var formData = new FormData();
        formData.append("attachment", file);
        await axios({
            method: "post",
            url: "/api/attachment/" + this.page.id,
            data: formData,
            validateStatus: false,
        });
        return {
            success: 1,
            file: {
                url: `/api/attachment/${this.page.id}/${file.name}`,
            }
        };
    },
    onEditorChange: async function(api, event){
      console.log("[evt]",event);
      console.log("[api]",api);
      var selection = document.getSelection();
      try{
        if(selection && selection.anchorOffset)
            this.cursor.offset = selection.anchorOffset();
        this.cursor.block = api.blocks.getCurrentBlockIndex();
      }catch{}
    },
    // eslint-disable-next-line
    initEditor: function (editor) {
      var content = this.page.content;
      if(!content)
        content = "Empty page";
      var convertedMarkdown = parseMarkdownToEditorJs(content);
      console.log(editor);
      window.ejs = editor;
      console.log(convertedMarkdown);
      setTimeout(this.render,1000,editor,convertedMarkdown);
    },
    render(editor, data){
        if(!editor.render){
            setTimeout(this.render, 1000, editor, data);
            return;
        }
        editor.render({
            blocks: data.filter(
              (value) => Object.keys(value).length !== 0
            ), // filter through array and remove empty objects
          });
    }
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
  min-height: calc(100vh - var(--header-height) - 52.5px) !important;
}
.buttons-block {
  background-color: #fff;
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