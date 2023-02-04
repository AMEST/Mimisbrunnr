using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Integration.Favorites;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Web.Mapping;

namespace Mimisbrunnr.Web.Favorites;

[Route("api/[controller]")]
[HandleFavoritesErrors]
[ApiController]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favorites;

    public FavoritesController(IFavoriteService favorites)
    {
        _favorites = favorites;
    }

    [HttpGet]
    [ProducesResponseType(typeof(FavoriteModel[]), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetAll([FromQuery] FavoriteFilterModel filter)
    {
        var user = User.ToInfo();
        return Ok(await _favorites.GetFavorites(filter, user));
    }

    [HttpPost]
    [ProducesResponseType(typeof(FavoriteModel), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Add([FromBody] FavoriteCreateModel favorite)
    {
        var user = User.ToInfo();
        return Ok(await _favorites.Add(favorite, user));
    }

    [HttpPost("exists")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Exists([FromBody] FavoriteCreateModel favorite)
    {
        var user = User.ToInfo();
        if (await _favorites.EnsureInFavorites(favorite, user))
            return Ok();
        return NotFound();
    }


    [HttpDelete("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Remove([FromRoute] string id)
    {
        var user = User.ToInfo();
        await _favorites.Remove(id, user);
        return Ok();
    }
}