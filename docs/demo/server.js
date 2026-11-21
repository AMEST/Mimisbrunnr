#!/usr/bin/env node
/**
 * Demo API server for the Mimisbrunnr SPA.
 *
 * Serves the built Vue.js frontend from `src/Mimisbrunnr.Web.Host/ClientApp/dist`
 * and answers `/api/*` requests with in-memory demo data. It exists so the UI can
 * be opened without MongoDB, OpenID or any other infrastructure — for docs
 * screenshots, UI demos and manual frontend checks.
 *
 * Usage:
 *   node docs/demo/server.js
 *   node docs/demo/server.js --port=4322 --quickstart
 *
 * See docs/demo/README.md for details.
 */
const http = require("http");
const fs = require("fs");
const path = require("path");

function parseArgs(argv) {
  const args = { port: Number(process.env.PORT || 4321), host: process.env.HOST || "127.0.0.1", quickstart: process.env.MODE === "quickstart", verbose: false, dist: process.env.DIST || null };
  const valueFlags = new Set(["port", "host", "dist"]);
  for (let i = 0; i < argv.length; i++) {
    const raw = argv[i].replace(/^--/, "");
    const eq = raw.indexOf("=");
    const key = eq === -1 ? raw : raw.slice(0, eq);
    let value = eq === -1 ? undefined : raw.slice(eq + 1);
    if (value === undefined && valueFlags.has(key) && argv[i + 1] && !argv[i + 1].startsWith("--")) {
      value = argv[++i];
    }
    if (key === "port") args.port = Number(value);
    else if (key === "host") args.host = value;
    else if (key === "dist") args.dist = value;
    else if (key === "quickstart") args.quickstart = true;
    else if (key === "verbose") args.verbose = true;
  }
  return args;
}

const ARGS = parseArgs(process.argv.slice(2));
const DIST = path.resolve(ARGS.dist || path.join(__dirname, "../../src/Mimisbrunnr.Web.Host/ClientApp/dist"));
const QUICKSTART_ONLY = ARGS.quickstart;

if (!fs.existsSync(path.join(DIST, "index.html"))) {
  console.error("Frontend build not found: " + DIST);
  console.error("Run `npm run build` in src/Mimisbrunnr.Web.Host/ClientApp first.");
  process.exit(1);
}

/* ------------------------------------------------------------------ *
 * Demo data
 * ------------------------------------------------------------------ */

const alice = { email: "alice@example.com", name: "Alice Johnson", avatarUrl: "" };
const bob = { email: "bob@example.com", name: "Bob Smith", avatarUrl: "" };
const carol = { email: "carol@example.com", name: "Carol White", avatarUrl: "" };
const dan = { email: "dan@example.com", name: "Dan Brown", avatarUrl: "" };

const now = "2026-11-20T14:32:00Z";

const spaces = {
  DOCS: {
    key: "DOCS", name: "Product Docs", description: "Documentation for the product team",
    homePageId: "pg-home", avatarUrl: "", type: "Public", status: "Active"
  },
  ENG: {
    key: "ENG", name: "Engineering", description: "Internal engineering space",
    homePageId: "pg-eng-home", avatarUrl: "", type: "Private", status: "Active"
  },
  ALICE: {
    key: "ALICE", name: "Alice Johnson", description: "my personal space",
    homePageId: "pg-alice-home", avatarUrl: "", type: "Personal", status: "Active"
  }
};

function page(id, name, content, extra) {
  return Object.assign({
    id, version: 3, spaceKey: "DOCS", name, content,
    created: "2026-11-02T09:00:00Z", updated: now,
    createdBy: bob, updatedBy: alice
  }, extra || {});
}

const pages = {
  "pg-home": page("pg-home", "Product Docs", "# Product Docs\n\nEverything the team needs to know about the product: onboarding, configuration and operations.\n\n* Getting started\n* Configuration\n* MCP server for AI agents"),
  "pg-1": page("pg-1", "Getting started", "# Getting started\n\n1. Sign in through the corporate account.\n2. Open **Spaces** and pick the space you need.\n3. Press **Create** to add a page.\n\n> Tip: press the arrow next to **Create** to start from a template."),
  "pg-2": page("pg-2", "Configuration", "# Configuration\n\nDeployment settings are read from `appsettings.json` or environment variables."),
  "pg-3": page("pg-3", "MCP server for AI agents", "# MCP server for AI agents\n\nThe MCP endpoint lets an AI assistant read and edit the wiki with the rights of its token owner.\n\n1. Enable **MCP server for agents** in Administration.\n2. Create a personal token.\n3. Add `https://wiki.example.com/mcp` to the client."),
  "pg-eng-home": page("pg-eng-home", "Engineering", "# Engineering\n\nRunbooks, architecture notes and postmortems.", { spaceKey: "ENG" }),
  "pg-alice-home": page("pg-alice-home", "Alice Johnson", "# Alice Johnson\n\nMy notes.", { spaceKey: "ALICE" })
};

