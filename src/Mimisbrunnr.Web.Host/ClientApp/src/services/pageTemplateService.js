async function request(url, options = {}) {
  const res = await fetch(url, {
    headers: { "Content-Type": "application/json" },
    ...options,
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw { status: res.status, message: err.message, data: err };
  }
  if (res.status === 200 && (res.headers.get("content-length") === "0" || res.headers.get("content-type") === null)) {
    return null;
  }
  return res.json();
}

export default {
  getAll(type, spaceKey) {
    const params = new URLSearchParams();
    if (type) params.set("type", type);
    if (spaceKey) params.set("spaceKey", spaceKey);
    const qs = params.toString();
    return request("/api/pagetemplate" + (qs ? "?" + qs : ""));
  },
  getById(id) {
    return request("/api/pagetemplate/" + id);
  },
  create(model) {
    return request("/api/pagetemplate", {
      method: "POST",
      body: JSON.stringify(model),
    });
  },
  update(id, model) {
    return request("/api/pagetemplate/" + id, {
      method: "PUT",
      body: JSON.stringify(model),
    });
  },
  delete(id) {
    return request("/api/pagetemplate/" + id, { method: "DELETE" });
  },
  render(templateId, spaceKey) {
    return request("/api/pagetemplate/" + templateId + "/render", {
      method: "POST",
      body: JSON.stringify({ templateId, spaceKey }),
    });
  },
};
