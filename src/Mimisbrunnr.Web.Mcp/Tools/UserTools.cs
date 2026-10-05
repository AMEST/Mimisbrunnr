using System.ComponentModel;
using Mimisbrunnr.Integration.User;
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

    [McpServerTool(Name = "get_current_user", ReadOnly = true, Idempotent = true)]
    [Description("Returns the current authenticated user's email, display name, avatar URL, admin status and whether the account is enabled.")]
    public Task<string> GetCurrentUser()
        => _executor.Execute("get_current_user", async () =>
        {
            var user = await _users.GetCurrent(_context.GetUser());
            if (user is null)
                throw new UserNotFoundException();

            return ToolJson.Serialize(user);
        });

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
