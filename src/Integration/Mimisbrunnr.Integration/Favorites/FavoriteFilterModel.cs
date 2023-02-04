namespace Mimisbrunnr.Integration.Favorites;

public class FavoriteFilterModel
{
    public int? Count { get; set; }

    public int? Skip { get; set; }

    public FavoriteFilterTypeModel? Type { get; set; }
}