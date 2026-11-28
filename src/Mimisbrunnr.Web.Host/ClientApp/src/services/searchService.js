import http from "@/services/http";
import { showToast } from "@/services/Utils";
/*eslint-disable */
var SearchService = {
    findSpaces: async function (search) {
        var request = await http.get(`/api/search/space?search=${search}`, {
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
    findPages: async function (search, limit, offset) {
        const params = new URLSearchParams({ search, limit, offset });
        var request = await http.get(`/api/search/page?${params}`, {
            validateStatus: false,
        });
        if (request.status == 200) {
            const nextOffset = request.headers["x-next-offset"];
            return { pages: request.data, nextOffset: nextOffset == null ? null : Number(nextOffset) };
        }
        if (request.status == 401 || request.status == 404)
            return null;

        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when searching spaces.", "warning");
        throw new Error("Can't find pages");
    },
    findUsers: async function (search) {
        var request = await http.get(`/api/search/user?search=${search}`, {
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