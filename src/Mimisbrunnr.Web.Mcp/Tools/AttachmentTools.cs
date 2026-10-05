using System.ComponentModel;
using Microsoft.AspNetCore.StaticFiles;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Web.Mcp.Internal;
using Mimisbrunnr.Web.Wiki;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace Mimisbrunnr.Web.Mcp.Tools;

/// <summary>
/// MCP tools for working with page attachments.
/// </summary>
[McpServerToolType]
public sealed class AttachmentTools
{
    private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();

    private readonly IAttachmentService _attachments;
    private readonly McpServerConfiguration _configuration;
    private readonly ToolContext _context;
    private readonly ToolExecutor _executor;

    public AttachmentTools(
        IAttachmentService attachments,
        McpServerConfiguration configuration,
        ToolContext context,
        ToolExecutor executor)
    {
        _attachments = attachments;
        _configuration = configuration;
        _context = context;
        _executor = executor;
    }

    [McpServerTool(Name = "list_attachments", ReadOnly = true, Idempotent = true)]
    [Description("Returns the list of attachments of a page.")]
    public Task<string> ListAttachments(
        [Description("Page identifier.")] string pageId)
        => _executor.Execute("list_attachments", async () =>
        {
            var attachments = await _attachments.GetAttachments(pageId, _context.GetUser());
            return ToolJson.Serialize(attachments ?? []);
        });

    [McpServerTool(Name = "get_attachment", ReadOnly = true, Idempotent = true)]
    [Description("Returns the content of a page attachment encoded as base64.")]
    public Task<string> GetAttachment(
        [Description("Page identifier.")] string pageId,
        [Description("Attachment file name.")] string name)
        => _executor.Execute("get_attachment", async () =>
        {
            await using var stream = await _attachments.GetAttachmentContent(
                pageId, name, _context.GetUser(), _configuration.MaxAttachmentBytes);
            if (stream is null)
                throw new McpException("attachment_not_found: Attachment not found.");

            if (stream.CanSeek && stream.Length - stream.Position > _configuration.MaxAttachmentBytes)
                throw new AttachmentTooLargeException(_configuration.MaxAttachmentBytes);

            using var memoryStream = new MemoryStream();
            var buffer = new byte[81920];
            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer)) != 0)
            {
                if (memoryStream.Length + bytesRead > _configuration.MaxAttachmentBytes)
                    throw new AttachmentTooLargeException(_configuration.MaxAttachmentBytes);
                await memoryStream.WriteAsync(buffer.AsMemory(0, bytesRead));
            }

            if (!ContentTypeProvider.TryGetContentType(name, out var contentType))
                contentType = "application/octet-stream";

            return ToolJson.Serialize(new
            {
                name,
                contentType,
                encoding = "base64",
                content = Convert.ToBase64String(memoryStream.ToArray())
            });
        });

    [McpServerTool(Name = "upload_attachment")]
    [Description("Uploads an attachment to a page. Content must be base64 encoded. Requires space edit permission.")]
    public Task<string> UploadAttachment(
        [Description("Page identifier.")] string pageId,
        [Description("Attachment file name.")] string name,
        [Description("Base64 encoded file content.")] string content)
        => _executor.Execute("upload_attachment", async () =>
        {
            ToolValidation.EnsureNotEmpty(name, nameof(name));
            ToolValidation.EnsureNotEmpty(content, nameof(content));

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(content);
            }
            catch (FormatException)
            {
                throw new McpException("invalid_argument: content must be valid base64.");
            }

            if (bytes.LongLength > _configuration.MaxAttachmentBytes)
                throw new McpException(
                    $"attachment_too_large: Attachment exceeds the {_configuration.MaxAttachmentBytes} bytes MCP limit.");

            using var stream = new MemoryStream(bytes);
            await _attachments.Upload(pageId, stream, name, _context.GetUser());
            return ToolJson.Ok();
        });

    [McpServerTool(Name = "delete_attachment", Destructive = true, Idempotent = true)]
    [Description("Deletes an attachment from a page. Requires space edit permission.")]
    public Task<string> DeleteAttachment(
        [Description("Page identifier.")] string pageId,
        [Description("Attachment file name.")] string name)
        => _executor.Execute("delete_attachment", async () =>
        {
            await _attachments.Remove(pageId, name, _context.GetUser());
            return ToolJson.Ok();
        });
}
