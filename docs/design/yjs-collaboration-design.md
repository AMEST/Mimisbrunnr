# Design Document: Yjs (CRDT) Collaborative Editing for Mimisbrunnr Wiki

**Status:** Draft  
**Date:** 2026-07-19  

---

## 1. Problem Statement

The current page editor (`PageEdit.vue`) uses a single-user model: one user edits at a time, with a simple draft auto-save (`PUT /api/draft/{pageId}`). There is no mechanism for multiple users to simultaneously edit the same page, no cursor visibility, and no awareness of other connected editors. This leads to conflicts when multiple users try to edit the same page, and there is no feedback about who else is working on the content.

## 2. Goals

| Goal | Priority |
|------|----------|
| Real-time collaborative editing of page content (Markdown) | P0 |
| Remote cursor display with user name labels | P0 |
| Awareness of connected users (avatar, name, online status) | P0 |
| Backend permission check on WebSocket connection to collaboration room | P0 |
| Document persistence to MongoDB (periodic + on last client disconnect) | P0 |
| Dynamic enable/disable of collaboration via admin panel (runtime toggle) | P0 |
| Cluster mode: multi-host support via Redis Pub/Sub relay | P0 |
| Seamless fallback to single-user mode (non-collaborative) | P1 |
| Support for the existing macro system within collaborative sessions | P1 |
| Metrics/monitoring for active collaboration sessions | P2 |

## 3. Non-Goals

- Migrating from CodeMirror 5 to CodeMirror 6 (future consideration)
- Real-time sync of page name changes (name is edited via a separate `<b-form-input>`)
- Real-time sync of page attachments or comments
- Conflict-free resolution of macro parameter edits (macros are edited via modal, not inline)
- Replacing the existing version history system with CRDT history

## 4. Architecture Overview

### 4.1 Technology Selection

| Component | Technology | Rationale |
|-----------|-----------|-----------|
| CRDT engine | **Yjs** (y-websocket protocol) | Battle-tested, Markdown-friendly, CodeMirror 5 binding available |
| WebSocket server | **C# native** (`System.Net.WebSockets` + ASP.NET Core middleware) | Keeps stack unified, leverages existing auth middleware |
| Inter-host relay | **Redis Pub/Sub** (existing `StackExchange.Redis` dependency) | Already in the stack; broadcasts Yjs updates between host instances |
| Client transport | **y-websocket** (`y-websocket` npm package) | Standard Yjs WebSocket provider, handles sync protocol |
| CM5 binding | **y-codemirror** (`y-codemirror` npm package) | Official Yjs binding for CodeMirror 5 |
| Awareness | **y-websocket awareness** (built-in) | Cursor positions, user info, selection ranges |
| Document persistence | **MongoDB** (existing `IMongoRepository<YjsDocument>`) | Same database, same patterns as existing data |
| Feature toggle | **Runtime config** stored in MongoDB (`ApplicationConfiguration`) | Dynamic admin panel toggle, no redeployment needed |

### 4.2 High-Level Architecture (Single Host)

```
┌──────────────────────────────────────────────────────────┐
│                      Browser (Vue.js)                     │
│                                                          │
│  PageEdit.vue                                            │
│    └── VueSimpleMde.vue (CodeMirror 5)                   │
│          └── y-codemirror CodemirrorBinding               │
│                └── Y.Doc (Yjs document)                   │
│                      └── WebsocketProvider (y-websocket)  │
│                            │                              │
│                    WebSocket (wss://)                      │
└────────────────────────────────────┼──────────────────────┘
                                     │
                                     ▼
┌──────────────────────────────────────────────────────────┐
│                 ASP.NET Core Host (.NET 8)                │
│                                                          │
│  WebSocket Middleware (ws://api/collaboration/{pageId})   │
│    ├── Feature gate check (CollaborationEnabled?)        │
│    ├── JWT/Cookie Auth Pipeline (existing)               │
│    ├── ValidateUserStateMiddleware (existing)             │
│    ├── PermissionService.EnsureEditPermission (existing) │
│    └── CollaborationHub (new)                             │
│          ├── Yjs Sync Protocol Handler                    │
│          ├── Awareness Protocol Handler                   │
│          └── YjsPersistenceService                        │
│                └── IMongoRepository<YjsDocument>          │
└──────────────────────────────────────────────────────────┘
                                     │
                                     ▼
┌──────────────────────────────────────────────────────────┐
│                      MongoDB                              │
│  ┌─────────────────┐  ┌───────────────────┐              │
│  │ Pages (existing) │  │ YjsDocuments (new)│              │
│  │ Content: string  │  │ PageId: string    │              │
│  │ Version: long    │  │ State: byte[]     │              │
│  └─────────────────┘  │ Awareness: byte[]  │              │
│                        │ UpdatedAt: DateTime│              │
│                        └───────────────────┘              │
└──────────────────────────────────────────────────────────┘
```

### 4.3 Cluster Architecture (Multi-Host)

When multiple API host instances run behind a load balancer, users may connect to different hosts. Yjs updates must be relayed between hosts in real-time.

