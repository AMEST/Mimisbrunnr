<template>
  <b-tab active class="max-tab-pane">
    <template #title>
     <b-icon icon="compass" class="text-secondary"></b-icon>
      <strong class="text-secondary"> Updates</strong>
    </template>
    <h2 style="text-align: left" >Updates</h2>
    <b-card v-for="n in 10" :key="n">
      <b-skeleton animation="wave" width="15%"></b-skeleton>
      <b-skeleton animation="wave" width="55%"></b-skeleton>
      <b-skeleton animation="wave" width="70%"></b-skeleton>
      <b-skeleton animation="wave" width="49%"></b-skeleton>
    </b-card>
  </b-tab>
</template>

<script>
export default {
  name: "Updates",
};
</script>

<style>
</style>