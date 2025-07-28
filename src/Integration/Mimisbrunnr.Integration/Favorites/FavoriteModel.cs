using System.ComponentModel.DataAnnotations;
using Mimisbrunnr.Integration.User;
using Mimisbrunnr.Integration.Wiki;

namespace Mimisbrunnr.Integration.Favorites;

/// <summary>
/// Base model for favorite items
/// </summary>
public abstract class FavoriteModel
{
    /// <summary>
    /// Gets or sets the unique identifier of the favorite
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the favorite was created
    /// </summary>
    public DateTime Created { get; set; }
}

/// <summary>
/// Model representing a favorited user
/// </summary>
public class FavoriteUserModel : FavoriteModel
{
    /// <summary>
    /// Gets or sets the user that was favorited
    /// </summary>
    public UserModel User { get; set; }
}

/// <summary>
/// Model representing a favorited page
/// </summary>
public class FavoritePageModel : FavoriteModel
{
    /// <summary>
    /// Gets or sets the page that was favorited
    /// </summary>
    public PageModel Page { get; set; }
}

/// <summary>
/// Model representing a favorited space
/// </summary>
public class FavoriteSpaceModel : FavoriteModel
{
    /// <summary>
    /// Gets or sets the space that was favorited
    /// </summary>
    public SpaceModel Space { get; set; }
}
