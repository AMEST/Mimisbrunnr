<template>
  <h1>{{this.errorText}}</h1>
</template>

<script>
export default {
    name:"Error",
    computed:{
        errorText() {
            switch(this.$route.params.err){
                case "notfound":
                    return "404 Resource not found";
                case "unauthorized":
                    return "Not found or user has no permissions";
                default:
                    return "Unknown error";
            }
        },
    }
}
</script>

<style>

</style>