const tree = {
  page: pages["pg-home"],
  childs: [
    { page: pages["pg-1"], childs: [] },
    { page: pages["pg-2"], childs: [] },
    { page: pages["pg-3"], childs: [] }
  ]
};

const users = [
  Object.assign({}, alice, { isAdmin: true, enable: true }),
  Object.assign({}, bob, { isAdmin: false, enable: true }),
  Object.assign({}, carol, { isAdmin: false, enable: false }),
  Object.assign({}, dan, { isAdmin: false, enable: true })
];

const groups = [
  { name: "engineering", description: "All engineers" },
  { name: "design-squad", description: "Design team" },
  { name: "docs-reviewers", description: "People who review documentation" }
];

const applicationConfiguration = {
  title: "Mimisbrunnr Wiki",
  allowAnonymous: true,
  userAutoCreation: true,
  swaggerEnabled: true,
  mcpEnabled: true,
  allowHtml: false,
  customCss: "",
  customHomepageEnabled: false,
  customHomepageSpaceKey: null
};

const plugins = [
  {
    id: "pl-1",
    pluginIdentifier: "com.amest.default",
    name: "Default plugin",
    version: "1.4.0",
    installedBy: alice,
    installation: "2026-09-01T10:00:00Z",
    disabled: false,
    macros: [
      { identifier: "attachments-list", name: "Page attachments", description: "List of page attachments" },
      { identifier: "child-pages", name: "Child pages", description: "List of child pages" },
      { identifier: "mentions", name: "Mentions", description: "Mention a user" },
      { identifier: "table-filter", name: "Table filter", description: "Filters and search for tables on the page" }
    ],
    pageTemplates: [
      {
        id: "plugin.636f6d2e616d6573742e64656661756c74.6d656574696e67",
        templateIdentifier: "meeting-notes",
        name: "Meeting notes",
        description: "Agenda and action items for a meeting"
      }
    ]
  },
  {
    id: "pl-2",
    pluginIdentifier: "com.example.drawio",
    name: "Drawio diagrams",
    version: "0.9.2",
    installedBy: bob,
    installation: "2026-10-12T08:15:00Z",
    disabled: true,
    macros: [
      { identifier: "drawio", name: "Drawio diagram", description: "Embed a drawio diagram" }
    ],
    pageTemplates: []
  }
];

const pageTemplates = [
  {
    id: "tpl-1", name: "Meeting notes", description: "Agenda, notes and action items",
    content: "# {{SpaceName}} meeting\n\n**Date:** {{CurrentDate}}\n**Author:** {{UserName}}",
    type: "System", isReadOnly: false, createdBy: alice, updatedBy: alice, created: now, updated: now
  },
  {
    id: "tpl-2", name: "Project proposal", description: "Goals, scope and milestones",
    content: "# Proposal: \n\n## Goals\n\n## Scope\n\n## Milestones",
    type: "System", isReadOnly: false, createdBy: bob, updatedBy: bob, created: now, updated: now
  },
  {
    id: "plugin.636f6d2e616d6573742e64656661756c74.6d656574696e67",
    name: "Meeting notes (plugin)", description: "Agenda and action items, shipped by a plugin",
    content: "# {{SpaceName}} meeting\n\nAgenda:\n- ...",
    type: "System", isReadOnly: true, pluginIdentifier: "com.amest.default", pluginName: "Default plugin",
    createdBy: alice, updatedBy: alice, created: now, updated: now
  }
];

const tokens = [
  { id: "tok-1", created: "2026-11-01T09:00:00Z", expired: "2027-05-01T09:00:00Z", revoked: false },
  { id: "tok-2", created: "2026-08-14T12:00:00Z", expired: "2026-11-12T12:00:00Z", revoked: false },
  { id: "tok-3", created: "2026-05-02T10:30:00Z", expired: "2027-05-02T10:30:00Z", revoked: true }
];

