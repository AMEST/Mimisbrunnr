import { showToast } from "@/services/Utils";

let activeApi = null;
const inactiveApi = createPluginApi();

function normalizeUrl(url) {
    if (typeof url !== "string") return null;
    const trimmed = url.trim();
    if (!trimmed) return null;
    if (!/^(https?:\/\/|\/)/i.test(trimmed)) return null;
    return trimmed;
}

export function createPluginApi(contextResolver = null) {
    const subscribers = new Set();
    const getContextObject = () => (typeof contextResolver === "function" ? contextResolver() : contextResolver);
    const getParams = () => {
        const context = getContextObject();
        return context && context.getParams ? { ...context.getParams() } : {};
    };
    const getContext = () => {
        const context = getContextObject();
        return context ? {
            pageId: context.pageId,
            macroId: context.macroId,
            macroIdentifier: context.macroIdentifier,
            mode: context.mode || "render"
        } : null;
    };

    return {
        helpers: {},
        getParams,
        getParam: (name) => getParams()[name],
        setParam: (name, value) => {
            const context = getContextObject();
            if (!context || !context.setParam || !name) return false;
            const params = getParams();
            if (Object.prototype.hasOwnProperty.call(params, name) && params[name] === value) return true;
            context.setParam(name, value);
            for (const handler of subscribers) {
                try {
                    handler();
                } catch (error) {
                    console.error("Plugin API params listener failed:", error);
                }
            }
            return true;
        },
        save: () => {
            const context = getContextObject();
            return context && context.save ? context.save() : Promise.resolve(false);
        },
        showToast: (message, title = "", variant = "info") => showToast(message, title, variant),
        openAttachmentPicker: async (callback) => {
            const context = getContextObject();
            const attachment = context && context.openAttachmentPicker
                ? await context.openAttachmentPicker()
                : null;
            if (typeof callback === "function") callback(attachment);
            return attachment;
        },
        openEditor: (options = {}) => {
            const context = getContextObject();
            if (context && context.openEditor) return context.openEditor(options);
            const url = normalizeUrl(typeof options === "string" ? options : options.url);
            if (!url) return Promise.resolve(false);
            const target = typeof options === "string" ? "_blank" : (options.target || "_blank");
            window.open(url, target, "noopener");
            return Promise.resolve(true);
        },
        getContext,
        onParamsChanged: (handler) => {
            if (typeof handler !== "function" || !getContextObject()) return () => {};
            subscribers.add(handler);
            return () => subscribers.delete(handler);
        }
    };
}

export function getPluginApi() {
    return activeApi || inactiveApi;
}

export function installPluginApi() {
    if (window.MimisbrunnrPluginApi) return;
    Object.defineProperty(window, "MimisbrunnrPluginApi", {
        configurable: true,
        get: getPluginApi
    });
}

export function setPluginApiContext(context) {
    const api = createPluginApi(() => activeApi === api ? context : null);
    activeApi = api;
    return api;
}

export function clearPluginApiContext(api) {
    if (activeApi === api) activeApi = null;
}
