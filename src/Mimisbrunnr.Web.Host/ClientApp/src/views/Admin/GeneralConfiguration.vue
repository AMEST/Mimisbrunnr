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
                    <b-form-textarea 
                    size="sm" 
                    placeholder=".custom-css{}" 
                    v-model="info.customCss"
                    rows="5"
                    max-rows="8"
                    ></b-form-textarea>
                </b-form-group>
                <br />
                <b-form-group description="Use custom home html page instead of default dashboard"
                    label="Custom homepage">
                    <b-form-checkbox switch v-model="info.customHomepageEnabled">
                        &nbsp;Enable Custom homepage
                    </b-form-checkbox>
                    <b-form-select v-model="info.customHomepageSpaceKey" :options="spaces" class="mr-3 mt-2" :disabled="!info.customHomepageEnabled">
                        <b-form-select-option :value="null" disabled>-- Please select space --</b-form-select-option>
                    </b-form-select>
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
            swaggerEnabled: true,
            customCss: "",
            customHomepageEnabled: false,
            customHomepageSpaceKey: null
        },
        spaces: []
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
        var spacesRequest = await axios.get("/api/space")
        for (let spaceIndex in spacesRequest.data){
            if(spacesRequest.data[spaceIndex].type != "Public") continue;
            this.spaces.push({ value: spacesRequest.data[spaceIndex].key, text: `${spacesRequest.data[spaceIndex].name} (${spacesRequest.data[spaceIndex].key})` })
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