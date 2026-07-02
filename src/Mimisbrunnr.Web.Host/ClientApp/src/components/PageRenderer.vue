<template>
  <vue-markdown
    :toc="true"
    :html="allowHtml"
    :source="content"
    :postrender="postProcess"
    :id="contentId"
  ></vue-markdown>
</template>

<script>
import hljs from "highlight.js/lib/common";
import "highlight.js/styles/github.css";
import { replaceRelativeLinksToRoute } from "@/services/Utils";
import PluginService from "@/services/pluginService";
import { setupImagePreview } from "@/services/imagePreview";

const VueMarkdown = () =>
  import(
    /* webpackChunkName: "vue-markdown-component" */ "@/thirdparty/VueMarkdown"
  );

export default {
  name: "PageRenderer",
  components: {
    VueMarkdown,
  },
  props: {
    page: {
      type: Object,
      required: true,
    },
    contentId: {
      type: String,
      default: "page-content",
    },
    disableImagePreview: {
      type: Boolean,
      default: false,
    },
  },
  data() {
    return {
      anchorScrolled: false,
    };
  },
  computed: {
    content() {
      return this.page.content;
    },
    allowHtml() {
      return this.$store.state.application.info.allowHtml;
    },
  },
  methods: {
    scrollToAnchor() {
      if (this.anchorScrolled) return;
      if (!window.location.hash) return;
      var hash = decodeURI(window.location.hash);
      if (hash.length == 1) return;
      const anchorName = hash.substring(1, hash.length);
      var anchor = document.getElementById(anchorName);
      if (!anchor) anchor = document.getElementsByName(anchorName)[0];
      if (!anchor) return;
      anchor.scrollIntoView();
      this.anchorScrolled = true;
    },
    postProcess(html) {
      setTimeout(() => hljs.highlightAll(), 100);
      setTimeout(this.scrollToAnchor, 100);
      setTimeout(replaceRelativeLinksToRoute, 100, this.contentId);
      setTimeout(async () => {
        try {
          await PluginService.renderMacroOnPage(this.page.id);
        } catch (e) {
          console.warn('Macro render failed', e);
        }
        this.scrollToAnchor();
        replaceRelativeLinksToRoute(this.contentId);
        if (!this.disableImagePreview) {
          setupImagePreview(document.getElementById(this.contentId || 'page-content'));
        }
      }, 200);
      return html;
    },
  },
  watch: {
    content() {
      this.anchorScrolled = false;
      var el = document.getElementById(this.contentId || 'page-content');
      if (el) delete el.dataset.previewInitialized;
    },
  },
};
</script>