<template>
  <div>
    <b-modal
      id="macro-param-edit-modal"
      size="lg"
      :title="$t('pageEditor.macroEditor.title')"
      @shown="onShow"
      @hide="onHide"
    >
      <b-form v-if="currentParams && (hasParams || customEditorHtml)" @submit.prevent="() => {}">
        <b-form-checkbox v-if="hasAdvanced" v-model="showAdvanced" class="mb-3">
          Show advanced parameters
        </b-form-checkbox>
        <template v-for="(param, index) in visibleParams">
          <div
            v-if="isGroupHeader(index)"
            :key="'group-' + param.name"
            class="param-group-header"
          >
            {{ param.group }}
          </div>
          <b-form-group
            :key="param.name"
            :label="param.label"
            :description="param.help"
          >
            <b-input-group v-if="param.type === 'attachment'">
              <b-form-input
                :value="currentParams[param.name]"
                @update="pluginApi.setParam(param.name, $event)"
              ></b-form-input>
              <b-input-group-append>
                <b-button variant="outline-secondary" @click="pickAttachmentFor(param.name)">
                  Select
                </b-button>
              </b-input-group-append>
            </b-input-group>
            <b-form-select
              v-else-if="param.type === 'select'"
              :value="currentParams[param.name]"
              @input="pluginApi.setParam(param.name, $event)"
              :options="param.options"
            ></b-form-select>
            <b-form-checkbox
              v-else-if="param.type === 'boolean'"
              :checked="currentParams[param.name]"
              value="true"
              unchecked-value="false"
              @input="pluginApi.setParam(param.name, $event)"
            ></b-form-checkbox>
            <b-form-input
              v-else-if="param.type === 'number'"
              type="number"
              :value="currentParams[param.name]"
              @update="pluginApi.setParam(param.name, $event)"
            ></b-form-input>
            <b-form-input
              v-else-if="param.type === 'date'"
              type="date"
              :value="currentParams[param.name]"
              @update="pluginApi.setParam(param.name, $event)"
            ></b-form-input>
            <b-form-textarea
              v-else-if="param.type === 'textarea' || param.type === 'code' || param.type === 'json'"
              :value="currentParams[param.name]"
              @update="pluginApi.setParam(param.name, $event)"
              :rows="param.rows || 3"
              :class="param.type === 'code' ? 'code-input' : 'textarea-input'"
            ></b-form-textarea>
            <b-form-input
              v-else
              :value="currentParams[param.name]"
              @update="pluginApi.setParam(param.name, $event)"
            ></b-form-input>
          </b-form-group>
        </template>
        <b-form-input :value="pageId" id="macro-pageid-input" hidden></b-form-input>
        <b-form-input :value="macroIdentifier" id="macro-identifier-input" hidden></b-form-input>
        <b-form-input :value="macroIdOnPage" id="macro-identifier-on-page-input" hidden></b-form-input>
        <div
          v-if="macroDefinition && customEditorHtml"
          ref="customEditor"
          v-html="customEditorHtml"
        ></div>
      </b-form>
      <b-alert v-else show variant="light">
        {{ $t("pageEditor.macroEditor.paramsEmpty") }}
      </b-alert>

      <template #modal-footer>
        <b-button
          variant="primary"
          @click="saveParams"
          :disabled="saving"
          v-if="hasParams || (macroDefinition && macroDefinition.customParamsEditor)"
        >
          {{ $t('pageEditor.macroEditor.save') }}
        </b-button>
        <b-button variant="secondary" @click="$bvModal.hide('macro-param-edit-modal')">
          {{ $t('pageEditor.macroEditor.cancel') }}
        </b-button>
      </template>
    </b-modal>

    <b-modal
      id="macro-attachment-picker"
      title="Select attachment"
      size="md"
      @hidden="onPickerHidden"
    >
      <b-alert v-if="attachmentPickerLoading" show variant="light">Loading...</b-alert>
      <b-alert v-else-if="attachmentPickerItems.length === 0" show variant="light">
        No attachments
      </b-alert>
      <b-list-group v-else>
        <b-list-group-item
          button
          v-for="attachment in attachmentPickerItems"
          :key="attachment.name"
          @click="chooseAttachment(attachment)"
        >
          {{ attachment.name }}
        </b-list-group-item>
      </b-list-group>
      <template #modal-footer>
        <b-button variant="secondary" @click="cancelAttachmentPicker">
          Cancel
        </b-button>
      </template>
    </b-modal>
  </div>
</template>

