# Дизайн-документ: Шаблоны страниц (Page Templates)

## 1. Обзор

Функционал позволяет создавать, хранить и использовать шаблоны страниц трех типов:
- **System** — системные шаблоны (например "Empty page"), доступны всем, редактируются только глобальными администраторами
- **User** — пользовательские шаблоны, видны только владельцу
- **Space** — шаблоны пространства, видны участникам пространства, редактируются администраторами пространства

Шаблоны используют Mustache-шаблонизацию (Stubble, существующий `ITemplateRenderer`) с подстановкой параметров.

---

## 2. Domain Model — PageTemplate

Новая сущность в `Mimisbrunnr.PageTemplates.Contracts`:

```
PageTemplate : IHasId<string>
  Id          : string          // MongoDB _id
  Name        : string          // название шаблона
  Description : string?         // описание
  Content     : string          // тело шаблона (Markdown + Mustache)
  Type        : TemplateType    // System | User | Space
  OwnerEmail  : string?         // владелец (для User/System)
  SpaceId     : string?         // пространство (для Space)
  Created     : DateTime
  Updated     : DateTime
  CreatedBy   : UserInfo
  UpdatedBy   : UserInfo

TemplateType : string (enum)
  System = "System"
  User   = "User"
  Space  = "Space"
```

---

## 3. Backend

### 3.1. Новый проект `src/Mimisbrunnr.PageTemplates/`

Структура:

```
Mimisbrunnr.PageTemplates/
├── Mimisbrunnr.PageTemplates.csproj
├── PageTemplatesModule.cs
├── Contracts/
│   ├── PageTemplate.cs
│   └── TemplateType.cs
└── Services/
    ├── IPageTemplateManager.cs
    └── PageTemplateManager.cs
```

**PageTemplatesModule.cs** — регистрирует `IPageTemplateManager` в DI:
```csharp
public class PageTemplatesModule : Module
{
    public override Type[] DependsModules => [];
    public override void Configure(IServiceCollection services)
    {
        services.AddSingleton<IPageTemplateManager, PageTemplateManager>();
    }
}
```

**IPageTemplateManager.cs** — бизнес-логика:
```csharp
public interface IPageTemplateManager
{
    Task<PageTemplate> GetById(string id);
    IQueryable<PageTemplate> GetAll();
    Task<PageTemplate> Create(PageTemplate template);
    Task Update(PageTemplate template);
    Task Delete(string id);
}
```

**PageTemplateManager.cs** — имплементация, работающая напрямую через `IRepository<PageTemplate>` (без промежуточного Store).

### 3.2. DTO (Integration) — `src/Integration/Mimisbrunnr.Integration/PageTemplates/`

```
PageTemplateModel.cs
PageTemplateCreateModel.cs
PageTemplateUpdateModel.cs
PageTemplateRenderRequest.cs
PageTemplateRenderResponse.cs
```

```csharp
// PageTemplateModel.cs
public class PageTemplateModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Content { get; set; }
    public string Type { get; set; }       // "System" | "User" | "Space"
    public string OwnerEmail { get; set; }
    public string SpaceId { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
    public UserInfo CreatedBy { get; set; }
    public UserInfo UpdatedBy { get; set; }
}

// PageTemplateCreateModel.cs
public class PageTemplateCreateModel
{
    [Required] public string Name { get; set; }
    public string Description { get; set; }
    [Required] public string Content { get; set; }
    [Required] public string Type { get; set; }  // "System" | "User" | "Space"
    public string SpaceKey { get; set; }          // для Space-типа
}

// PageTemplateUpdateModel.cs
public class PageTemplateUpdateModel
{
    [Required] public string Name { get; set; }
    public string Description { get; set; }
    [Required] public string Content { get; set; }
}

// PageTemplateRenderRequest.cs
public class PageTemplateRenderRequest
{
    [Required] public string TemplateId { get; set; }
    [Required] public string SpaceKey { get; set; }  // для вычисления параметров
}

// PageTemplateRenderResponse.cs
public class PageTemplateRenderResponse
{
    public string Content { get; set; }
}
```

### 3.3. API Controller — `src/Mimisbrunnr.Web/PageTemplates/`

```
PageTemplateController.cs
IPageTemplateService.cs
PageTemplateService.cs
HandlePageTemplateErrorsAttribute.cs
PageTemplateMapper.cs (Mapperly)
```

**Архитектурный принцип: контроллер — только точка входа**

