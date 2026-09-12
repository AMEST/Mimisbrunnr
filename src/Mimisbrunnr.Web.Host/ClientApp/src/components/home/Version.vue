<template>
  <div class="text-muted version">
      Version: {{this.version}}
  </div>
</template>

<script>
import http from "@/services/http";
export default {
    name: "Version",
    data() {
        return {
            version: ""
        }
    },
    mounted: async function(){
        var versionRequest = await http.get("/api/version");
        if(versionRequest.status == 200)
            this.version = versionRequest.data.version;
    }
}
</script>

<style scoped>
.version{
    position: absolute;
    bottom: 1em;
    max-width: 90%;
}
@media (max-width: 860px) {
  .version {
    bottom: 4em;
  }
}
</style>