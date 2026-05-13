<template>
  <b-container fluid class="full-size-container text-left embedded-page embedded-mode">
    <div class="pt-3 pl-5 pr-5">
      <vue-markdown
        :toc="true"
        :html="this.$store.state.application.info.allowHtml"
        :source="page.content"
        :postrender="postProcess"
        id="embedded-page-content"
      ></vue-markdown>
    </div>
  </b-container>
</template>

<script>
import hljs from "highlight.js/lib/common";
import "highlight.js/styles/github.css";
import { replaceRelativeLinksToRoute } from "@/services/Utils";
const VueMarkdown = () =>
  import(
    /* webpackChunkName: "vue-markdown-component" */ "@/thirdparty/VueMarkdown"
  );
import axios from "axios";
import PluginService from "@/services/pluginService";
export default {
  name: "EmbeddedPage",
  components: {
    VueMarkdown,
  },
  data() {
    return {
      page: { content: "" },
      anchorScrolled: false,
    };
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
      setTimeout(replaceRelativeLinksToRoute, 100, "embedded-page-content");
      setTimeout(async () => {
        await PluginService.renderMacroOnPage(this.page.id);
        this.scrollToAnchor();
        replaceRelativeLinksToRoute("embedded-page-content");
      }, 200);
      return html;
    },
    async loadPage() {
      this.anchorScrolled = false;
      var pageRequest = await axios.get(
        `/api/page/${this.$route.params.pageId}`,
        {
          validateStatus: false,
        }
      );
      if (pageRequest.status == 401)
        this.$router.push("/error/unauthorized");
      else if (pageRequest.status == 403)
        this.$router.push("/error/forbidden");
      else if (pageRequest.status != 200){
        this.$router.push("/dashboard");
        return;
      }
      this.page = pageRequest.data;
      document.title = this.page.name;
    },
  },
  mounted: function () {
    document.getElementById("header-nav").hidden = true;
    this.loadPage();
  },
  beforeDestroy: function() {
    document.getElementById("header-nav").hidden = false;
  },
  watch: {
    // eslint-disable-next-line
    $route(to, from) {
      this.loadPage();
    },
  },
};
</script>