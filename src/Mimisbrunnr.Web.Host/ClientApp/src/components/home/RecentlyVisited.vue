<template>
  <b-tab class="max-tab-pane text-left">
    <template #title>
      <b-icon icon="clock-history" class="text-secondary"></b-icon>
      <strong class="text-secondary"> Recently visited</strong>
    </template>
    <h2>Recently visited</h2>
    <b-list-group class="pt-3">
      <b-list-group-item 
        button 
        v-for="h in this.$store.state.application.history" 
        :key="h.id"
        v-on:click="go('/space/' + h.key + ( h.type == 'Space' ? '' : '/' + h.id ))"
        >
        <b-icon icon="folder" v-if="h.type == 'Space'"/>
        <b-icon icon="file-earmark-text" v-else/>
        {{h.name}}
        <span style="float: right">{{new Date(h.date).toLocaleString()}}</span>
      </b-list-group-item>
    </b-list-group>
  </b-tab>
</template>

<script>
export default {
  name: "RecentlyVisited",
  methods: {
    go: function(route) {
      this.$router.push(route)
    },
  },
};
</script>

<style>
</style>