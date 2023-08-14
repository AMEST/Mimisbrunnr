import axios from "axios";
import { showToast } from "@/services/Utils";
/*eslint-disable */
var PageService = {
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
    deleteVersion: async function (pageId, version) {
        var request = await axios.delete(`/api/page/${pageId}/versions/${version}`,
            { validateStatus: false }
        );
        if (request.status == 200)
            return;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
            "Error when page version.", "warning");
    },
}
export default PageService
/* eslint-enable */