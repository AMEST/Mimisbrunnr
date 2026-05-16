# Mimisbrunnr - Wiki System

A wiki system for storing knowledge (Atlassian Confluence alternative).

## Technology Stack

| Component | Technologies |
|-----------|------------|
| **Backend** | .NET 8.0, ASP.NET Core |
| **Frontend** | Vue.js 2.x, Vuex, Vue Router |
| **Database** | MongoDB |
| **Caching** | Memory / MongoDB / Redis |
| **File Storage** | Local FS / GridFS / WebDAV / S3 |
| **Authentication** | OpenID Connect + JWT Bearer |
| **Containerization** | Docker, Docker Swarm |

## Project Structure

```
/workspace/
├── src/
│   ├── Mimisbrunnr.Web.Host/          # Main project (ASP.NET Core + Vue.js SPA)
│   │   ├── ClientApp/                 # Vue.js frontend
│   │   ├── Configuration/             # Configuration
│   │   ├── Services/                  # DI services
│   │   └── Program.cs                 # Entry point
│   │
│   ├── Mimisbrunnr.Web/               # API controllers
│   │
│   ├── Mimisbrunnr.Wiki/              # Wiki business logic
│   │
│   ├── Mimisbrunnr.Storage.MongoDb/   # MongoDB repositories
│   │
│   ├── Mimisbrunnr.Users/             # User management
│   │
│   ├── Mimisbrunnr.Web.Infrastructure/# Web infrastructure
│   │
│   ├── Mimisbrunnr.Persistent/        # File storage (Local/GridFS/WebDAV/S3)
│   │
│   ├── Mimisbrunnr.Favorites/         # Favorites
│   │
│   ├── Mimisbrunnr.Json/              # JSON utilities
│   │
│   ├── Integration/                  # Integrations (Mimisbrunnr.Integration, Mimisbrunnr.Integration.Client)
│   │
│   ├── DataImport/                    # Confluence import
│   │   ├── Mimisbrunnr.DataImport/
│   │   ├── Mimisbrunnr.DataImport.Confluence/
│   │   └── Mimisbrunnr.DataImport.Cli/           # CLI for import
│   │
│   └── Migration/                     # Storage migration
│       └── Mimisbrunnr.Migration.Persistent.Cli/ # CLI for migration between storages
│
└── tests/
    └── Mimisbrunnr.Web.Tests/         # Tests
```

## Key Paths

| Component | Path |
|-----------|------|
| **Frontend (Vue.js)** | `src/Mimisbrunnr.Web.Host/ClientApp/` |
| **Backend API** | `src/Mimisbrunnr.Web/` |
| **Models/Business Logic** | `src/Mimisbrunnr.Wiki/` |
| **MongoDB Repositories** | `src/Mimisbrunnr.Storage.MongoDb/` |
| **Entry Point** | `src/Mimisbrunnr.Web.Host/Program.cs` |

## Frontend Details

**Stack:**
- Vue 2.6.x
- Vuex 3.x (state management)
- Vue Router 3.x
- Bootstrap 4 + Bootstrap-Vue
- EasyMDE (Markdown editor)
- CodeMirror (code highlighting)
- vue-i18n (localization EN/RU)

**Structure:**
```
ClientApp/src/
├── main.js              # Vue entry point
├── App.vue               # Root component
├── router.js             # Vue Router configuration
├── services/
│   └── store/           # Vuex store
├── components/          # Vue components
├── views/               # Pages
├── assets/
│   └── lang.json        # Translations
└── thirdparty/          # Third-party styles
```

## Commands

### Frontend (ClientApp)
```bash
cd src/Mimisbrunnr.Web.Host/ClientApp
npm run serve    # Dev server
npm run build    # Production build
npm run lint     # Linting
```

### Backend
```bash
dotnet build              # Build
dotnet test               # Tests
dotnet run --project src/Mimisbrunnr.Web.Host  # Run
```

### Docker
```bash
docker build -t mimisbrunnr-wiki .  # Build image
```

## Configuration

Main settings via `appsettings.json` or environment variables:

- `Storage:ConnectionString` - MongoDB
- `Openid:*` - OpenID authentication
- `Bearer:*` - JWT tokens
- `Caching:Type` - Memory/MongoDb/Redis
- `Persistent:Type` - Local/GridFs/WebDav/S3

For details: see README.md Configuration section.

## Key Architectural Details

1. **SPA Integration**: Vue.js SPA builds to `ClientApp/dist/` and integrates via VueCliMiddleware
2. **Modular System**: Uses Skidbladnir.Modules for DI
3. **Mapperly**: Mapper generation via Riok.Mapperly (NET 8 source generators)
4. **Swagger**: API documented via Swashbuckle.AspNetCore
5. **Prometheus Metrics**: Metrics collection via prometheus-net
