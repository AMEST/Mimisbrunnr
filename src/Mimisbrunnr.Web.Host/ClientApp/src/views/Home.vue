<template>
  <div class="home">
    <b-container fluid>
      <b-row class="h-100vh">
        <b-col>
          <Menu/>
        </b-col>

        <b-col>
          none
          <b-button v-b-toggle.home-sidebar>Toggle Sidebar</b-button>
        </b-col>
      </b-row>
    </b-container>
  </div>
</template>

<script>
import Menu from '@/components/home/Menu.vue'
export default {
  name: 'home',
  components: {
    Menu
  }
}
</script>

<style scoped>
</style>