Вся бизнес-логика, включая рендеринг шаблонов, должна находиться в сервисе (`PageTemplateService`), а не в контроллере. Контроллер выполняет только:
1. Проверку прав (через атрибуты или вызов сервиса)
2. Валидацию контракта (атрибут `[ValidateModel]`)
3. Получение текущего пользователя (если нужно)
4. Вызов соответствующего метода `AppService`
5. Возврат результата (или обработку ошибок через `HandlePageTemplateErrorsAttribute`)

**Архитектурный принцип: контракты API — только типизированные DTO**

Контроллеры должны возвращать строго типизированные DTO из сборки Integration (или локальных Contracts), а не anonymous types (`new { ... }`). Это обеспечивает корректную генерацию схемы в Swagger/OpenAPI. Сервис (`AppService`) должен возвращать готовый внешний контракт, который контроллер отдаёт наружу без дополнительной трансформации:

```csharp
// Сервис возвращает готовый DTO для API
public async Task<PageTemplateRenderResponse> Render(string templateId, string spaceKey, UserInfo user)
{
    // ... логика ...
    return new PageTemplateRenderResponse { Content = rendered };
}

// Контроллер просто оборачивает в Ok()
public async Task<IActionResult> Render(string id, PageTemplateRenderRequest request)
{
    var user = await _userService.GetCurrentUser();
    var result = await _pageTemplateService.Render(id, request.SpaceKey, user);
    return Ok(result);  // PageTemplateRenderResponse, НЕ new { content = result.Content }
}
```

Запрещено:
```csharp
// НЕЛЬЗЯ — anonymous type, Swagger не видит схему
return Ok(new { content = rendered });
```

**Пример правильной структуры метода контроллера:**

```csharp
[HttpPost("{id}/render")]
[ValidateModel]
[HandlePageTemplateErrors]
public async Task<IActionResult> Render(string id, PageTemplateRenderRequest request)
{
    var user = await _userService.GetCurrentUser();
    var result = await _pageTemplateService.Render(id, request.SpaceKey, user);
    return Ok(result);
}
```

Логика формирования параметров рендеринга (даты, данные пользователя, данные пространства) и вызов `ITemplateRenderer` — целиком в `PageTemplateService.Render()`.

**Маршрут:** `[Route("api/[controller]")]` → `/api/pagetemplate`

**Эндпоинты:**

| Method | Path | Auth | Описание |
|--------|------|------|----------|
| GET | `/api/pagetemplate` | Authorize | Получить доступные шаблоны (фильтр по типу, query params: `?type=System&spaceKey=xxx`) |
| GET | `/api/pagetemplate/{id}` | Authorize | Получить шаблон по ID |
| POST | `/api/pagetemplate` | Authorize | Создать шаблон (права проверяются внутри) |
| PUT | `/api/pagetemplate/{id}` | Authorize | Обновить шаблон (права проверяются внутри) |
| DELETE | `/api/pagetemplate/{id}` | Authorize | Удалить шаблон (права проверяются внутри) |
| POST | `/api/pagetemplate/{id}/render` | Authorize | Сгенерировать контент из шаблона |

**Логика разграничения прав в `PageTemplateService`:**

```
Create:
  System → только admin (RequiredAdminRole)
  User   → любой авторизованный (владелец = текущий пользователь)
  Space  → admin пространства

GetAll (список доступных):
  System → все
  User   → только свои (OwnerEmail == current.Email)
  Space  → участники пространства (есть edit permission)

Update/Delete:
  System → только admin
  User   → только владелец
  Space  → admin пространства
```

**Рендеринг шаблона — логика в `PageTemplateService.Render()`:**

```csharp
public async Task<PageTemplateRenderResponse> Render(string templateId, string spaceKey, UserInfo user)
{
    var template = await _pageTemplateManager.GetById(templateId);
    var space = await _spaceManager.GetByKey(spaceKey);

    var parameters = new Dictionary<string, object>
    {
        ["CurrentDate"] = DateTime.UtcNow.ToString("yyyy-MM-dd"),
        ["CurrentTime"] = DateTime.UtcNow.ToString("HH:mm:ss"),
        ["CurrentDateTime"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
        ["UserName"] = user.Name,
        ["UserEmail"] = user.Email,
        ["UserAvatarUrl"] = user.AvatarUrl,
        ["SpaceName"] = space.Name,
        ["SpaceKey"] = space.Key
    };

    var rendered = await _templateRenderer.Render(template.Content, parameters);
    return new PageTemplateRenderResponse { Content = rendered };
}
```

**Контроллер вызывает сервис и возвращает готовый DTO:**

