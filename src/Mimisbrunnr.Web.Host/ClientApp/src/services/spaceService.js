import axios from "axios";
import { showToast } from "@/services/Utils";
/*eslint-disable */
var SpaceService = {
    getSpaces: async function() {
        var spacesRequest = await axios.get("/api/space", {
            validateStatus: false,
        });
        if (spacesRequest.status == 200) 
            return spacesRequest.data;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
         "Error when get spaces", "warning");
         return [];
    },
    getSpace: async function (spaceKey) {
        var request = await axios.get(`/api/space/${spaceKey}`, {
            validateStatus: false,
        });
        if (request.status == 200)
            return request.data;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when get space", "warning");
        if (request.status == 401 || request.status == 403)
            throw request.status;
        return null;
    },
    getSpacePermissions: async function (spaceKey) {
        var request = await axios.get(`/api/space/${spaceKey}/permission`, {
            validateStatus: false,
        });
        if (request.status == 200)
            return request.data;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when get space permissions", "warning");
        if (request.status == 401 || request.status == 403)
            throw request.status;
        return null;
    }
}
export default SpaceService
/* eslint-enable */