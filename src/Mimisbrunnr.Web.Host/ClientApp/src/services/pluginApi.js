import { showToast } from "@/services/Utils";

let currentContext = null;
let apiInstance = null;
const subscribers = new Set();

function notifyParamsChanged() {
    for (const handler of subscribers) {
        try {
            handler();
        } catch (error) {
            console.error("Plugin API params listener failed:", error);
        }
    }
}

function normalizeUrl(url) {
    if (typeof url !== "string") return null;
    const trimmed = url.trim();
    if (!trimmed) return null;
    if (!/^(https?:\/\/|\/)/i.test(trimmed)) return null;
    return trimmed;
}

export function createPluginApi(contextResolver = null) {
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
            context.setParam(name, value);
            notifyParamsChanged();
            return true;
        },
        save: () => {
            const context = getContextObject();
            return context && context.save ? context.save() : Promise.resolve(false);
        },
        showToast: (message, title = "", variant = "info") => showToast(message, title, variant),
        openAttachmentPicker: () => {
            const context = getContextObject();
            return context && context.openAttachmentPicker
                ? context.openAttachmentPicker()
                : Promise.resolve(null);
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
            if (typeof handler !== "function") return () => {};
            subscribers.add(handler);
            return () => subscribers.delete(handler);
        }
    };
}

export function getPluginApi() {
    return apiInstance || createPluginApi(() => currentContext);
}

export function installPluginApi() {
    if (window.MimisbrunnrPluginApi) {
        apiInstance = window.MimisbrunnrPluginApi;
        return;
    }
    apiInstance = createPluginApi(() => currentContext);
    Object.defineProperty(window, "MimisbrunnrPluginApi", {
        configurable: true,
        value: apiInstance
    });
}

export function setPluginApiContext(context) {
    currentContext = context;
    notifyParamsChanged();
}

export function clearPluginApiContext(context) {
    if (currentContext !== context) return;
    currentContext = null;
    notifyParamsChanged();
}
