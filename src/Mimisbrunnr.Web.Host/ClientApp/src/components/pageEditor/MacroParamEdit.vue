<template>
  <b-modal
    id="macro-param-edit-modal"
    size="lg"
    :title="$t('pageEditor.macroEditor.title')"
    @shown="onShow"
    @hide="$emit('close')"
  >
    <b-form v-if="macroDefinition && macroDefinition.params && macroDefinition.params.length > 0 && currentParams">
      <b-form-group
        v-for="param in macroDefinition.params"
        :key="param"
        :label="param"
      >
        <b-form-input
          v-model="currentParams[param]"
          type="text"
        />
      </b-form-group>
    </b-form>
    <b-alert v-else show variant="light">
        {{$t("pageEditor.macroEditor.paramsEmpty")}}
    </b-alert>

    <template #modal-footer>
      <b-button variant="primary" @click="saveParams">
        {{ $t('pageEditor.macroEditor.save') }}
      </b-button>
      <b-button variant="secondary" @click="$bvModal.hide('macro-param-edit-modal')">
        {{ $t('pageEditor.macroEditor.cancel') }}
      </b-button>
    </template>
  </b-modal>
</template>

<script>
import PluginService from '@/services/pluginService';

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
      currentParams: null
    };
  },
  methods: {
    async onShow(){
        await this.loadMacroDefinition();
        await this.loadCurrentParams();
    },
    async loadMacroDefinition() {
      try {
        this.macroDefinition = await PluginService.getMacroInfo(this.macroIdentifier);
      } catch (error) {
        console.error('Failed to load macro definition:', error);
      }
    },
    async loadCurrentParams() {
      try {
        const macroState = await PluginService.getMacroState(
          this.pageId, 
          this.macroIdOnPage
        );
        this.currentParams = { ...this.parameters, ...macroState.params };
      } catch (error) {
        console.error('Failed to load macro state:', error);
        this.currentParams = { ...this.parameters };
      }
    },
    async saveParams() {
      try {      
        if (this.macroDefinition.storeParamsInDatabase) {
          await PluginService.saveMacroState(
            this.pageId,
            this.macroDefinition.macroIdentifier,
            this.macroIdOnPage,
            this.currentParams
          );
        }else{
            this.$emit('save', this.macroContent, this.currentParams, this.macroIdOnPage, this.macroIdentifier);
        }
        
        this.$bvModal.hide('macro-param-edit-modal');
      } catch (error) {
        console.error('Failed to save macro params:', error);
      }
    }
  }
};
</script>