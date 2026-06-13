using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Integration.PageTemplates;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Web.Mapping;

namespace Mimisbrunnr.Web.PageTemplates;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[HandlePageTemplateErrors]
public class PageTemplateController : ControllerBase
{
    private readonly IPageTemplateService _pageTemplateService;

    public PageTemplateController(IPageTemplateService pageTemplateService)
    {
        _pageTemplateService = pageTemplateService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PageTemplateModel[]), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetAll([FromQuery] string? type = null, [FromQuery] string? spaceKey = null)
    {
        var user = User?.ToInfo();
        var result = await _pageTemplateService.GetAll(type, spaceKey, user);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PageTemplateModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(string id)
    {
        var user = User?.ToInfo();
        var result = await _pageTemplateService.GetById(id, user);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PageTemplateModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Create([FromBody] PageTemplateCreateModel model)
    {
        var user = User?.ToInfo();
        var result = await _pageTemplateService.Create(model, user);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(string id, [FromBody] PageTemplateUpdateModel model)
    {
        var user = User?.ToInfo();
        await _pageTemplateService.Update(id, model, user);
        return Ok();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(string id)
    {
        var user = User?.ToInfo();
        await _pageTemplateService.Delete(id, user);
        return Ok();
    }

    [HttpPost("{id}/render")]
    [ProducesResponseType(typeof(PageTemplateRenderResponse), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Render(string id, [FromBody] PageTemplateRenderRequest request)
    {
        var user = User?.ToInfo();
        var result = await _pageTemplateService.Render(id, request.SpaceKey, user);
        return Ok(result);
    }
}