```csharp
[HttpPost("{id}/render")]
[ValidateModel]
[HandlePageTemplateErrors]
public async Task<IActionResult> Render(string id, PageTemplateRenderRequest request)
{
    var user = await _userService.GetCurrentUser();
    var result = await _pageTemplateService.Render(id, request.SpaceKey, user);
    return Ok(result);  // PageTemplateRenderResponse — схема видна в Swagger
}
```

**Пример Mustache-шаблона:**
```markdown
# {{SpaceName}}

Создано пользователем {{User.Name}} {{CurrentDate}}

## Описание

...
```

### 3.4. Storage — MongoDB

EntityMapClass используется по тому же принципу, что и существующие маппинги (см. `PageMap.cs`, `SpaceMap.cs` и др. в `Mimisbrunnr.Storage.MongoDb.Mappings`).

`PageTemplateManager` работает напрямую через существующую абстракцию `IRepository<PageTemplate>`, которая уже умеет CRUD-операции:

```csharp
public interface IRepository<TEntity> where TEntity : class
{
    Task Create(TEntity obj, CancellationToken cancellationToken = default);
    Task Update(TEntity obj, CancellationToken cancellationToken = default);
    Task Delete(TEntity obj, CancellationToken cancellationToken = default);
    IQueryable<TEntity> GetAll();
}
```

Поэтому **не нужно** создавать отдельный `IPageTemplateStore` / `PageTemplateStore` — просто инжектим `IRepository<PageTemplate>` в `PageTemplateManager`.

**Новая entity map** `src/Mimisbrunnr.Storage.MongoDb/Mappings/PageTemplateMap.cs`:
```csharp
public class PageTemplateMap : EntityMapClass<PageTemplate>
{
    public PageTemplateMap()
    {
        ToCollection("PageTemplates");
        MapId(x => x.Id, BsonType.String);
    }
}
```

**Индексы** — добавить метод `CreatePageTemplateIndexes` в `MongoDbStoreModule` по аналогии с существующими `CreateUserIndexes`, `CreatePageIndexes` и т.д. Метод вызывается в `StartAsync`:

```csharp
private static async Task CreatePageTemplateIndexes(IMongoDbContext mongoContext)
{
    var collection = mongoContext.GetCollection<PageTemplate>();

    var typeKeyDefinition = Builders<PageTemplate>.IndexKeys.Ascending(x => x.Type);
    await collection.Indexes.CreateOneAsync(new CreateIndexModel<PageTemplate>(typeKeyDefinition, new CreateIndexOptions()
    {
        Background = true
    }));

    var ownerEmailKeyDefinition = Builders<PageTemplate>.IndexKeys.Ascending(x => x.OwnerEmail);
    await collection.Indexes.CreateOneAsync(new CreateIndexModel<PageTemplate>(ownerEmailKeyDefinition, new CreateIndexOptions()
    {
        Background = true
    }));

    var spaceIdKeyDefinition = Builders<PageTemplate>.IndexKeys.Ascending(x => x.SpaceId);
    await collection.Indexes.CreateOneAsync(new CreateIndexModel<PageTemplate>(spaceIdKeyDefinition, new CreateIndexOptions()
    {
        Background = true
    }));
}
```

**Регистрация entity** в `MongoDbStoreModule.Configure`:
```csharp
.AddEntity<PageTemplate, PageTemplateMap>()
```

### 3.5. Регистрация модулей

**StartupModule.cs** — добавить зависимость:
```csharp
typeof(PageTemplatesModule)
```

**WebModule.cs** — добавить сервисы:
```csharp
services.AddSingleton<IPageTemplateService, PageTemplateService>();
```

**MongoDbStoreModule.cs** — зарегистрировать `IRepository<PageTemplate>` (если ещё не зарегистрировано автоматически через `AddEntity`):

### 3.6. Обработка ошибок

Новый фильтр `HandlePageTemplateErrorsAttribute` по аналогии с `HandleWikiErrorsAttribute`:
- `PageTemplateNotFoundException` → 404
- `UserHasNotPermissionException` → 403
- `InvalidOperationException` → 400

### 3.7. Маппинг (Mapperly)

`PageTemplateMapper.cs` в `Mimisbrunnr.Web.Mapping`:
```csharp
[Mapper]
public static partial class PageTemplateMapper
{
    public static partial PageTemplateModel ToModel(this PageTemplate template);
}
```

---

## 4. Frontend

Все HTTP-запросы на фронте выполняются через нативный **`fetch`**. Использование `axios` не допускается.

