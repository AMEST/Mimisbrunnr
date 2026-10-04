using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Integration.PageTemplates;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Web.Mapping;

namespace Mimisbrunnr.Web.PageTemplates;

/// <summary>
/// API controller for managing page templates
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[HandlePageTemplateErrors]
public class PageTemplateController : ControllerBase
{
    private readonly IPageTemplateService _pageTemplateService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageTemplateController"/> class
    /// </summary>
    /// <param name="pageTemplateService">Service providing page template operations</param>
    public PageTemplateController(IPageTemplateService pageTemplateService)
    {
        _pageTemplateService = pageTemplateService;
    }

    /// <summary>
    /// Retrieves page templates filtered by type and space
    /// </summary>
    /// <param name="type">The template type filter</param>
    /// <param name="spaceKey">The key of the space to filter templates by</param>
    /// <returns>An action result containing the array of page template models</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PageTemplateModel[]), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetAll([FromQuery] string? type = null, [FromQuery] string? spaceKey = null)
    {
        var user = User?.ToInfo();
        var result = await _pageTemplateService.GetAll(type, spaceKey, user);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a page template by its unique identifier
    /// </summary>
    /// <param name="id">Unique identifier of the page template</param>
    /// <returns>An action result containing the page template model</returns>
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

    /// <summary>
    /// Creates a new page template
    /// </summary>
    /// <param name="model">Model containing the page template details</param>
    /// <returns>An action result containing the created page template model</returns>
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

    /// <summary>
    /// Updates an existing page template
    /// </summary>
    /// <param name="id">Unique identifier of the page template to update</param>
    /// <param name="model">Model containing the updated page template details</param>
    /// <returns>An action result indicating the outcome of the operation</returns>
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

    /// <summary>
    /// Deletes a page template
    /// </summary>
    /// <param name="id">Unique identifier of the page template to delete</param>
    /// <returns>An action result indicating the outcome of the operation</returns>
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

    /// <summary>
    /// Renders a page template in the context of a space
    /// </summary>
    /// <param name="id">Unique identifier of the page template to render</param>
    /// <param name="request">Request containing the rendering context</param>
    /// <returns>An action result containing the rendered page template response</returns>
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