```
┌─────────────┐  ┌─────────────┐  ┌─────────────┐
│  Host A      │  │  Host B      │  │  Host C      │
│  (in-memory  │  │  (in-memory  │  │  (in-memory  │
│   rooms)     │  │   rooms)     │  │   rooms)     │
│              │  │              │  │              │
│ User 1 ─────┤  │ User 2 ─────┤  │ User 3 ─────┤
│ User 4 ─────┤  │              │  │ User 5 ─────┤
└──────┬───────┘  └──────┬───────┘  └──────┬───────┘
       │                 │                 │
       └────────┬────────┴────────┬────────┘
                │                 │
                ▼                 ▼
┌─────────────────────┐  ┌─────────────────────┐
│   Redis Pub/Sub      │  │     MongoDB          │
│   (real-time relay)  │  │   (persistence)      │
│                      │  │                      │
│  Channel per page:   │  │  YjsDocuments        │
│  collab:{pageId}     │  │  Pages               │
│                      │  │  ApplicationConfig   │
│  Messages:           │  │                      │
│  - Yjs update bytes  │  │                      │
│  - Awareness state   │  │                      │
└─────────────────────┘  └─────────────────────┘
```

**How it works:**

1. **User connects** to Host A via WebSocket. Host A loads the Yjs document from MongoDB, joins the in-memory room.
2. **User types** a character. Host A applies the Yjs update locally and broadcasts to local clients on the same page.
3. **Host A publishes** the Yjs update to Redis channel `collab:{pageId}`.
4. **Host B and Host C** (subscribed to the same channel) receive the update. Each applies it to its local Y.js document copy and forwards to any locally connected clients.
5. **Awareness updates** (cursor positions) are also relayed via Redis Pub/Sub, with a throttled publish (max 2 updates/sec per user) to avoid flooding.

**Redis message format:**

```csharp
public sealed class CollaborationRedisMessage
{
    public string PageId { get; set; }
    public string SourceHostId { get; set; }   // to avoid echo
    public CollaborationMessageType Type { get; set; }
    public byte[] Payload { get; set; }         // Yjs encoded update or awareness state
    public DateTime Timestamp { get; set; }
}

public enum CollaborationMessageType : byte
{
    YjsUpdate = 0,
    AwarenessUpdate = 1,
    DocumentPersist = 2     // signal to persist from Redis (not a real-time message)
}
```

**Key design decisions for cluster mode:**

