using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Integration.User;
using Mimisbrunnr.Integration.Wiki;

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
    public async Task<IEnumerable<SpaceModel>> SearchSpaces([FromQuery] string search)
    {
        if (string.IsNullOrEmpty(search))
            return Array.Empty<SpaceModel>();

        return await _searchService.SearchSpaces(search, User?.ToEntity());
    }

    [HttpGet("page")]
    [ProducesResponseType(typeof(IEnumerable<PageModel>), 200)]
    [ProducesResponseType(401)]
    public async Task<IEnumerable<PageModel>> SearchPages([FromQuery] string search)
    {
        if (string.IsNullOrEmpty(search))
            return Array.Empty<PageModel>();
            
        return await _searchService.SearchPages(search, User?.ToEntity());
    }

    [HttpGet("user")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<UserModel>), 200)]
    [ProducesResponseType(401)]
    public async Task<IEnumerable<UserModel>> SearchUsers([FromQuery] string search)
    {
        if (string.IsNullOrEmpty(search))
            return Array.Empty<UserModel>();
            
        return await _searchService.SearchUsers(search, User?.ToEntity());
    }
}