### 4.1. API Service — `ClientApp/src/services/pageTemplateService.js`

```javascript
async function request(url, options = {}) {
  const res = await fetch(url, {
    headers: { "Content-Type": "application/json" },
    ...options,
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw { status: res.status, message: err.message, data: err };
  }
  return res.json();
}

export default {
  getAll(type, spaceKey) {
    const params = new URLSearchParams();
    if (type) params.set("type", type);
    if (spaceKey) params.set("spaceKey", spaceKey);
    const qs = params.toString();
    return request(`/api/pagetemplate${qs ? "?" + qs : ""}`);
  },
  getById(id) {
    return request(`/api/pagetemplate/${id}`);
  },
  create(model) {
    return request("/api/pagetemplate", {
      method: "POST",
      body: JSON.stringify(model),
    });
  },
  update(id, model) {
    return request(`/api/pagetemplate/${id}`, {
      method: "PUT",
      body: JSON.stringify(model),
    });
  },
  delete(id) {
    return request(`/api/pagetemplate/${id}`, { method: "DELETE" });
  },
  render(templateId, spaceKey) {
    return request(`/api/pagetemplate/${templateId}/render`, {
      method: "POST",
      body: JSON.stringify({ templateId, spaceKey }),
    });
  },
};
```

### 4.2. Vuex Store — добавления

В `store.js` новое состояние:
```javascript
state: {
  application: { /* ... */ },
  templates: {
    system: [],   // PageTemplateModel[]
    user: [],     // PageTemplateModel[]
    space: [],    // PageTemplateModel[]
  }
}
```

Мутации: `setSystemTemplates`, `setUserTemplates`, `setSpaceTemplates`.

### 4.3. Общий компонент управления шаблонами

**`ClientApp/src/components/templates/PageTemplateManager.vue`**

Универсальный компонент для CRUD шаблонов. Props:
- `type` — `"System" | "User" | "Space"`
- `spaceKey` — ключ пространства (только для Space)
- `readonly` — если true, скрыть кнопки редактирования (для не-админов в Space)

Функционал:
- Таблица/список шаблонов с колонками: Name, Description, Created, Actions
- Кнопка "Create template" → модалка создания
- Каждая строка → edit/delete (с подтверждением)
- Модалка создания/редактирования: поля Name, Description, Content (textarea с EasyMDE)

```
PageTemplateManager.vue
├── PageTemplateList.vue        (таблица)
├── PageTemplateModal.vue       (create/edit форма)
└── (использует pageTemplateService.js)
```

### 4.4. Интеграция: Админка

**`src/components/admin/PageTemplatesTab.vue`**

Новая вкладка в `admin/Menu.vue`. Добавить пункт "Page Templates" в меню.

```javascript
// В admin/Menu.vue добавить:
{ title: 'admin.templates.title', to: '/admin/templates' }
```

**Новый view** `views/Admin/PageTemplates.vue`:
```vue
<template>
  <div>
    <h3>{{ $t('admin.templates.title') }}</h3>
    <PageTemplateManager type="System" />
  </div>
</template>
```

**Роутер** `router.js`:
```javascript
{ path: '/admin/templates', name: 'PageTemplates', component: PageTemplates }
```

### 4.5. Интеграция: Профиль пользователя

В `SettingsModal.vue` добавить новую вкладку:
```vue
<page-templates-tab />
```

**`src/components/people/profile/settings/PageTemplatesTab.vue`:**
```vue
<PageTemplateManager type="User" />
```

### 4.6. Интеграция: Пространство

В `space/Menu.vue` добавить пункт "Page Templates" в список действий (виден админам пространства):

```html
<b-list-group-item v-b-modal.space-page-templates-modal v-if="userPermissions.isAdmin">
  <b-icon-files />&nbsp; {{ $t("space.actions.templates") }}
</b-list-group-item>
```

И модалку `components/space/modal/PageTemplates.vue`:
```vue
<template>
  <b-modal id="space-page-templates-modal" title="..." size="lg" hide-footer>
    <PageTemplateManager type="Space" :spaceKey="space.key" />
  </b-modal>
</template>
```

### 4.7. Создание страницы из шаблона

**Кнопка в Header.vue** рядом с "Create page":

После `<b-button variant="light" class="create-button" ...>` добавить:
```html
<b-button
  variant="light"
  class="create-from-template-button"
  v-if="$store.state.application.profile"
  @click="showTemplatePicker = true"
  size="sm"
>
  <b-icon-chevron-down />
</b-button>
```

