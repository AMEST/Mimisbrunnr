using System.ComponentModel.DataAnnotations;
using Mimisbrunnr.Integration.User;
using Mimisbrunnr.Integration.Wiki;

namespace Mimisbrunnr.Integration.Favorites;

public abstract class FavoriteModel
{
    public string Id { get; set; }

    public DateTime Created { get; set; }
}

public class FavoriteUserModel : FavoriteModel
{
    public UserModel User { get; set; }
}

public class FavoritePageModel : FavoriteModel
{
    public PageModel Page { get; set; }
}

public class FavoriteSpaceModel : FavoriteModel
{
    public SpaceModel Space { get; set; }
}