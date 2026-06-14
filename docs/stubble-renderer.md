# Stubble Template Renderer

The Stubble Template Renderer is a Mustache-based templating engine used in Mimisbrunnr for rendering dynamic content. It is built on the [Stubble.Core](https://github.com/StubbleOrg/Stubble) library.

## Usage Areas

The template renderer is used in two places:

1. **Plugin macros** — for rendering macro templates with user-provided parameters and predefined context variables
2. **Page templates** — for creating new pages from pre-defined templates (System, User, and Space templates)

## Basic Syntax

### Variable Substitution

Replace `{{variable}}` with the value from the parameters:

```
{{name}}
{{PageName}}
```

### Sections (Conditionals / Iteration)

Sections render their content only if the value is truthy (non-empty, non-null):

```
{{#title}}<h4>{{title}}</h4>{{/title}}
```

## Predefined Context Variables

### Page Templates

These variables are automatically available when rendering a page template via the `/api/pagetemplate/{id}/render` endpoint:

| Variable | Description |
|----------|-------------|
| `{{CurrentDate}}` | Current UTC date (`yyyy-MM-dd`) |
| `{{CurrentTime}}` | Current UTC time (`HH:mm:ss`) |
| `{{CurrentDateTime}}` | Current UTC date and time (`yyyy-MM-dd HH:mm:ss`) |
| `{{UserName}}` | Current user's display name |
| `{{UserEmail}}` | Current user's email |
| `{{UserAvatarUrl}}` | Current user's avatar URL |
| `{{SpaceName}}` | Current space name |
| `{{SpaceKey}}` | Current space key |

### Plugin Macros

These variables are automatically available when rendering a plugin macro:

| Variable | Description |
|----------|-------------|
| `{{MacroIdOnPage}}` | Unique macro instance identifier on the page |
| `{{PageId}}` | Current page ID |
| `{{PageName}}` | Current page name |
| `{{SpaceKey}}` | Current space key |
| `{{SpaceName}}` | Current space name |
| `{{UserName}}` | Current user's display name |
| `{{UserEmail}}` | Current user's email |
| `{{UserRole}}` | Current user's role (e.g., `Admin`, `User`) |

In addition to the predefined variables, plugin macros receive **user-defined parameters** configured in the macro settings.

## Built-in Helpers

Helpers are functions that transform values. They use the section syntax `{{#Helper}}...{{/Helper}}`.

The following helpers are available in **both** page templates and plugin macros:

### Generators

#### Uuid

Generates a new UUID (v4) without hyphens on each invocation. Useful for unique element IDs in generated HTML:

```mustache
<div id="{{#Uuid}}{{/Uuid}}">
  <p>Content</p>
</div>
```

Each call produces a different UUID, so multiple macros on the same page won't have ID collisions:

```mustache
<div id="tab-{{#Uuid}}{{/Uuid}}" class="tab">Tab 1</div>
<div id="tab-{{#Uuid}}{{/Uuid}}" class="tab">Tab 2</div>
```

Renders to:
```html
<div id="tab-a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4" class="tab">Tab 1</div>
<div id="tab-f7e8d9c0b1a2f7e8d9c0b1a2f7e8d9c0" class="tab">Tab 2</div>
```

### String Transformations

#### ToLower

Converts the rendered content to lowercase:

```mustache
{{#ToLower}}{{SomeParam}}{{/ToLower}}
```

#### ToUpper

Converts the rendered content to uppercase:

```mustache
{{#ToUpper}}{{title}}{{/ToUpper}}
```

### Encoding

#### UrlEncode

URL-encodes the rendered content (useful for query parameters and URLs):

```mustache
<a name="{{#UrlEncode}}{{name}}{{/UrlEncode}}"></a>
```

#### HtmlEncode

HTML-encodes the rendered content (useful for preventing XSS):

```mustache
<iframe src="/space/{{#HtmlEncode}}{{space}}{{/HtmlEncode}}/{{#HtmlEncode}}{{page}}{{/HtmlEncode}}/embedded"></iframe>
```

### Chaining Helpers

Helpers can be chained — the inner helper is applied first:

```json
{
  "Template": "<h4>{{#HtmlEncode}}{{#ToUpper}}{{title}}{{/ToUpper}}{{/HtmlEncode}}</h4>"
}
```

This converts `title` to uppercase first, then HTML-encodes the result.

## Examples

### Simple Variable Substitution

```json
{
  "Template": "<h1>Hello {{name}} {{surname}}</h1>"
}
```

### Using Predefined Variables (Plugin Macro)

```json
{
  "Template": "<div>Page: {{PageName}} in space {{SpaceKey}}</div>"
}
```

### Using Predefined Variables (Page Template)

```mustache
# Meeting notes — {{CurrentDate}}

Author: {{UserName}} ({{UserEmail}})
Space: {{SpaceName}}

## Agenda

- Item 1
- Item 2
```

### Conditional Rendering

```json
{
  "Template": "<div>{{#title}}<h3>{{title}}</h3>{{/title}}<p>{{text}}</p></div>"
}
```

The `<h3>` tag is only rendered if `title` has a value.

### Complete Example: Warning Block Macro

```json
{
  "Template": "<div class=\"warning\">\n{{#title}}<h4>{{#HtmlEncode}}{{#ToUpper}}{{title}}{{/ToUpper}}{{/HtmlEncode}}</h4>{{/title}}\n<span style=\"white-space: pre-wrap;\">{{text}}</span>\n</div>"
}
```

## Configuration

The renderer is registered as a singleton in the DI container:

```csharp
services.AddSingleton<ITemplateRenderer, StubbleTemplateRenderer>();
```

Case-insensitive key lookup is enabled by default:

```csharp
settings.SetIgnoreCaseOnKeyLookup(true);
```

## Limitations

1. **Mustache spec subset**: Only basic Mustache features are supported (variables, sections). Partials, inverted sections (`{{^var}}`), and set-delimiters are not tested and may not work as expected.

2. **Helpers require content**: Due to Stubble's lambda signature, transformation helpers must wrap some content — `{{#Helper}}value{{/Helper}}`. Helpers that don't need input (like `Uuid`) can be called with empty content: `{{#Uuid}}{{/Uuid}}`.

3. **No custom helper registration**: Built-in helpers are hardcoded in `StubbleTemplateRenderer`. Users cannot register their own helpers.

4. **Case-insensitive keys**: Key lookup is case-insensitive by default (`{{name}}` and `{{Name}}` resolve to the same value).

5. **No template inheritance**: Mustache's block inheritance feature is not supported.

6. **Async-only rendering**: Templates are rendered asynchronously via `RenderAsync`. Synchronous rendering is not available.

## Source

- Implementation: `src/Mimisbrunnr.Web.Infrastructure/StubbleTemplateRenderer.cs`
- Interface: `src/Mimisbrunnr.Web.Infrastructure/ITemplateRenderer.cs`
