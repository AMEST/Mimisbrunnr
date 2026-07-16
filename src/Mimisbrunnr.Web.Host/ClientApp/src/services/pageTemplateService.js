import { showToast } from "@/services/Utils";

async function request(url, options = {}) {
  const res = await fetch(url, {
    headers: { "Content-Type": "application/json" },
    ...options,
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    const msg = err.message || "Request failed";
    showToast(`Status: ${res.status}. ${msg}`, "Error", "danger");
    throw { status: res.status, message: msg, data: err };
  }
  if (res.status === 200 && (res.headers.get("content-length") === "0" || res.headers.get("content-type") === null)) {
    return null;
  }
  return res.json();
}

export default {
  async getAll(type, spaceKey) {
    try {
      const params = new URLSearchParams();
      if (type) params.set("type", type);
      if (spaceKey) params.set("spaceKey", spaceKey);
      const qs = params.toString();
      const result = await request("/api/pagetemplate" + (qs ? "?" + qs : ""));
      return Array.isArray(result) ? result : [];
    } catch (e) {
      return [];
    }
  },
  async getById(id) {
    try {
      return await request("/api/pagetemplate/" + id);
    } catch (e) {
      return null;
    }
  },
  async create(model) {
    try {
      const result = await request("/api/pagetemplate", {
        method: "POST",
        body: JSON.stringify(model),
      });
      showToast("Template created", "Success", "success");
      return result;
    } catch (e) {
      return null;
    }
  },
  async update(id, model) {
    try {
      await request("/api/pagetemplate/" + id, {
        method: "PUT",
        body: JSON.stringify(model),
      });
      showToast("Template updated", "Success", "success");
    } catch (e) {
      return null;
    }
  },
  async delete(id) {
    try {
      await request("/api/pagetemplate/" + id, { method: "DELETE" });
      showToast("Template deleted", "Success", "success");
    } catch (e) {
      return null;
    }
  },
  async render(templateId, spaceKey) {
    try {
      return await request("/api/pagetemplate/" + templateId + "/render", {
        method: "POST",
        body: JSON.stringify({ templateId, spaceKey }),
      });
    } catch (e) {
      return null;
    }
  },
};
