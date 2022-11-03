<template>
    <b-container fluid class="center-error">
        <b-alert show variant="warning" class="center-error-alert">
            <h4 class="alert-heading">{{ this.$route.params.err }}</h4>
            <p>
                {{ this.errorText }}
            </p>
        </b-alert>
    </b-container>
</template>

<script>
export default {
    name: "Error",
    computed: {
        errorText() {
            switch (this.$route.params.err) {
                case "notfound":
                    return "404 Resource not found";
                case "unauthorized":
                    return "Not found or user has no permissions";
                default:
                    return "Unknown error";
            }
        },
    },
    mounted () {
        document.title = `Error - ${this.$store.state.application.info.title}`;
    },
}
</script>

<style>
.center-error{
    margin-right: auto;
    margin-left: auto;
    width: 100%;
    top: 40%;
    -webkit-transform: translateY(-15%);
    transform: translateY(-15%);
    position: absolute;
}
.center-error-alert {
    margin-left: auto;
    margin-right: auto;
    width: 350px;
}
</style>