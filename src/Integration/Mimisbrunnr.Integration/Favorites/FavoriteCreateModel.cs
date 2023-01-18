namespace Mimisbrunnr.Integration.Favorites;

public abstract class FavoriteCreateModel
{
}

public class FavoriteUserCreateModel : FavoriteCreateModel
{
    public string UserEmail { get; set; }
}

public class FavoritePageCreateModel : FavoriteCreateModel
{
    public string PageId { get; set; }
}

public class FavoriteSpaceCreateModel : FavoriteCreateModel
{
    public string SpaceKey { get; set; }
}