using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Integration.Favorites;

public abstract class FavoriteFindModel
{
}

public class FavoriteUserFindModel : FavoriteFindModel
{
    [Required]
    [EmailAddress]
    public string UserEmail { get; set; }
}

public class FavoritePageFindModel : FavoriteFindModel
{
    [Required]
    public string PageId { get; set; }
}

public class FavoriteSpaceFindModel : FavoriteFindModel
{
    [Required]
    public string SpaceKey { get; set; }
}