<template>
  <div class="home">
    <b-container fluid class="full-size-container">
      <b-card no-body class="h-100vh">
        <b-tabs
          pills
          card
          vertical
          nav-class="home-nav"
          active-nav-item-class="home-nav-active-item"
        >
          <template #tabs-start>
            <br />
            <h5 style="text-align: left">DISCOVER</h5>
            <br />
          </template>
          <template #tabs-end v-if="this.$store.state.application.profile">
            <br />
            <h5 style="text-align: left">
              MY SPACES
              <b-link class="favorites-all-link" to="/favorites">ALL</b-link>
            </h5>
            <br />
            <MySpaces/>
          </template>

          <Updates />
          <RecentlyVisited />

        </b-tabs>
      </b-card>
    </b-container>
  </div>
</template>

<script>
import Updates from "@/components/home/Updates.vue";
import RecentlyVisited from "@/components/home/RecentlyVisited.vue";
import MySpaces from "@/components/home/MySpaces.vue";
export default {
  name: "home",
  components: {
    Updates,
    RecentlyVisited,
    MySpaces
  },
};
</script>
<style>
.tabs {
  height: 100%;
}
.home-nav {
  width: 250px !important;
  text-align: left;
}
.home-nav-active-item {
  background-color: rgba(9, 30, 66, 0.08) !important;
  color: #42526e !important;
}
.favorites-all-link {
  text-decoration: none;
  float: right;
}
</style>