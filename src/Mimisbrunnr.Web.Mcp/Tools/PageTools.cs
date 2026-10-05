using System.ComponentModel;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Web.Mcp.Internal;
using Mimisbrunnr.Web.Wiki;
using ModelContextProtocol.Server;

namespace Mimisbrunnr.Web.Mcp.Tools;

/// <summary>
/// MCP tools for reading and editing wiki pages, including version history.
/// </summary>
[McpServerToolType]
public sealed class PageTools
{
    private readonly IPageService _pages;
    private readonly ToolContext _context;
    private readonly ToolExecutor _executor;

    public PageTools(IPageService pages, ToolContext context, ToolExecutor executor)
    {
        _pages = pages;
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