const feed = [
  { spaceKey: "DOCS", pageId: "pg-3", pageTitle: "MCP server for AI agents", updated: "2026-11-20T14:32:00Z", updatedBy: alice },
  { spaceKey: "DOCS", pageId: "pg-2", pageTitle: "Configuration", updated: "2026-11-19T11:05:00Z", updatedBy: bob },
  { spaceKey: "ENG", pageId: "pg-eng-home", pageTitle: "Engineering", updated: "2026-11-18T16:40:00Z", updatedBy: dan },
  { spaceKey: "DOCS", pageId: "pg-1", pageTitle: "Getting started", updated: "2026-11-17T09:20:00Z", updatedBy: alice }
];

const favorites = {
  space: [
    { id: "fav-1", created: now, space: spaces.DOCS },
    { id: "fav-2", created: now, space: spaces.ENG }
  ],
  page: [
    { id: "fav-3", created: now, page: pages["pg-1"] },
    { id: "fav-4", created: now, page: pages["pg-3"] }
  ]
};

const attachments = [
  { name: "release-notes.pdf", created: "2026-11-18T10:00:00Z", createdBy: bob },
  { name: "architecture.png", created: "2026-11-20T09:30:00Z", createdBy: alice }
];

const draft = {
  id: "dr-1",
  originalPageId: "pg-1",
  name: "Getting started",
  content: "# Getting started\n\n1. Sign in through the corporate account.\n2. Open **Spaces** and pick the space you need.\n3. Press **Create** to add a page.\n\nDraft note: add a section about templates.",
  updated: "2026-11-20T15:10:00Z",
  updatedBy: bob
};



/* ------------------------------------------------------------------ *
 * HTTP helpers
 * ------------------------------------------------------------------ */

function json(res, data, status = 200) {
  const body = JSON.stringify(data);
  res.writeHead(status, {
    "Content-Type": "application/json; charset=utf-8",
    "Content-Length": Buffer.byteLength(body),
    "Cache-Control": "no-store"
  });
  res.end(body);
}

function empty(res, status = 200) {
  res.writeHead(status, { "Content-Length": 0, "Cache-Control": "no-store" });
  res.end();
}

const MIME = {
  ".js": "application/javascript", ".css": "text/css", ".html": "text/html",
  ".json": "application/json", ".png": "image/png", ".jpg": "image/jpeg",
  ".svg": "image/svg+xml", ".ico": "image/x-icon", ".woff": "font/woff",
  ".woff2": "font/woff2", ".ttf": "font/ttf", ".map": "application/json"
};

const ASSET_DIRS = ["/js/", "/css/", "/img/", "/assets/", "/fonts/", "/media/"];

function serveStatic(req, res, pathname) {
  const rel = pathname === "/" ? "/index.html" : pathname;
  const file = path.join(DIST, rel);
  if (!file.startsWith(DIST)) {
    res.writeHead(403);
    return res.end();
  }
  if (fs.existsSync(file) && fs.statSync(file).isFile()) {
    const ext = path.extname(file);
    res.writeHead(200, { "Content-Type": MIME[ext] || "application/octet-stream" });
    return fs.createReadStream(file).pipe(res);
  }
  // Missing bundle assets must stay 404: the SPA-shell fallback would break chunk loading.
  if (ASSET_DIRS.some(dir => rel.startsWith(dir))) {
    res.writeHead(404);
    return res.end("not found");
  }
  res.writeHead(200, { "Content-Type": "text/html; charset=utf-8" });
  return fs.createReadStream(path.join(DIST, "index.html")).pipe(res);
}

/* ------------------------------------------------------------------ *
 * Demo API
 * ------------------------------------------------------------------ */

