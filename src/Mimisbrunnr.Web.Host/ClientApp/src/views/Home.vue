<template>
  <div
    v-touch:swipe.left="swipeCloseMenu"
    v-touch:swipe.right="swipeOpenMenu"
    class="home"
  >
    <b-container fluid class="full-size-container">
      <b-card no-body class="h-100vh">
        <b-badge
          @click="switchMenu"
          href="#"
          class="fixed-tabs-badge"
          variant="primary"
          ><b-icon icon="arrow-right-short" />
        </b-badge>
        <b-tabs
          pills
          card
          vertical
          nav-class="home-nav"
          active-nav-item-class="home-nav-active-item"
        >
          <template #tabs-start>
            <br />
            <h5 style="text-align: left">
              {{$t("home.menu.discover")}}
              <b-badge
                @click="switchMenu"
                href="#"
                class="tabs-badge"
                variant="primary"
                ><b-icon icon="arrow-left-short" />
              </b-badge>
            </h5>
            <br />
          </template>
          <template #tabs-end v-if="this.$store.state.application.profile">
            <br />
            <h5 style="text-align: left">
              {{$t("home.menu.mySpaces")}}
              <b-link class="favorites-all-link" to="/favorites">{{$t("home.menu.allFavoritesSpaces")}}</b-link>
            </h5>
            <br />
            <MySpaces />
            <version />
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
import Version from "@/components/home/Version.vue";
export default {
  name: "home",
  components: {
    Updates,
    RecentlyVisited,
    MySpaces,
    Version,
  },
  data() {
    return {
      menuClosed: false,
    };
  },
  computed: {
    isSmallScreen() {
      return window.innerWidth < 860;
    },
  },
  methods: {
    swipeOpenMenu: function () {
      if (!this.menuClosed || !this.isSmallScreen) return;
      this.switchMenu();
    },
    swipeCloseMenu: function () {
      if (this.menuClosed || !this.isSmallScreen) return;
      this.switchMenu();
    },
    switchMenu: function () {
      this.menuClosed = !this.menuClosed;
      this.$store.commit("changeHomeMenuClose", this.menuClosed);
      document.getElementsByClassName("home-nav")[0].hidden = this.menuClosed;
    },
  },
  mounted: function () {
    if (window.innerWidth > 860) {
      this.$store.commit("changeHomeMenuClose", false);
      return;
    }
    this.menuClosed = this.$store.state.application.homeMenuClosed;
    document.getElementsByClassName("home-nav")[0].hidden = this.menuClosed;
  },
};
</script>
<style>
.tabs {
  height: 100%;
}
.home-nav {
  width: 280px !important;
  text-align: left;
  box-shadow: inset 0 0rem 0.5em rgba(0, 0, 0, 0.15) !important;
  background-color: rgb(247 247 247) !important;
}
@media (max-width: 860px) {
  .home-nav {
    position: fixed;
    z-index: 100;
  }
}
.home-nav-active-item {
  background-color: rgba(9, 30, 66, 0.08) !important;
  color: #42526e !important;
}
.favorites-all-link {
  text-decoration: none;
  float: right;
}
.tabs-badge {
  width: 35px;
  float: right;
  color: white !important;
}
.fixed-tabs-badge {
  position: fixed;
  z-index: 5;
  width: 35px;
  color: white !important;
  margin-top: 2.5em;
  margin-left: 0.3em;
}
@media (min-width: 860px) {
  .tabs-badge {
    display: none !important;
  }
  .fixed-tabs-badge {
    display: none !important;
  }
}
</style>