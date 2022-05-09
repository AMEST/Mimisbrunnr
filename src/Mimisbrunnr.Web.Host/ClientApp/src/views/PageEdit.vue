<template>
  <b-container v-if="loaded" fluid class="full-size-container text-left">
    <div class="h-100vh">
      <b-form-input
        v-model="page.name"
        placeholder="PageName"
        class="page-edit-name"
        :state="nameState"
      ></b-form-input>
      <vue-simplemde
        :configs="mdeConfig"
        v-model="page.content"
        ref="markdownEditor"
      />
      <div style="float: right; padding-right: 1em">
        <b-button
          @click="save"
          variant="primary"
          style="margin-right: 0.5em"
          :disabled="!nameState"
        >
          Update
        </b-button>
        <b-button @click="cancel" variant="secondary"> Close </b-button>
      </div>
    </div>
  </b-container>
</template>

<script>
import VueSimplemde from "vue-simplemde";
import axios from "axios";
export default {
  name: "PageEdit",
  components: {
    VueSimplemde,
  },
  data() {
    return {
      page: { content: "" },
      loaded: false,
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
          "table",
          "fullscreen",
          "|",
          "guide",
        ],
        spellChecker: false,
      },
    };
  },
  computed: {
    nameState() {
      return this.page.name.length > 0 ? true : false;
    },
    isAnonymous() {
      return this.$store.state.application.profile == undefined;
    },
  },
  methods: {
    init: async function () {
      if (this.isAnonymous) {
        this.$router.push("/error/unauthorized");
        return;
      }
      
      var pageId = this.$route.params.pageId;

      var pageRequest = await axios.get("/api/page/" + pageId, {
        validateStatus: false,
      });
      if (pageRequest.status != 200) {
        this.$router.push("/error/unauthorized");
        return;
      }

      this.page = pageRequest.data;
      this.loaded = true;
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
    cancel: function () {
      this.$router.push("/space/" + this.page.spaceKey + "/" + this.page.id);
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
@import "~simplemde/dist/simplemde.min.css";
.vue-simplemde .CodeMirror {
  height: calc(100vh - 240px) !important;
}
.page-edit-name {
  background-color: transparent;
  border: unset;
  font-size: 1.5em;
}
</style>