<template>
  <b-modal
    id="page-preview-modal"
    size="xl"
    title="Page Preview"
    hide-footer
  >
    <div class="page-preview-modal-body" v-if="htmlContent" v-html="htmlContent"></div>
  </b-modal>
</template>

<script>
export default {
  name: 'PagePreviewModal',
  props: {
    htmlContent: {
      type: String,
    }
  }
}
</script>

<style scoped>
.modal-body {
  padding: 1rem;
}
.page-preview-modal-body {
    overflow-x: auto;
}
</style>
