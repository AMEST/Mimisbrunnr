<template>
  <b-container fluid class="full-size-container text-left">
    <div style="float: right" class="pr-5">
      <b-button
        :to="'/space/' + this.page.spaceKey + '/' + this.page.id + '/edit'"
        variant="outline-secondary"
        size="sm"
        class="m-2"
        v-if="
          this.$store.state.application.profile &&
          this.$store.state.application.profile.isAdmin
        "
      >
        <b-icon-pencil-fill font-scale="0.9" />
        {{ $t("page.edit") }}
      </b-button>
    </div>
    <div class="pt-5 pl-5 pr-5">
      <vue-markdown
        :toc="true"
        :html="this.$store.state.application.info.allowHtml"
        :source="this.page.content"
        :postrender="postProcess"
        id="custom-home-page-content"
      ></vue-markdown>
    </div>
  </b-container>
</template>

<script>
// eslint-disable-next-line
import hljs from "highlight.js/lib/common";
import "highlight.js/styles/github.css";
import { BIconPencilFill } from "bootstrap-vue";
import { replaceRelativeLinksToRoute } from "@/services/Utils";
const VueMarkdown = () =>
  import(
    /* webpackChunkName: "vue-markdown-component" */ "@/thirdparty/VueMarkdown"
  );
import axios from "axios";
import PluginService from "@/services/pluginService";
export default {
  name: "CustomHome",
  components: {
    VueMarkdown,
    BIconPencilFill,
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
      setTimeout(replaceRelativeLinksToRoute, 100, "custom-home-page-content");
      setTimeout(async () => {
        await PluginService.renderMacroOnPage(this.page.id);
        this.scrollToAnchor();
        replaceRelativeLinksToRoute("custom-home-page-content");
      }, 200);
      return html;
    },
  },
  mounted: async function () {
    if (!this.$store.state.application.info.customHomepageEnabled) {
      this.$router.push("/dashboard");
      return;
    }

    document.title = this.$store.state.application.info.title;

    var customHomeRequest = await axios.get("/api/Customization/homepage", {
      validateStatus: false,
    });
    if (customHomeRequest.status != 200) {
      this.$router.push("/dashboard");
      return;
    }
    var pageRequest = await axios.get(
      `/api/page/${customHomeRequest.data.homepageId}`,
      {
        validateStatus: false,
      }
    );
    if (pageRequest.status != 200) {
      this.$router.push("/dashboard");
      return;
    }
    this.page = pageRequest.data;
  },
};
</script>