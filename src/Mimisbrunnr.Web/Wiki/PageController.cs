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
public class PageController : ControllerBase 
{
    private readonly IPageService _pageService;

    public PageController(IPageService pageService)
    {
        _pageService = pageService;
    }

    [HttpGet("{pageId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PageModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Get([FromRoute] string pageId)
    {
        return Ok(await _pageService.GetById(pageId, User?.ToInfo()));
    }
    
    [HttpGet("{pageId}/tree")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PageTreeModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetTree([FromRoute] string pageId)
    {
        return Ok(await _pageService.GetPageTreeByPageId(pageId, User?.ToInfo()));
    }

    [HttpGet("{pageId}/versions")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PageVersionsListModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetVersions([FromRoute] string pageId)
    {
        return Ok(await _pageService.GetPageVersions(pageId, User?.ToInfo()));
    }
    
    
    [HttpPost]
    [ProducesResponseType(typeof(PageModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Create([FromBody] PageCreateModel createModel)
    {
        return Ok(await _pageService.Create(createModel, User?.ToInfo()));
    }    
    
    [HttpPut("{pageId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update([FromRoute] string pageId, [FromBody] PageUpdateModel updateModel)
    {
        await _pageService.Update(pageId, updateModel, User?.ToInfo());
        return Ok();
    }

    [HttpPut("{pageId}/versions/{version}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RestoreVersion([FromRoute] string pageId, [FromRoute] long version)
    {
        await _pageService.RestoreVersion(pageId, version, User?.ToInfo());
        return Ok();
    }
    

    [HttpPut("{pageId}/editor-type")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateEditorType([FromRoute] string pageId, [FromBody] PageEditorTypeUpdateModel updateModel)
    {
        await _pageService.UpdateEditorType(pageId, updateModel, User?.ToInfo());
        return Ok();
    }
    
    [HttpDelete("{pageId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete([FromRoute] string pageId, [FromQuery] bool recursively = false)
    {
        await _pageService.Delete(pageId, User?.ToInfo(), recursively);
        return Ok();
    }

    [HttpDelete("{pageId}/versions/{version}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteVersion([FromRoute] string pageId, [FromRoute] long version)
    {
        await _pageService.DeleteVersion(pageId, version, User?.ToInfo());
        return Ok();
    }
    
    [HttpPost("copy/{sourcePageId}/{destinationParentPageId}")]
    [ProducesResponseType(typeof(PageModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Copy([FromRoute] string sourcePageId, [FromRoute] string destinationParentPageId)
    {
        return Ok(await _pageService.Copy(sourcePageId, destinationParentPageId, User?.ToInfo()));
    }
    
    [HttpPost("move/{sourcePageId}/{destinationParentPageId}")]
    [ProducesResponseType(typeof(PageModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Move([FromRoute] string sourcePageId, [FromRoute] string destinationParentPageId)
    {
        return Ok(await _pageService.Move(sourcePageId, destinationParentPageId, User?.ToInfo()));
    }
}