namespace Mimisbrunnr.Favorites.Contracts;

public class FavoriteFilter
{
    public int? Count { get; set; }

    public int? Skip { get; set; }

    public FavoriteFilterType? Type { get; set; }
}