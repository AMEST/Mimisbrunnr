<template>
  <b-tab class="max-tab-pane text-left">
    <template #title>
      <b-icon-clock-history class="text-secondary"/>
      <strong class="text-secondary">
        {{ $t("home.recentlyVisited.title") }}</strong
      >
    </template>
    <h2>{{ $t("home.recentlyVisited.title") }}</h2>
    <b-list-group class="pt-3">
      <b-list-group-item
        button
        v-for="h in visited"
        :key="h.id"
        v-on:click="
          go('/space/' + h.key + (h.type == 'Space' ? '' : '/' + h.id))
        "
      >
        <b-icon-folder v-if="h.type == 'Space'" />
        <b-icon-file-earmark-text v-else />
        {{ h.name }}
        <span style="float: right">{{
          new Date(h.date).toLocaleString()
        }}</span>
      </b-list-group-item>
    </b-list-group>
  </b-tab>
</template>

<script>
import {
  BIconClockHistory,
  BIconFolder,
  BIconFileEarmarkText,
} from "bootstrap-vue";
export default {
  name: "RecentlyVisited",
  computed: {
    visited() {
      return this.$store.state.application.history
        .slice(0)
        .sort(this.sortDateAsc);
    },
  },
  components: {
    BIconClockHistory,
    BIconFolder,
    BIconFileEarmarkText,
  },
  methods: {
    go: function (route) {
      this.$router.push(route);
    },
    sortDateAsc: function (a, b) {
      return b.date - a.date;
    },
  },
};
</script>

<style>
</style>