```css
.create-button + .create-from-template-button {
  margin-left: 2px;
  height: 32px;
  margin-top: 0.2em;
  padding-left: 6px;
  padding-right: 6px;
}

/* Скрываем кнопку на мобильных — места нет, адаптивность сложная */
@media (max-width: 576px) {
  .create-from-template-button {
    display: none !important;
  }
}
```

**Модалка выбора шаблона** `components/base/CreateFromTemplateModal.vue`:

```vue
<template>
  <b-modal id="create-from-template-modal" title="..." centered @show="loadTemplates">
    <b-list-group>
      <b-list-group-item
        v-for="tpl in templates"
        :key="tpl.id"
        button
        @click="createFromTemplate(tpl)"
      >
        <strong>{{ tpl.name }}</strong>
        <small class="text-muted"> — {{ tpl.type }}</small>
        <br><small v-if="tpl.description">{{ tpl.description }}</small>
      </b-list-group-item>
    </b-list-group>
  </b-modal>
</template>
```

**Логика `createFromTemplate`:**
```javascript
async createFromTemplate(template) {
  // 1. Определяем spaceKey (как в create())
  var spaceKey = this.$route.params.key;
  if (!spaceKey) {
    spaceKey = await ProfileService.getOrCreatePersonalSpace(profile);
  }

  // 2. Рендерим шаблон
  var renderResult = await pageTemplateService.render(template.id, spaceKey);
  var content = renderResult.content;

  // 3. Определяем parentPageId
  var pageId = this.$route.params.pageId;
  if (!pageId) {
    var spaceReq = await fetch("/api/space/" + spaceKey);
    var spaceData = await spaceReq.json();
    pageId = spaceData.homePageId;
  }

  // 4. Создаём страницу с готовым контентом
  var createResult = await fetch("/api/page", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      spaceKey: spaceKey,
      parentPageId: pageId,
      name: template.name,
      content: content,
    }),
  });
  var newPage = await createResult.json();
  this.$router.push("/space/" + spaceKey + "/" + newPage.id + "/edit");
}
```

### 4.8. Локализация — `assets/lang.json`

Добавить ключи:
```json
{
  "en": {
    "pageTemplates": {
      "title": "Page Templates",
      "create": "Create Template",
      "edit": "Edit Template",
      "delete": "Delete Template",
      "deleteConfirm": "Are you sure you want to delete this template?",
      "name": "Name",
      "description": "Description",
      "content": "Content",
      "source": "Source",
      "system": "System",
      "user": "Personal",
      "space": "Space",
      "createFromTemplate": "Create from Template",
      "noTemplates": "No templates available",
      "createPageFromTemplate": "Create page from template"
    },
    "header": {
      "pageCreateFromTemplateButton": "Create from template"
    },
    "admin": {
      "templates": {
        "title": "Page Templates"
      }
    },
    "profile": {
      "settings": {
        "pageTemplates": "My Templates"
      }
    },
    "space": {
      "actions": {
        "templates": "Page Templates"
      }
    }
  },
  "ru": {
    "pageTemplates": {
      "title": "Шаблоны страниц",
      "create": "Создать шаблон",
      "edit": "Редактировать шаблон",
      "delete": "Удалить шаблон",
      "deleteConfirm": "Вы уверены, что хотите удалить этот шаблон?",
      "name": "Название",
      "description": "Описание",
      "content": "Содержимое",
      "source": "Источник",
      "system": "Системный",
      "user": "Личный",
      "space": "Пространства",
      "createFromTemplate": "Создать из шаблона",
      "noTemplates": "Нет доступных шаблонов",
      "createPageFromTemplate": "Создать страницу из шаблона"
    },
    "header": {
      "pageCreateFromTemplateButton": "Создать из шаблона"
    },
    "admin": {
      "templates": {
        "title": "Шаблоны страниц"
      }
    },
    "profile": {
      "settings": {
        "pageTemplates": "Мои шаблоны"
      }
    },
    "space": {
      "actions": {
        "templates": "Шаблоны страниц"
      }
    }
  }
}
```

---

## 5. Схема взаимодействия (Sequence)

```
User clicks [▼] in Header
  → CreateFromTemplateModal opens
  → GET /api/pagetemplate (через pageTemplateService.getAll())
  → PageTemplateController.GetAll() → PageTemplateService.GetAll()
  → список шаблонов (System + User + Space, если в пространстве)
  → User выбирает шаблон
  → POST /api/pagetemplate/{id}/render { spaceKey }
      → PageTemplateController.Render()
        → [ValidateModel] → проверка контракта
        → получение UserInfo
        → PageTemplateService.Render()
          → IPageTemplateManager.GetById()
          → ISpaceManager.GetByKey()
          → ITemplateRenderer.Render(template.Content, parameters)
        ← PageTemplateRenderResponse { Content: "..." }
      ← PageTemplateRenderResponse { Content: "..." }
  → POST /api/page { spaceKey, parentPageId, name, content }
      → PageService.Create()
  → redirect to /space/{key}/{pageId}/edit
```