| Decision | Rationale |
|----------|-----------|
| Redis **Pub/Sub** (not Redis Streams) | Low latency, fire-and-forget is acceptable (Yjs is self-healing via sync protocol) |
| Each host maintains its own `Y.Doc` in memory | No shared-state dependency between hosts; Yjs CRDT guarantees convergence |
| MongoDB is the only shared persistent state | All hosts read/write the same MongoDB collection |
| `SourceHostId` in messages | Prevents echo loops (don't relay a message back to the host that sent it) |
| Awareness throttled to 2/sec per user | Cursor positions are high-frequency; Redis Pub/Sub would be overwhelmed without throttling |
| Yjs updates are **not** throttled | Text changes are low-frequency (bounded by typing speed) and must be delivered immediately |

### 4.4 Feature Toggle: Dynamic Enable/Disable via Admin Panel

Collaboration is an **opt-in feature** controlled at runtime through the admin panel. No redeployment is required.

**Storage:** The toggle is stored in the existing `ApplicationConfiguration` MongoDB collection (same mechanism used by `IApplicationConfigurationManager` for `AllowAnonymous`, `AllowHtml`, etc.).

```csharp
// Extended ApplicationConfiguration entity
public class ApplicationConfiguration
{
    // ... existing fields ...
    public bool CollaborationEnabled { get; set; }  // default: false
}
```

**Admin API:**

| Endpoint | Method | Description |
|----------|--------|-------------|
| `GET /api/admin/config` | GET | Returns current configuration (existing) |
| `PUT /api/admin/config` | PUT | Updates configuration including `CollaborationEnabled` (existing endpoint, extended) |

**Frontend admin UI:**

Add a toggle to the existing admin settings page (if one exists) or to the header admin menu:

```html
<b-form-checkbox switch v-model="collaborationEnabled" @change="toggleCollaboration">
  Collaborative Editing
</b-form-checkbox>
```

**Runtime behavior when collaboration is disabled:**

| State | Behavior |
|-------|----------|
| `CollaborationEnabled = false` (server config) | WebSocket endpoint returns HTTP 403 with `{"error": "Collaboration is disabled"}`. Frontend hides collaboration UI. |
| `CollaborationEnabled = true` | WebSocket endpoint accepts connections. Frontend shows collaboration UI (connects on editor open). |
| Toggle `false → true` while users are in editor | No effect on current session (they already loaded the page without collab). New editor sessions will connect. |
| Toggle `true → false` while users are collaborating | **Graceful shutdown:** Server sends a close frame to all active WebSocket connections. Clients receive the close, fall back to single-user mode, and can still save via the existing REST API. |

**Frontend feature detection:**

```javascript
// collaborationService.js
async function isCollaborationEnabled() {
  const config = await axios.get('/api/quickstart/initialize');
  return config.data.collaborationEnabled;
}
```

The frontend checks the feature flag from the application config (already loaded at startup via `/api/quickstart/initialize`) and conditionally shows/hides collaboration UI elements. If the flag changes at runtime, a periodic check (every 60s) or a WebSocket close frame triggers fallback.

## 5. Backend Design (C# / .NET 8)

### 5.1 New Project: `Mimisbrunnr.Collaboration`

A new Skidbladnir module to encapsulate all collaboration logic.

```
src/Mimisbrunnr.Collaboration/
├── Contracts/
│   ├── YjsDocument.cs                # MongoDB entity
│   ├── IYjsPersistenceService.cs     # Persistence interface
│   ├── ICollaborationFeatureToggle.cs # Feature toggle interface
│   └── CollaborationRedisMessage.cs  # Redis Pub/Sub message envelope
├── Services/
│   ├── YjsPersistenceService.cs      # MongoDB persistence implementation
│   ├── CollaborationFeatureToggle.cs # Reads/writes ApplicationConfiguration
│   └── CollaborationRoomManager.cs   # In-memory room tracking per host
├── Protocol/
│   ├── YjsSyncHandler.cs             # Yjs sync step 1/2 + update messages
│   ├── AwarenessHandler.cs           # Awareness protocol (cursors, presence)
│   └── YjsMessageTypes.cs            # Protocol message type constants
├── Cluster/
│   ├── IRedisCollaborationRelay.cs   # Redis relay interface
│   └── RedisCollaborationRelay.cs    # Pub/Sub relay implementation
├── CollaborationHub.cs               # WebSocket endpoint (middleware)
├── CollaborationModule.cs            # Skidbladnir DI module
└── CollaborationModuleConfiguration.cs
```

### 5.2 WebSocket Endpoint: `CollaborationHub`

**Route:** `ws://host/api/collaboration/{pageId}`

This is **NOT** a SignalR hub. It is a raw WebSocket endpoint implemented as ASP.NET Core middleware, because Yjs uses its own binary protocol over WebSocket.

```csharp
// CollaborationHub.cs
public class CollaborationHub
{
    private readonly RequestDelegate _next;

    public CollaborationHub(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 0. FEATURE GATE: Check ICollaborationFeatureToggle.IsEnabled()
        //    If disabled → return HTTP 403 + JSON error, close connection
        // 1. Extract pageId from route
        // 2. Authenticate user (uses existing auth middleware pipeline)
        // 3. Check edit permission on the page's space
        // 4. Check MaxConnectionsPerPage limit
        // 5. Accept WebSocket upgrade
        // 6. Initialize Yjs document (load from or create in MongoDB)
        // 7. Subscribe to Redis Pub/Sub channel for this pageId (cluster relay)
        // 8. Run sync/awareness loop
        // 9. On disconnect: unsubscribe from Redis, persist document, update awareness
    }
}
```

**Registration in Program.cs:**
```csharp
// In Program.cs, after UseAuthorization() and UseUserValidationMiddleware()
app.UseCollaboration();  // Maps /api/collaboration/{pageId} WebSocket endpoint
```

### 5.3 Authentication & Permission Flow

The WebSocket endpoint reuses the existing ASP.NET Core authentication pipeline:

```
1. HTTP Upgrade Request: ws://host/api/collaboration/{pageId}
   └── Cookie: .AspNetCore.Cookies=... (or Authorization: Bearer ...)

2. Authentication middleware runs automatically:
   ├── JwtBearer / Cookie scheme resolves ClaimsPrincipal
   ├── ValidateUserStateMiddleware verifies user exists & is enabled
   └── ValidateTokenNotRevokedMiddleware checks token validity

3. CollaborationHub extracts UserInfo:
   └── ClaimsPrincipal.ToInfo() → UserInfo { Email, Name, AvatarUrl }

4. Permission check (before accepting WebSocket):
   ├── Resolve page by ID: IPageManager.GetById(pageId)
   ├── Resolve space: ISpaceManager.GetById(page.SpaceId)
   ├── Call IPermissionService.EnsureEditPermission(spaceKey, userInfo)
   └── If denied: return HTTP 403, close connection

5. Connection accepted → enter collaboration room
```

**Critical:** Permission is checked **once** at connection time. If a user's permissions are revoked while connected, the connection is not actively killed (acceptable trade-off; next edit attempt will fail at save time via the existing `PUT /api/page/{pageId}` endpoint).

### 5.4 Yjs Sync Protocol

The Yjs WebSocket protocol uses binary messages with the following structure:

```
Message format: [messageType: uint8] [payload: varint + bytes]

Message types:
  0 = messageSync        → Yjs sync step 1, sync step 2, or update
  1 = messageAwareness   → Awareness state (cursor, user info)
  2 = messageAuth        → Auth verification (optional)
```

**Sync flow for a connecting client:**

```
Client                          Server
  │                               │
  │──── HTTP Upgrade ────────────▶│  (auth + permission check)
  │◀─── 101 Switching Protocols ──│
  │                               │
  │◀─── SyncStep1 (server state) ─│  (server sends its Yjs state vector)
  │──── SyncStep2 (diff) ────────▶│  (client sends diff based on state vector)
  │◀─── SyncStep2 (diff) ────────│  (server sends remaining diff)
  │                               │
  │◀─── Awareness (all users) ────│  (current cursor positions + user info)
  │                               │
  │──── Awareness (own state) ───▶│  (client's cursor position)
  │                               │
  │──── Update (local edits) ────▶│  (real-time document changes)
  │◀─── Update (remote edits) ───│  (broadcast to other clients)
```

### 5.5 Document Persistence

**Entity:** `YjsDocument`

```csharp
public class YjsDocument : IHasId<string>
{
    public string Id { get; set; }
    public string PageId { get; set; }       // FK to Page.Id (unique index)
    public byte[] State { get; set; }         // Yjs encoded document state (Uint8Array)
    public DateTime UpdatedAt { get; set; }
}
```

**Persistence strategy:**

| Event | Action |
|-------|--------|
| First client connects to room | Load `YjsDocument` from MongoDB by `PageId`. If not found, create from `Page.Content` (seed initial state). |
| Document update (debounced) | Persist encoded state to MongoDB every **5 seconds** of inactivity (debounced from last update). |
| Last client disconnects | Persist immediately (force save). |
| Server shutdown | Persist all active documents (via `IHostApplicationLifetime` / graceful shutdown). |

**Initial state seeding (first collaboration on a page):**

When a page has never been collaboratively edited:
1. Load `Page.Content` (Markdown string) from existing `IPageManager`
2. Create a new `Y.Doc()`
3. Insert the Markdown content into a `Y.Text` type (named `"content"`)
4. Encode the state and persist to `YjsDocument` collection
5. Subsequent clients load from the `YjsDocument` collection

**Bidirectional sync with Page.Content:**

When a collaborative session ends (all clients disconnect) and the document is persisted:
- The final Yjs state is decoded back to a Markdown string
- The existing `Page.Content` field is updated via `IPageManager.Update()`
- This ensures the page remains accessible via the REST API and search

### 5.6 Room Management

**In-memory room tracking per host** (singleton service):

```csharp
public class CollaborationRoomManager
{
    // pageId → list of connected WebSocket clients (LOCAL to this host only)
    private readonly ConcurrentDictionary<string, ConcurrentBag<CollaborationClient>> _rooms;

    // pageId → in-memory Y.Doc (shared across all local clients of the same page)
    private readonly ConcurrentDictionary<string, Y.Doc> _localDocuments;

    // pageId → last Yjs state vector received from Redis (for diff calculation)
    private readonly ConcurrentDictionary<string, byte[]> _lastRemoteState;

    public void JoinRoom(string pageId, CollaborationClient client);
    public void LeaveRoom(string pageId, CollaborationClient client);
    public int GetClientCount(string pageId);
    public IReadOnlyCollection<CollaborationClient> GetClients(string pageId);
    public Y.Doc GetOrCreateLocalDocument(string pageId, byte[] initialState);
    public void ApplyRemoteUpdate(string pageId, byte[] update);
}
```

**Cluster-aware persistence:**

When a Yjs document needs to be persisted to MongoDB, the host that performs the persistence acquires a lightweight distributed lock via MongoDB (using `FindOneAndUpdate` with an `etag` field) to prevent concurrent writes from different hosts from overwriting each other:

```csharp
public async Task PersistDocument(string pageId, byte[] state)
{
    var filter = Builders<YjsDocument>.Filter.Eq(d => d.PageId, pageId);
    var update = Builders<YjsDocument>.Update
        .Set(d => d.State, state)
        .Set(d => d.UpdatedAt, DateTime.UtcNow)
        .Set(d => d.Etag, ObjectId.GenerateNewId());  // optimistic concurrency
    var options = new FindOneAndUpdateOptions<YjsDocument> { IsUpsert = true };
    await _repository.Collection.FindOneAndUpdateAsync(filter, update, options);
}
```

### 5.7 Metrics

Extend the existing Prometheus metrics (in `MetricsModule`) with:

```
collaboration_active_sessions gauge         → current number of active rooms
collaboration_connected_users gauge         → total connected users across all rooms
collaboration_document_updates_total counter → total Yjs updates processed
collaboration_connection_duration_seconds   → histogram of session durations
```

The existing vestigial `/ws/` path labeling in `MetricsApplicationBuilderExtensions.cs` (line 39) already maps to `"signalR"` label — rename to `"collaboration"`.

## 6. Frontend Design (Vue.js 2.x)

### 6.1 New NPM Dependencies

```json
{
  "yjs": "^13.6.0",
  "y-websocket": "^2.0.0",
  "y-codemirror": "^1.0.0"
}
```

- **yjs**: Core CRDT engine
- **y-websocket**: WebSocket provider implementing Yjs sync/awareness protocol (client side)
- **y-codemirror**: CodeMirror 5 binding for Yjs (`CodemirrorBinding` class)

### 6.2 New Files

```
ClientApp/src/
├── services/
│   └── collaboration/
│       ├── collaborationService.js   # WebSocket connection management
│       └── awarenessExtensions.js    # Custom awareness fields (avatar, etc.)
└── components/
    └── pageEditor/
        └── CollaboratorsBar.vue      # UI: connected users avatars + count
```

### 6.3 `collaborationService.js`

```javascript
// Key responsibilities:
// 1. Manage Yjs document lifecycle per page
// 2. Create/destroy WebsocketProvider connections
// 3. Bind Yjs document to CodeMirror editor
// 4. Expose awareness state (connected users)
// 5. Handle reconnection logic
```

**API:**

```javascript
class CollaborationService {
  // Initialize collaboration for a page
  connect(pageId, codeMirrorInstance, userInfo) → { ydoc, provider, binding }

  // Disconnect and cleanup
  disconnect()

  // Get list of currently connected users (from awareness)
  getConnectedUsers() → [{ name, email, avatarUrl, color, cursor: { line, ch } }]

  // Subscribe to awareness changes
  onAwarenessChange(callback) → unsubscribe function
}
```

### 6.4 Integration into `PageEdit.vue`

**Changes to `PageEdit.vue`:**

```diff
  import ProfileService from "@/services/profileService";
  import PageService from "@/services/pageService";
+ import collaborationService from "@/services/collaboration/collaborationService";

  export default {
+   components: {
+     // ... existing ...
+     CollaboratorsBar,
+   },
    data() {
      return {
        // ... existing ...
+       collaborationActive: false,
+       connectedUsers: [],
      };
    },
    methods: {
      init: async function () {
        // ... existing auth check, loadPage, loadDraft ...
        this.loaded = true;
        this.initHandlers();
+       this.initCollaboration();
      },

+     initCollaboration: function () {
+       const profile = this.$store.state.application.profile;
+       const result = collaborationService.connect(
+         this.page.id,
+         this.simplemde.codemirror,
+         { name: profile.name, email: profile.email, avatarUrl: profile.avatarUrl }
+       );
+       this.collaborationActive = true;
+       collaborationService.onAwarenessChange((users) => {
+         this.connectedUsers = users;
+       });
+     },

      save: async function () {
-       var isPageSaved = await PageService.savePage(this.page);
+       // Get final content from Yjs document if collaboration is active
+       var content = this.collaborationActive
+         ? collaborationService.getDocumentContent()
+         : this.page.content;
+       this.page.content = content;
+       var isPageSaved = await PageService.savePage(this.page);
        if (isPageSaved)
          this.$router.push(`/space/${this.page.spaceKey}/${this.page.id}`);
      },
    },
    destroyed: function() {
      this.hideMacroButtons();
+     collaborationService.disconnect();
    },
  };
```

**Key integration points:**

1. **On mount** (`init`): After editor is loaded, call `collaborationService.connect()` to join the room
2. **On save**: Read content from Yjs document (not from `page.content` v-model) to ensure all collaborative changes are captured
3. **On unmount**: Disconnect from the room
4. **Draft system**: When collaboration is active, the draft system is **disabled** (Yjs provides its own persistence). The `saveDraft()` debounced call is skipped.

### 6.5 `y-codemirror` Binding Details

The `y-codemirror` package provides `CodemirrorBinding` which:

1. Creates a `Y.Text` fragment named `"content"` in the Yjs document
2. Bidirectionally syncs CodeMirror 5 document changes with the Yjs text type
3. Renders remote cursors as colored Caret widgets with user name labels
4. Handles cursor position awareness (selection ranges)

```javascript
import { CodemirrorBinding } from 'y-codemirror';
import { WebsocketProvider } from 'y-websocket';
import * as Y from 'yjs';

const ydoc = new Y.Doc();
const provider = new WebsocketProvider(
  `wss://${window.location.host}`,
  `page-${pageId}`,
  ydoc,
  { params: { /* auth token if needed */ } }
);

