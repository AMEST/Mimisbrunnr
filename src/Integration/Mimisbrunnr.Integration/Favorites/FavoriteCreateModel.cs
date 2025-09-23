using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Integration.Favorites;

/// <summary>
/// Base model for creating favorite items
/// </summary>
public abstract class FavoriteCreateModel
{
}

/// <summary>
/// Model for creating a user favorite
/// </summary>
public class FavoriteUserCreateModel : FavoriteCreateModel
{
    [Required]
    [EmailAddress]
    /// <summary>
    /// Gets or sets the email of the user to favorite
    /// </summary>
    public string UserEmail { get; set; }
}

/// <summary>
/// Model for creating a page favorite
/// </summary>
public class FavoritePageCreateModel : FavoriteCreateModel
{
    [Required]
    /// <summary>
    /// Gets or sets the ID of the page to favorite
    /// </summary>
    public string PageId { get; set; }
}

/// <summary>
/// Model for creating a space favorite
/// </summary>
public class FavoriteSpaceCreateModel : FavoriteCreateModel
{
    [Required]
    /// <summary>
    /// Gets or sets the key of the space to favorite
    /// </summary>
    public string SpaceKey { get; set; }
}