---

## 6. Состав изменений по файлам

### Backend (C#):
| # | Файл | Действие |
|---|------|----------|
| 1 | `src/Mimisbrunnr.PageTemplates/Mimisbrunnr.PageTemplates.csproj` | создать |
| 2 | `src/Mimisbrunnr.PageTemplates/Contracts/PageTemplate.cs` | создать |
| 3 | `src/Mimisbrunnr.PageTemplates/Contracts/TemplateType.cs` | создать |
| 4 | `src/Mimisbrunnr.PageTemplates/Services/IPageTemplateManager.cs` | создать |
| 5 | `src/Mimisbrunnr.PageTemplates/Services/PageTemplateManager.cs` | создать |
| 6 | `src/Mimisbrunnr.PageTemplates/PageTemplatesModule.cs` | создать |
| 7 | `src/Integration/Mimisbrunnr.Integration/PageTemplates/PageTemplateModel.cs` | создать |
| 8 | `src/Integration/Mimisbrunnr.Integration/PageTemplates/PageTemplateCreateModel.cs` | создать |
| 9 | `src/Integration/Mimisbrunnr.Integration/PageTemplates/PageTemplateUpdateModel.cs` | создать |
| 10 | `src/Integration/Mimisbrunnr.Integration/PageTemplates/PageTemplateRenderRequest.cs` | создать |
| 11 | `src/Integration/Mimisbrunnr.Integration/PageTemplates/PageTemplateRenderResponse.cs` | создать |
| 12 | `src/Mimisbrunnr.Web/PageTemplates/PageTemplateController.cs` | создать |
| 13 | `src/Mimisbrunnr.Web/PageTemplates/IPageTemplateService.cs` | создать |
| 14 | `src/Mimisbrunnr.Web/PageTemplates/PageTemplateService.cs` | создать |
| 15 | `src/Mimisbrunnr.Web/Filters/HandlePageTemplateErrorsAttribute.cs` | создать |
| 16 | `src/Mimisbrunnr.Web/Mapping/PageTemplateMapper.cs` | создать |
| 17 | `src/Mimisbrunnr.Storage.MongoDb/Mappings/PageTemplateMap.cs` | создать |
| 18 | `src/Mimisbrunnr.Storage.MongoDb/MongoDbStoreModule.cs` | изменить (добавить entity + indexes) |
| 19 | `src/Mimisbrunnr.Web.Host/StartupModule.cs` | изменить (добавить PageTemplatesModule) |
| 20 | `src/Mimisbrunnr.Web/WebModule.cs` | изменить (добавить ITemplateService) |

### Frontend (Vue.js):
| # | Файл | Действие |
|---|------|----------|
| 1 | `ClientApp/src/services/pageTemplateService.js` | создать |
| 2 | `ClientApp/src/components/templates/PageTemplateManager.vue` | создать |
| 3 | `ClientApp/src/components/templates/PageTemplateModal.vue` | создать |
| 4 | `ClientApp/src/components/base/CreateFromTemplateModal.vue` | создать |
| 5 | `ClientApp/src/components/admin/PageTemplatesTab.vue` | создать |
| 6 | `ClientApp/src/components/people/profile/settings/PageTemplatesTab.vue` | создать |
| 7 | `ClientApp/src/components/space/modal/PageTemplates.vue` | создать |
| 8 | `ClientApp/src/components/base/Header.vue` | изменить (добавить кнопку ▼ + модалку) |
| 9 | `ClientApp/src/components/space/Menu.vue` | изменить (добавить пункт меню) |
| 10 | `ClientApp/src/components/people/profile/SettingsModal.vue` | изменить (добавить вкладку) |
| 11 | `ClientApp/src/views/Admin/PageTemplates.vue` | создать |
| 12 | `ClientApp/src/views/Admin/GeneralConfiguration.vue` | изменить (добавить таб) |
| 13 | `ClientApp/src/services/store.js` | изменить (добавить состояние templates) |
| 14 | `ClientApp/src/router.js` | изменить (добавить маршрут) |
| 15 | `ClientApp/src/assets/lang.json` | изменить (добавить переводы) |

