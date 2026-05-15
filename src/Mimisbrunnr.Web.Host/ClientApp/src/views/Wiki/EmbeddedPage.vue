<template>
  <b-container fluid class="full-size-container text-left embedded-page embedded-mode">
    <div class="pt-3">
      <PageRenderer :page="page" content-id="embedded-page-content" />
    </div>
  </b-container>
</template>

<script>
import axios from "axios";
import PageRenderer from "@/components/PageRenderer.vue";
export default {
  name: "EmbeddedPage",
  components: {
    PageRenderer,
  },
  data() {
    return {
      page: { content: "" },
    };
  },
  methods: {
    async loadPage() {
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
      setTimeout(() => {
        try{
            if(window.parent)
                window.parent.postMessage({type: "page-loaded"}, '*');
        }catch{
            //nothing
        }
      }, 200);
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
<style>
.embedded-page {
    background-color: white;
}
</style>