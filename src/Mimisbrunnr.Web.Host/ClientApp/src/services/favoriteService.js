import axios from "axios";
import { showToast } from "@/services/Utils";
/*eslint-disable */
var FavoriteService = {
    getAll: async function (count = 15, skip = 0, type = undefined) {
        var request = await axios.get(`/api/favorites?count=${count}&skip=${skip}`+(type == undefined ? "" : `&type=${type}`),
            { validateStatus: false }
        );
        if (request.status == 200)
            return request.data;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when get favorites.", "warning");
        return [];
    },
    getUser: async function (userEmail) {
        var request = await axios.post(`/api/favorites/findOne`,
            {
                "$type": "FavoriteUserFindModel",
                "userEmail": userEmail,
            },
            { validateStatus: false }
        );
        if (request.status == 200)
            return request.data;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when get favorite.", "warning");
        return null;
    },
    getSpace: async function (spaceKey) {
        var request = await axios.post(`/api/favorites/findOne`,
            {
                "$type": "FavoriteSpaceFindModel",
                "spaceKey": spaceKey,
            },
            { validateStatus: false }
        );
        if (request.status == 200)
            return request.data;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when get favorite.", "warning");
        return null;
    },
    getPage: async function (pageId) {
        var request = await axios.post(`/api/favorites/findOne`,
            {
                "$type": "FavoritePageFindModel",
                "pageId": pageId,
            },
            { validateStatus: false }
        );
        if (request.status == 200)
            return request.data;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when get favorite.", "warning");
        return null;
    },
    addUser: async function (userEmail) {
        var request = await axios.post(`/api/favorites`,
            {
                "$type": "FavoriteUserCreateModel",
                "userEmail": userEmail,
            },
            { validateStatus: false }
        );
        if (request.status == 200) {
            return true;
        }
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when adding user to favorite.", "warning")
        return false;
    },
    addSpace: async function (spaceKey) {
        var request = await axios.post(`/api/favorites`,
            {
                "$type": "FavoriteSpaceCreateModel",
                "spaceKey": spaceKey,
            },
            { validateStatus: false }
        );
        if (request.status == 200) {
            return true;
        }
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when adding space to favorite.", "warning")
        return false;
    },
    addPage: async function (pageId) {
        var request = await axios.post(`/api/favorites`,
            {
                "$type": "FavoritePageCreateModel",
                "pageId": pageId,
            },
            { validateStatus: false }
        );
        if (request.status == 200) {
            return true;
        }
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when adding page to favorite.", "warning")
        return false;
    },
    existsUser: async function (userEmail) {
        var request = await axios.post(`/api/favorites/exists`,
            {
                "$type": "FavoriteUserFindModel",
                "userEmail": userEmail,
            },
            { validateStatus: false }
        );
        return request.status == 200;
    },
    existsSpace: async function (spaceKey) {
        var request = await axios.post(`/api/favorites/exists`,
            {
                "$type": "FavoriteSpaceFindModel",
                "spaceKey": spaceKey,
            },
            { validateStatus: false }
        );
        return request.status == 200;
    },
    existsPage: async function (pageId) {
        var request = await axios.post(`/api/favorites/exists`,
            {
                "$type": "FavoritePageFindModel",
                "pageId": pageId,
            },
            { validateStatus: false }
        );
        return request.status == 200;
    },
    delete: async function (id) {
        var request = await axios.delete(`/api/favorites/${id}`,
            {
                validateStatus: false,
            }
        );
        if (request.status == 200)
            return true;

        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when deleting from favorites.", "warning")
        return false;
    }
}
export default FavoriteService
/* eslint-enable */