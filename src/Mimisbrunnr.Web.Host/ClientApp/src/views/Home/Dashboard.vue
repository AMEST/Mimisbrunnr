<template>
  <div
    v-touch:swipe.left="swipeCloseMenu"
    v-touch:swipe.right="swipeOpenMenu"
    class="home"
  >
    <b-container fluid class="full-size-container">
      <b-card no-body class="h-100vh">
        <a
          v-if="menuClosed"
          @click="switchMenu"
          href="#"
          class="circle-little-link fixed-tabs-badge"
          ><b-icon width="1.7em" height="1.7em" icon="arrow-right-short" />
        </a>
        <b-tabs
          pills
          card
          vertical
          :nav-class="['home-nav', menuClosed ? 'home-nav-closed':'']"
          active-nav-item-class="home-nav-active-item"
        >
          <template #tabs-start>
            <br />
            <a
              @click="switchMenu"
              href="#"
              class="circle-little-link tabs-badge"
              ><b-icon width="1.7em" height="1.7em"  icon="arrow-left-short" />
            </a>
            <ProfileBlock />
          </template>

          <Updates />
          <RecentlyVisited />

          <template #tabs-end>
            <div class="nav-end"></div>
            <!-- Temporary disable skeleton while my spaces not implemented
            <br />
            <h5 style="text-align: left">
              {{$t("home.menu.mySpaces")}}
              <b-link class="favorites-all-link" to="/favorites">{{$t("home.menu.allFavoritesSpaces")}}</b-link>
            </h5>
            <br />
            <MySpaces /> 
            -->
            <version />
          </template>
        </b-tabs>
      </b-card>
    </b-container>
  </div>
</template>

<script>
import Updates from "@/components/home/Updates.vue";
import RecentlyVisited from "@/components/home/RecentlyVisited.vue";
import Version from "@/components/home/Version.vue";
import ProfileBlock from "@/components/home/ProfileBlock.vue";
export default {
  name: "Dashboard",
  components: {
    Updates,
    RecentlyVisited,
    Version,
    ProfileBlock,
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
    },
  },
  mounted: function () {
    this.menuClosed = this.$store.state.application.homeMenuClosed;
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
.home-nav-closed {
    max-width: 0.5em !important;
    content-visibility: hidden !important;
    position: initial !important;
}
.home-nav .nav-item {
  background-color: #fff;
  background-clip: border-box;
  border: 1px solid rgba(0, 0, 0, 0.125);
  border-top: unset;
  border-bottom: unset;
}
.home-nav .nav-item .nav-link {
    border-radius: unset;
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
.nav-end {
    border-bottom: 1px solid rgba(0,0,0,.125);
    width:100%;
}
.favorites-all-link {
  text-decoration: none;
  float: right;
}
.circle-little-link 
{
    display: block;
    width: 25px;
    height: 25px;
    border-radius: 25px;
    background-color: #2780e3;
    line-height: 25px;
}
.tabs-badge {
  float: right;
  color: white !important;
  position: relative;
  top: -0.5em;
  left: 16.5em;
}
.fixed-tabs-badge {
  position: fixed;
  z-index: 5;
  color: white !important;
  margin-top: 2.5em;
  margin-left: 0.4em;
}
</style>