const binding = new CodemirrorBinding(
  ydoc.getText('content'),
  codeMirrorInstance,
  provider.awareness,
  { yUndoManager: new Y.UndoManager(ydoc.getText('content')) }
);
```

### 6.6 Remote Cursors

`y-codemirror` handles cursor rendering automatically:

- Each user's cursor position is broadcast via the Yjs awareness protocol
- Remote cursors appear as **colored vertical lines** with a **name label** above
- Colors are automatically assigned from a palette (one per user)
- Cursor positions update in real-time as users type

**Customization:**

```javascript
// Set local user state (broadcast to others)
provider.awareness.setLocalStateField('user', {
  name: profile.name,
  color: '#4a90d9',      // assigned from palette
  avatarUrl: profile.avatarUrl,
});
```

### 6.7 `CollaboratorsBar.vue`

A small UI component showing connected users:

```html
<template>
  <div class="collaborators-bar" v-if="connectedUsers.length > 0">
    <div v-for="user in connectedUsers" :key="user.clientId"
         class="collaborator-avatar"
         :title="user.name">
      <img v-if="user.avatarUrl" :src="user.avatarUrl" />
      <span v-else class="avatar-placeholder"
            :style="{ backgroundColor: user.color }">
        {{ user.name.charAt(0) }}
      </span>
    </div>
    <span class="collaborator-count" v-if="connectedUsers.length > 1">
      {{ connectedUsers.length }} editing
    </span>
  </div>
