<template>
  <b-modal
    :id="modalId"
    :title="isEdit ? $t('pageTemplates.edit') : $t('pageTemplates.create')"
    hide-footer
    centered
    size="lg"
    @shown="onShown"
  >
    <b-form @submit.prevent="onSubmit">
      <b-form-group :label="$t('pageTemplates.name')">
        <b-form-input v-model="form.name" required />
      </b-form-group>
      <b-form-group :label="$t('pageTemplates.description')">
        <b-form-input v-model="form.description" />
      </b-form-group>
      <b-form-group :label="$t('pageTemplates.content')">
        <div class="d-flex template-group">
          <b-button variant="link" class="ml-2 guide-btn" @click="showGuide" :title="$t('pageTemplates.markdownGuide')">❔</b-button>
          <b-form-textarea v-model="form.content" rows="12" max-rows="24" required class="flex-grow-1" />
        </div>
      </b-form-group>
      <div class="d-flex justify-content-end">
        <b-button variant="secondary" class="mr-2" @click="close">{{ $t('pageEditor.close') }}</b-button>
        <b-button variant="primary" type="submit">{{ isEdit ? $t('space.settings.save') : $t('pageTemplates.create') }}</b-button>
      </div>
    </b-form>
    <guide-modal modal-id="guide-modal-template" />
  </b-modal>
</template>

<script>
import pageTemplateService from "@/services/pageTemplateService";
import GuideModal from "@/components/pageEditor/GuideModal.vue";

export default {
  name: "PageTemplateModal",
  components: { GuideModal },
  props: {
    modalId: { type: String, default: "page-template-modal" },
    template: { type: Object, default: null },
  },
  data() {
    return {
      form: { name: "", description: "", content: "" },
    };
  },
  computed: {
    isEdit() {
      return this.template != null;
    },
  },
  methods: {
    onShown() {
      if (this.template) {
        this.form.name = this.template.name;
        this.form.description = this.template.description || "";
        this.form.content = this.template.content;
      } else {
        this.form = { name: "", description: "", content: "" };
      }
    },
    async onSubmit() {
      try {
        if (this.isEdit) {
          await pageTemplateService.update(this.template.id, {
            name: this.form.name,
            description: this.form.description,
            content: this.form.content,
          });
        } else {
          await pageTemplateService.create({
            name: this.form.name,
            description: this.form.description,
            content: this.form.content,
            type: this.$parent.type,
            spaceKey: this.$parent.spaceKey || "",
          });
        }
        this.close();
        this.$emit("saved");
      } catch (e) {
        alert(e.message || "Error saving template");
      }
    },
    close() {
      this.$bvModal.hide(this.modalId);
    },
    showGuide() {
      this.$bvModal.show("guide-modal-template");
    },
  },
};
</script>
<style>
.template-group {
    flex-direction: column;
    align-items: self-end;
    margin-top: -2em;
}
</style>