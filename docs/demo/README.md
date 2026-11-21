# Demo mode (frontend without backend)

The Vue.js SPA normally talks to ASP.NET Core, MongoDB, OpenID and the persistent
storage. For UI demos, documentation screenshots and quick visual checks you do
not need any of that: `server.js` serves the built SPA and answers every `/api/*`
call with in-memory demo data.

## Quick start

```bash
cd src/Mimisbrunnr.Web.Host/ClientApp

npm install        # first time only
npm run build      # produces dist/, the demo server serves exactly this bundle
npm run demo       # http://127.0.0.1:4321
```

Alternative without npm scripts:

```bash
node docs/demo/server.js
```

The server refuses to start if `dist/index.html` is missing and prints the build
command. Always rebuild after frontend changes — the demo serves the compiled
bundle, not the dev server.

### First-run (Quickstart) screen

The Quickstart page is shown only when the instance is not initialized. Start a
second server in quickstart mode:

```bash
npm run demo:quickstart      # http://127.0.0.1:4322
```

Both servers can run at the same time: `4321` = initialized wiki, `4322` = fresh
instance with the Quickstart form.

### Options

| Flag | Env | Default | Description |
|---|---|---|---|
| `--port=4321` | `PORT` | `4321` | HTTP port |
| `--host=127.0.0.1` | `HOST` | `127.0.0.1` | Bind address |
| `--dist=<path>` | `DIST` | `src/Mimisbrunnr.Web.Host/ClientApp/dist` | Bundle directory |
| `--quickstart` | `MODE=quickstart` | off | Serve an uninitialized instance |
| `--verbose` | — | off | Log every unhandled API request |

## How it works

1. Static files come from `dist/`. Unknown paths without a file extension return
   `index.html` so Vue Router handles deep links (`/space/DOCS/pg-1`). Missing
   assets under `/js/`, `/css/`, `/img/`, `/assets/`, `/fonts/`, `/media/` stay
   `404` — returning HTML instead would break chunk loading.
2. `/service-worker.js` returns a no-op worker. The real one caches the bundle
   and would hide frontend rebuilds.
3. `/api/*` is matched by path and method in the `api()` function. Unknown
   requests answer `404` with a descriptive JSON body (and are logged with
   `--verbose`), so missing endpoints are easy to spot while clicking through
   the UI.

All demo data (users, spaces, pages, tree, feed, favorites, templates, plugins,
tokens, draft, attachments) is defined at the top of `server.js`.

## Extending the demo data

- **New endpoint**: add a branch in `api()` before the final `404`, following the
  existing `if (p === ...) return json(res, ...)` style.
- **New page**: add an entry to `pages` (or `spaces`) and, if it should appear in
  the sidebar tree, to `tree`.
- **Demo user**: the current user is always `alice@example.com` with `isAdmin:
  true` (`/api/user/current`); add other users to `users` and `groups`.
- **Current dates**: data uses fixed timestamps so screenshots stay reproducible.

If the UI needs a field the real API returns, copy the shape from
`src/Integration/Mimisbrunnr.Integration/` (e.g. `Wiki/PageModel.cs`,
`User/UserViewModel.cs`, `Plugin/PluginModel.cs` both for the mock and the frontend `services/*.js`.

## Using it for documentation screenshots

The screenshots in [`docs/ui-screenshots`](../ui-screenshots) were produced with
this server. A reproducible flow:

1. Review the UI text and button labels in
   `src/Mimisbrunnr.Web.Host/ClientApp/src/assets/lang.json` first — the demo uses
   the same English strings as the real build.
2. `npm run build && npm run demo && npm run demo:quickstart`.
3. Open the routes directly (deep links work):
   - `/` — dashboard: Updates / Recently visited / Favorites, version in the sidebar
   - `/spaces`, `/people`
   - `/space/DOCS/pg-1` — page view, page `⋮` menu, comments
   - `/space/DOCS/pg-1/edit` — editor and the pending-draft dialog
   - `/admin` — General, `/admin/users`, `/admin/groups`, `/admin/plugins`, `/admin/templates`
   - `/profile/alice@example.com` → **Settings** — API tokens, User groups, My templates, Profile
   - `/space/DOCS/pg-1/edit` → **Continue editing** for the editor screenshot
   - Quickstart is on `http://127.0.0.1:4322/`
4. Modals are opened from the UI (page `⋮` → Copy/Move/Page history/Attachments,
   header `Create ▾` → Create from Template, space actions → Permissions/Settings/Page templates,
   Spaces ▾ → Create Space, search icon → search sidebar).
5. Keep the browser at 1280 px width: the app switches to compact layouts below.

Notes gathered while making the screenshots:

- The search field in the header is a disabled input used as a toggle for the
  right sidebar (`#search-sidebar`); click the input group, not the icon button.
- `Recently visited` comes from local browser history (Vuex), not from the API —
  open a couple of pages before capturing it.
- The attachment, history and draft modals only render content when their API
  endpoints return data; the mock provides attachments for any page and a draft
  only for `pg-1`.
- Plugin page templates are read-only in the UI (`Managed by plugin updates`),
  which is why the demo marks one template as `isReadOnly: true`.

## Limitations

- Data is in memory: mutations (create/update/delete) return plausible responses
  but are not persisted, so reloading resets the demo.
- No authentication is emulated — the mock always returns an authenticated admin.
- Search, favorites and the feed are pre-filtered simple implementations, enough
  to render lists and empty states.