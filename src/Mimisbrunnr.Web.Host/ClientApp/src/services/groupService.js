import axios from "axios";
import { showToast } from "@/services/Utils";
/*eslint-disable */
var GroupService = {
    getGroup: async function (name) {
        var request = await axios.get(`/api/group/${name}`,
            { validateStatus: false }
        );
        if (request.status == 200)
            return request.data;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when get group.", "warning");
        return null;
    },
    getGroups: async function (offset) {
        var request = await axios.get(`/api/group?offset=${offset}`,
            { validateStatus: false }
        );
        if (request.status == 200)
            return request.data;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when getting groups.", "warning");
        return null;
    },
    createGroup: async function (name, description) {
        var request = await axios.post(`/api/group`,
            {
                name: name,
                description: description,
            },
            { validateStatus: false }
        );
        if (request.status == 200) {
            return true;
        }
        showToast( `status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when adding group.", "warning")
        return false;
    },
    deleteGroup: async function (name) {
        var request = await axios.delete(`/api/Group/${name}`,
            {
                validateStatus: false,
            }
        );
        if (request.status == 200)
            return true;

        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when deleting group.", "warning")
        return false;
    }
}
export default GroupService
/* eslint-enable */