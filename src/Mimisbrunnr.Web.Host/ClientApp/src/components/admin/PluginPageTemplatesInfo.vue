<template>
  <details class="mt-3 text-left">
    <summary class="font-weight-bold">{{ $t("pageTemplates.title") }} ({{ templates.length }})</summary>
    <div v-for="template in templates" :key="template.templateIdentifier" class="border rounded p-3 mt-2">
      <strong>{{ template.name }}</strong>
      <p class="text-muted mb-1">{{ template.templateIdentifier }}</p>
      <p v-if="template.description">{{ template.description }}</p>
      <details>
        <summary>{{ $t("pageTemplates.content") }}</summary>
        <pre class="template-content mt-2">{{ template.content }}</pre>
      </details>
    </div>
    <p v-if="!templates.length" class="text-muted mt-2">{{ $t("pageTemplates.noTemplates") }}</p>
  </details>
</template>

<script>
export default {
  name: "PluginPageTemplatesInfo",
  props: {
    templates: { type: Array, default: () => [] },
  },
};
</script>

<style scoped>
.template-content {
  white-space: pre-wrap;
  overflow-wrap: anywhere;
}
</style>
