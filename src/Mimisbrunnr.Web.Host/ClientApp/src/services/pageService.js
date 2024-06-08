import axios from "axios";
import { showToast } from "@/services/Utils";
/*eslint-disable */
var PageService = {
    getPageTree: async function(pageId) {
        var request = await axios.get(
            "/api/page/" + pageId + "/tree",
            {
              validateStatus: false,
            }
          );
        if (request.status == 200)
            return request.data;

        showToast(
            `${request.statusText} (${request.status})`,
            "Can't load page tree for this space",
            "danger"
        );
        return null;
    },
    getPage: async function (pageId) {
        var request = await axios.get(`/api/page/${pageId}`, {
            validateStatus: false,
        });

        if (request.status == 404) return null;
        if (request.status == 200)
            return request.data;

        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when get draft.", "warning");
        throw 401;
    },
    savePage: async function (page) {
        var request = await axios.put(
            `/api/page/${page.id}`,
            page,
            {
                validateStatus: false,
            }
        );
        if (request.status == 200) return true;

        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when save page.", "danger");
        return false;
    },
    getDraft: async function (pageId) {
        var request = await axios.get(`/api/draft/${pageId}`, {
            validateStatus: false,
        });

        if (request.status == 404) return null;
        if (request.status == 200)
            return request.data;

        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when get draft.", "warning");
        throw 401;
    },
    saveDraft: async function (pageId, draft) {
        var request = await axios.put(`/api/draft/${pageId}`, draft, {
            validateStatus: false,
        });
        if (request.status == 200)
            return request.data;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when save draft.", "warning");
        return null;
    },
    deleteDraft: async function (pageId) {
        var request = await axios.delete(`/api/draft/${pageId}`, {
            validateStatus: false,
        });
        if (request.status == 200)
            return;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when deleting draft.", "warning");
    },
    getComments: async function (pageId) {
        var request = await axios.get(`/api/page/${pageId}/comments`,
            { validateStatus: false }
        );
        if (request.status == 200)
            return request.data;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when get comments.", "warning");
        return null;
    },
    createComment: async function (pageId, comment) {
        var request = await axios.post(`/api/page/${pageId}/comments`,
            {
                "message": comment
            },
            { validateStatus: false }
        );
        if (request.status == 200)
            return request.data;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when create comment.", "warning");
        return null;
    },
    deleteComment: async function (pageId, commentId) {
        var request = await axios.delete(`/api/page/${pageId}/comments/${commentId}`,
            { validateStatus: false }
        );
        if (request.status == 200)
            return;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when deleting comment.", "warning");
    },
    getVersions: async function (pageId) {
        var request = await axios.get(`/api/page/${pageId}/versions`,
            { validateStatus: false }
        );
        if (request.status == 200)
            return request.data;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when get page versions.", "warning");
        return null;
    },
    restoreVersion: async function (pageId, version) {
        var request = await axios.put(`/api/page/${pageId}/versions/${version}`,
            { validateStatus: false }
        );
        if (request.status == 200)
            return;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when restore page version.", "warning");
    },
    deleteVersion: async function (pageId, version) {
        var request = await axios.delete(`/api/page/${pageId}/versions/${version}`,
            { validateStatus: false }
        );
        if (request.status == 200)
            return;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when delete page version.", "warning");
    },
}
export default PageService
/* eslint-enable */