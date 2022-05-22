using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Web.Mapping;

namespace Mimisbrunnr.Web.Feed;

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

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PageUpdateEventModel[]), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Get()
    {
        return Ok(await _feedService.GetPageUpdates(User?.ToEntity()));
    }

    [HttpGet("{emailFilter}")]
    [ProducesResponseType(typeof(PageUpdateEventModel[]), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Get([FromRoute] string emailFilter)
    {
        return Ok(await _feedService.GetPageUpdates(User?.ToEntity(), emailFilter));
    }
}