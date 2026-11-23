import { getPluginApi } from "@/services/pluginApi";

export function runCode(code, api = null) {
    if (!code) return null;
    try {
        return new Function("MimisbrunnrPluginApi", code)(api || getPluginApi());
    } catch (error) {
        console.error("Error executing macro script:", error);
        return null;
    }
}

export function runScriptElements(container, api = null) {
    if (!container) return [];
    return Array.from(container.querySelectorAll("script"))
        .map(script => runCode(script.textContent, api));
}