</template>
```

Placed in the editor toolbar area (next to Save/Close buttons).

## 7. Collaboration Flow Diagram

### 7.1 Single-Host Flow

```
User A opens page editor          User B opens same page editor
         │                                    │
         ▼                                    │
  initCollaboration()                         │
         │                                    │
         ▼                                    │
  Check feature flag                          │
  (from /api/quickstart)                      │
         │                                    │
    ┌────┴────┐                               │
    │ Enabled? │                               │
    └────┬────┘                               │
    Yes  │  No → use draft system             │
         │                                    │
         ▼                                    ▼
  collaborationService.connect()      Check feature flag
         │                           → Enabled
         ▼                                    │
  WebSocket: /api/collaboration/{pageId}      │
         │                                    │
    ┌────┴──────────┐                         │
    │ Feature gate   │                         │
    │ check (server) │                         │
    └────┬──────────┘                         │
    Enabled│  Disabled → HTTP 403             │
         │                                    │
    ┌────┴────┐                               │
    │ Auth +   │                               │
    │ Perm     │                               │
    │ Check    │                               │
    └────┬────┘                               │
         │                                    │
         ▼                                    ▼
  Load YjsDocument from DB          WebSocket: /api/collaboration/{pageId}
         │                                    │
         ▼                              ┌─────┴─────┐
  Seed from Page.Content               │ Auth + Perm │
  (if first time)                      │ Check       │
         │                             └─────┬─────┘
         │                                    │
         ▼                                    ▼
  Yjs sync protocol                    Load YjsDocument from DB
  (exchange state vectors)             (already exists)
         │                                    │
         ▼                                    ▼
  Both users now have                  Apply Yjs updates
  same Yjs document state             (get diff from state vector)
         │                                    │
         ├────────────────────────────────────┤
         │                                    │
         ▼                                    ▼
  User A types "Hello"               User sees "Hello" appear
  → Yjs update broadcast             → CodemirrorBinding renders
         │                            remote cursor + text
         │                                    │
         ▼                                    ▼
  User B types "World"               User sees "World" appear
  → Yjs update broadcast             → CodemirrorBinding renders
         │                                    │
         ▼                                    ▼
  Both disconnect                     Both disconnect
         │                                    │
         ▼                                    ▼
  Server persists YjsDocument        Server persists YjsDocument
  to MongoDB (force save)            (no-op if same state)
         │
         ▼
  Sync back to Page.Content          Page.Content is now updated
  (decode Yjs → Markdown string)     for REST API access
