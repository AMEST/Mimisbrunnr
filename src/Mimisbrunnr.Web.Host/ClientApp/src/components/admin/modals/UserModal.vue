<template>
    <b-modal id="user-modal" :title="$t('admin.users.modal.title')" @shown="onShow">
        <b-form @submit.stop.prevent>
            <b-form-group :label="$t('admin.users.modal.name')">
                <b-form-input v-model="name" :state="isUserValid" required></b-form-input>
            </b-form-group>
            <b-form-group :label="$t('admin.users.modal.email')">
                <b-form-input v-model="email" :state="isEmailValid" type="email" required></b-form-input>
                <b-form-invalid-feedback :state="isEmailValid">
                    {{ $t('admin.users.modal.invalidEmail') }}
                </b-form-invalid-feedback>
            </b-form-group>
            <b-form-group :label="$t('admin.users.modal.avatarUrl')">
                <b-form-input v-model="avatarUrl" :state="isAvatarUrlValid"></b-form-input>
            </b-form-group>
        </b-form>
        <template #modal-footer="{ cancel }">
            <b-button size="sm" variant="success" @click="ok()"> OK </b-button>
            <b-button size="sm" @click="cancel()"> Cancel </b-button>
        </template>
    </b-modal>
</template>

<script>
import UserService from "@/services/userService";
export default {
    name: "UserModal",
    data() {
        return {
            name: "",
            email: "",
            avatarUrl: "",
        };
    },
    props: {
        createAction: Function,
    },
    computed: {
        isUserValid() {
            return this.name.trim().length > 2;
        },
        isEmailValid() {
            const atIndex = this.email.trim().indexOf('@');
            return atIndex > 0 &&
                atIndex < this.email.trim().length - 1 &&
                this.email.trim().split('@').length === 2
                && this.email.trim().indexOf(" ") === -1;
        },
        isAvatarUrlValid() {
            return !this.avatarUrl || this.avatarUrl.startsWith("http://") || this.avatarUrl.startsWith("https://");
        }
    },
    methods: {
        ok: async function () {
            if (!this.isUserValid) return;
            if (!this.isEmailValid) return;

            var created = await UserService.create(this.email.trim(), this.name.trim(), this.avatarUrl.trim());
            if (created) {
                this.$bvModal.hide("user-modal");
                this.createAction();
            }
        },
        onShow: function () {
            this.name = "";
            this.email = "";
            this.avatarUrl = "";
        },
    },
};
</script>

<style></style>
