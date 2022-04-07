<template>
  <div>
    <b-navbar toggleable="lg" type="dark" variant="primary">
      <b-container>
        <b-navbar-brand to="/">{{
          this.$store.state.application.info.title
        }}</b-navbar-brand>

        <b-navbar-nav class="mr-auto">
          <b-nav-item-dropdown text="Spaces" right>
            <b-dropdown-item href="#">Space directory</b-dropdown-item>
            <b-dropdown-item href="#">Create Space</b-dropdown-item>
          </b-nav-item-dropdown>
        </b-navbar-nav>

        <!-- Right aligned nav items -->
        <b-navbar-nav class="ml-auto">
          <b-nav-form class="flex-search invisibleComponentBorder">
            <b-form-input
              size="sm"
              class="mr-sm-2"
              placeholder="Search"
              disabled
            ></b-form-input>
            <b-button size="sm" class="my-2 my-sm-0" type="submit" disabled
              >Search</b-button
            >
          </b-nav-form>

          <div v-if="!this.$store.state.application.profile">
            <b-button class="text-light" variant="link" href="/api/account/login">Log in</b-button>
          </div>
          <b-nav-item-dropdown v-else right class="custom-dropdown">
            <!-- Using 'button-content' slot -->
            <template #button-content>
              <b-avatar
                class="avatar-bg"
                :text="getInitials()"
                :src="$store.state.application.profile.avatarUrl"
              ></b-avatar>
            </template>
            <b-dropdown-item href="#" disabled>Profile</b-dropdown-item>
            <b-dropdown-item href="/api/account/logout">Sign Out</b-dropdown-item>
          </b-nav-item-dropdown>
        </b-navbar-nav>
      </b-container>
    </b-navbar>
  </div>
</template>

<script>
export default {
  name: "Header",
  data: function () {
    return {};
  },
  methods: {
    getInitials: function () {
      if (!this.$store.state.application.profile.name) return "";
      var splited = this.$store.state.application.profile.name.split(" ");
      if (splited.length > 1) return splited[0][0] + splited[1][0];
      return splited[0][0];
    },
  },
};
</script>

<style>
.mr-auto {
  margin-right: auto;
}
.flex-search {
  margin: 4px;
}
.flex-search form {
  display: flex;
}
@media (max-width: 993px) {
  .invisibleComponentBorder {
    display: none;
  }
}
.dropdown-menu {
  position: absolute !important;
  right: 0 !important;
}
.custom-dropdown .nav-link {
  padding: 0 !important;
}
.avatar-bg .b-avatar-img img {
  background-color: var(--bs-body-bg);
}
</style>