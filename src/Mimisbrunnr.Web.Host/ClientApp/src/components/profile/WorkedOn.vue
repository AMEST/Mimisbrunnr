<template>
  <b-col sm="9" class="text-left">
    <div class="profile-worked-on">
      <h5>Worked on</h5>
      <b-card>
        <b-card-text v-if="!this.loaded">
          <b-list-group-item button v-for="h in [1, 2, 3, 4, 5, 6]" :key="h">
            <b-icon icon="file-earmark-text" style="float: left" />
            <b-skeleton animation="wave" width="25%"></b-skeleton>
          </b-list-group-item>
        </b-card-text>
        <b-card-text v-else>
            <b-list-group-item button v-for="work in this.workedOn" :key="new Date(work.updated).getTime()">
              <b-icon class="work-pageIcon" icon="file-earmark-text" style="float: left" />
              <b-link :to="'/space/'+work.spaceKey+'/'+work.pageId">{{ work.pageTitle }}</b-link>
              <span class="text-muted work-date">{{new Date(work.updated).toLocaleString()}}</span>
          </b-list-group-item>
          <b-list-group-item v-if="this.workedOn.length == 0">There are no recent updates.</b-list-group-item>
        </b-card-text>
      </b-card>
    </div>
  </b-col>
</template>

<script>
import axios from "axios";
export default {
  name: "WorkedOn",
  data() {
    return {
      workedOn: [],
      loaded: false,
    };
  },
  methods: {
    init: async function () {
      if (this.$route.params.email == null) return;
      var feedRequest = await axios.get("/api/feed/" + this.$route.params.email);
      this.workedOn = feedRequest.data;
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
</style>