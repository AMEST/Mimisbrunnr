using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Integration.Wiki;

namespace Mimisbrunnr.Web.Wiki;

/// <summary>
/// Controller for managing page attachments
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[HandleWikiErrors]
public class AttachmentController : ControllerBase
{
    private readonly IAttachmentService _attachmentService;

    public AttachmentController(IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    /// <summary>
    /// Get all attachments for a page
    /// </summary>
    /// <param name="pageId">ID of the page</param>
    /// <returns>List of attachments</returns>
    [HttpGet("{pageId}")]
    [ProducesResponseType(typeof(AttachmentModel[]), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAll([FromRoute] string pageId)
    {
        var attachments = await _attachmentService.GetAttachments(pageId, User?.ToInfo());
        if (attachments is null)
            return Ok(Array.Empty<AttachmentModel>());

        return Ok(attachments);
    }

    /// <summary>
    /// Get attachment content
    /// </summary>
    /// <param name="pageId">ID of the page</param>
    /// <param name="name">Name of the attachment</param>
    /// <returns>Attachment file content</returns>
    [HttpGet("{pageId}/{name}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Stream), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetContent([FromRoute] string pageId, [FromRoute] string name)
    {
        var attachmentContent = await _attachmentService.GetAttachmentContent(pageId, name, User?.ToInfo());
        if (attachmentContent is null)
            return NotFound();
        return File(attachmentContent, MimeTypes.GetMimeType(name));
    }

    /// <summary>
    /// Upload an attachment to a page
    /// </summary>
    /// <param name="pageId">ID of the page</param>
    [HttpPost("{pageId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> Upload([FromRoute] string pageId)
    {
        var uploadedFile = HttpContext.Request.Form.Files.FirstOrDefault();

        await _attachmentService.Upload(pageId, uploadedFile.OpenReadStream(), uploadedFile.FileName, User?.ToInfo());

        return Ok();
    }

    /// <summary>
    /// Remove an attachment from a page
    /// </summary>
    /// <param name="pageId">ID of the page</param>
    /// <param name="name">Name of the attachment</param>
    [HttpDelete("{pageId}/{name}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Remove([FromRoute] string pageId, [FromRoute] string name)
    {
        await _attachmentService.Remove(pageId, name, User?.ToInfo());
        return Ok();
    }
}
