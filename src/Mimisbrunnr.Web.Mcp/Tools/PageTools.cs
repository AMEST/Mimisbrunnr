using System.ComponentModel;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Web.Mcp.Internal;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Web.Wiki;
using ModelContextProtocol.Server;

namespace Mimisbrunnr.Web.Mcp.Tools;

/// <summary>
/// MCP tools for reading, creating and editing wiki pages, including version history.
/// </summary>
[McpServerToolType]
public sealed class PageTools
{
    private readonly IPageService _pages;
    private readonly IPermissionService _permissions;
    private readonly ToolContext _context;
    private readonly ToolExecutor _executor;

    public PageTools(IPageService pages, IPermissionService permissions, ToolContext context, ToolExecutor executor)
    {
        _pages = pages;
        _permissions = permissions;
        _context = context;
        _executor = executor;
    }

    [McpServerTool(Name = "get_page", ReadOnly = true, Idempotent = true)]
    [Description("Returns a wiki page by its identifier, including its Markdown content.")]
    public Task<string> GetPage(
        [Description("Page identifier.")] string pageId)
        => _executor.Execute("get_page", async () =>
        {
            var page = await _pages.GetById(pageId, _context.GetUser());
            return ToolJson.Serialize(page);
        });

    [McpServerTool(Name = "get_page_tree", ReadOnly = true, Idempotent = true)]
    [Description("Returns the page tree starting from the given page: the page itself and all of its descendant pages. Use the space home page id to get the full page tree of a space.")]
    public Task<string> GetPageTree(
        [Description("Identifier of the root page of the tree.")] string pageId)
        => _executor.Execute("get_page_tree", async () =>
        {
            var tree = await _pages.GetPageTreeByPageId(pageId, _context.GetUser());
            return ToolJson.Serialize(tree);
        });

    [McpServerTool(Name = "create_page", Destructive = false)]
    [Description("Creates and returns a new wiki page under a parent page in the specified space. Requires space view and edit permissions.")]
    public Task<string> CreatePage(
        [Description("Space key where the page will be created.")] string spaceKey,
        [Description("Identifier of the parent page in the same space. Use the space home page id for a top-level page.")] string parentPageId,
        [Description("Page name.")] string name,
        [Description("Optional page content (Markdown). Defaults to empty content.")] string content = "")
        => _executor.Execute("create_page", async () =>
        {
            var model = new PageCreateModel
            {
                SpaceKey = spaceKey,
                ParentPageId = parentPageId,
                Name = name,
                Content = content
            };
            ToolValidation.EnsureValid(model);

            var user = _context.GetUser();
            await _permissions.EnsureViewPermission(spaceKey, user);
            var page = await _pages.Create(model, user);
            return ToolJson.Serialize(page);
        });

    [McpServerTool(Name = "update_page")]
    [Description("Updates a page's name and content. Call get_page first to avoid losing the current content. Requires space edit permission.")]
    public Task<string> UpdatePage(
        [Description("Page identifier.")] string pageId,
        [Description("New page name.")] string name,
        [Description("New page content (Markdown).")] string content)
        => _executor.Execute("update_page", async () =>
        {
            ToolValidation.EnsureNotEmpty(name, nameof(name));

            var model = new PageUpdateModel { Name = name, Content = content };
            ToolValidation.EnsureValid(model);

            await _pages.Update(pageId, model, _context.GetUser());
            return ToolJson.Ok();
        });

    [McpServerTool(Name = "get_page_versions", ReadOnly = true, Idempotent = true)]
    [Description("Returns the version history of a page, including the content of each version.")]
    public Task<string> GetPageVersions(
        [Description("Page identifier.")] string pageId)
        => _executor.Execute("get_page_versions", async () =>
        {
            var versions = await _pages.GetPageVersions(pageId, _context.GetUser());
            return ToolJson.Serialize(versions);
        });

    [McpServerTool(Name = "get_page_version", ReadOnly = true, Idempotent = true)]
    [Description("Returns a single historical version of a page, including its content.")]
    public Task<string> GetPageVersion(
        [Description("Page identifier.")] string pageId,
        [Description("Version number to open.")] long version)
        => _executor.Execute("get_page_version", async () =>
        {
            var historicalPage = await _pages.GetVersion(pageId, version, _context.GetUser());
            return ToolJson.Serialize(historicalPage);
        });

    [McpServerTool(Name = "restore_page_version")]
    [Description("Restores a page to the specified historical version. Requires space edit permission.")]
    public Task<string> RestorePageVersion(
        [Description("Page identifier.")] string pageId,
        [Description("Version number to restore.")] long version)
        => _executor.Execute("restore_page_version", async () =>
        {
            await _pages.RestoreVersion(pageId, version, _context.GetUser());
            return ToolJson.Ok();
        });
}
