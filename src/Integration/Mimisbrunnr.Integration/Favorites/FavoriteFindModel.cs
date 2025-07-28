using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Integration.Favorites;

/// <summary>
/// Base model for finding favorite items
/// </summary>
public abstract class FavoriteFindModel
{
}

/// <summary>
/// Model for finding a user favorite
/// </summary>
public class FavoriteUserFindModel : FavoriteFindModel
{
    [Required]
    [EmailAddress]
    /// <summary>
    /// Gets or sets the email of the user to find in favorites
    /// </summary>
    public string UserEmail { get; set; }
}

/// <summary>
/// Model for finding a page favorite
/// </summary>
public class FavoritePageFindModel : FavoriteFindModel
{
    [Required]
    /// <summary>
    /// Gets or sets the ID of the page to find in favorites
    /// </summary>
    public string PageId { get; set; }
}

/// <summary>
/// Model for finding a space favorite
/// </summary>
public class FavoriteSpaceFindModel : FavoriteFindModel
{
    [Required]
    /// <summary>
    /// Gets or sets the key of the space to find in favorites
    /// </summary>
    public string SpaceKey { get; set; }
}
