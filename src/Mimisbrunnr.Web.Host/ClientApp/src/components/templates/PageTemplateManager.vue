<template>
  <div>
    <div class="d-flex justify-content-between align-items-center mb-3">
      <component v-if="type !== 'Space'" :is="adminMode ? 'h3' : 'h5'" :style="adminMode ? 'margin-bottom: unset;' : ''">{{ $t("pageTemplates.title") }}</component>
      <b-button v-if="!readonly" variant="primary" size="sm" @click="openCreate">
        {{ $t("pageTemplates.create") }}
      </b-button>
    </div>
    <b-table-simple responsive small>
      <b-thead>
        <b-tr>
          <b-th>{{ $t("pageTemplates.name") }}</b-th>
          <b-th>{{ $t("pageTemplates.description") }}</b-th>
          <b-th>{{ $t("pageTemplates.source") }}</b-th>
          <b-th v-if="!readonly">{{ $t("admin.users.table.fields.actions") }}</b-th>
        </b-tr>
      </b-thead>
      <b-tbody>
        <b-tr v-for="tpl in templates" :key="tpl.id">
          <b-td>{{ tpl.name }}</b-td>
          <b-td>{{ tpl.description || "" }}</b-td>
          <b-td>{{ typeLabel }}</b-td>
          <b-td v-if="!readonly">
            <b-button variant="outline-primary" size="sm" class="mr-1" @click="openEdit(tpl)">
              {{ $t("pageTemplates.edit") }}
            </b-button>
            <b-button variant="outline-danger" size="sm" @click="confirmDelete(tpl)">
              {{ $t("pageTemplates.delete") }}
            </b-button>
          </b-td>
        </b-tr>
        <b-tr v-if="templates.length === 0">
          <b-td :colspan="readonly ? 3 : 4" class="text-center text-muted">
            {{ $t("pageTemplates.noTemplates") }}
          </b-td>
        </b-tr>
      </b-tbody>
    </b-table-simple>
    <page-template-modal
      :modal-id="'page-template-modal-' + type"
      :template="editingTemplate"
      @saved="loadTemplates"
    />
  </div>
</template>

<script>
import pageTemplateService from "@/services/pageTemplateService";
import PageTemplateModal from "@/components/templates/PageTemplateModal.vue";

export default {
  name: "PageTemplateManager",
  components: { PageTemplateModal },
  props: {
    type: { type: String, default: "System" },
    spaceKey: { type: String, default: "" },
    readonly: { type: Boolean, default: false },
    adminMode: { type: Boolean, default: false },
  },
  data() {
    return {
      templates: [],
      editingTemplate: null,
    };
  },
  computed: {
    typeLabel() {
      if (this.type === "System") return this.$t("pageTemplates.system");
      if (this.type === "User") return this.$t("pageTemplates.user");
      return this.$t("pageTemplates.space");
    },
  },
  async mounted() {
    await this.loadTemplates();
  },
  methods: {
    async loadTemplates() {
      try {
        this.templates = await pageTemplateService.getAll(this.type, this.spaceKey);
      } catch (e) {
        this.templates = [];
      }
    },
    openCreate() {
      this.editingTemplate = null;
      this.$bvModal.show("page-template-modal-" + this.type);
    },
    openEdit(template) {
      this.editingTemplate = template;
      this.$bvModal.show("page-template-modal-" + this.type);
    },
    async confirmDelete(template) {
      if (confirm(this.$t("pageTemplates.deleteConfirm"))) {
        try {
          await pageTemplateService.delete(template.id);
          await this.loadTemplates();
        } catch (e) {
          alert(e.message || "Error deleting template");
        }
      }
    },
  },
};
</script>
