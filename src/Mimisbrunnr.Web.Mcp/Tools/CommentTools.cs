using System.ComponentModel;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Web.Mcp.Internal;
using Mimisbrunnr.Web.Wiki;
using ModelContextProtocol.Server;

namespace Mimisbrunnr.Web.Mcp.Tools;

/// <summary>
/// MCP tools for reading and managing page comments.
/// </summary>
[McpServerToolType]
public sealed class CommentTools
{
    private readonly ICommentService _comments;
    private readonly ToolContext _context;
    private readonly ToolExecutor _executor;

    public CommentTools(ICommentService comments, ToolContext context, ToolExecutor executor)
    {
        _comments = comments;
        _context = context;
        _executor = executor;
    }

    [McpServerTool(Name = "list_comments", ReadOnly = true, Idempotent = true)]
    [Description("Returns all comments of a page.")]
    public Task<string> ListComments(
        [Description("Page identifier.")] string pageId)
        => _executor.Execute("list_comments", async () =>
        {
            var comments = await _comments.GetComments(pageId, _context.GetUser());
            return ToolJson.Serialize(comments ?? Enumerable.Empty<CommentModel>());
        });

    [McpServerTool(Name = "create_comment")]
    [Description("Creates a new comment on a page.")]
    public Task<string> CreateComment(
        [Description("Page identifier.")] string pageId,
        [Description("Comment text.")] string message)
        => _executor.Execute("create_comment", async () =>
        {
            ToolValidation.EnsureNotEmpty(message, nameof(message));

            var model = new CommentCreateModel { Message = message };
            ToolValidation.EnsureValid(model);

            var comment = await _comments.Create(pageId, model, _context.GetUser());
            return ToolJson.Serialize(comment);
        });

    [McpServerTool(Name = "delete_comment", Destructive = true, Idempotent = true)]
    [Description("Deletes a comment from a page. Allowed for the comment author or a space admin.")]
    public Task<string> DeleteComment(
        [Description("Page identifier.")] string pageId,
        [Description("Comment identifier.")] string commentId)
        => _executor.Execute("delete_comment", async () =>
        {
            await _comments.Remove(pageId, commentId, _context.GetUser());
            return ToolJson.Ok();
        });
}
