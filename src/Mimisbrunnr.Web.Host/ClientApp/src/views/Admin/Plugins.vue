<template>
  <b-container>
    <Menu activeMenuItem="Plugins" />
    <b-card class="admin-plugin-card">
      <div class="d-flex justify-content-between align-items-center mb-3">
        <h3>{{ $t('admin.plugins.title') }}</h3>
        <b-button variant="primary" @click="$bvModal.show('install-plugin-modal')">
          {{ $t('admin.plugins.installButton') }}
        </b-button>
      </div>
      <b-table
        :items="plugins"
        :fields="fields"
        striped
        responsive="sm"
        class="text-left"
      >
        <template #cell(name)="row">
          <s v-if="row.item['disabled']">{{ row.item["name"] }}</s>
          <span v-if="!row.item['disabled']">{{ row.item["name"] }}</span>
        </template>
        <template #cell(version)="row">
          <s v-if="row.item['disabled']">{{ row.item["version"] }}</s>
          <span v-if="!row.item['disabled']">{{ row.item["version"] }}</span>
        </template>
        <template #cell(actions)="row">
          <div class="text-right">
            <b-button
              size="sm"
              variant="info"
              @click="row.toggleDetails()"
              class="mr-2"
            >
              {{ $t("admin.plugins.table.details") }}
            </b-button>
          </div>
        </template>
        
        <template #row-details="row">
          <b-card>
            <p><strong>{{ $t('admin.plugins.details.identifier') }}:</strong> {{ row.item.pluginIdentifier }}</p>
            <p><strong>{{ $t('admin.plugins.details.installedBy') }}:</strong> {{ row.item.installedBy.name }} ( {{ row.item.installedBy.email }} )</p>
            <p><strong>{{ $t('admin.plugins.details.installedAt') }}:</strong> {{ formatDate(row.item.installation) }}</p>
            
            <PluginMacrosesInfo :macroses="row.item.macros || []" />
            
            <div class="text-right mt-3">
              <b-button
                v-if="row.item['disabled']"
                size="sm"
                variant="success"
                class="mr-2"
                @click="enablePlugin(row.item['pluginIdentifier'])"
              >
                {{ $t("admin.plugins.table.enable") }}
              </b-button>
              <b-button
                v-if="!row.item['disabled']"
                size="sm"
                variant="warning"
                class="mr-2"
                @click="disablePlugin(row.item['pluginIdentifier'])"
              >
                {{ $t("admin.plugins.table.disable") }}
              </b-button>
              <b-button
                size="sm"
                variant="danger"
                @click="uninstallPlugin(row.item['pluginIdentifier'])"
              >
                {{ $t("admin.plugins.table.uninstall") }}
              </b-button>
            </div>
          </b-card>
        </template>
      </b-table>
      <div class="text-right">
        <b-button variant="light" class="load-more-button" @click="loadPlugins">
          <b-icon-arrow-clockwise
            :animation="loading ? 'spin' : 'none'"
            font-scale="1"
          />
          {{ $t("admin.plugins.loadMore") }}
        </b-button>
      </div>
      <InstallPluginModal @install="installPlugin" />
    </b-card>
  </b-container>
</template>

<script>
import { BIconArrowClockwise } from "bootstrap-vue";
import Menu from "@/components/admin/Menu.vue";
import PluginService from "@/services/pluginService";
import PluginMacrosesInfo from "@/components/admin/PluginMacrosesInfo.vue";
import InstallPluginModal from "@/components/admin/modals/InstallPluginModal.vue";

export default {
  name: "PluginsAdministration",
  components: {
    Menu,
    BIconArrowClockwise,
    PluginMacrosesInfo,
    InstallPluginModal
  },
  data() {
    return {
      plugins: [],
      loading: false,
    };
  },
  computed: {
    fields() {
      return [
        {
          key: "name",
          label: this.$t("admin.plugins.table.fields.name"),
        },
        {
          key: "version", 
          label: this.$t("admin.plugins.table.fields.version"),
        },
        {
          key: "actions",
          label: this.$t("admin.plugins.table.fields.actions"),
        },
      ];
    },
  },
  methods: {
    async loadPlugins() {
      this.loading = true;
      const pluginsList = await PluginService.getPlugins(this.plugins.length, 10);
      if (pluginsList) {
        this.plugins.push(...pluginsList);
      }
      this.loading = false;
    },
    async enablePlugin(identifier) {
      const success = await PluginService.enablePlugin(identifier);
      if (success) {
        const plugin = this.plugins.find(p => p.pluginIdentifier === identifier);
        if (plugin) {
          plugin.disabled = false;
        }
      }
    },
    async disablePlugin(identifier) {
      const success = await PluginService.disablePlugin(identifier);
      if (success) {
        const plugin = this.plugins.find(p => p.pluginIdentifier === identifier);
        if (plugin) {
          plugin.disabled = true;
        }
      }
    },
    async uninstallPlugin(identifier) {
      const success = await PluginService.unInstallPlugin(identifier);
      if (success) {
        this.plugins = this.plugins.filter(p => p.pluginIdentifier !== identifier);
      }
    },
    formatDate(dateString) {
      return new Date(dateString).toLocaleString();
    },
    async installPlugin(pluginInfo) {
      try {
        const success = await PluginService.installPlugin(pluginInfo);
        if (success) {
          this.$bvToast.toast(this.$t('admin.plugins.installSuccess'), {
            title: this.$t('admin.plugins.success'),
            variant: 'success',
            solid: true
          });
          this.loadPlugins();
        }
      } catch (error) {
        this.$bvToast.toast(error.message, {
          title: this.$t('admin.plugins.error'),
          variant: 'danger',
          solid: true
        });
      }
    }
  },
  mounted() {
    this.loadPlugins();
  },
};
</script>

<style scoped>
.admin-plugin-card {
  border-top: unset !important;
  border-top-left-radius: unset !important;
  border-top-right-radius: unset !important;
  text-align: right;
}

.admin-plugin-card .card-title {
  text-align: left;
}

.admin-plugin-card p {
  text-align: left;
}

@media (min-width: 440px) {
    .admin-plugin-card .card-body {
    margin: 1.25rem 1.25rem 1.25rem 1.25rem;
    }
}

.load-more-button {
  width: 100%;
}
</style>
