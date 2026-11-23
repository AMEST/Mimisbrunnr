<template>
  <div>
    <b-modal
      id="macro-param-edit-modal"
      size="lg"
      :title="$t('pageEditor.macroEditor.title')"
      @shown="onShow"
      @hide="onHide"
    >
      <b-form v-if="hasParams && currentParams" @submit.prevent="() => {}">
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
              <b-form-input v-model="currentParams[param.name]"></b-form-input>
              <b-input-group-append>
                <b-button variant="outline-secondary" @click="pickAttachmentFor(param.name)">
                  Select
                </b-button>
              </b-input-group-append>
            </b-input-group>
            <b-form-select
              v-else-if="param.type === 'select'"
              v-model="currentParams[param.name]"
              :options="param.options"
            ></b-form-select>
            <b-form-checkbox
              v-else-if="param.type === 'boolean'"
              v-model="currentParams[param.name]"
            ></b-form-checkbox>
            <b-form-input
              v-else-if="param.type === 'number'"
              type="number"
              v-model="currentParams[param.name]"
            ></b-form-input>
            <b-form-input
              v-else-if="param.type === 'date'"
              type="date"
              v-model="currentParams[param.name]"
            ></b-form-input>
            <b-form-input
              v-else-if="param.type === 'secret'"
              type="password"
              v-model="currentParams[param.name]"
            ></b-form-input>
            <b-form-textarea
              v-else-if="param.type === 'textarea' || param.type === 'code' || param.type === 'json'"
              v-model="currentParams[param.name]"
              :rows="param.rows || 3"
              :class="param.type === 'code' ? 'code-input' : 'textarea-input'"
            ></b-form-textarea>
            <b-form-input
              v-else
              v-model="currentParams[param.name]"
            ></b-form-input>
          </b-form-group>
        </template>
        <b-form-input v-model="pageId" id="macro-pageid-input" hidden></b-form-input>
        <b-form-input v-model="macroIdentifier" id="macro-identifier-input" hidden></b-form-input>
        <b-form-input v-model="macroIdOnPage" id="macro-identifier-on-page-input" hidden></b-form-input>
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
import { getPluginApi, setPluginApiContext, clearPluginApiContext } from '@/services/pluginApi';
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
      apiContext: null,
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
  methods: {
    async onShow() {
      this.showAdvanced = false;
      await this.loadMacroDefinition();
      await this.loadCurrentParams();
      this.applyDefaults();
      this.registerApiContext();
      this.$forceUpdate();
      this.$nextTick(() => {
        runScriptElements(this.$refs.customEditor, getPluginApi());
      });
    },
    onHide() {
      clearPluginApiContext(this.apiContext);
      this.apiContext = null;
      if (this.attachmentPickerResolver) {
        this.attachmentPickerResolver(null);
        this.attachmentPickerResolver = null;
      }
      this.macroDefinition = null;
      this.currentParams = null;
      this.$emit('close');
    },
    async loadMacroDefinition() {
      try {
        this.macroDefinition = await PluginService.getMacroInfo(this.macroIdentifier);
      } catch (error) {
        console.error('Failed to load macro definition:', error);
        this.macroDefinition = null;
      }
    },
    async loadCurrentParams() {
      try {
        const macroState = await PluginService.getMacroState(
          this.pageId,
          this.macroIdOnPage
        );
        this.currentParams = { ...this.parameters, ...(macroState && macroState.params ? macroState.params : {}) };
      } catch (error) {
        console.error('Failed to load macro state:', error);
        this.currentParams = { ...this.parameters };
      }
    },
    applyDefaults() {
      if (!this.macroDefinition || !this.currentParams) return;
      const definitions = this.macroDefinition.paramDefinitions || [];
      const defaultValues = this.macroDefinition.defaultValues || {};
      for (const param of this.paramList) {
        const value = this.currentParams[param.name];
        if (value !== undefined && value !== null && value !== '') continue;
        const definition = definitions.find(item => item && item.name === param.name);
        const defaultValue = definition && definition.defaultValue !== undefined
          ? definition.defaultValue
          : defaultValues[param.name];
        if (defaultValue !== undefined && defaultValue !== null && defaultValue !== '') {
          this.$set(this.currentParams, param.name, defaultValue);
        }
      }
    },
    registerApiContext() {
      this.apiContext = {
        pageId: this.pageId,
        macroId: this.macroIdOnPage,
        macroIdentifier: this.macroIdentifier,
        mode: 'edit',
        getParams: () => ({ ...this.currentParams }),
        setParam: (name, value) => {
          if (this.currentParams && name) {
            this.$set(this.currentParams, name, value);
            this.$forceUpdate();
          }
        },
        save: () => this.saveParams(),
        openAttachmentPicker: () => this.openAttachmentPicker()
      };
      setPluginApiContext(this.apiContext);
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
      if (!this.currentParams) return false;
      const validationError = this.validateParams();
      if (validationError) {
        showToast(validationError, 'Invalid macro parameters', 'warning');
        return false;
      }
      try {
        if (this.macroDefinition.storeParamsInDatabase) {
          await PluginService.saveMacroState(
            this.pageId,
            this.macroDefinition.macroIdentifier,
            this.macroIdOnPage,
            this.currentParams
          );
        } else {
          this.$emit('save', this.macroContent, this.currentParams, this.macroIdOnPage, this.macroIdentifier);
        }
        this.$bvModal.hide('macro-param-edit-modal');
        return true;
      } catch (error) {
        console.error('Failed to save macro params:', error);
        showToast('Failed to save macro parameters', 'Error', 'warning');
        return false;
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
      const value = await this.openAttachmentPicker();
      if (value && this.currentParams) {
        this.$set(this.currentParams, name, value);
      }
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
