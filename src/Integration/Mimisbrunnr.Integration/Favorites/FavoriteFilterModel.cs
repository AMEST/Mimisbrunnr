namespace Mimisbrunnr.Integration.Favorites;

/// <summary>
/// Model for filtering favorite items
/// </summary>
public class FavoriteFilterModel
{
    /// <summary>
    /// Gets or sets the maximum number of items to return
    /// </summary>
    public int? Count { get; set; }

    /// <summary>
    /// Gets or sets the number of items to skip
    /// </summary>
    public int? Skip { get; set; }

    /// <summary>
    /// Gets or sets the type of favorites to filter by
    /// </summary>
    public FavoriteFilterTypeModel? Type { get; set; }
}
