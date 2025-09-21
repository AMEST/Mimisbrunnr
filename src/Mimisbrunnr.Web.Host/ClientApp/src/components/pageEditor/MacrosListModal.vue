<template>
  <b-modal id="macros-list-modal" 
        size="lg" 
        :title="$t('pageEditor.macrosListModal.title')"
        hide-footer>
    <b-alert v-if="this.macros.length == 0" show variant="light">
            {{$t("pageEditor.macrosListModal.empty")}}
    </b-alert>
    <b-card-group deck>
      <b-card
        v-for="macro in macros"
        :key="macro.macroIdentifier"
        no-body
        class="overflow-hidden mb-3"
        style="min-width: 350px;max-width: 350px; min-height: 135px; max-height: 135px; cursor: pointer;"
        @click="insertMacro(macro)"
      >
        <b-row no-gutters>
          <b-col md="4">
            <b-card-img
              v-if="macro.icon"
              :src="macro.icon"
              alt="Macro image"
              class="rounded-0 macro-icon "
            ></b-card-img>
            <b-skeleton-img v-else card-img="top" aspect="4:6"></b-skeleton-img>
          </b-col>
          <b-col>
            <b-card-body :title="macro.name" title-tag="h5">
              <b-card-text>
                {{ macro.description }}
              </b-card-text>
            </b-card-body>
          </b-col>
        </b-row>
      </b-card>
    </b-card-group>

    <div class="pagination-controls mt-3">
      <b-button
        @click="loadPrev"
        :disabled="skip === 0"
        variant="outline-secondary"
      >
        {{this.$t('pageEditor.macrosListModal.previous')}}
      </b-button>
      <b-button
        @click="loadNext"
        :disabled="macros.length < take"
        variant="outline-secondary"
        class="ml-2"
      >
        {{this.$t('pageEditor.macrosListModal.next')}}
      </b-button>
    </div>
  </b-modal>
</template>

<script>
import PluginService from "@/services/pluginService";

export default {
  data() {
    return {
      macros: [],
      skip: 0,
      take: 10,
    };
  },
  async created() {
    await this.loadMacros();
  },
  methods: {
    async loadMacros() {
      this.macros = await PluginService.getMacros(this.skip, this.take);
    },
    async loadNext() {
      this.skip += this.take;
      await this.loadMacros();
    },
    async loadPrev() {
      this.skip = Math.max(0, this.skip - this.take);
      await this.loadMacros();
    },
    insertMacro(macroInfo){
        this.$emit("insert", macroInfo);
        this.$bvModal.hide('macros-list-modal');
    }
  },
};
</script>

<style scoped>
.pagination-controls {
  display: flex;
  justify-content: center;
  padding: 1rem;
}
.macro-icon {
    width: 64px;
    height: 64px;
    margin-left: 1.5em;
    margin-top: 2.1em;
}
</style>
