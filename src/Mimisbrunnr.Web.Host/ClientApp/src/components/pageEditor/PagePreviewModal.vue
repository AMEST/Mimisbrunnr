<template>
  <b-modal
    id="page-preview-modal"
    size="xl"
    :title="$t('pageEditor.previewModal.title')"
    hide-footer
    @hide="$emit('close', false)"
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
<style>
.page-preview-modal-body .mm-macro-block{
    display: inline-block;
}
.page-preview-modal-body p:has(+ .mm-macro-block) {
    display: inline;
}
.mm-macro-block + p {
    display: block;
}
</style>