function api(req, res, url) {
  const p = url.pathname;
  const q = url.searchParams;

  if (p === "/api/version") return json(res, { version: "2.4.0" });
  if (p === "/api/customization/css") {
    res.writeHead(200, { "Content-Type": "text/css" });
    return res.end("");
  }
  if (p === "/api/customization/homepage") return json(res, { homepageId: "pg-home" });

  if (p === "/api/quickstart/initialize") {
    return req.method === "POST" ? empty(res) : json(res, { isInitialized: !QUICKSTART_ONLY });
  }
  if (p === "/api/quickstart") return json(res, applicationConfiguration);

  if (p === "/api/user/current") return json(res, Object.assign({}, alice, { isAdmin: true, enable: true }));

  if (p.toLowerCase() === "/api/admin/applicationconfiguration") return json(res, applicationConfiguration);

  if (p === "/api/user" && req.method === "GET") return json(res, users);
  if (p === "/api/user" && req.method === "POST") return json(res, {}, 200);
  if (/^\/api\/user\/[^/]+\/groups$/.test(p)) return json(res, [groups[0], groups[2]]);
  if (/^\/api\/user\/[^/]+\/(promote|demote|enable|disable)$/.test(p)) return empty(res);
  if (/^\/api\/user\/[^/]+$/.test(p) && req.method === "GET") {
    const email = decodeURIComponent(p.split("/")[3]);
    const u = users.find(x => x.email === email);
    return u ? json(res, u) : json(res, { message: "not found" }, 404);
  }

  if (p === "/api/group" && req.method === "GET") return json(res, groups);
  if (/^\/api\/group\/[^/]+\/users$/i.test(p)) {
    const name = decodeURIComponent(p.split("/")[3]);
    if (name === "engineering") return json(res, [alice, bob, dan]);
    if (name === "design-squad") return json(res, [carol]);
    return json(res, []);
  }
  if (/^\/api\/group\/[^/]+$/i.test(p) && req.method === "GET") {
    const name = decodeURIComponent(p.split("/")[3]);
    const g = groups.find(x => x.name === name);
    return g ? json(res, g) : json(res, { message: "not found" }, 404);
  }

  if (p === "/api/account/token" && req.method === "GET") return json(res, tokens);
  if (p === "/api/account/token" && req.method === "POST") {
    return json(res, { token: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhbGljZUBleGFtcGxlLmNvbSJ9.demo-signature" });
  }
  if (/^\/api\/account\/token\/[^/]+$/.test(p) && req.method === "DELETE") return empty(res);

  if (p === "/api/space" && req.method === "GET") return json(res, Object.values(spaces));
  if (/^\/api\/space\/[^/]+$/i.test(p) && req.method === "GET") {
    const key = decodeURIComponent(p.split("/")[3]).toUpperCase();
    return spaces[key] ? json(res, spaces[key]) : json(res, { message: "not found" }, 404);
  }
  if (/^\/api\/space\/[^/]+\/permission$/i.test(p)) {
    return json(res, { isAdmin: true, canView: true, canEdit: true, canRemove: true });
  }
  if (/^\/api\/space\/[^/]+\/permissions$/i.test(p)) {
    return json(res, [
      { user: alice, group: null, isAdmin: true, canView: true, canEdit: true, canRemove: true },
      { user: null, group: { name: "engineering", description: "All engineers" }, isAdmin: false, canView: true, canEdit: true, canRemove: false }
    ]);
  }

  if (/^\/api\/page\/[^/]+\/tree$/.test(p)) return json(res, tree);
  if (/^\/api\/page\/[^/]+\/comments$/.test(p)) {
    return json(res, [{ id: "c-1", message: "Looks good. Added a note about templates.", created: "2026-11-19T10:00:00Z", author: bob }]);
  }
  if (/^\/api\/page\/[^/]+\/versions$/.test(p)) {
    return json(res, {
      pageId: "pg-1",
      versions: [
        { id: "v-3", pageId: "pg-1", version: 3, name: "Getting started", content: pages["pg-1"].content, created: now, updated: now, createdBy: bob, updatedBy: alice },
        { id: "v-2", pageId: "pg-1", version: 2, name: "Getting started", content: "# Getting started\n\nFirst version", created: "2026-11-10T09:00:00Z", updated: "2026-11-10T09:00:00Z", createdBy: bob, updatedBy: bob }
      ]
    });
  }
  if (/^\/api\/page\/[^/]+$/.test(p) && req.method === "GET") {
    const id = decodeURIComponent(p.split("/")[3]);
    return pages[id] ? json(res, pages[id]) : json(res, { message: "not found" }, 404);
  }
  if (p === "/api/page" && req.method === "POST") return json(res, page("pg-new", "New page", ""));

  if (/^\/api\/attachment\/[^/]+$/.test(p) && req.method === "GET") return json(res, attachments);

  if (/^\/api\/draft\/[^/]+$/.test(p)) {
    const id = decodeURIComponent(p.split("/")[3]);
    if (req.method === "GET") return id === "pg-1" ? json(res, draft) : json(res, { message: "not found" }, 404);
    return empty(res);
  }

  if (p === "/api/feed" || /^\/api\/feed\/[^/]+$/.test(p)) return json(res, feed);

  if (p === "/api/favorites" && req.method === "GET") {
    const type = q.get("type") || "space";
    return json(res, favorites[type] || favorites.space);
  }
  if (p === "/api/favorites/exists" && req.method === "POST") return json(res, true);
  if (p === "/api/favorites/findOne" && req.method === "POST") return json(res, favorites.page[0]);
  if (p === "/api/favorites" && req.method === "POST") return json(res, favorites.page[0]);
  if (/^\/api\/favorites\/[^/]+$/.test(p) && req.method === "DELETE") return empty(res);

  if (p === "/api/search/page") {
    const text = (q.get("search") || "").toLowerCase();
    return json(res, Object.values(pages).filter(x => x.name.toLowerCase().includes(text) || x.content.toLowerCase().includes(text)));
  }
  if (p === "/api/search/space") {
    const text = (q.get("search") || "").toLowerCase();
    return json(res, Object.values(spaces).filter(x => x.name.toLowerCase().includes(text)));
  }
  if (p === "/api/search/user") return json(res, users.filter(x => x.enable));

  if (p === "/api/plugin" && req.method === "GET") return json(res, plugins);
  if (p === "/api/plugin/macros" && req.method === "GET") return json(res, plugins.flatMap(x => x.macros));
  if (p === "/api/plugin/macros/state") return empty(res);

  if (p === "/api/pagetemplate" && req.method === "GET") {
    const type = q.get("type");
    if (type === "System") return json(res, pageTemplates);
    if (type === "User") {
      return json(res, [{
        id: "tpl-user", name: "My retro", description: "Personal retrospective template",
        content: "# Retro {{CurrentDate}}", type: "User", isReadOnly: false, ownerEmail: "alice@example.com",
        createdBy: alice, updatedBy: alice, created: now, updated: now
      }]);
    }
    if (type === "Space") {
      return json(res, [{
        id: "tpl-space", name: "Sprint review", description: "Space template for sprint reviews",
        content: "# Sprint review {{CurrentDate}}", type: "Space", isReadOnly: false, spaceId: "DOCS",
        createdBy: alice, updatedBy: alice, created: now, updated: now
      }]);
    }
    return json(res, pageTemplates);
  }
  if (/^\/api\/pagetemplate\/[^/]+\/render$/.test(p)) {
    return json(res, { content: "# Product Docs meeting\n\n**Date:** 2026-11-21\n**Author:** Alice Johnson\n\n## Agenda\n- ..." });
  }
  if (/^\/api\/pagetemplate\/[^/]+$/.test(p) && req.method === "GET") {
    const id = decodeURIComponent(p.split("/")[3]);
    const t = pageTemplates.find(x => x.id === id);
    return t ? json(res, t) : json(res, { message: "not found" }, 404);
  }

  if (ARGS.verbose) console.log("[demo] unmatched: " + req.method + " " + p);
  return json(res, { message: "demo server: not implemented " + req.method + " " + p }, 404);
}

/* ------------------------------------------------------------------ *
 * Server
 * ------------------------------------------------------------------ */

const server = http.createServer((req, res) => {
  const url = new URL(req.url, "http://" + ARGS.host + ":" + ARGS.port);
  // A real service worker caches the production bundle and breaks demo edits.
  if (url.pathname === "/service-worker.js" || url.pathname.startsWith("/workbox-")) {
    res.writeHead(200, { "Content-Type": "application/javascript", "Cache-Control": "no-store" });
    return res.end("self.addEventListener('install', () => self.skipWaiting());");
  }
  if (url.pathname.startsWith("/api/")) return api(req, res, url);
  serveStatic(req, res, url.pathname);
});

server.listen(ARGS.port, ARGS.host, () => {
  console.log("Mimisbrunnr demo");
  console.log("  UI:  http://" + ARGS.host + ":" + ARGS.port + (QUICKSTART_ONLY ? "  (quickstart mode)" : ""));
  console.log("  API: http://" + ARGS.host + ":" + ARGS.port + "/api/version");
  console.log("  dist: " + DIST);
});