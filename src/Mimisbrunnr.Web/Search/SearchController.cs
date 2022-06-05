using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Web.Wiki;

namespace Mimisbrunnr.Web.Search;

[Route("api/[controller]")]
[ApiController]
[HandleSearchErrors]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet("space")]
    [ProducesResponseType(typeof(IEnumerable<SpaceModel>), 200)]
    [ProducesResponseType(401)]
    public Task<IEnumerable<SpaceModel>> SearchSpace([FromQuery] string search)
    {
        return _searchService.SearchSpaces(search, User?.ToEntity());
    }

    [HttpGet("page")]
    [ProducesResponseType(typeof(IEnumerable<PageModel>), 200)]
    [ProducesResponseType(401)]
    public Task<IEnumerable<PageModel>> SearchPage([FromQuery] string search)
    {
        return _searchService.SearchPages(search, User?.ToEntity());
    }
}
