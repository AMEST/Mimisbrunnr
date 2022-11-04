<template>
  <b-tab active class="max-tab-pane" style="text-align: left">
    <template #title>
      <b-icon icon="compass" class="text-secondary"></b-icon>
      <strong class="text-secondary"> {{$t("home.updates.title")}}</strong>
    </template>
    <h2>{{$t("home.updates.title")}}</h2>
    <div v-if="!loaded">
      <b-card v-for="n in 10" :key="n" class="updates-card">
        <b-skeleton type="avatar" class="updates-avatar"></b-skeleton>
        <b-skeleton animation="wave" width="15%"></b-skeleton>
        <b-skeleton animation="wave" width="55%"></b-skeleton>
        <b-skeleton animation="wave" width="70%"></b-skeleton>
        <b-skeleton animation="wave" width="49%"></b-skeleton>
      </b-card>
    </div>
    <div v-else>
      <b-alert v-if="this.updates.length == 0" show variant="light">{{$t("home.updates.emptyUpdates")}}</b-alert>
      <b-card
        class="updates-card"
        v-for="pageEvent in this.updates"
        :key="new Date(pageEvent.updated).getTime()"
      >
        <b-avatar
          class="updates-avatar"
          :text="getUserInitials(pageEvent.updatedBy.name)"
          :src="pageEvent.updatedBy.avatarUrl"
        ></b-avatar>
        <b-link
          class="updates-updatedBy"
          :to="'/profile/' + pageEvent.updatedBy.email"
          >{{ pageEvent.updatedBy.name }}</b-link
        ><br />
        <b-link class="updates-pagelink" :to="'/space/'+pageEvent.spaceKey+'/'+pageEvent.pageId">
          <b-icon icon="file-earmark-text" class="updates-pageIcon" />
          {{ pageEvent.pageTitle }}
        </b-link>
        <br/><br/>
        <span class="text-muted">{{new Date(pageEvent.updated).toLocaleString()}}</span>
      </b-card>
    </div>
  </b-tab>
</template>

<script>
import { getNameInitials } from "@/services/Utils";
import FeedService from "@/services/feedService";
export default {
  name: "Updates",
  data() {
    return {
      updates: [],
      loaded: false,
    };
  },
  methods: {
    getUserInitials: function (name) {
      return getNameInitials(name);
    },
  },
  mounted: async function () {
    this.updates = await FeedService.getFeed();
    this.loaded = true;
  },
};
</script>

<style>
.updates-avatar {
  float: left;
  width: 3.5em !important;
  height: 3.5em !important;
  margin-right: 1em;
  background-color: #f4f5f7;
}
.updates-updatedBy {
  text-decoration: none;
  font-weight: bold;
  font-size: 14px;
}
.updates-pageIcon {
  float: left;
  margin-top: 3px;
  margin-right: 5px;
}
.updates-pagelink {
  text-decoration: none;
  color: black;
}
.updates-card {
  border: unset !important;
}
.updates-card:hover {
  background: rgba(0,0,0,0.01) !important;
}
</style>