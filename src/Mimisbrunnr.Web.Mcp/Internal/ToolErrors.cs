using Mimisbrunnr.Integration.Group;
using Mimisbrunnr.Integration.User;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Wiki.Contracts;
using ModelContextProtocol;

namespace Mimisbrunnr.Web.Mcp.Internal;

/// <summary>
/// Maps domain exceptions to safe MCP error messages.
/// </summary>
internal static class ToolErrors
{
    public static McpException Map(Exception exception) => exception switch
    {
        AttachmentTooLargeException => new McpException($"attachment_too_large: {exception.Message}"),
        PageVersionNotFoundException => new McpException("version_not_found: Page version not found."),
        PageNotFoundException => new McpException("page_not_found: Page not found."),
        SpaceNotFoundException => new McpException("space_not_found: Space not found."),
        CommentNotFoundException => new McpException("comment_not_found: Comment not found."),
        GroupNotFoundException => new McpException("principal_not_found: Group not found."),
        UserNotFoundException => new McpException("principal_not_found: User not found."),
        UserHasNotPermissionException => new McpException("forbidden: Insufficient permissions."),
        AnonymousNotAllowedException => new McpException("forbidden: Anonymous access is not allowed."),
        UnauthorizedAccessException => new McpException("forbidden: Insufficient permissions."),
        ArgumentNullException or ArgumentException => new McpException($"invalid_argument: {exception.Message}"),
        _ => new McpException("internal_error: An internal error occurred.")
    };
}
