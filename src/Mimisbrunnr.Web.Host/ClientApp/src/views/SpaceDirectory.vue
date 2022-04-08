<template>
  <container fluid>
    <b-card>
      <h2 class="text-left">Space Directory</h2>
    </b-card>
    <b-table-simple>
      <b-thead>
        <b-tr>
          <b-th>Name</b-th>
          <b-th>Description</b-th>
          <b-th>Action</b-th>
        </b-tr>
      </b-thead>
      <b-tbody>
        <b-tr v-for="space in spaces" :key="space.key">
          <b-td><b-link :to="'/space/'+space.key"> {{space.name}} </b-link></b-td>
          <b-td>{{space.description}}</b-td>
          <b-td><b-icon icon="star"></b-icon></b-td>
        </b-tr>
      </b-tbody>
    </b-table-simple>
  </container>
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