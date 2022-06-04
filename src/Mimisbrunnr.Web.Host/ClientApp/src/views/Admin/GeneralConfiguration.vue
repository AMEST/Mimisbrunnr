<template>
    <b-container>
        <Menu activeMenuItem="General" />
        <b-card title="General settings Mimisbrunnr wiki" class="admin-general-card">
            <b-card-text>
                <br />
                <b-form-group description="The title of your wiki instance." label="Title">
                    <b-form-input v-model="info.title" trim></b-form-input>
                </b-form-group>
                <br />
                <b-form-group label="Publish mode"
                    description="Allow unauthorized (anonymous) users to read public wiki information">
                    <b-form-checkbox v-model="info.allowAnonymous" switch>
                        &nbsp;Allow anonymous access
                    </b-form-checkbox>
                </b-form-group>
                <b-form-group label="Markdown settings" description="Allow using html inside markdown on pages">
                    <b-form-checkbox v-model="info.allowHtml" switch>
                        &nbsp;Allow html
                    </b-form-checkbox>
                </b-form-group>
                <b-form-group label="Application swagger" description="Enable swagger for view application api">
                    <b-form-checkbox v-model="info.swaggerEnabled" switch>
                        &nbsp;Enable swagger
                    </b-form-checkbox>
                </b-form-group>
                <br />
                <b-form-group description="Add custom css to instance" label="Custom css">
                    <b-form-textarea size="sm" placeholder=".custom-css{}" disabled></b-form-textarea>
                </b-form-group>
                <br />
                <b-form-group description="Use custom home html page instead of default dashboard"
                    label="Custom homepage">
                    <b-form-checkbox switch disabled>
                        &nbsp;Enable Custom homepage
                    </b-form-checkbox>
                    <b-form-textarea 
                        class="mt-1"
                        size="sm" 
                        rows="3"
                        placeholder="# Welcome to wiki 
                        <hr> 
                        <h3> allowed html code </h3>" 
                        disabled>
                    </b-form-textarea>
                </b-form-group>
            </b-card-text>
            <br />
            <b-button @click="save" variant="primary" size="sm"> save </b-button>
        </b-card>
    </b-container>
</template>

<script>
import Menu from "@/components/admin/Menu.vue"
import axios from 'axios';
export default {
    name: "GeneralConfiguration",
    components: {
        Menu,
    },
    data: () => ({
        info: {
            title: "",
            allowAnonymous: false,
            allowHtml: true,
            swaggerEnabled: true
        },
    }),
    methods: {
        save: async function () {
            await axios.put("/api/admin/applicationConfiguration", this.info);
            window.location.reload();
        },
    },
    mounted: async function () {
        if(!this.$store.state.application.profile || !this.$store.state.application.profile.isAdmin){
            this.$router.push("/error/unauthorized");
            return;
        }
        var configurationRequest = await axios.get("/api/admin/applicationConfiguration");
        this.info = configurationRequest.data;
    }
}
</script>

<style scoped>
.admin-general-card {
    border-top: unset !important;
    border-top-left-radius: unset !important;
    border-top-right-radius: unset !important;
    text-align: right;
}

.admin-general-card .card-title {
    text-align: left;
}

.admin-general-card p {
    text-align: left;
}

.admin-general-card .card-body {
    margin: 2.25rem 2.25rem 2.25rem 2.25rem;
}
</style>