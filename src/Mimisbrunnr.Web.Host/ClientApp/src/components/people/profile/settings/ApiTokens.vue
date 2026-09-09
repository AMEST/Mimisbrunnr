<template>
  <b-tab :title="$t('profile.settings.tokens.title')" active no-body>
    <b-card
      :title="$t('profile.settings.tokens.title')"
      :sub-title="$t('profile.settings.tokens.description')"
    >
      <b-alert v-if="this.tokens.length == 0" show variant="light">{{$t('profile.settings.tokens.empty')}}</b-alert>
      <div class="api-token-list">
        <b-list-group>
          <b-list-group-item
            button
            v-for="token in this.tokens"
            :key="token.id"
            :class="[token.revoked || new Date(token.expired) < new Date() ? 'revoked-token' : '']"
          >
            {{ $t("profile.settings.tokens.notBefore") }}
            {{ new Date(token.created).toLocaleString() }}
            {{ $t("profile.settings.tokens.notAfter") }}
            {{ new Date(token.expired).toLocaleString() }}
            <b-button
              v-if="!token.revoked && new Date(token.expired) > new Date()"
              variant="outline-danger"
              style="float: right"
              v-on:click="revoke(token)"
            >
              {{ $t("profile.settings.tokens.revoke") }}
            </b-button>
          </b-list-group-item>
        </b-list-group>
      </div>
      <hr />
      <b-card-sub-title>{{
        $t("profile.settings.tokens.createNewDescription")
      }}</b-card-sub-title>
      <b-input-group v-if="!token" :prepend="$t('profile.settings.tokens.days')" class="mt-3">
        <b-form-input v-model="tokenLifetimeInDays" required></b-form-input>
        <b-input-group-append>
          <b-button size="sm" text="Button" variant="success" @click="create">{{
            $t("profile.settings.tokens.create")
          }}</b-button>
        </b-input-group-append>
      </b-input-group>
      <b-form-group v-if="token" class="mt-3">
        <label for="generatedToken">{{ $t("profile.settings.tokens.generatedToken") }}</label>
        <b-form-textarea
          id="generatedToken"
          plaintext
          :value="token"
          :rows="7"
          readonly
        ></b-form-textarea>
        <b-button class="mt-2" block variant="success" @click="copyToken">{{
          $t("profile.settings.tokens.copy")
        }}</b-button>
      </b-form-group>
    </b-card>
  </b-tab>
</template>

<script>
import http from "@/services/http";
export default {
  name: "ApiTokens",
  data() {
    return {
      tokens: [],
      tokenLifetimeInDays: "180",
      token: null,
    };
  },
  methods: {
    load: async function () {
      var tokenRequest = await http.get("/api/account/token");
      this.tokens = tokenRequest.data;
    },
    revoke: async function (token) {
      var approve = await this.$bvModal.msgBoxConfirm(this.$t("profile.settings.tokens.approveModal.revoke"), {
            title: this.$t("profile.settings.tokens.approveModal.title"),
            centered: true,
            size: 'sm',
            buttonSize: 'sm',
            cancelTitle: this.$t("profile.settings.tokens.approveModal.cancel"),
            okTitle: this.$t("profile.settings.tokens.approveModal.ok"),
            okVariant: 'danger',
            headerClass: 'p-2 border-bottom-0',
            footerClass: 'p-2 border-top-0',
      });
      if(!approve) return;
      await http.delete(`/api/account/token/${token.id}`);
      await this.load();
    },
    create: async function () {
      var tokenLifetime = parseInt(this.tokenLifetimeInDays);
      if (isNaN(tokenLifetime) || tokenLifetime < 1) {
        this.$bvToast.toast("Invalid token lifetime", {
          title: `Token creation error`,
          variant: "danger",
          solid: true,
        });
        return;
      }

      var tokenLifetimeTimeSpan = `${tokenLifetime}.00:00:00`
      var tokenRequest = await http.post(`/api/account/token`, {
        "lifetime": tokenLifetimeTimeSpan
      });
      this.token = tokenRequest.data.token;
      await this.load();
    },
    copyToken: async function () {
      try {
        await navigator.clipboard.writeText(this.token);
      } catch (e) {
        return;
      }
      this.$bvToast.toast(this.$t("profile.settings.tokens.tokenCopied"), {
        title: this.$t("profile.settings.tokens.copy"),
        variant: "success",
        solid: true,
      });
    },
  },
  mounted: function () {
    this.load();
  },
};
</script>

<style>
.revoked-token {
  text-decoration: line-through !important;
}

.api-token-list {
  max-height: 420px;
  overflow-y: auto;
}
</style>