---

## 7. Тестирование

### 7.1. Стек

- **xUnit** — test framework
- **FakeItEasy** — mocking
- **AwesomeAssertions** — assertions (замена FluentAssertions)

### 7.2. Тестируемые компоненты

#### 7.2.1. PageTemplateManager (бизнес-логика, `Mimisbrunnr.PageTemplates`)

Тесты в `tests/Mimisbrunnr.Web.Tests/PageTemplates/PageTemplateManagerTests.cs`:

```csharp
public class PageTemplateManagerTests
{
    private readonly IRepository<PageTemplate> _repository;
    private readonly PageTemplateManager _manager;

    public PageTemplateManagerTests()
    {
        _repository = A.Fake<IRepository<PageTemplate>>();
        _manager = new PageTemplateManager(_repository);
    }

    [Fact]
    public async Task Create_Should_Set_CreatedAndUpdated()
    {
        var template = new PageTemplate
        {
            Name = "Test",
            Content = "# Hello",
            Type = TemplateType.User,
            OwnerEmail = "user@test.com"
        };

        var result = await _manager.Create(template);

        using (new AssertionScope())
        {
            result.Created.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
            result.Updated.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
            result.CreatedBy.Email.Should().Be("user@test.com");
        }

        A.CallTo(() => _repository.Create(A<PageTemplate>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetById_Should_Return_Template_From_Repository()
    {
        var templates = new[]
        {
            new PageTemplate { Id = "123", Name = "Test" },
            new PageTemplate { Id = "456", Name = "Other" },
        }.AsQueryable();
        A.CallTo(() => _repository.GetAll()).Returns(templates);

        var result = await _manager.GetById("123");

        result.Should().NotBeNull();
        result.Id.Should().Be("123");
    }

    [Fact]
    public async Task GetById_Should_Throw_When_Not_Found()
    {
        A.CallTo(() => _repository.GetAll()).Returns(Enumerable.Empty<PageTemplate>().AsQueryable());

        await _manager.Invoking(m => m.GetById("missing"))
            .Should().ThrowAsync<PageTemplateNotFoundException>();
    }

    [Fact]
    public async Task Update_Should_Set_Updated_And_UpdatedBy()
    {
        var existing = new PageTemplate
        {
            Id = "1",
            Name = "Old",
            Content = "Old content",
            Updated = DateTime.UtcNow.AddDays(-1),
            UpdatedBy = new UserInfo { Email = "old@test.com" }
        };
        var templates = new[] { existing }.AsQueryable();
        A.CallTo(() => _repository.GetAll()).Returns(templates);

        var updateInfo = new UserInfo { Email = "new@test.com" };
        await _manager.Update("1", "New Name", "New content", updateInfo);

        using (new AssertionScope())
        {
            existing.Name.Should().Be("New Name");
            existing.Content.Should().Be("New content");
            existing.UpdatedBy.Email.Should().Be("new@test.com");
            existing.Updated.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
        }

        A.CallTo(() => _repository.Update(existing, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Delete_Should_Remove_From_Repository()
    {
        var template = new PageTemplate { Id = "1" };
        var templates = new[] { template }.AsQueryable();
        A.CallTo(() => _repository.GetAll()).Returns(templates);

        await _manager.Delete("1");

        A.CallTo(() => _repository.Delete(template, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetAll_Should_Return_From_Repository()
    {
        var templates = new[]
        {
            new PageTemplate { Name = "A" },
            new PageTemplate { Name = "B" }
        }.AsQueryable();

        A.CallTo(() => _repository.GetAll()).Returns(templates);

        var result = _manager.GetAll();

        result.Should().HaveCount(2);
    }
}
```

### 7.3. Запуск тестов

```bash
dotnet test tests/Mimisbrunnr.Web.Tests/
```

При реализации тестов следует убедиться, что:
1. Все публичные методы `PageTemplateManager` покрыты unit-тестами
2. Все эндпоинты `PageTemplateController` проверены на корректный HTTP-статус при успехе/ошибке
3. В `PageTemplateService` проверены все сценарии разграничения прав (admin/non-admin, владелец/чужой, участник/не-участник пространства)
4. Рендеринг проверен с корректными значениями всех подставляемых параметров

---

## 8. Порядок реализации

Все комментарии в коде пишутся на **английском языке**.

### Этап 1: Domain + Storage (фундамент)

