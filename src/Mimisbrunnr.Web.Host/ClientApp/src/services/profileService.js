import axios from "axios";
import store from "@/services/store";
import { showToast } from "@/services/Utils";
/*eslint-disable */
var ProfileService = {
    getCurrentUser: async function () {
        var request = await this.getCurrentUserRaw();
        if (request.status == 200)
            return request.data;
        if (request.status == 401 || request.status == 404)
            return null;

        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when getting profile.", "warning");
        throw new Exception("Internal sever error. Can't get current user")
    },
    getCurrentUserRaw: async function () {
        return await axios.get("/api/user/current", {
            validateStatus: false,
        });
    },
    getOrCreatePersonalSpace: async function (profile) {
        var personalSpaceKey = profile.email.toUpperCase();
        var getPersonalSpaceRequest = await axios.get(
            "/api/space/" + personalSpaceKey,
            {
                validateStatus: false,
            }
        );
        if (getPersonalSpaceRequest.status == 200) return personalSpaceKey;
        var createPersonalSpaceRequest = await axios.post(
            "/api/space",
            {
                key: personalSpaceKey,
                name: profile.name,
                type: "Personal",
                description: "my personal space",
            },
            {
                validateStatus: false,
            }
        );
        if (createPersonalSpaceRequest.status == 200)
            return personalSpaceKey;

        showToast(`status:${createPersonalSpaceRequest.status} - ${createPersonalSpaceRequest}.${JSON.stringify(createPersonalSpaceRequest.data)}`,
            "Error when getting users.", "warning");
        throw new Exception();
    },
    isAdmin: function () {
        return store.state.application.profile && store.state.application.profile.isAdmin;
    },
    isAnonymous() {
        return store.state.application.profile == undefined;
    },
}
export default ProfileService
/* eslint-enable */