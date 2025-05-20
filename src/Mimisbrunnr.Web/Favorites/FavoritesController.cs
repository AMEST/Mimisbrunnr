using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Integration.Favorites;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Web.Mapping;

namespace Mimisbrunnr.Web.Favorites;

/// <summary>
/// Controller for managing user favorites
/// </summary>
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

    /// <summary>
    /// Get all favorites for the current user
    /// </summary>
    /// <param name="filter">Filter criteria for favorites</param>
    /// <returns>List of favorites</returns>
    [HttpGet]
    [ProducesResponseType(typeof(FavoriteModel[]), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetAll([FromQuery] FavoriteFilterModel filter)
    {
        var user = User.ToInfo();
        return Ok(await _favorites.GetFavorites(filter, user));
    }

    /// <summary>
    /// Add a new favorite
    /// </summary>
    /// <param name="favorite">Favorite to add</param>
    /// <returns>The added favorite</returns>
    [HttpPost]
    [ProducesResponseType(typeof(FavoriteModel), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Add([FromBody] FavoriteCreateModel favorite)
    {
        var user = User.ToInfo();
        return Ok(await _favorites.Add(favorite, user));
    }

    /// <summary>
    /// Find a specific favorite
    /// </summary>
    /// <param name="findModel">Criteria to find the favorite</param>
    /// <returns>The found favorite if exists</returns>
    [HttpPost("findOne")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> FindOne([FromBody] FavoriteFindModel findModel)
    {
        var user = User.ToInfo();
        var favorite = await _favorites.GetFavorite(findModel, user);
        if (favorite is not null)
            return Ok(favorite);
        return NotFound();
    }

    /// <summary>
    /// Check if a favorite exists
    /// </summary>
    /// <param name="findModel">Criteria to check for the favorite</param>
    /// <returns>200 if exists, 404 if not</returns>
    [HttpPost("exists")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Exists([FromBody] FavoriteFindModel findModel)
    {
        var user = User.ToInfo();
        if (await _favorites.EnsureInFavorites(findModel, user))
            return Ok();
        return NotFound();
    }

    /// <summary>
    /// Remove a favorite
    /// </summary>
    /// <param name="id">ID of the favorite to remove</param>
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
