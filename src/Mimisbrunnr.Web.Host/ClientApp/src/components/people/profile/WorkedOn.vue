<template>
  <b-col class="text-left">
    <div class="profile-worked-on">
      <h5>{{$t('profile.workedOn.title')}}</h5>
      <b-card>
        <b-card-text v-if="!this.loaded">
          <b-list-group-item button v-for="h in [1, 2, 3, 4, 5, 6]" :key="h">
            <b-icon-file-earmark-text style="float: left" />
            <b-skeleton animation="wave" width="25%"></b-skeleton>
          </b-list-group-item>
        </b-card-text>
        <b-card-text v-else>
            <b-list-group-item button v-for="work in this.workedOn" :key="new Date(work.updated).getTime()">
              <b-icon-file-earmark-text class="work-pageIcon" style="float: left" />
              <b-link :to="'/space/'+work.spaceKey+'/'+work.pageId">{{ work.pageTitle }}</b-link>
              <span class="text-muted work-date">{{new Date(work.updated).toLocaleString()}}</span>
          </b-list-group-item>
          <b-list-group-item v-if="this.workedOn.length == 0">{{$t('profile.workedOn.emptyUpdates')}}</b-list-group-item>
        </b-card-text>
      </b-card>
    </div>
  </b-col>
</template>

<script>
import { BIconFileEarmarkText } from "bootstrap-vue";
import FeedService from "@/services/feedService";
export default {
  name: "WorkedOn",
  data() {
    return {
      workedOn: [],
      loaded: false,
    };
  },
  components: {
    BIconFileEarmarkText,
  },
  methods: {
    init: async function () {
      if (this.$route.params.email == null) return;
      this.workedOn = await FeedService.getUserFeed(this.$route.params.email);
      this.loaded = true;
    },
  },
  watch: {
    // eslint-disable-next-line
    "$route.params.email": function (to, from) {
      this.init();      
    },
  },
  mounted: function(){
    this.init();
  }
};
</script>

<style>
.work-date {
  float: right;
}

.work-pageIcon {
  float: left;
  margin-top: 3px;
  margin-right: 5px;
}
.profile-worked-on .card {
    background-color: #f4f5f7 !important;
    border: unset;
}
.profile-worked-on .card-body {
    padding-top: unset;
}
.profile-worked-on h5 {
    padding-left: 1em;
}
</style>