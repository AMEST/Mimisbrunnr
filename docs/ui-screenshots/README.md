# UI screenshots

User-facing screenshots of the Mimisbrunnr interface, used in the wiki documentation
(`MM-DOCS` space) and in design reviews. Captured at 1280×900 from the production
bundle running against the [demo server](../demo/README.md) — no real backend,
database or identity provider involved.

## Regenerating

```bash
cd src/Mimisbrunnr.Web.Host/ClientApp
npm run build
npm run demo                # http://127.0.0.1:4321
npm run demo:quickstart     # http://127.0.0.1:4322 (Quickstart page)
```

Routes and click paths for each screenshot are listed in
[`docs/demo/README.md`](../demo/README.md#using-it-for-documentation-screenshots).
The demo data is deterministic (fixed dates and users), so a rebuild produces the
same pictures.

## Files

| File | What it shows | Documented on |
|---|---|---|
| `quickstart.jpg` | First-run form: title, anonymous access, Allow html, swagger, Initialization button | First run and Quickstart |
| `admin-general.jpg` | Administration → General: title, publish mode, user creation, Allow html, swagger, Enable MCP, Custom css | Administration, Application settings, MCP server for AI agents |
| `admin-users.jpg` | Administration → Users: search, Disable/Enable, Promote/Demote | Users and groups, Administration |
| `admin-groups.jpg` | Administration → Groups | Users and groups, Administration |
| `admin-plugins.jpg` | Administration → Plugins: install, disabled plugin struck through | Administration, Operations |
| `admin-plugins-details.jpg` | Expanded plugin details: identifier, macros, page templates, Disable/Uninstall | Administration, Operations, Plugin page templates |
| `admin-templates.jpg` | Administration → Templates: system templates and read-only plugin template | Page templates, Plugin page templates |
| `profile.jpg` | Profile page: Settings, Favorites, "Worked on" | Personal API tokens and REST API, Users and groups |
| `profile-settings-api-tokens.jpg` | Settings → API tokens: token list, Revoke, issuing by days | Personal API tokens and REST API |
| `profile-settings-user-groups.jpg` | Settings → User groups | Personal API tokens and REST API, Users and groups |
| `profile-settings-my-templates.jpg` | Settings → My templates: Source "Personal", Edit/Delete | Page templates, Plugin page templates |
| `dashboard-home.jpg` | Dashboard: Updates, Recently visited, Favorites, version in the sidebar | Search, favorites and activity feed |
| `spaces.jpg` | Space directory: favorite spaces and all spaces with search | Search, favorites and activity feed |
| `people.jpg` | People directory with user search | Users and groups |
| `page-view.jpg` | Page view: breadcrumbs, Edit, favorite star, `⋮` menu, comments | Copy and move pages (base view) |
| `page-menu.jpg` | Page `⋮` menu: Page history, Print, Attachments, Copy, Move, Delete | Copy and move pages |
| `page-copy-modal.jpg` | Copy dialog: target space and parent page | Copy and move pages |
| `page-move-modal.jpg` | Move dialog | Copy and move pages |
| `page-history.jpg` | Page history with restore/delete actions | Copy and move pages, Troubleshooting and FAQ |
| `page-attachments.jpg` | Attachments dialog with upload controls | Troubleshooting and FAQ |
| `page-editor.jpg` | Editor: toolbar, Side-By-Side, Preview, Update, Close, save indicator | Page drafts |
| `draft-modal.jpg` | "There are pending changes" dialog: Continue editing / Reset changes | Page drafts |
| `create-from-template-modal.jpg` | Create from Template: System, User and plugin templates | Plugin page templates |
| `space-templates-modal.jpg` | Space actions → Page templates | Page templates, Plugin page templates |
| `space-permissions-modal.jpg` | Space actions → Permissions | Administration, Users and groups |
| `space-settings-modal.jpg` | Space actions → Settings | Administration |
| `create-space-modal.jpg` | Create space: key, name, type, description, Confluence import | Administration, First run and Quickstart |
| `search-recent.jpg` | Header search sidebar: Recently visited and "Search space" switch | Search, favorites and activity feed |
| `search-results.jpg` | Search results for pages | Search, favorites and activity feed |

## Notes

- Screenshots are JPEG (1280×900). They are stored in the repository for history;
  the wiki pages themselves link to uploaded attachments, because the wiki API
  used by the documentation workflow does not expose attachment upload.
- UI labels come from `src/Mimisbrunnr.Web.Host/ClientApp/src/assets/lang.json`
  (English). If labels change, regenerate the affected screenshots.
- `Recently visited` is client-side history, so open a page before capturing it.
- Keep the window at 1280 px: the SPA switches to compact layouts on narrow screens.