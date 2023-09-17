<template>
  <div :id="holder"></div>
</template>
  
<script>
import EditorJS from '@editorjs/editorjs';

export default {
  name: "EditorJsComponent",
  data() {
    return {
        editor: null
    }
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
    initialized: {
      type: Function,
      default: () => {},
    },
  },
  mounted: function () {
    this.editor = new EditorJS({
        holder: this.holder || 'editor-js',
        ...this.config
      })
      this.initialized(this.editor)
  },
};
</script>