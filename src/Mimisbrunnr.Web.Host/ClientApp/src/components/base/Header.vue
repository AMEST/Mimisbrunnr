<template>
  <div>
    <b-navbar toggleable="lg" type="dark" variant="primary">
      <b-container>
        <b-navbar-brand href="#">Mimisbrunnr</b-navbar-brand>

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

          <b-nav-item-dropdown right>
            <!-- Using 'button-content' slot -->
            <template #button-content>
              <em>User</em>
            </template>
            <b-dropdown-item href="#">Profile</b-dropdown-item>
            <b-dropdown-item href="#">Sign Out</b-dropdown-item>
          </b-nav-item-dropdown>
        </b-navbar-nav>
      </b-container>
    </b-navbar>
  </div>
</template>

<script>
export default {
  name: "Header",
    data: function(){
    return {
    }
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
</style>