| Шаг | Действие | Файлы |
|-----|----------|-------|
| 1.1 | Создать проект `Mimisbrunnr.PageTemplates` | `.csproj` |
| 1.2 | Определить `TemplateType` enum | `Contracts/TemplateType.cs` |
| 1.3 | Определить `PageTemplate` entity | `Contracts/PageTemplate.cs` |
| 1.4 | Определить `IPageTemplateManager` | `Services/` |
| 1.5 | Реализовать `PageTemplateManager` (через `IRepository<PageTemplate>`) | `Services/PageTemplateManager.cs` |
| 1.6 | Создать `PageTemplatesModule` | `PageTemplatesModule.cs` |
| 1.7 | Создать `PageTemplateMap` (MongoDB mapping) | `Mimisbrunnr.Storage.MongoDb/Mappings/` |
| 1.8 | Зарегистрировать entity в `MongoDbStoreModule` | `MongoDbStoreModule.cs` |
| 1.9 | Добавить индексы (Type, OwnerEmail, SpaceId) | `MongoDbStoreModule.cs` |
| 1.10 | Добавить `PageTemplatesModule` в `StartupModule` | `StartupModule.cs` |

**Проверка:** `dotnet build` успешно собирается.

### Этап 2: Web-слой API

| Шаг | Действие | Файлы |
|-----|----------|-------|
| 2.1 | Создать DTO в `Mimisbrunnr.Integration` | `PageTemplates/` |
| 2.2 | Создать `PageTemplateMapper` (Mapperly) | `Mapping/PageTemplateMapper.cs` |
| 2.3 | Создать `HandlePageTemplateErrorsAttribute` | `Filters/` |
| 2.4 | Создать `IPageTemplateService` | `PageTemplates/` |
| 2.5 | Реализовать `PageTemplateService` с разграничением прав | `PageTemplates/PageTemplateService.cs` |
| 2.6 | Создать `PageTemplateController` | `PageTemplates/PageTemplateController.cs` |
| 2.7 | Зарегистрировать `IPageTemplateService` в `WebModule` | `WebModule.cs` |

**Проверка:** `dotnet build` успешно. Можно протестировать через Swagger.

### Этап 3: Тесты (Backend)

| Шаг | Действие |
|-----|----------|
| 3.1 | Написать тесты `PageTemplateManagerTests` (repository mocking) |
| 3.2 | Написать тесты `PageTemplateServiceTests` (все сценарии прав) |
| 3.3 | Написать тесты `PageTemplateControllerTests` (routing, validation, errors) |
| 3.4 | Запустить `dotnet test` — все тесты зелёные |

### Этап 4: Frontend — инфраструктура

| Шаг | Действие | Файлы |
|-----|----------|-------|
| 4.1 | Создать `pageTemplateService.js` (API client) | `services/` |
| 4.2 | Добавить состояние `templates` в Vuex store | `store.js` |
| 4.3 | Добавить переводы в `lang.json` | `assets/lang.json` |

### Этап 5: Frontend — общий компонент управления шаблонами

| Шаг | Действие | Файлы |
|-----|----------|-------|
| 5.1 | Создать `PageTemplateModal.vue` (create/edit форма) | `components/templates/` |
| 5.2 | Создать `PageTemplateManager.vue` (список + CRUD) | `components/templates/` |

### Этап 6: Frontend — интеграция точек входа

| Шаг | Действие | Файлы |
|-----|----------|-------|
| 6.1 | **Админка:** создать `Admin/PageTemplates.vue`, добавить роутер, пункт меню | `views/Admin/`, `router.js`, `admin/Menu.vue` |
| 6.2 | **Профиль:** создать `PageTemplatesTab.vue`, добавить вкладку в `SettingsModal` | `components/people/profile/settings/` |
| 6.3 | **Пространство:** создать `space/modal/PageTemplates.vue`, добавить пункт в `Menu.vue` | `components/space/` |

### Этап 7: Frontend — создание страницы из шаблона

| Шаг | Действие | Файлы |
|-----|----------|-------|
| 7.1 | Создать `CreateFromTemplateModal.vue` | `components/base/` |
| 7.2 | Добавить кнопку `▼` в `Header.vue` рядом с кнопкой Create | `components/base/Header.vue` |

### Этап 8: Финальная проверка

| Шаг | Действие |
|-----|----------|
| 8.1 | `dotnet build` — без ошибок |
| 8.2 | `dotnet test` — все тесты проходят |
| 8.3 | `npm run lint` — во фронтенде без ошибок |
| 8.4 | Ручная проверка через Swagger всех эндпоинтов API |
| 8.5 | Ручная проверка в браузере: админка, профиль, пространство, создание из шаблона |
