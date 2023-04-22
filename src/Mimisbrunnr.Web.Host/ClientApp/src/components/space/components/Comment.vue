<template>
  <b-card class="comment-card">
    <b-avatar
      class="comment-avatar"
      :text="getUserInitials(comment.author.name)"
      :src="comment.author.avatarUrl"
      :style="comment.author.avatarUrl ? 'background-color: transparent' : ''"
    ></b-avatar>
    <b-link class="comment-updatedBy" :to="'/profile/' + comment.author.email"
      >{{ comment.author.name }}
    </b-link>
    <br />
    <span class="text-muted date-small">
      {{ new Date(comment.created).toLocaleString() }}
    </span>
    <p class="pl-2 comment">{{ comment.message }}</p>
    <span
        v-if="itsMe"
        class="delete-button pl-2" 
        @click="remove"
    >
        <b-icon icon="trash"/> {{ $t("page.comments.delete") }}
    </span>
  </b-card>
</template>

<script>
import { getInitials } from "@/services/Utils";
export default {
  name: "Comment",
  props: {
    comment: Object,
    deleteAction: Function
  },
  computed: {
    itsMe() {
        if(!this.$store.state.application.profile)
            return false;
        return this.comment.author.email == this.$store.state.application.profile.email;
    }
  },
  methods: {
    getUserInitials: function (name) {
      return getInitials(name);
    },
    remove: async function() {
        await this.deleteAction(this.comment);
    }
  },
};
</script>

<style scoped>
.comment-avatar {
  float: left;
  margin-right: 1em;
  background-color: #f4f5f7;
}
.comment-updatedBy {
  text-decoration: none;
  font-weight: bold;
  font-size: 16px;
}
.comment-card {
  border: unset !important;
  margin-left: -15px;
  width: calc(100% + 15px);
}
.comment-card .card-body {
  padding-top: 5px;
  padding-bottom: 5px;
}
.comment-card .date-small {
  font-size: 13px;
}
.comment-card .delete-button {
    font-size: 12px;
    cursor: pointer;
}
.comment-card .delete-button:hover{
    color: #007bff;
}
.comment-card .comment {
    white-space: pre-wrap;
}
</style>