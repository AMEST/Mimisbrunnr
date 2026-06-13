<template>
  <b-container>
    <Menu activeMenuItem="Templates" />
    <b-card class="admin-templates-card">
      <page-template-manager type="System" admin-mode />
    </b-card>
  </b-container>
</template>

<script>
import Menu from "@/components/admin/Menu.vue";
import PageTemplateManager from "@/components/templates/PageTemplateManager.vue";

export default {
  name: "PageTemplates",
  components: { Menu, PageTemplateManager },
};
</script>

<style scoped>
.admin-templates-card {
  border-top: unset !important;
  border-top-left-radius: unset !important;
  border-top-right-radius: unset !important;
  text-align: right;
}

.admin-templates-card .card-title {
  text-align: left;
}

.admin-templates-card p {
  text-align: left;
}

@media (min-width: 440px) {
  .admin-templates-card .card-body {
    margin: 1.25rem 1.25rem 1.25rem 1.25rem;
  }
}
</style>
