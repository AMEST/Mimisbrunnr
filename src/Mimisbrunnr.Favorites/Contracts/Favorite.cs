using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Favorites.Contracts;

public class Favorite : IHasId<string>
{
    public string Id { get; set; }

    public string FavoriteItemId { get; set; }

    public string UserId { get; set; }

    public FavoriteType Type { get; set; }
}