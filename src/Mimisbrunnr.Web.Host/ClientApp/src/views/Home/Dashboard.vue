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
          ><b-icon-arrow-right-short width="1.7em" height="1.7em" />
        </a>
        <b-tabs
          pills
          card
          vertical
          :nav-class="['home-nav', menuClosed ? 'home-nav-closed' : '']"
          active-nav-item-class="home-nav-active-item"
          @activate-tab="tabChanged"
          content-class="tab-content-overflow"
          v-model="tabIndex"
        >
          <template #tabs-start>
            <br />
            <a
              @click="switchMenu"
              href="#"
              class="circle-little-link tabs-badge"
              ><b-icon-arrow-left-short width="1.7em" height="1.7em" />
            </a>
            <ProfileBlock />
          </template>

          <Updates />
          <RecentlyVisited />
          <Favorites v-if="!this.isAnonymous" />

          <template #tabs-end>
            <div class="nav-end"></div>
            <version :hidden="menuClosed" />
          </template>
        </b-tabs>
      </b-card>
    </b-container>
  </div>
</template>

<script>
import { BIconArrowRightShort, BIconArrowLeftShort } from "bootstrap-vue";
import Updates from "@/components/home/Updates.vue";
import RecentlyVisited from "@/components/home/RecentlyVisited.vue";
import Favorites from "@/components/home/Favorites.vue";
import Version from "@/components/home/Version.vue";
import ProfileBlock from "@/components/home/ProfileBlock.vue";
export default {
  name: "Dashboard",
  components: {
    Updates,
    RecentlyVisited,
    Favorites,
    Version,
    ProfileBlock,
    BIconArrowRightShort,
    BIconArrowLeftShort,
  },
  data() {
    return {
      menuClosed: false,
      tabIndex: 0,
      availableTabs: ["updates", "recently", "favorites"],
    };
  },
  computed: {
    isSmallScreen() {
      return window.innerWidth < 860;
    },
    isAnonymous() {
      return this.$store.state.application.profile == undefined;
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
    // eslint-disable-next-line
    tabChanged: function (newTabIndex, oldTabIndex, event) {
      this.$router.push(`/dashboard/${this.availableTabs[newTabIndex]}`);
    },
    tabRouter: function () {
      if (this.$route.params.section != null) {
        var requestedIndex = this.availableTabs.indexOf(
          this.$route.params.section
        );
        this.tabIndex = requestedIndex >= 0 ? requestedIndex : 0;
      }
    },
  },
  watch: {
    // eslint-disable-next-line
    "$route.params.section": function (to, from) {
      // eslint-disable-next-line
      this.tabRouter();
    },
  },
  mounted: function () {
    document.title = `Dashboard - ${this.$store.state.application.info.title}`;
    this.menuClosed = this.$store.state.application.homeMenuClosed;
  },
  created() {
    this.tabRouter();
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
  overflow-x: hidden !important;
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
  border-bottom: 1px solid rgba(0, 0, 0, 0.125);
  width: 100%;
}
.favorites-all-link {
  text-decoration: none;
  float: right;
}
.circle-little-link {
  display: block;
  width: 25px;
  height: 25px;
  border-radius: 25px;
  background-color: #2780e3;
  line-height: 25px;
}
.tabs-badge {
  z-index: 5;
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
@media (max-width: 450px) {
  .tab-content-overflow {
    overflow-x: auto;
  }
}
</style>
