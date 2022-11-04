import axios from "axios";

/*eslint-disable */
var FeedService = {
    getFeed: async function(){
        var request = await axios.get("/api/feed");
        return request.data;
    },
    getUserFeed: async function(email){
        var request = await axios.get("/api/feed/" + email);
        return request.data;
    } 
}
export default FeedService
/* eslint-enable */