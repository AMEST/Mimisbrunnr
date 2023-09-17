using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Web.Mapping;

namespace Mimisbrunnr.Web.Wiki;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[HandleWikiErrors]
public class DraftController : ControllerBase
{
    private readonly IDraftService _draftService;

    public DraftController(IDraftService draftService)
    {
        _draftService = draftService;
    }

    [HttpGet("{pageId}")]
    [ProducesResponseType(typeof(DraftModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Get([FromRoute] string pageId)
    {
        var draft = await _draftService.GetByPageId(pageId, User?.ToInfo());
        if(draft is null)
            return NotFound();

        return Ok(draft);
    }

    [HttpPut("{pageId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateOrCreate([FromRoute] string pageId, [FromBody] DraftUpdateModel updateModel)
    {
        await _draftService.Update(pageId, updateModel, User?.ToInfo());
        return Ok();
    }

    [HttpDelete("{pageId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete([FromRoute] string pageId)
    {
        await _draftService.Delete(pageId, User?.ToInfo());
        return Ok();
    }
}