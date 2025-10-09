using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Integration.User;
using Mimisbrunnr.Integration.Wiki;

namespace Mimisbrunnr.Web.Search;

/// <summary>
/// Controller for searching across different entities
/// </summary>
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

    /// <summary>
    /// Search for spaces by name
    /// </summary>
    /// <param name="search">Search query</param>
    /// <returns>List of matching spaces</returns>
    [HttpGet("space")]
    [ProducesResponseType(typeof(IEnumerable<SpaceModel>), 200)]
    [ProducesResponseType(401)]
    public async Task<IEnumerable<SpaceModel>> SearchSpaces([FromQuery] string search)
    {
        if (string.IsNullOrEmpty(search))
            return [];

        return await _searchService.SearchSpaces(search, User?.ToInfo());
    }

    /// <summary>
    /// Search for pages by title or content
    /// </summary>
    /// <param name="search">Search query</param>
    /// <returns>List of matching pages</returns>
    [HttpGet("page")]
    [ProducesResponseType(typeof(IEnumerable<PageModel>), 200)]
    [ProducesResponseType(401)]
    public async Task<IEnumerable<PageModel>> SearchPages([FromQuery] string search)
    {
        if (string.IsNullOrEmpty(search))
            return [];
            
        return await _searchService.SearchPages(search, User?.ToInfo());
    }

    /// <summary>
    /// Search for users by name or email
    /// </summary>
    /// <param name="search">Search query</param>
    /// <returns>List of matching users</returns>
    [HttpGet("user")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<UserModel>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> SearchUsers([FromQuery] string search)
    {
        if (string.IsNullOrEmpty(search))
            return Ok(Array.Empty<UserViewModel>());
        return Ok(await _searchService.SearchUsers(search, User?.ToInfo()));
    }
}
