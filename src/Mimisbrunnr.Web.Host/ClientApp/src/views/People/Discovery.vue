<template>
  <b-container class="pt-5 text-left">
      <b-form-input class="search-field discovery-search" v-model="searchText" size="lg" :placeholder="$t('people.search.placeholder')" disabled></b-form-input>
      <users/>
      <!--TODO: Add groups list-->
  </b-container>
</template>

<script>
import Users from '@/components/people/discovery/Users.vue';
export default {
    name: "Discovery",
    components: { 
        Users 
    },
    data() {
        return {
            searchText: ""
        };
    },
    computed: {
        isAnonymous() {
            return this.$store.state.application.profile == undefined;
        },
    },
    methods: {
        ensureAnonymous: function () {
            if (this.isAnonymous) {
                this.$router.push("/error/unauthorized");
                return;
            }
        }
    },
    mounted: function () {
        this.ensureAnonymous();
    }
}
</script>

<style>
.discovery-search {
    background-color: transparent !important;
}
</style>