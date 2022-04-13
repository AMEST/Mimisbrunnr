<template>
  <b-modal
    @shown="onShow"
    id="page-move-modal"
    centered
    title="Move page"
  >
    <div role="group">
      <label>Destination type:</label>
      <b-form-select 
        v-model="destinationType" 
        class="form-select"
        :options="types" 
        :state="typeState"
        required
      ></b-form-select>
      <b-form-text>Parent page or space</b-form-text>
    </div>
    <div role="group">
      <label>Destination:</label>
      <b-form-input
        v-model="destination"
        required
        placeholder="Enter pageId or space key"
        :state="destinationState"
        trim
      ></b-form-input>
      <b-form-text>Parent page or space</b-form-text>
    </div>
    <template #modal-footer>
      <div align="right">
        <b-button variant="warning" class="mr-05em" @click="pageAction">Move</b-button>
        <b-button variant="secondary" @click="close">Cancel</b-button>
      </div>
    </template>
  </b-modal>
</template>

<script>
import axios from 'axios';
export default {
  name: "MovePage",
  data() {
      return {
          destination: '',
          destinationType: '',
          types: ["Space", "Page"]
      }
  },
  computed: {
    typeState() {
      return this.destinationType.length > 0 ? true : false;
    },
    destinationState() {
      return this.destination.length > 0 ? true : false;
    },
  },
  methods: {
    pageAction: async function() {
      var pageId = this.$route.params.pageId;
      if(pageId == null) return;
      var destinationPageId = "";
      if(this.destinationType == 'Space'){
        var spaceRequest = await axios.get("/api/space/" + this.destination);
        destinationPageId = spaceRequest.data.homePageId;
        
      }else{
        destinationPageId = this.destination;
      }
      var newPageRequest = await axios.post("/api/page/move/" + pageId + '/' + destinationPageId);
      this.$router.push("/space/" + newPageRequest.data.spaceKey + '/' + newPageRequest.data.id);
      this.$bvModal.hide("page-move-modal");
    },
    close: function () {
      this.$bvModal.hide("page-move-modal");
    },
    onShow: function () {
      this.destination = '';
      this.destinationType = '';
    },
  },
};
</script>

<style>
</style>