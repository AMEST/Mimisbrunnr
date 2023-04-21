<template>
  <b-container class="bg-light comment-create-block pb-2" fluid>
    <div class="d-flex flex-row pt-2">
      <b-avatar
        class="avatar-bg"
        :text="getUserInitials()"
        :src="$store.state.application.profile.avatarUrl"
        :style="
          $store.state.application.profile.avatarUrl
            ? 'background-color: transparent'
            : ''
        "
      ></b-avatar>
      <b-form-textarea
        class="ml-2"
        v-model="comment"
        placeholder="..."
        rows="3"
        max-rows="6"
        @keyup.enter="create"
      ></b-form-textarea>
    </div>
    <div class="mt-2 text-right">
        <b-button 
            variant="info" 
            size="sm" 
            @click="create"
        >
        {{ $t("page.comments.add") }}
    </b-button>
    </div>
  </b-container>
</template>

<script>
import { getInitials } from "@/services/Utils";
export default {
  name: "CommentCreate",
  data() {
    return {
      comment: "",
    };
  },
  props: {
    createAction: Function,
  },
  methods: {
    getUserInitials: function () {
      return getInitials(this.$store.state.application.profile);
    },
    create: async function(e){
        if (e.ctrlKey || e.shiftKey) return;
        if (this.comment.replace(/\n/g,"") == "") return;
        await this.createAction(this.comment);
        this.comment = "";
    }
  },
};
</script>

<style scoped>
.comment-create-block {
  margin-left: -15px;
  width: calc(100% + 15px);
}
.comment-create-block textarea {
    overflow-y: auto !important;
}
.avatar-bg {
  background-color: var(--bs-body-bg);
  color: black;
}
.avatar-bg .b-avatar-img img {
  background-color: var(--bs-body-bg);
}
</style>