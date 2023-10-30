<template>
  <div :id="holder"></div>
</template>
  
<script>
import EditorJS from "@editorjs/editorjs";
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
import ImageTool from "@editorjs/image";
import Delimiter from "@editorjs/delimiter";

export default {
  name: "EditorJsComponent",
  data() {
    return {
      editor: null,
    };
  },
  props: {
    holder: {
      type: String,
      default: () => "editor-js",
      require: true,
    },
    config: {
      type: Object,
      default: () => ({}),
      require: true,
    },
    readOnly: {
      type: Boolean,
      default: () => false,
    },
    initialized: {
      type: Function,
      default: () => {},
    },
    imageUpload: {
        type: Function,
        default: async () => {}
    },
    onChange: {
        type: Function,
        default: () => {}
    }
  },
  mounted: function () {
    var configuration = {
      holder: this.holder || "editor-js",
      readOnly: this.readOnly,
      onChange: this.onChange,
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
        image: {
          class: ImageTool,
          config: {
            uploader: {
              uploadByFile: this.imageUpload,
              async uploadByUrl(url) {
                return { success: 1, file: { url: url } };
              },
            },
          },
        },
        delimiter: Delimiter,
      },
    };
    this.editor = new EditorJS(configuration);
    this.initialized(this.editor);
  },
};
</script>

<style>
.codex-editor__redactor {
  padding-bottom: 38px !important;
}
.ce-block__content,
.ce-toolbar__content {
  max-width: unset;
}
</style>