<script>
import http from '@/services/http';
import PluginService from '@/services/pluginService';
import { showToast } from '@/services/Utils';
import { setPluginApiContext, clearPluginApiContext } from '@/services/pluginApi';
import { runScriptElements } from '@/services/scriptRunner';

export default {
  name: 'MacroParamEdit',
  props: {
    pageId: {
      type: String
    },
    macroIdentifier: {
      type: String
    },
    macroIdOnPage: {
      type: String
    },
    parameters: {
      type: Object,
      default: () => ({})
    },
    macroContent: {
      type: String
    }
  },
  data() {
    return {
      macroDefinition: null,
      currentParams: null,
      showAdvanced: false,
      pluginApi: null,
      loadGeneration: 0,
      saving: false,
      attachmentPickerItems: [],
      attachmentPickerLoading: false,
      attachmentPickerResolver: null
    };
  },
  computed: {
    paramList() {
      const definitions = this.macroDefinition && this.macroDefinition.paramDefinitions
        ? this.macroDefinition.paramDefinitions
        : [];
      if (definitions.length > 0) {
        return definitions
          .filter(definition => definition && definition.name)
          .map(definition => ({
            name: definition.name,
            type: (definition.type || 'text').toLowerCase(),
            label: definition.label || definition.name,
            help: definition.help || '',
            rows: definition.rows || 3,
            pattern: definition.pattern || '',
            required: !!definition.required,
            advanced: !!definition.advanced,
            group: definition.group || '',
            options: definition.options || []
          }));
      }
      const params = this.macroDefinition && this.macroDefinition.params
        ? this.macroDefinition.params
        : [];
      return params.map(name => ({
        name,
        type: 'textarea',
        label: name,
        help: '',
        rows: 3,
        pattern: '',
        required: false,
        advanced: false,
        group: '',
        options: []
      }));
    },
    visibleParams() {
      return this.paramList.filter(param => !param.advanced || this.showAdvanced);
    },
    hasAdvanced() {
      return this.paramList.some(param => param.advanced);
    },
    hasParams() {
      return this.paramList.length > 0;
    },
    customEditorHtml() {
      if (!this.macroDefinition) return '';
      const custom = this.macroDefinition.customParamsEditor || '';
      const shared = this.macroDefinition.sharedCode || '';
      return shared ? '<script>' + shared + '<' + '/script>' + custom : custom;
    }
  },
  beforeDestroy() {
    this.resetEditor();
  },
  methods: {
    async onShow() {
      const generation = ++this.loadGeneration;
      const { pageId, macroIdOnPage, macroIdentifier, parameters } = this;
      this.showAdvanced = false;
      const [macroDefinition, macroState] = await Promise.all([
        PluginService.getMacroInfo(macroIdentifier).catch(error => {
          console.error('Failed to load macro definition:', error);
          return null;
        }),
        PluginService.getMacroState(pageId, macroIdOnPage).catch(error => {
          console.error('Failed to load macro state:', error);
          return null;
        })
      ]);
      if (generation !== this.loadGeneration || !macroDefinition) return;
      if (macroDefinition.storeParamsInDatabase && !macroState) return;

      const schemaDefaults = Object.fromEntries((macroDefinition.paramDefinitions || [])
        .filter(definition => definition && definition.name && definition.defaultValue != null)
        .map(definition => [definition.name, definition.defaultValue]));
      this.macroDefinition = macroDefinition;
      this.currentParams = {
        ...macroDefinition.defaultValues,
        ...schemaDefaults,
        ...parameters,
        ...macroState?.params
      };
      const api = setPluginApiContext({
        pageId,
        macroId: macroIdOnPage,
        macroIdentifier,
        mode: 'edit',
        getParams: () => this.currentParams,
        setParam: (name, value) => this.$set(this.currentParams, name, value),
        save: () => this.saveParams(),
        openAttachmentPicker: () => this.openAttachmentPicker()
      });
      this.pluginApi = api;
      this.$nextTick(() => {
        if (this.pluginApi === api) runScriptElements(this.$refs.customEditor, api);
      });
    },
    onHide() {
      this.resetEditor();
      this.$emit('close');
    },
    resetEditor() {
      ++this.loadGeneration;
      clearPluginApiContext(this.pluginApi);
      this.pluginApi = null;
      this.onPickerHidden();
      this.macroDefinition = null;
      this.currentParams = null;
      this.saving = false;
    },
    isGroupHeader(index) {
      if (!this.visibleParams[index] || !this.visibleParams[index].group) return false;
      if (index === 0) return true;
      return this.visibleParams[index].group !== this.visibleParams[index - 1].group;
    },
    validateParams() {
      for (const param of this.paramList) {
        const value = this.currentParams[param.name];
        const isEmpty = value === undefined || value === null || (typeof value === 'string' && value.trim() === '');
        if (param.required && isEmpty) return `${param.label} is required`;
        if (!isEmpty && param.pattern) {
          try {
            if (!new RegExp(param.pattern).test(String(value))) {
              return `${param.label} has an invalid value`;
            }
          } catch (error) {
            return `Invalid validation pattern for ${param.label}`;
          }
        }
        if (!isEmpty && param.type === 'json') {
          try {
            JSON.parse(value);
          } catch (error) {
            return `${param.label} must be valid JSON`;
          }
        }
      }
      return null;
    },
    async saveParams() {
      if (!this.currentParams || this.saving) return false;
      const validationError = this.validateParams();
      if (validationError) {
        showToast(validationError, 'Invalid macro parameters', 'warning');
        return false;
      }
      const api = this.pluginApi;
      this.saving = true;
      try {
        const parameters = Object.fromEntries(Object.entries(this.currentParams)
          .map(([name, value]) => [name, String(value ?? '')]));
        if (this.macroDefinition.storeParamsInDatabase) {
          const saved = await PluginService.saveMacroState(
            this.pageId,
            this.macroDefinition.macroIdentifier,
            this.macroIdOnPage,
            parameters
          );
          if (!saved) return false;
        } else {
          this.$emit('save', this.macroContent, parameters, this.macroIdOnPage, this.macroIdentifier);
        }
        if (this.pluginApi === api) this.$bvModal.hide('macro-param-edit-modal');
        return true;
      } catch (error) {
        console.error('Failed to save macro params:', error);
        showToast('Failed to save macro parameters', 'Error', 'warning');
        return false;
      } finally {
        if (this.pluginApi === api) this.saving = false;
      }
    },
    openAttachmentPicker() {
      return new Promise(resolve => {
        if (this.attachmentPickerResolver) this.attachmentPickerResolver(null);
        this.attachmentPickerResolver = resolve;
        this.attachmentPickerItems = [];
        this.attachmentPickerLoading = true;
        this.$bvModal.show('macro-attachment-picker');
        http.get('/api/attachment/' + this.pageId, { validateStatus: false })
          .then(response => {
            if (response.status === 200) {
              this.attachmentPickerItems = response.data || [];
            } else {
              showToast(`status:${response.status}`, 'Error when loading attachments', 'warning');
            }
          })
          .catch(error => {
            console.error('Failed to load attachments:', error);
            showToast('Failed to load attachments', 'Error', 'warning');
          })
          .then(() => {
            this.attachmentPickerLoading = false;
          });
      });
    },
    chooseAttachment(attachment) {
      if (this.attachmentPickerResolver) this.attachmentPickerResolver(attachment.name);
      this.attachmentPickerResolver = null;
      this.$bvModal.hide('macro-attachment-picker');
    },
    cancelAttachmentPicker() {
      if (this.attachmentPickerResolver) this.attachmentPickerResolver(null);
      this.attachmentPickerResolver = null;
      this.$bvModal.hide('macro-attachment-picker');
    },
    onPickerHidden() {
      if (this.attachmentPickerResolver) this.attachmentPickerResolver(null);
      this.attachmentPickerResolver = null;
    },
    async pickAttachmentFor(name) {
      const api = this.pluginApi;
      const value = await api.openAttachmentPicker();
      if (value !== null) api.setParam(name, value);
    }
  }
};
</script>

<style>
.textarea-input {
    width: 100%;
    padding: 8px 12px;
    border: 1px solid rgb(204, 204, 204);
    border-radius: 4px;
    box-sizing: border-box;
    font-family: sans-serif;
    font-size: 16px;
    line-height: 1.5;
    min-height: 40px;
    resize: vertical;
    height: 40px;
}
.code-input {
    width: 100%;
    padding: 8px 12px;
    border: 1px solid rgb(204, 204, 204);
    border-radius: 4px;
    box-sizing: border-box;
    font-family: monospace;
    font-size: 14px;
    line-height: 1.5;
    resize: vertical;
}
.param-group-header {
    margin-top: 12px;
    margin-bottom: 4px;
    font-weight: 600;
    border-bottom: 1px solid rgb(220, 220, 220);
}
</style>
