import axios from "axios";
import { showToast } from "@/services/Utils";
/*eslint-disable */
var UserService = {
    getUser: async function (email) {
        var request = await axios.get(
            `/api/user/${email}`,
            { validateStatus: false }
        );
        if (request.status == 200)
            return request.data;

        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when getting user.", "warning");
        return null;
    },
    getUsers: async function (offset) {
        var request = await axios.get(
            `/api/user?offset=${offset}`,
            { validateStatus: false }
        );
        if (request.status == 200)
            return request.data;

        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when getting users.", "warning");
        return null;
    },
    promote: async function (email) {
        var request = await axios.post(`/api/user/${email}/promote`, {
            validateStatus: false,
        });
        if(request.status == 200)
            return true;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
          "Error when promote user.", "warning");
        return false;
    },
    demote: async function (email) {
        var request = await axios.post(`/api/user/${email}/demote`, {
            validateStatus: false,
        });
        if(request.status == 200)
            return true;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
          "Error when demote user.", "warning");
        return false;
    },
    enable: async function (email) {
        var request = await axios.post(`/api/user/${email}/enable`, {
            validateStatus: false,
        });
        if(request.status == 200)
            return true;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
          "Error when enable user.", "warning");
        return false;
    },
    disable: async function (email) {
        var request = await axios.post(`/api/user/${email}/disable`, {
            validateStatus: false,
        });
        if(request.status == 200)
            return true;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
          "Error when disable user.", "warning");
        return false;
    },
}
export default UserService
/* eslint-enable */