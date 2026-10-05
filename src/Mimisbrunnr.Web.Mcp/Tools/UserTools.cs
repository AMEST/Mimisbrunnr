using System.ComponentModel;
using Mimisbrunnr.Web.Mcp.Internal;
using Mimisbrunnr.Web.User;
using ModelContextProtocol.Server;

namespace Mimisbrunnr.Web.Mcp.Tools;

/// <summary>
/// MCP tools for reading users.
/// </summary>
[McpServerToolType]
public sealed class UserTools
{
    private readonly IUserService _users;
    private readonly ToolContext _context;
    private readonly ToolExecutor _executor;

    public UserTools(IUserService users, ToolContext context, ToolExecutor executor)
    {
        _users = users;
        _context = context;
        _executor = executor;
    }

    [McpServerTool(Name = "list_users", ReadOnly = true, Idempotent = true)]
    [Description("Returns the list of wiki users.")]
    public Task<string> ListUsers(
        [Description("Pagination offset.")] int? offset = null)
        => _executor.Execute("list_users", async () =>
        {
            var users = await _users.GetUsers(_context.GetUser(), offset);
            return ToolJson.Serialize(users);
        });
}
