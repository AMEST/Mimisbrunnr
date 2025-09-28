<template>
  <b-modal
    id="macro-param-edit-modal"
    size="lg"
    :title="$t('pageEditor.macroEditor.title')"
    @shown="onShow"
    @hide="onHide"
  >
    <b-form v-if="macroDefinition && macroDefinition.params && macroDefinition.params.length > 0 && currentParams"
        @submit.prevent="() => {}">
      <b-form-group
        v-for="param in macroDefinition.params"
        :key="param"
        :label="param"
      >
        <textarea v-model="currentParams[param]" class="textarea-input"></textarea>
      </b-form-group>
      <b-form-input v-model="pageId" id="macro-pageid-input" hidden></b-form-input>
      <b-form-input v-model="macroIdentifier" id="macro-identifier-input" hidden></b-form-input>
      <b-form-input v-model="macroIdOnPage" id="macro-identifier-on-page-input" hidden></b-form-input>
      <div v-if="macroDefinition.customParamsEditor" v-html="macroDefinition.customParamsEditor"></div>
    </b-form>
    <b-alert v-else show variant="light">
        {{$t("pageEditor.macroEditor.paramsEmpty")}}
    </b-alert>

    <template #modal-footer>
      <b-button variant="primary" @click="saveParams" v-if="macroDefinition && macroDefinition.params && macroDefinition.params.length > 0">
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
        for(const paramName of this.macroDefinition.params){
            if(this.currentParams[paramName])
                continue;
            if(!this.macroDefinition.defaultValues[paramName])
                continue;
            this.currentParams[paramName] = this.macroDefinition.defaultValues[paramName];
        }
        this.$forceUpdate();
    },
    onHide() {
        this.macroDefinition = null;
        this.currentParams = null;
        this.$emit('close');
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
</style>