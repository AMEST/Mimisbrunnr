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
        return Ok(await _pageService.GetById(pageId, UserMapper.Instance.ToInfo(User)));
    }
    
    [HttpGet("{pageId}/tree")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PageTreeModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetTree([FromRoute] string pageId)
    {
        return Ok(await _pageService.GetPageTreeByPageId(pageId, UserMapper.Instance.ToInfo(User)));
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(PageModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Create([FromBody] PageCreateModel createModel)
    {
        return Ok(await _pageService.Create(createModel, UserMapper.Instance.ToInfo(User)));
    }    
    
    [HttpPut("{pageId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update([FromRoute] string pageId, [FromBody] PageUpdateModel updateModel)
    {
        await _pageService.Update(pageId, updateModel, UserMapper.Instance.ToInfo(User));
        return Ok();
    }
    
    [HttpDelete("{pageId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete([FromRoute] string pageId, [FromQuery] bool recursively = false)
    {
        await _pageService.Delete(pageId, UserMapper.Instance.ToInfo(User), recursively);
        return Ok();
    }
    
    [HttpPost("copy/{sourcePageId}/{destinationParentPageId}")]
    [ProducesResponseType(typeof(PageModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Copy([FromRoute] string sourcePageId, [FromRoute] string destinationParentPageId)
    {
        return Ok(await _pageService.Copy(sourcePageId, destinationParentPageId, UserMapper.Instance.ToInfo(User)));
    }
    
    [HttpPost("move/{sourcePageId}/{destinationParentPageId}")]
    [ProducesResponseType(typeof(PageModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Move([FromRoute] string sourcePageId, [FromRoute] string destinationParentPageId)
    {
        return Ok(await _pageService.Move(sourcePageId, destinationParentPageId, UserMapper.Instance.ToInfo(User)));
    }
}