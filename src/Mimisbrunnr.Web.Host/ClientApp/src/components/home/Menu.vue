<template>
  <b-sidebar id="home-sidebar" title="Sidebar" sidebar-class="h-100vh sidebar-bottom" visible>
    <b-list-group>
      <b-list-group-item to="/" class="light-item t-align-left">
        <b-icon icon="rss" class="mr-2" /> Feed
      </b-list-group-item>
      <b-list-group-item to="/favorite" class="light-item t-align-left">
        <b-icon icon="star-fill" class="mr-2" /> Favorite </b-list-group-item
      >
      <b-list-group-item to="/channels" class="light-item t-align-left">
        <b-icon icon="receipt" class="mr-2" /> Channels
      </b-list-group-item>
    </b-list-group>
  </b-sidebar>
</template>

<script>
export default {
  name: "Menu",
};
</script>

<style scope>
.sidebar-bottom{
    top: 56px!important;
}
</style>