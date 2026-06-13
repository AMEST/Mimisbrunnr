# Stubble Template Renderer

The Stubble Template Renderer is a Mustache-based templating engine used in Mimisbrunnr for rendering dynamic content. It is built on the [Stubble.Core](https://github.com/StubbleOrg/Stubble) library.

## Current Usage

Currently, the template renderer is used **exclusively in plugin macros** for rendering macro templates with user-provided parameters and predefined context variables.

> **Coming Soon:** The template renderer will also be available for **page templates** when the page templates feature is implemented.

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

These variables are automatically available in all macro templates:

| Variable | Description |
|----------|-------------|
| `{{PageId}}` | Current page ID |
| `{{PageName}}` | Current page name |
| `{{SpaceKey}}` | Current space key |
| `{{SpaceName}}` | Current space name |
| `{{UserEmail}}` | Current user's email |
| `{{UserName}}` | Current user's display name |
| `{{MacroIdOnPage}}` | Unique macro instance identifier on the page |

## Built-in Helpers

Helpers are functions that transform values. They use the section syntax `{{#Helper}}...{{/Helper}}`.

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

## Examples

### Simple Variable Substitution

```json
{
  "Template": "<h1>Hello {{name}} {{surname}}</h1>"
}
```

### Using Predefined Variables

```json
{
  "Template": "<div>Page: {{PageName}} in space {{SpaceKey}}</div>"
}
```

### Chaining Helpers

Helpers can be chained — the inner helper is applied first:

```json
{
  "Template": "<h4>{{#HtmlEncode}}{{#ToUpper}}{{title}}{{/ToUpper}}{{/HtmlEncode}}</h4>"
}
```

This converts `title` to uppercase first, then HTML-encodes the result.

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

## Limitations

1. **Mustache spec subset**: Only basic Mustache features are supported (variables, sections). Partials, inverted sections (`{{^var}}`), and set-delimiters are not tested and may not work as expected.

2. **Helpers require content**: Due to Stubble's lambda signature, helpers must wrap some content between tags — `{{#Helper}}value{{/Helper}}`. Empty helper calls like `{{#Helper}}{{/Helper}}` work only for helpers that don't need input (e.g., `Now`, `Today`, `CurrentTime`).

3. **No custom helper registration**: Built-in helpers are hardcoded in `StubbleTemplateRenderer`. Users cannot register their own helpers.

4. **Case-insensitive keys**: Key lookup is case-insensitive by default (`{{name}}` and `{{Name}}` resolve to the same value).

5. **No template inheritance**: Mustache's block inheritance feature is not supported.

6. **Async-only rendering**: Templates are rendered asynchronously via `RenderAsync`. Synchronous rendering is not available.

## Configuration

The renderer is registered as a singleton in the DI container:

```csharp
services.AddSingleton<ITemplateRenderer, StubbleTemplateRenderer>();
```

Case-insensitive key lookup is enabled by default:

```csharp
settings.SetIgnoreCaseOnKeyLookup(true);
```

## Source

- Implementation: `src/Mimisbrunnr.Web.Infrastructure/StubbleTemplateRenderer.cs`
- Interface: `src/Mimisbrunnr.Web.Infrastructure/ITemplateRenderer.cs`
