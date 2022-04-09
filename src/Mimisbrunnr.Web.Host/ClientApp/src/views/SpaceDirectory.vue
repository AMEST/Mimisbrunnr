<template>
  <b-container fluid class="text-left full-size-container">
    <b-card>
      <h2>Space Directory</h2>
    </b-card>
    <br>
    <h5>&nbsp;All spaces</h5>
    <b-table-simple style="text-align: left">
      <b-thead>
        <b-tr>
          <b-th>Name</b-th>
          <b-th>Description</b-th>
          <b-th class="text-right">Action</b-th>
        </b-tr>
      </b-thead>
      <b-tbody>
        <b-tr v-for="space in spaces" :key="space.key">
          <b-td><b-link :to="'/space/'+space.key"> {{space.name}} </b-link></b-td>
          <b-td>{{space.description}}</b-td>
          <b-td class="text-right"><b-icon icon="star"></b-icon></b-td>
        </b-tr>
      </b-tbody>
    </b-table-simple>
  </b-container>
</template>
<script>
import axios from 'axios'
export default {
  name: "SpaceDirectory",
  data: () => ({
    spaces: []
  }),
  created: async function(){
    var spacesRequest = await axios.get("/api/space")
    if(spacesRequest.status == 200)
      this.spaces = spacesRequest.data
  }
};
</script>