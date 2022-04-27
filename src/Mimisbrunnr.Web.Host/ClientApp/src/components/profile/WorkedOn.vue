<template>
  <b-col sm="9" class="text-left">
    <div class="profile-worked-on">
      <h5>Worked on</h5>
      <b-card>
        <b-card-text>
          <b-list-group-item button v-for="h in [1, 2, 3, 4, 5, 6]" :key="h">
            <b-icon icon="file-earmark-text" style="float: left" />
            <b-skeleton animation="wave" width="25%"></b-skeleton>
          </b-list-group-item>
        </b-card-text>
      </b-card>
    </div>
  </b-col>
</template>

<script>
export default {
    name: "WorkedOn"
};
</script>

<style>
</style>