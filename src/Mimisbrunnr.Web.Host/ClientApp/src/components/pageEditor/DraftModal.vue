<template>
  <b-modal
    id="draft-modal"
    no-close-on-backdrop
    no-close-on-esc
    centered
    hide-header-close
    lazy
    :title="$t('pageEditor.draftModal.title')"
  >
    <p class="my-2">{{ $t("pageEditor.draftModal.content") }}</p>
    <p class="my-2">{{ changedText }}</p>
    <template #modal-footer>
      <b-button size="sm" variant="success" @click="continueCallBack()">
        {{ $t("pageEditor.draftModal.continue") }}
      </b-button>
      <b-button size="sm"  variant="danger" @click="resetCallBack()">
        {{ $t("pageEditor.draftModal.reset") }}
      </b-button>
    </template>
  </b-modal>
</template>

<script>
export default {
  name: "DraftModal",
  props: {
    draft: Object,
    continueCallBack: Function,
    resetCallBack: Function,
  },
  computed: {
    changedText() {
      var changedTemplate = this.$t("pageEditor.draftModal.changed");
      return changedTemplate
        .replace("{FIO}", this.draft.updatedBy.name)
        .replace("{DATE}", new Date(this.draft.updated).toLocaleString());
    },
  },
};
</script>