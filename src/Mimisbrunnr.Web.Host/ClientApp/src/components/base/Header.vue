<template>
  <div>
    <b-navbar toggleable="sm" type="dark" variant="primary" id="header-nav">
      <b-container fluid>
        <b-navbar-brand to="/">{{
          this.$store.state.application.info.title
        }}</b-navbar-brand>

        <b-navbar-nav class="mr-auto main-menu-flex">
          <b-nav-item-dropdown :text="$t('header.spacesDropdown.title')" right>
            <!-- Using 'button-content' slot-->
            <template #button-content>
              <b-icon-globe class="spaces-icon"/>
              <span class="spaces-title">{{
                $t("header.spacesDropdown.title")
              }}</span>
            </template>
            <b-dropdown-item to="/spaces">{{
              $t("header.spacesDropdown.list")
            }}</b-dropdown-item>
            <b-dropdown-item
              href="#"
              v-b-modal.space-create-modal
              :disabled="!this.$store.state.application.profile"
              >{{ $t("header.spacesDropdown.create") }}</b-dropdown-item
            >
          </b-nav-item-dropdown>
          <b-nav-item
            v-if="this.$store.state.application.profile"
            to="/people"
            class="people"
          >
            <b-icon-people-fill class="people-icon" />
            <span>{{ $t("header.people") }}</span>
          </b-nav-item>
          <b-button
            variant="light"
            class="create-button"
            v-if="this.$store.state.application.profile"
            @click="create"
            size="sm"
          >
            <b-icon-plus class="create-icon" />
            <span>{{ $t("header.pageCreateButton") }}</span>
          </b-button>
        </b-navbar-nav>
        <!-- Right aligned nav items -->
        <b-navbar-nav class="header-right-menu">
          <b-nav-form class="flex-search invisibleComponentBorder">
            <b-input-group v-b-toggle.search-sidebar size="sm">
              <b-form-input
                class="search-input"
                :placeholder="$t('header.search')"
                disabled
              ></b-form-input>
              <b-input-group-append>
                <b-button
                  class="search-input-button"
                  size="sm"
                  text="Button"
                  variant="success"
                  disabled
                  ><b-icon-search/>
                </b-button>
              </b-input-group-append>
              <div style="position: absolute; top:0;left: 0;bottom: 0;right: 0; z-index: 999;"></div>
            </b-input-group>
          </b-nav-form>

          <div v-if="!this.$store.state.application.profile">
            <b-nav-item-dropdown
              style="display: inline-block"
              :text="$t('header.lang')"
              right
            >
              <b-dropdown-item v-on:click="$i18n.locale = 'en'">
                {{ $i18n.locale == "en" ? "⏵" : "&nbsp;&nbsp;&nbsp;&nbsp;" }}
                English
              </b-dropdown-item>
              <b-dropdown-item v-on:click="$i18n.locale = 'ru'">
                {{ $i18n.locale == "ru" ? "⏵" : "&nbsp;&nbsp;&nbsp;&nbsp;" }}
                Русский
              </b-dropdown-item>
            </b-nav-item-dropdown>
            <b-button
              style="display: inline-block"
              class="text-light"
              variant="link"
              @click="auth"
              >{{ $t("header.login") }}</b-button
            >
          </div>
          <b-nav-item-dropdown v-else right class="custom-dropdown">
            <!-- Using 'button-content' slot -->
            <template #button-content>
              <b-avatar
                class="avatar-bg"
                :text="getUserInitials()"
                :src="$store.state.application.profile.avatarUrl"
                :style="
                  $store.state.application.profile.avatarUrl
                    ? 'background-color: transparent'
                    : ''
                "
              ></b-avatar>
            </template>
            <b-dropdown-text style="width: 240px">
              <span class="username">{{
                $store.state.application.profile.name
              }}</span>
            </b-dropdown-text>
            <b-dropdown-divider></b-dropdown-divider>
            <b-form-select
              v-model="$i18n.locale"
              :options="langs"
              class="header-lang-select"
            ></b-form-select>
            <b-dropdown-divider></b-dropdown-divider>
            <b-dropdown-item @click="goToPersonalSpace">
              {{ $t("header.profileDropdown.personalSpace") }}
            </b-dropdown-item>
            <b-dropdown-divider></b-dropdown-divider>
            <b-dropdown-item
              v-if="this.$store.state.application.profile.isAdmin"
              to="/admin"
            >
              <b-icon-gear /> {{ $t("header.profileDropdown.admin") }}
            </b-dropdown-item>
            <b-dropdown-divider
              v-if="this.$store.state.application.profile.isAdmin"
            ></b-dropdown-divider>
            <b-dropdown-item
              :to="'/profile/' + $store.state.application.profile.email"
            >
              {{ $t("header.profileDropdown.profile") }}
            </b-dropdown-item>
            <b-dropdown-item href="/api/account/logout">
              {{ $t("header.profileDropdown.signOut") }}
            </b-dropdown-item>
          </b-nav-item-dropdown>
        </b-navbar-nav>
      </b-container>
    </b-navbar>
  </div>
