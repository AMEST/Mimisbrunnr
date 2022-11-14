import axios from "axios";
import { showToast } from "@/services/Utils";
/*eslint-disable */
var SearchService = {
    findSpaces: async function (search) {
        var request = await axios.get(`/api/search/space?search=${search}`, {
            validateStatus: false,
        });
        if (request.status == 200)
            return request.data;
        if (request.status == 401 || request.status == 404)
            return null;

        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when searching spaces.", "warning");
        throw new Exception("Internal sever error. Can't find spaces")
    },
    findPages: async function (search) {
        var request = await axios.get(`/api/search/page?search=${search}`, {
            validateStatus: false,
        });
        if (request.status == 200)
            return request.data;
        if (request.status == 401 || request.status == 404)
            return null;

        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when searching spaces.", "warning");
        throw new Exception("Internal sever error. Can't find pages")
    },
    findUsers: async function (search) {
        var request = await axios.get(`/api/search/user?search=${search}`, {
            validateStatus: false,
        });
        if (request.status == 200)
            return request.data;
        if (request.status == 401 || request.status == 404)
            return null;

        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when searching users.", "warning");
        throw new Exception("Internal sever error. Can't find users")
    },
}
export default SearchService
/* eslint-enable */