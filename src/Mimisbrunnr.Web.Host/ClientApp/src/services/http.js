const DEFAULT_VALIDATE_STATUS = (status) => status >= 200 && status < 300;

function parseResponseData(text) {
    if (!text) return "";
    try {
        return JSON.parse(text);
    } catch (e) {
        return text;
    }
}

function buildRequest(config) {
    const headers = Object.assign({}, config.headers || {});
    const data = config.data;
    let body = undefined;

    if (data != null) {
        if (data instanceof FormData) {
            // Let the browser set the multipart/form-data boundary.
            body = data;
        } else if (typeof data === "string") {
            if (!headers["Content-Type"]) headers["Content-Type"] = "text/plain;charset=UTF-8";
            body = data;
        } else {
            if (!headers["Content-Type"]) headers["Content-Type"] = "application/json";
            body = JSON.stringify(data);
        }
    }

    return { headers, body };
}

function parseHeaders(rawHeader) {
    const headers = {};
    if (!rawHeader) return headers;
    rawHeader.trim().split(/[\r\n]+/).forEach((line) => {
        const separatorIndex = line.indexOf(":");
        if (separatorIndex > 0) {
            const name = line.substring(0, separatorIndex).trim();
            const value = line.substring(separatorIndex + 1).trim();
            if (name) headers[name] = value;
        }
    });
    return headers;
}

function parseFetchHeaders(headers) {
    const result = {};
    headers.forEach((value, name) => {
        result[name] = value;
    });
    return result;
}

function createRequestError(message, config, response) {
    const error = new Error(message);
    error.name = "HttpError";
    error.config = config;
    error.response = response || { data: "", status: 0, statusText: "", headers: {} };
    return error;
}

function resolveValidateStatus(validateStatus) {
    if (validateStatus === false) return () => true;
    if (typeof validateStatus === "function") return validateStatus;
    return DEFAULT_VALIDATE_STATUS;
}

function xhrRequest(config, headers, body) {
    const validateStatus = resolveValidateStatus(config.validateStatus);

    return new Promise((resolve, reject) => {
        const xhr = new XMLHttpRequest();
        xhr.open((config.method || "get").toUpperCase(), config.url);

        Object.keys(headers).forEach((key) => {
            xhr.setRequestHeader(key, headers[key]);
        });

        if (typeof config.onUploadProgress === "function" && xhr.upload) {
            xhr.upload.onprogress = config.onUploadProgress;
        }

        xhr.onload = () => {
            const data = parseResponseData(xhr.responseText);
            const response = {
                data,
                status: xhr.status,
                statusText: xhr.statusText,
                headers: parseHeaders(xhr.getAllResponseHeaders()),
                config,
                request: xhr,
            };

            if (!validateStatus(response.status)) {
                reject(createRequestError("Request failed with status code " + response.status, config, response));
            } else {
                resolve(response);
            }
        };

        xhr.onerror = () => reject(createRequestError("Network Error", config));
        xhr.ontimeout = () => reject(createRequestError("timeout exceeded", config));

        xhr.send(body);
    });
}

async function request(config) {
    const { headers, body } = buildRequest(config);

    if (typeof config.onUploadProgress === "function") {
        return xhrRequest(config, headers, body);
    }

    let response;
    try {
        response = await fetch(config.url, {
            method: (config.method || "get").toLowerCase(),
            headers,
            body,
            credentials: "same-origin",
        });
    } catch (e) {
        throw createRequestError("Network Error", config);
    }

    const data = parseResponseData(await response.text());
    const result = {
        data,
        status: response.status,
        statusText: response.statusText,
        headers: parseFetchHeaders(response.headers),
        config,
        request: response,
    };

    const validateStatus = resolveValidateStatus(config.validateStatus);
    if (!validateStatus(result.status)) {
        throw createRequestError("Request failed with status code " + result.status, config, result);
    }

    return result;
}

function http(config) {
    return request(config);
}

http.request = request;
http.get = (url, config) => request(Object.assign({ method: "get", url }, config));
http.post = (url, data, config) => request(Object.assign({ method: "post", url, data }, config));
http.put = (url, data, config) => request(Object.assign({ method: "put", url, data }, config));
http.delete = (url, config) => request(Object.assign({ method: "delete", url }, config));
http.patch = (url, data, config) => request(Object.assign({ method: "patch", url, data }, config));

export default http;