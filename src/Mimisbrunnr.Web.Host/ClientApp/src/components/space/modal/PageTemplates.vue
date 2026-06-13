<template>
  <b-modal id="space-page-templates-modal" :title="$t('space.actions.templates')" size="lg" hide-footer>
    <page-template-manager type="Space" :spaceKey="spaceKey" />
  </b-modal>
</template>

<script>
import PageTemplateManager from "@/components/templates/PageTemplateManager.vue";

export default {
  name: "SpacePageTemplatesModal",
  components: { PageTemplateManager },
  props: {
    spaceKey: { type: String, required: true },
  },
};
</script>
