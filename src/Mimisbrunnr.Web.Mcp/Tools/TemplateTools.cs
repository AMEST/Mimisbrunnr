using System.ComponentModel;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Web.Mcp.Internal;
using Mimisbrunnr.Web.PageTemplates;
using Mimisbrunnr.Web.Wiki;
using ModelContextProtocol.Server;

namespace Mimisbrunnr.Web.Mcp.Tools;

/// <summary>
/// MCP tools for working with page templates.
/// </summary>
[McpServerToolType]
public sealed class TemplateTools
{
    private readonly IPageTemplateService _templates;
    private readonly IPageService _pages;
    private readonly ToolContext _context;
    private readonly ToolExecutor _executor;

    public TemplateTools(
        IPageTemplateService templates,
        IPageService pages,
        ToolContext context,
        ToolExecutor executor)
    {
        _templates = templates;
        _pages = pages;
        _context = context;
        _executor = executor;
    }

    [McpServerTool(Name = "list_page_templates", ReadOnly = true, Idempotent = true)]
    [Description("Returns page templates filtered by optional type (System, User, Space) and space key.")]
    public Task<string> ListPageTemplates(
        [Description("Optional template type filter: System, User or Space.")] string type = null,
        [Description("Optional space key filter.")] string spaceKey = null)
        => _executor.Execute("list_page_templates", async () =>
        {
            var templates = await _templates.GetAll(type, spaceKey, _context.GetUser());
            return ToolJson.Serialize(templates);
        });

    [McpServerTool(Name = "create_page_from_template")]
    [Description("Creates a new page from a page template, rendering the template in the space context. Requires page creation permission.")]
    public Task<string> CreatePageFromTemplate(
        [Description("Template identifier.")] string templateId,
        [Description("Space key where the page will be created.")] string spaceKey,
        [Description("Identifier of the parent page.")] string parentPageId,
        [Description("Optional page name. Defaults to the template name.")] string name = null)
        => _executor.Execute("create_page_from_template", async () =>
        {
            ToolValidation.EnsureNotEmpty(templateId, nameof(templateId));
            ToolValidation.EnsureNotEmpty(spaceKey, nameof(spaceKey));
            ToolValidation.EnsureNotEmpty(parentPageId, nameof(parentPageId));

            var user = _context.GetUser();
            var rendered = await _templates.Render(templateId, spaceKey, user);

            var pageName = name;
            if (string.IsNullOrWhiteSpace(pageName))
            {
                var template = await _templates.GetById(templateId, user);
                pageName = template.Name;
            }

            var model = new PageCreateModel
            {
                SpaceKey = spaceKey,
                ParentPageId = parentPageId,
                Name = pageName,
                Content = rendered.Content
            };
            ToolValidation.EnsureValid(model);

            var page = await _pages.Create(model, user);
            return ToolJson.Serialize(page);
        });
}
