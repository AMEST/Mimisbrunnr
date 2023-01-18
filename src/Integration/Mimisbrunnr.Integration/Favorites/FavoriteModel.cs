namespace Mimisbrunnr.Integration.Favorites;

public abstract class FavoriteModel
{
    public string Id { get; set; }
}

public class FavoriteUserModel : FavoriteModel
{
    public string UserEmail { get; set; }
}

public class FavoritePageModel : FavoriteModel
{
    public string PageId { get; set; }
}

public class FavoriteSpaceModel : FavoriteModel
{
    public string SpaceKey { get; set; }
}