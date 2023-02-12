using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Integration.Favorites;

public abstract class FavoriteCreateModel
{
}

public class FavoriteUserCreateModel : FavoriteCreateModel
{
    [Required]
    [EmailAddress]
    public string UserEmail { get; set; }
}

public class FavoritePageCreateModel : FavoriteCreateModel
{
    [Required]
    public string PageId { get; set; }
}

public class FavoriteSpaceCreateModel : FavoriteCreateModel
{
    [Required]
    public string SpaceKey { get; set; }
}