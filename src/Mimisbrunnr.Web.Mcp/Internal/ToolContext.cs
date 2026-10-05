using Microsoft.AspNetCore.Http;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Wiki.Contracts;
using ModelContextProtocol;

namespace Mimisbrunnr.Web.Mcp.Internal;

/// <summary>
/// Resolves the authenticated user of the current MCP request.
/// </summary>
public sealed class ToolContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ToolContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Returns the current user or throws when no authenticated user is available.
    /// </summary>
    public UserInfo GetUser()
    {
        var user = TryGetUser();
        if (user is null)
            throw new McpException("forbidden: No authenticated user for the current MCP request.");

        return user;
    }

    /// <summary>
    /// Returns the current user or <c>null</c> when the request is not authenticated.
    /// </summary>
    public UserInfo TryGetUser() => _httpContextAccessor.HttpContext?.User?.ToInfo();
}
