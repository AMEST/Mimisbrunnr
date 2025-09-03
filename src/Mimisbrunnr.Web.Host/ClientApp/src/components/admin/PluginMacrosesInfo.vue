<template>
  <div class="macros-info-container mt-3">
    <details>
      <summary class="macros-summary">
        {{ $t('admin.plugins.details.macrosInfo') }}
      </summary>
      <div class="macros-content">
        <b-table
          :items="macroses"
          :fields="fields"
          striped
          responsive="sm"
          class="text-left"
        >
          <template #cell(sendUserToken)="row">
            <b-icon-check v-if="row.item['sendUserToken']" />
            <b-icon-x v-else />
          </template>
        </b-table>
      </div>
    </details>
  </div>
</template>

<script>
import { BIconCheck, BIconX } from 'bootstrap-vue';

export default {
  name: 'PluginMacrosesInfo',
  components: {
    BIconCheck,
    BIconX
  },
  props: {
    macroses: {
      type: Array,
      required: true
    }
  },
  data() {
    return {
      fields: [
        {
          key: 'name',
          label: this.$t('admin.plugins.details.macrosName')
        },
        {
          key: 'description',
          label: this.$t('admin.plugins.details.macrosDescription')
        },
        {
          key: 'sendUserToken',
          label: this.$t('admin.plugins.details.sendUserToken')
        }
      ]
    };
  }
};
</script>

<style scoped>
.macros-info-container {
  border: 2px solid #dee2e6;
  border-radius: 0.25rem;
  background-color: #f8f9fa;
}

.macros-summary {
  font-weight: bold;
  padding: 1rem;
  background-color: #e9ecef;
  cursor: pointer;
  border-bottom: 2px solid #dee2e6;
}

.macros-summary:hover {
  background-color: #dee2e6;
}

.macros-content {
  background-color: white;
}
</style>
