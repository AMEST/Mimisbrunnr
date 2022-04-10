<template>
  <b-modal @shown="onShow" id="page-delete-modal" centered title="Are you shure delete page?">
    <b-form-checkbox v-model="recursive">&nbsp;Delete page and all children</b-form-checkbox>
    <template #modal-footer >
        <div align="right">
            <b-button variant="danger" class="mr-05em" @click="deletePage">Delete</b-button>
            <b-button variant="secondary" @click="close">Cancel</b-button>
        </div>
    </template>
  </b-modal>
</template>

<script>
import axios from 'axios';
export default {
    name: "DeletePage",
    data() {
        return {
            recursive: false,
            pageId: null
        }
    },
    methods: {
        deletePage: async function(){
            if(this.pageId == null) return;
            await axios.delete("/api/page/"+this.pageId + '?recursively='+this.recursive.toString());
            this.$bvModal.hide("page-delete-modal");
            var spaceKey = this.$route.params.key;
            if(spaceKey != null) this.$router.push("/space/"+spaceKey);

        },
        close: function(){
            this.$bvModal.hide("page-delete-modal");
        },
        onShow: function() {
            this.recursive = false;
            this.pageId = this.$route.params.pageId;
        },
    },
    watch: {
    // eslint-disable-next-line
    "$route.params.pageId": function (to, from) {
      // eslint-disable-next-line
      this.onShow();
    },
  },
}
</script>

<style>
.mr-05em{
    margin-right: 0.5em;
}
</style>