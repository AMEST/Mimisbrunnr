<template>
  <b-container class="pt-5 text-left">
      <b-form-input class="search-field discovery-search" v-model="searchText" size="lg" :placeholder="$t('people.search.placeholder')" disabled></b-form-input>
      <users/>
      <!--TODO: Add groups list-->
  </b-container>
</template>

<script>
import Users from '@/components/people/discovery/Users.vue';
import ProfileService from '@/services/profileService';
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
            if (!ProfileService.isAnonymous()) return;
            this.$router.push("/error/unauthorized");
        }
    },
    mounted: function () {
        this.ensureAnonymous();
        document.title = `${this.$store.state.application.info.title}`;
    }
}
</script>

<style>
.discovery-search {
    background-color: transparent !important;
}
</style>