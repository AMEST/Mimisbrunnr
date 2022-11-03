import axios from "axios";

export async function getFeed() {
    var request = await axios.get("/api/feed");
    return request.data;
}

export async function getUserFeed(email) {
    var request = await axios.get("/api/feed/" + email);
    return request.data;
}