```

### 7.2 Cluster Flow (Multi-Host)

```
User A → connects to Host 1         User B → connects to Host 2
         │                                    │
         ▼                                    ▼
  Load YjsDocument from DB          Load YjsDocument from DB
  Create local Y.Doc (Host 1)       Create local Y.Doc (Host 2)
         │                                    │
         ▼                                    ▼
  Subscribe to Redis                Subscribe to Redis
  channel: collab:{pageId}          channel: collab:{pageId}
         │                                    │
         ▼                                    ▼
  User A types "Hello"              User B types "World"
         │                                    │
         ├──────────┐                  ┌──────┤
         ▼          │                  ▼      │
  Apply to local   │          Apply to local  │
  Y.Doc (Host 1)   │          Y.Doc (Host 2)  │
         │          │                  │      │
         ▼          │                  ▼      │
  Broadcast to     │          Broadcast to    │
  local clients    │          local clients   │
  (User A sees it) │          (User B sees it)│
         │          │                  │      │
         ▼          │                  ▼      │
  Publish to Redis │                  Publish  │
  channel          │                  to Redis │
  {                │                  channel  │
    "pageId": "...",│                  {        │
    "sourceHostId": │                    "pageId": "...",
     "host-1",     │                    "sourceHostId":
    "type": 0,     │                     "host-2",
    "payload": [...]│                    "type": 0,
  }                │                    "payload": [...]
         │         │                  }       │
         │         ▼                  │       │
         │  Host 2 receives           │       │
         │  from Redis                │       │
         │  (skip: local Y.Doc       │       │
         │   already has update)      │       │
         │                            │       │
         ▼                            ▼       │
  Host 1 receives from Redis                │
  (skip: sourceHostId = "host-1")           │
         │                                   │
         └───────────────────────────────────┘
                         │
                         ▼
              Both hosts persist to MongoDB
              (only when document is modified
               and debounce timer fires)
