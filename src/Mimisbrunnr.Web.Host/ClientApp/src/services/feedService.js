import http from "@/services/http";
/*eslint-disable */
var FeedService = {
    getFeed: async function(){
        var request = await http.get("/api/feed");
        return request.data;
    },
    getUserFeed: async function(email){
        var request = await http.get("/api/feed/" + email);
        return request.data;
    } 
}
export default FeedService
/* eslint-enable */