</template>

<script>
import {
  BIconGlobe,
  BIconPeopleFill,
  BIconPlus,
  BIconSearch,
  BIconGear,
} from "bootstrap-vue";
import axios from "axios";
import ProfileService from "@/services/profileService";
import { getInitials } from "@/services/Utils";
export default {
  name: "Header",
  data: function () {
    return {
      langs: [
        { value: "ru", text: "Русский" },
        { value: "en", text: "English" },
      ],
    };
  },
  components: {
    BIconGlobe,
    BIconPeopleFill,
    BIconPlus,
    BIconSearch,
    BIconGear,
  },
  watch: {
    // eslint-disable-next-line
    "$i18n.locale": function (to, from) {
      window.localStorage["lang"] = to;
    },
  },
  methods: {
    auth: function () {
      window.location.href =
        "/api/account/login?redirectUri=" + window.location.pathname;
    },
    getUserInitials: function () {
      return getInitials(this.$store.state.application.profile);
    },
    goToPersonalSpace: async function () {
      var personalSpaceKey = await ProfileService.getOrCreatePersonalSpace(
        this.$store.state.application.profile
      );
      this.$router.push("/space/" + personalSpaceKey);
    },
    create: async function () {
      var spaceKey = this.$route.params.key;
      var pageId = this.$route.params.pageId;
      if (spaceKey == null) {
        spaceKey = await ProfileService.getOrCreatePersonalSpace(
          this.$store.state.application.profile
        );
      }
      if (pageId == null) {
        var spaceHomePageRequest = await axios.get("/api/space/" + spaceKey);
        pageId = spaceHomePageRequest.data.homePageId;
      }
      await this.createPage(spaceKey, pageId);
    },
    createPage: async function (spaceKey, parentPageId) {
      var newPage = {
        spaceKey: spaceKey,
        parentPageId: parentPageId,
        name: this.$t("newPage.defaultTitle"),
        content: this.$t("newPage.defaultContent"),
      };
      var createPageRequest = await axios.post("/api/page", newPage, {
        validateStatus: false,
      });
      if (createPageRequest.status == 200)
        this.$router.push(
          "/space/" + spaceKey + "/" + createPageRequest.data.id + "/edit"
        );
      else
        this.$bvToast.toast(createPageRequest.data.message, {
          title: "Error creating page",
          variant: "danger",
          solid: true,
        });
    },
  },
};
</script>

<style scoped>
.search-input {
  color: white !important;
  background-color: #1974d9 !important;
  border: unset !important;
}
.search-input-button {
  background-color: #1974d9 !important;
  border-color: #1974d9 !important;
}
.create-button {
  margin-left: 7px;
  height: 32px;
  margin-top: 0.2em;
}
.create-button .create-icon {
  display: none;
}
.create-button span {
  padding-top: 0.5em;
}
@media (max-width: 834px) {
  .create-button .create-icon {
    display: unset;
  }
  .create-button span {
    display: none;
  }
}
.people .people-icon {
  display: none;
}
@media (max-width: 764px) {
  .people .people-icon {
    display: unset;
  }
  .people span {
    display: none;
  }
  .spaces-title {
    display: none;
  }
}
@media (min-width: 765px) {
  .spaces-icon {
    display: none !important;
  }
}
.main-menu-flex {
  flex-direction: row !important;
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
@media (max-width: 577px) {
  .flex-search {
    display: none !important;
  }
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
.username {
  font-weight: bold;
}
.header-right-menu {
  margin-left: auto;
}
@media (max-width: 574px) {
  .header-right-menu {
    margin-left: unset;
  }
}
@media (max-width: 387px) {
  .header-right-menu .flex-search {
    display: none !important;
  }
}
.header-lang-select {
  border: unset !important;
  padding-left: 1.5rem !important;
}
</style>
