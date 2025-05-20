using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Web.Mapping;

namespace Mimisbrunnr.Web.Feed;

/// <summary>
/// Controller for managing page update feeds
/// </summary>
[Route("api/[controller]")]
[ApiController]
[HandleWikiErrors]
[Authorize]
public class FeedController : ControllerBase 
{
    private readonly IFeedService _feedService;

    public FeedController(IFeedService feedService)
    {
        _feedService = feedService;
    }

    /// <summary>
    /// Get all page updates
    /// </summary>
    /// <returns>List of page update events</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PageUpdateEventModel[]), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Get()
    {
        return Ok(await _feedService.GetPageUpdates(User?.ToInfo()));
    }

    /// <summary>
    /// Get page updates filtered by user email
    /// </summary>
    /// <param name="emailFilter">Email to filter updates by</param>
    /// <returns>List of filtered page update events</returns>
    [HttpGet("{emailFilter}")]
    [ProducesResponseType(typeof(PageUpdateEventModel[]), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Get([FromRoute] string emailFilter)
    {
        return Ok(await _feedService.GetPageUpdates(User?.ToInfo(), emailFilter));
    }
}
