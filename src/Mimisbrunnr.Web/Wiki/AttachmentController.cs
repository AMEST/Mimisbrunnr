using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Integration.Wiki;

namespace Mimisbrunnr.Web.Wiki;

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

    [HttpGet("{pageId}")]
    [ProducesResponseType(typeof(AttachmentModel[]), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAll([FromRoute] string pageId)
    {
        var attachments = await _attachmentService.GetAttachments(pageId, User?.ToEntity());
        if (attachments is null)
            return Ok(Array.Empty<AttachmentModel>());

        return Ok(attachments);
    }

    [HttpGet("{pageId}/{name}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Stream), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetContent([FromRoute] string pageId, [FromRoute] string name)
    {
        var attachmentContent = await _attachmentService.GetAttachmentContent(pageId, name, User?.ToEntity());
        if (attachmentContent is null)
            return NotFound();
        return File(attachmentContent, MimeTypes.GetMimeType(name));
    }

    [HttpPost("{pageId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> Upload([FromRoute] string pageId)
    {
        var uploadedFile = HttpContext.Request.Form.Files.FirstOrDefault();

        using var file = new MemoryStream();
        await uploadedFile.CopyToAsync(file);
        file.Position = 0;

        await _attachmentService.Upload(pageId, file, uploadedFile.FileName, User?.ToEntity());

        return Ok();
    }

    [HttpDelete("{pageId}/{name}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Remove([FromRoute] string pageId, [FromRoute] string name)
    {
        await _attachmentService.Remove(pageId, name, User?.ToEntity());
        return Ok();
    }
}
