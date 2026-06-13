<template>
  <b-tab :title="$t('profile.settings.pageTemplates')">
    <div class="p-3">
      <page-template-manager type="User" />
    </div>
  </b-tab>
</template>

<script>
import PageTemplateManager from "@/components/templates/PageTemplateManager.vue";

export default {
  name: "ProfilePageTemplatesTab",
  components: { PageTemplateManager },
};
</script>
