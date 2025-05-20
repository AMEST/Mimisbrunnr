using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Integration.Wiki;

namespace Mimisbrunnr.Web.Wiki;

/// <summary>
/// Controller for managing wiki pages
/// </summary>
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

    /// <summary>
    /// Get a page by its ID
    /// </summary>
    /// <param name="pageId">The ID of the page to retrieve</param>
    /// <returns>The requested page</returns>
    [HttpGet("{pageId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PageModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Get([FromRoute] string pageId)
    {
        return Ok(await _pageService.GetById(pageId, User?.ToInfo()));
    }
    
    /// <summary>
    /// Get the page tree structure starting from the specified page
    /// </summary>
    /// <param name="pageId">The ID of the root page</param>
    /// <returns>The page tree structure</returns>
    [HttpGet("{pageId}/tree")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PageTreeModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetTree([FromRoute] string pageId)
    {
        return Ok(await _pageService.GetPageTreeByPageId(pageId, User?.ToInfo()));
    }

    /// <summary>
    /// Get version history for a page
    /// </summary>
    /// <param name="pageId">The ID of the page</param>
    /// <returns>List of page versions</returns>
    [HttpGet("{pageId}/versions")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PageVersionsListModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetVersions([FromRoute] string pageId)
    {
        return Ok(await _pageService.GetPageVersions(pageId, User?.ToInfo()));
    }
    
    /// <summary>
    /// Create a new page
    /// </summary>
    /// <param name="createModel">The page creation data</param>
    /// <returns>The created page</returns>
    [HttpPost]
    [ProducesResponseType(typeof(PageModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Create([FromBody] PageCreateModel createModel)
    {
        return Ok(await _pageService.Create(createModel, User?.ToInfo()));
    }    
    
    /// <summary>
    /// Update an existing page
    /// </summary>
    /// <param name="pageId">The ID of the page to update</param>
    /// <param name="updateModel">The page update data</param>
    [HttpPut("{pageId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update([FromRoute] string pageId, [FromBody] PageUpdateModel updateModel)
    {
        await _pageService.Update(pageId, updateModel, User?.ToInfo());
        return Ok();
    }

    /// <summary>
    /// Restore a specific version of a page
    /// </summary>
    /// <param name="pageId">The ID of the page</param>
    /// <param name="version">The version number to restore</param>
    [HttpPut("{pageId}/versions/{version}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RestoreVersion([FromRoute] string pageId, [FromRoute] long version)
    {
        await _pageService.RestoreVersion(pageId, version, User?.ToInfo());
        return Ok();
    }
    
    /// <summary>
    /// Delete a page
    /// </summary>
    /// <param name="pageId">The ID of the page to delete</param>
    /// <param name="recursively">Whether to delete child pages</param>
    [HttpDelete("{pageId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete([FromRoute] string pageId, [FromQuery] bool recursively = false)
    {
        await _pageService.Delete(pageId, User?.ToInfo(), recursively);
        return Ok();
    }

    /// <summary>
    /// Delete a specific version of a page
    /// </summary>
    /// <param name="pageId">The ID of the page</param>
    /// <param name="version">The version number to delete</param>
    [HttpDelete("{pageId}/versions/{version}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteVersion([FromRoute] string pageId, [FromRoute] long version)
    {
        await _pageService.DeleteVersion(pageId, version, User?.ToInfo());
        return Ok();
    }
    
    /// <summary>
    /// Copy a page to a new location
    /// </summary>
    /// <param name="sourcePageId">The ID of the page to copy</param>
    /// <param name="destinationParentPageId">The ID of the destination parent page</param>
    /// <returns>The copied page</returns>
    [HttpPost("copy/{sourcePageId}/{destinationParentPageId}")]
    [ProducesResponseType(typeof(PageModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Copy([FromRoute] string sourcePageId, [FromRoute] string destinationParentPageId)
    {
        return Ok(await _pageService.Copy(sourcePageId, destinationParentPageId, User?.ToInfo()));
    }
    
    /// <summary>
    /// Move a page to a new location
    /// </summary>
    /// <param name="sourcePageId">The ID of the page to move</param>
    /// <param name="destinationParentPageId">The ID of the destination parent page</param>
    /// <param name="withChilds">Whether to move child pages</param>
    /// <returns>The moved page</returns>
    [HttpPost("move/{sourcePageId}/{destinationParentPageId}")]
    [ProducesResponseType(typeof(PageModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Move([FromRoute] string sourcePageId, [FromRoute] string destinationParentPageId, [FromQuery] bool? withChilds = null)
    {
        return Ok(await _pageService.Move(sourcePageId, destinationParentPageId, withChilds ?? true, User?.ToInfo()));
    }
}
