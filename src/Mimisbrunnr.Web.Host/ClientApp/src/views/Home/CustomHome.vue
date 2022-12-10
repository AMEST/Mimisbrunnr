<template>
  <b-container fluid class="full-size-container text-left">
    <div style="float: right;" class="pr-5">
      <b-button
        :to="'/space/' + this.page.spaceKey + '/' + this.page.id + '/edit'"
        variant="outline-secondary"
        size="sm"
        class="m-2"
        v-if="this.$store.state.application.profile && this.$store.state.application.profile.isAdmin"
      >
        <b-icon icon="pencil-fill" font-scale="0.9" />
        {{ $t("page.edit") }}
      </b-button>
    </div>
    <div class="pt-5 pl-5 pr-5">
      <vue-markdown
        :html="this.$store.state.application.info.allowHtml"
        :source="this.page.content"
      ></vue-markdown>
    </div>
  </b-container>
</template>

<script>
// eslint-disable-next-line
import hljs from "highlight.js";
import "highlight.js/styles/github.css";
import VueMarkdown from "@/thirdparty/VueMarkdown";
import axios from "axios";
export default {
  name: "CustomHome",
  components: {
    VueMarkdown,
  },
  data() {
    return {
      page: { content: "" },
    };
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
    setTimeout(() => hljs.highlightAll(), 100);
  },
};
</script>