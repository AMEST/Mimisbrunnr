import axios from "axios";
import { showToast } from "@/services/Utils";
/*eslint-disable */
var PluginService = {
    getPlugins: async function(skip = null, take = null){
        var url = "/api/plugin";
        if (take){
            url += `?skip=${skip ? skip : 0}&take=${take}`;
        }
        var request = await axios.get(url, {
            validateStatus: false,
        });
        if (request.status == 200) 
            return request.data;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
         "Error when sending request", "warning");
        return [];
    },
    getMacros: async function(skip = null, take = null){
        var url = "/api/plugin/macros";
        if (take){
            url += `?skip=${skip ? skip : 0}&take=${take}`;
        }
        var request = await axios.get(url, {
            validateStatus: false,
        });
        if (request.status == 200) 
            return request.data;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
         "Error when sending request", "warning");
        return [];
    },
    getMacroInfo: async function(macroIdentifier){
        var request = await axios.get(`/api/plugin/macros/${macroIdentifier}`, {
            validateStatus: false,
        });
        if (request.status == 200) 
            return request.data;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
         "Error when sending request", "warning");
        return null;
    },
    getMacroState: async function(pageId, macroIdOnPage) {
        var request = await axios.get(`/api/plugin/macros/${pageId}/${macroIdOnPage}/state`, {
            validateStatus: false,
        });
        if (request.status == 200) 
            return request.data;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
         "Error when sending request", "warning");
        return null;
    },

    saveMacroState: async function(pageId, macroId, macroIdOnPage, parameters) {
        const requestBody = {
            pageId: pageId,
            macroIdentifier: macroId,
            macroIdentifierOnPage: macroIdOnPage,
            params: parameters
        };
        
        var request = await axios.post('/api/plugin/macros/state', requestBody, {
            validateStatus: false,
        });
        if (request.status == 200)
            return request.data;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
         "Error when saving macro state", "warning");
        return null;
    },

    installPlugin: async function(model) {
        var request = await axios.post("/api/plugin", model, {
            validateStatus: false,
        });
        if (request.status == 200)
            return request.data;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
         "Error when installing plugin", "warning");
        return null;
    },

    unInstallPlugin: async function(pluginIdentifier) {
        var request = await axios.delete(`/api/plugin/${pluginIdentifier}`, {
            validateStatus: false,
        });
        if (request.status == 200)
            return true;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
         "Error when uninstalling plugin", "warning");
        return false;
    },

    disablePlugin: async function(pluginIdentifier) {
        var request = await axios.post(`/api/plugin/${pluginIdentifier}/disable`, null, {
            validateStatus: false,
        });
        if (request.status == 200)
            return true;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
         "Error when disabling plugin", "warning");
        return false;
    },

    enablePlugin: async function(pluginIdentifier) {
        var request = await axios.post(`/api/plugin/${pluginIdentifier}/enable`, null, {
            validateStatus: false,
        });
        if (request.status == 200)
            return true;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
         "Error when enabling plugin", "warning");
        return false;
    },

    render: async function(pageId, macroIdOnPage, userRequest) {
        var request = await axios.post(`/api/plugin/macros/${pageId}/${macroIdOnPage}/render`, userRequest, {
            validateStatus: false,
        });
        if (request.status == 200)
            return request.data;
        showToast(`status:${request.status}.${JSON.stringify(request.data)}`,
         "Error when rendering macro", "warning");
        return null;
    },

    renderMacroOnPage: async function(pageId, searchContentId = null) {
        let searchRoot = document;
        if (searchContentId) {
            const element = document.getElementById(searchContentId);
            if (!element) 
                return;
            searchRoot = element;
        }
        const macroDivs = searchRoot.querySelectorAll('div[aria-type="macros"]');
        
        for (const div of macroDivs) {
            try {
                const macroId = div.id.replace('macro_', '');
                const macroName = div.getAttribute('aria-name');
                const paramsString = decodeURI(div.getAttribute('aria-params'));
                
                // Parse params into dictionary
                const params = {};
                paramsString.split('&').forEach(pair => {
                    const [key, value] = pair.split('=');
                    if(key)
                        params[key] = value;
                });

                const userRequest = {
                    MacroIdentifier: macroName,
                    Params: params
                };

                const result = await this.render(pageId, macroId, userRequest);
                if (result && result.html) {
                    div.innerHTML = result.html;
                }
            } catch (error) {
                console.error('Error rendering macro:', error);
                showToast(`Error rendering macro: ${error.message}`, 'Macro Rendering Error', 'error');
            }
        }

        // executing script inside macros
        macroDivs.forEach(div => {
            const directScript = Array.from(div.children).find(child => child.tagName === 'SCRIPT');
            if (directScript) {
                try {
                    new Function(directScript.textContent)();
                } catch (e) {
                    console.error('Error macro script executing:', e);
                }
            }
        });
    }
}
export default PluginService
/* eslint-enable */
