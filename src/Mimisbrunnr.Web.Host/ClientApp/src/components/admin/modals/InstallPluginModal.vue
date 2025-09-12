<template>
    <b-modal
      id="install-plugin-modal"
      :title="$t('admin.plugins.installModal.title')"
      size="lg"
      centered
      hide-footer
      @hidden="resetModal"
    >
    <b-form @submit.prevent="handleSubmit">
      <b-form-group
        :label="$t('admin.plugins.installModal.fileLabel')"
        label-for="plugin-file"
      >
        <b-form-file
          id="plugin-file"
          v-model="file"
          :state="Boolean(file)"
          :placeholder="$t('admin.plugins.installModal.filePlaceholder')"
          accept=".json"
          required
        />
      </b-form-group>

      <b-alert
        v-if="pluginInfo"
        variant="info"
        show
      >
        <h3>{{ $t("admin.plugins.installModal.infoBlock") }}</h3>
        <p><strong>{{ $t('admin.plugins.installModal.pluginName') }}:</strong> {{ pluginInfo.Name }}</p>
        <p><strong>{{ $t('admin.plugins.installModal.pluginVersion') }}:</strong> {{ pluginInfo.Version }}</p>
      </b-alert>

      <div class="text-right mt-3">
        <b-button
          type="submit"
          variant="primary"
          :disabled="!pluginInfo"
        >
          {{ $t('admin.plugins.installModal.installButton') }}
        </b-button>
      </div>
    </b-form>
  </b-modal>
</template>

<script>
export default {
  name: 'InstallPluginModal',
  data() {
    return {
      file: null,
      pluginInfo: null
    }
  },
  watch: {
    file(newFile) {
      if (newFile) {
        this.readPluginFile(newFile)
      }
    }
  },
  methods: {
    readPluginFile(file) {
      const reader = new FileReader()
      reader.onload = (e) => {
        try {
          this.pluginInfo = JSON.parse(e.target.result)
        } catch (error) {
          this.$bvToast.toast(this.$t('admin.plugins.installModal.invalidFileError'), {
            title: this.$t('admin.plugins.error'),
            variant: 'danger',
            solid: true
          })
          this.pluginInfo = null
        }
      }
      reader.readAsText(file)
    },
    handleSubmit() {
      this.$emit('install', this.pluginInfo)
      this.$bvModal.hide('install-plugin-modal')
    },
    resetModal() {
      this.file = null
      this.pluginInfo = null
    }
  }
}
</script>

<style scoped>
</style>
