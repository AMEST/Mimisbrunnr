<template>
  <div>
    <b-navbar toggleable="sm" type="dark" variant="primary">
      <b-container fluid>
        <b-navbar-brand to="/">{{
          this.$store.state.application.info.title
        }}</b-navbar-brand>

        <b-navbar-nav class="mr-auto">
          <b-nav-item-dropdown text="Spaces" right>
            <b-dropdown-item to="/spaces">Space directory</b-dropdown-item>
            <b-dropdown-item
              href="#"
              v-b-modal.space-create-modal
              :disabled="!this.$store.state.application.profile"
              >Create Space</b-dropdown-item
            >
          </b-nav-item-dropdown>
          <b-button
            variant="light"
            class="create-button"
            v-if="this.$store.state.application.profile"
            @click="create"
            size="sm"
          >
            Create
          </b-button>
        </b-navbar-nav>
        <!-- Right aligned nav items -->
        <b-navbar-nav class="ml-auto">
          <b-nav-form class="flex-search invisibleComponentBorder">
            <b-input-group size="sm">
              <b-form-input placeholder="Search" disabled></b-form-input>
              <b-input-group-append>
                <b-button size="sm" text="Button" variant="success" disabled
                  ><b-icon icon="search"
                /></b-button>
              </b-input-group-append>
            </b-input-group>
          </b-nav-form>

          <div v-if="!this.$store.state.application.profile">
            <b-button
              class="text-light"
              variant="link"
              href="/api/account/login"
              >Log in</b-button
            >
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
            <b-dropdown-item
              :to="'/profile/' + $store.state.application.profile.email"
              >Profile</b-dropdown-item
            >
            <b-dropdown-item href="/api/account/logout"
              >Sign Out</b-dropdown-item
            >
          </b-nav-item-dropdown>
        </b-navbar-nav>
      </b-container>
    </b-navbar>
  </div>
</template>

<script>
import axios from "axios";
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
    create: async function () {
      var spaceKey = this.$route.params.key;
      var pageId = this.$route.params.pageId;
      if (spaceKey == null) {
        spaceKey = await this.getOrCreatePersonalSpace();
      }
      if (pageId == null) {
        var spaceHomePageRequest = await axios.get("/api/space/" + spaceKey);
        pageId = spaceHomePageRequest.data.homePageId;
      }
      await this.createPage(spaceKey, pageId);
    },
    getOrCreatePersonalSpace: async function () {
      var personalSpaceKey =
        this.$store.state.application.profile.email.toUpperCase();
      var getPersonalSpaceRequest = await axios.get(
        "/api/space/" + personalSpaceKey,
        {
          validateStatus: false,
        }
      );
      if (getPersonalSpaceRequest.status == 200) return personalSpaceKey;
      var createPersonalSpaceRequest = await axios.post(
        "/api/space",
        {
          key: personalSpaceKey,
          name: this.$store.state.application.profile.name,
          type: "Personal",
          description: "my personal space",
        },
        {
          validateStatus: false,
        }
      );
      if (createPersonalSpaceRequest.status != 200) {
        alert(
          createPersonalSpaceRequest.statusText +
            "\n" +
            createPersonalSpaceRequest.data
        );
        throw new Exception();
      }
      return personalSpaceKey;
    },
    createPage: async function (spaceKey, parentPageId) {
      var newPage = {
        spaceKey: spaceKey,
        parentPageId: parentPageId,
        name: "New page",
        content: "**Page content**",
      };
      var createPageRequest = await axios.post("/api/page", newPage);
      if (createPageRequest.status == 200)
        this.$router.push(
          "/space/" + spaceKey + "/" + createPageRequest.data.id + "/edit"
        );
    },
  },
};
</script>

<style scoped>
.create-button {
  margin-left: 7px;
  height: 32px;
  margin-top: 0.2em;
}
</style>

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
@media (min-width: 390px) {
  .navbar-expand-sm .navbar-nav {
    -webkit-box-orient: horizontal;
    -webkit-box-direction: normal;
    -ms-flex-direction: row;
    flex-direction: row;
  }
}
.dropdown-menu {
  position: absolute !important;
  right: 0 !important;
}
.custom-dropdown .nav-link {
  padding: 0 !important;
}
.avatar-bg {
  background-color: var(--bs-body-bg);
  color: black;
}
.avatar-bg .b-avatar-img img {
  background-color: var(--bs-body-bg);
}
</style>