```

## 8. Migration & Coexistence

### 8.1 Backward Compatibility

- Pages without a `YjsDocument` record in MongoDB work exactly as before (non-collaborative)
- The `Page.Content` field remains the source of truth for read-only access (API, search, feed)
- The existing `PUT /api/page/{pageId}` endpoint continues to work for non-collaborative saves
- When a collaborative session persists, `Page.Content` is updated to reflect the latest state

### 8.2 Seed Data Migration

No migration needed. `YjsDocument` records are created on-demand when the first user opens a page in collaborative mode. The initial state is seeded from the existing `Page.Content`.

### 8.3 Draft System Interaction

| Scenario | Behavior |
|----------|----------|
| User opens editor, page has no active collab session | Draft system works as before (if draft exists, show modal) |
| User opens editor, other users are collaborating | Draft is ignored; user joins the collaborative session |
| User was editing offline (draft exists), then collaboration starts | Draft content is offered as initial merge (UX decision: prompt user) |
| Collaboration is active | Auto-save draft is **disabled** (Yjs persistence handles it) |

## 9. Configuration

New configuration section in `appsettings.json`:

```json
{
  "Collaboration": {
    "Enabled": true,
    "PersistenceIntervalMs": 5000,
    "MaxConnectionsPerPage": 20,
    "WebSocketKeepAliveMs": 30000,
    "ConnectionTimeoutMs": 60000,
    "AwarenessThrottleMs": 500,
    "Cluster": {
      "Enabled": false,
      "RedisChannelPrefix": "mimisbrunnr:collab",
      "RedisChannelSuffix": ":updates"
    }
  }
}
```

| Setting | Default | Description |
|---------|---------|-------------|
| `Enabled` | `true` | Startup default; can be overridden at runtime via admin panel |
| `PersistenceIntervalMs` | `5000` | Debounce interval for persisting Yjs state to MongoDB |
| `MaxConnectionsPerPage` | `20` | Maximum concurrent users per page (prevents abuse) |
| `WebSocketKeepAliveMs` | `30000` | Ping/pong interval to detect stale connections |
| `ConnectionTimeoutMs` | `60000` | Close connection if no activity for this duration |
| `AwarenessThrottleMs` | `500` | Throttle cursor position broadcasts via Redis (max 2/sec per user) |
| `Cluster.Enabled` | `false` | Enable Redis Pub/Sub relay for multi-host deployments |
| `Cluster.RedisChannelPrefix` | `"mimisbrunnr:collab"` | Redis channel prefix for collaboration messages |
| `Cluster.RedisChannelSuffix` | `":updates"` | Redis channel suffix |

**Cluster mode detection:** If `Cluster.Enabled` is `false`, the Redis relay is not subscribed and no inter-host communication happens. The system operates as a single-host deployment. When `true`, all hosts subscribe to Redis channels and relay updates.

**Redis connection:** Reuses the existing Redis connection configured via `Caching:ConnectionStrings:Redis` (already in the stack via `StackExchange.Redis` and `Microsoft.Extensions.Caching.StackExchangeRedis`). The collaboration module adds its own `IConnectionMultiplexer` registration that shares the same connection string.

## 10. Security Considerations

| Concern | Mitigation |
|---------|-----------|
| Unauthorized WebSocket connection | Full auth pipeline runs before WebSocket upgrade (JWT/Cookie validation) |
| Permission escalation via WebSocket | `EnsureEditPermission` checked at connection time using existing `PermissionService` |
| Feature toggle bypass | Server-side check in `CollaborationHub`; client UI is cosmetic only — WebSocket rejects connection if disabled |
| Denial of service (too many connections) | `MaxConnectionsPerPage` limit per page, enforced per-host |
| Stale document state on server crash | Periodic persistence (5s debounce) limits data loss window; `Page.Content` is also updated periodically |
| WebSocket without origin validation | Configure allowed origins in production; validate `Origin` header |
| Token revocation not enforced on WebSocket | Acceptable: revoked token connections are terminated on next reconnect; existing `ValidateTokenNotRevokedMiddleware` runs at upgrade time |
| Redis channel security (cluster mode) | Redis channels use application-specific prefix (`mimisbrunnr:collab:*`); no sensitive data in awareness messages (cursor positions only) |
| Concurrent MongoDB writes from multiple hosts | Optimistic concurrency via `Etag` field on `YjsDocument`; last-write-wins is acceptable for Yjs state |
| Feature toggle disabled while users are connected | Graceful shutdown: server sends WebSocket close frame; clients fall back to single-user mode |

## 11. Risks & Mitigations

| Risk | Impact | Likelihood | Mitigation |
|------|--------|-----------|-----------|
| `y-codemirror` (CM5 binding) is less maintained than CM6 variant | Medium | Medium | Pin to known working version; fork if needed |
| Yjs state grows unbounded for very large documents | Low | Low | Yjs compacts state; periodic full-state persistence resets state vector |
| MongoDB document size limit (16MB) for very large pages | Low | Very Low | 16MB Yjs state = millions of characters of text; unlikely for wiki pages |
| CodeMirror 5 cursor widget conflicts with existing macro hover buttons | Medium | High | Careful CSS z-index management; test macro interactions |
| Draft system confusion when collaboration starts | Medium | Medium | Clear UX: show toast "Collaborative editing is active" when joining room |
| Redis Pub/Sub message loss (fire-and-forget) | Medium | Low | Yjs sync protocol self-heals on reconnect; clients re-exchange state vectors |
| Multiple hosts persisting same page concurrently | Low | Medium | Optimistic concurrency via `Etag` on `YjsDocument` |
| Redis outage during cluster mode | Medium | Low | Hosts continue operating with local in-memory state; no new cross-host relay until Redis recovers; Yjs sync catches up on next client reconnect |
| Feature toggle disabled during active collaboration | Low | Low | Graceful WebSocket close; clients fall back to single-user mode and can save via REST |

## 12. Testing Strategy

| Test Type | Scope |
|-----------|-------|
| **Unit** | Yjs sync protocol message parsing, `YjsPersistenceService` CRUD, `PermissionService` integration, `CollaborationFeatureToggle` |
| **Integration** | WebSocket connection lifecycle (connect, auth, permission check, feature gate, disconnect), document persistence round-trip, Redis Pub/Sub relay between two in-process hosts |
| **E2E** | Two browser instances editing same page, cursor display, awareness updates, save to page |
| **Cluster E2E** | Two browser instances connecting to different backend hosts (different ports), editing same page, verifying real-time sync via Redis |
| **Feature Toggle** | Admin enables/disables collaboration at runtime; verify WebSocket accept/reject behavior; verify graceful close of active sessions |
| **Performance** | 20 concurrent users on single page, document update throughput, MongoDB write load, Redis Pub/Sub throughput with 3+ hosts |

## 13. Implementation Phases

### Phase 1: Core Collaboration (P0)
- New `Mimisbrunnr.Collaboration` project with WebSocket middleware
- Feature toggle: `ICollaborationFeatureToggle` + admin panel integration
- Yjs sync protocol implementation (binary message parsing)
- MongoDB persistence for `YjsDocument`
- Permission check on connection
- Frontend: `y-websocket` + `y-codemirror` integration in `PageEdit.vue`
- Basic cursor display

### Phase 2: Awareness & UX (P0)
- Awareness protocol (user name, avatar, color)
- `CollaboratorsBar.vue` component
- Toast notification when collaborative editing is active
- Disable draft system during collaborative sessions
- Sync `Page.Content` on session end
- Graceful WebSocket close when feature is toggled off

### Phase 3: Cluster Mode (P0)
- `RedisCollaborationRelay` (Pub/Sub publisher + subscriber)
- Per-page Redis channel subscription management
- Awareness throttling for Redis (max 2 updates/sec per user)
- Host ID generation (stable per instance, e.g. hostname + process ID)
- `SourceHostId` echo prevention
- Optimistic concurrency on `YjsDocument` persistence (`Etag` field)
- Integration tests with 2+ host instances

### Phase 4: Polish & Hardening (P1)
- Reconnection logic (exponential backoff)
- Graceful degradation when WebSocket is unavailable
- Graceful degradation when Redis is unavailable (single-host fallback)
- Metrics and monitoring
- Configuration via `appsettings.json`
- Connection limit enforcement
- Periodic feature toggle refresh on frontend (poll every 60s)

### Phase 5: Advanced (P2)
- Macro system awareness in collaborative sessions
- Per-section locking (optional)
- Collaboration history/audit log
- Rate limiting for document updates

## 14. File Changes Summary

### New Files

| File | Purpose |
|------|---------|
| `src/Mimisbrunnr.Collaboration/Mimisbrunnr.Collaboration.csproj` | New project |
| `src/Mimisbrunnr.Collaboration/Contracts/YjsDocument.cs` | MongoDB entity (with `Etag` for optimistic concurrency) |
| `src/Mimisbrunnr.Collaboration/Contracts/IYjsPersistenceService.cs` | Persistence interface |
| `src/Mimisbrunnr.Collaboration/Contracts/ICollaborationFeatureToggle.cs` | Feature toggle interface |
| `src/Mimisbrunnr.Collaboration/Contracts/CollaborationRedisMessage.cs` | Redis Pub/Sub message envelope |
| `src/Mimisbrunnr.Collaboration/Services/YjsPersistenceService.cs` | MongoDB persistence |
| `src/Mimisbrunnr.Collaboration/Services/CollaborationFeatureToggle.cs` | Runtime toggle via `ApplicationConfiguration` |
| `src/Mimisbrunnr.Collaboration/Services/CollaborationRoomManager.cs` | In-memory room tracking per host |
| `src/Mimisbrunnr.Collaboration/Protocol/YjsSyncHandler.cs` | Sync protocol |
| `src/Mimisbrunnr.Collaboration/Protocol/AwarenessHandler.cs` | Awareness protocol |
| `src/Mimisbrunnr.Collaboration/Protocol/YjsMessageTypes.cs` | Constants |
| `src/Mimisbrunnr.Collaboration/Cluster/IRedisCollaborationRelay.cs` | Redis relay interface |
| `src/Mimisbrunnr.Collaboration/Cluster/RedisCollaborationRelay.cs` | Pub/Sub relay implementation |
| `src/Mimisbrunnr.Collaboration/CollaborationHub.cs` | WebSocket endpoint |
| `src/Mimisbrunnr.Collaboration/CollaborationModule.cs` | DI module |
| `src/Mimisbrunnr.Collaboration/CollaborationModuleConfiguration.cs` | Config model |
| `src/Mimisbrunnr.Web.Host/ClientApp/src/services/collaboration/collaborationService.js` | Frontend service |
| `src/Mimisbrunnr.Web.Host/ClientApp/src/components/pageEditor/CollaboratorsBar.vue` | UI component |

### Modified Files

| File | Changes |
|------|---------|
| `src/Mimisbrunnr.Web.Host/Program.cs` | Add `app.UseCollaboration()` |
| `src/Mimisbrunnr.Web.Host/ClientApp/package.json` | Add `yjs`, `y-websocket`, `y-codemirror` |
| `src/Mimisbrunnr.Web.Host/ClientApp/src/views/Wiki/PageEdit.vue` | Integration with collaboration service, CollaboratorsBar, feature flag check |
| `src/Mimisbrunnr.Web.Host/Services/Metrics/MetricsApplicationBuilderExtensions.cs` | Update `/ws/` label |
| `src/Mimisbrunnr.Wiki/Contracts/ApplicationConfiguration.cs` | Add `CollaborationEnabled` field |
| `appsettings.json` | Add `Collaboration` config section |
