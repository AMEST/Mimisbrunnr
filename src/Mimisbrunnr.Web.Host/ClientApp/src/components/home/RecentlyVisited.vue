<template>
  <b-tab active class="max-tab-pane">
    <template #title>
      <b-icon icon="clock-history" class="text-secondary"></b-icon>
      <strong class="text-secondary"> Recently visited</strong>
    </template>
    <h2 style="text-align: left" >Recently visited</h2>
    <b-skeleton-table
      :rows="15"
      :columns="1"
    ></b-skeleton-table>
  </b-tab>
</template>

<script>
export default {
  name: "RecentlyVisited",
};
</script>

<style>
</style>