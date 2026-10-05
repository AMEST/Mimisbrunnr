using System.ComponentModel;
using Mimisbrunnr.Web.Mcp.Internal;
using Mimisbrunnr.Web.Search;
using ModelContextProtocol.Server;

namespace Mimisbrunnr.Web.Mcp.Tools;

/// <summary>
/// MCP tools for searching wiki entities with the current user's permissions.
/// </summary>
[McpServerToolType]
public sealed class SearchTools
{
    private readonly ISearchService _search;
    private readonly ToolContext _context;
    private readonly ToolExecutor _executor;

    public SearchTools(ISearchService search, ToolContext context, ToolExecutor executor)
    {
        _search = search;
        _context = context;
        _executor = executor;
    }

    [McpServerTool(Name = "search_pages", ReadOnly = true, Idempotent = true)]
    [Description("Searches wiki pages by title or content. Returns only pages visible to the current user.")]
    public Task<string> SearchPages(
        [Description("Non-empty search query.")] string text)
        => _executor.Execute("search_pages", async () =>
        {
            ToolValidation.EnsureNotEmpty(text, nameof(text));
            var pages = await _search.SearchPages(text, _context.GetUser());
            return ToolJson.Serialize(pages);
        });

    [McpServerTool(Name = "search_spaces", ReadOnly = true, Idempotent = true)]
    [Description("Searches wiki spaces by name. Returns only spaces visible to the current user.")]
    public Task<string> SearchSpaces(
        [Description("Non-empty search query.")] string text)
        => _executor.Execute("search_spaces", async () =>
        {
            ToolValidation.EnsureNotEmpty(text, nameof(text));
            var spaces = await _search.SearchSpaces(text, _context.GetUser());
            return ToolJson.Serialize(spaces);
        });

    [McpServerTool(Name = "search_users", ReadOnly = true, Idempotent = true)]
    [Description("Searches wiki users by name or email.")]
    public Task<string> SearchUsers(
        [Description("Non-empty search query.")] string text)
        => _executor.Execute("search_users", async () =>
        {
            ToolValidation.EnsureNotEmpty(text, nameof(text));
            var users = await _search.SearchUsers(text, _context.GetUser());
            return ToolJson.Serialize(users);
        });
}
