<template>
  <b-modal
    id="create-from-template-modal"
    :title="$t('pageTemplates.createFromTemplate')"
    centered
    @shown="loadTemplates"
  >
    <div v-if="loading" class="text-center py-3">
      <b-spinner variant="secondary"></b-spinner>
    </div>
    <b-list-group v-else-if="templates.length > 0">
      <b-list-group-item
        v-for="tpl in templates"
        :key="tpl.id"
        button
        @click="createFromTemplate(tpl)"
      >
        <strong>{{ tpl.name }}</strong>
        <small class="text-muted"> — {{ tpl.type }}</small>
        <br />
        <small v-if="tpl.description">{{ tpl.description }}</small>
      </b-list-group-item>
    </b-list-group>
    <div v-else class="text-center text-muted py-3">
      {{ $t("pageTemplates.noTemplates") }}
    </div>
  </b-modal>
</template>

<script>
import pageTemplateService from "@/services/pageTemplateService";
import ProfileService from "@/services/profileService";

export default {
  name: "CreateFromTemplateModal",
  data() {
    return {
      loading: false,
      templates: [],
    };
  },
  methods: {
    async loadTemplates() {
      this.loading = true;
      try {
        var spaceKey = this.$route.params.key || "";
        this.templates = await pageTemplateService.getAll("", spaceKey);
      } catch (e) {
        this.templates = [];
      } finally {
        this.loading = false;
      }
    },
    async createFromTemplate(template) {
      var profile = this.$store.state.application.profile;
      var spaceKey = this.$route.params.key;
      if (!spaceKey) {
        spaceKey = await ProfileService.getOrCreatePersonalSpace(profile);
      }

      var renderResult = await pageTemplateService.render(template.id, spaceKey);
      var content = renderResult.content;

      var pageId = this.$route.params.pageId;
      if (!pageId) {
        var spaceReq = await fetch("/api/space/" + spaceKey);
        var spaceData = await spaceReq.json();
        pageId = spaceData.homePageId;
      }

      var createResult = await fetch("/api/page", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          spaceKey: spaceKey,
          parentPageId: pageId,
          name: template.name,
          content: content,
        }),
      });
      var newPage = await createResult.json();
      this.$bvModal.hide("create-from-template-modal");
      this.$router.push("/space/" + spaceKey + "/" + newPage.id + "/edit");
    },
  },
};
</script>
