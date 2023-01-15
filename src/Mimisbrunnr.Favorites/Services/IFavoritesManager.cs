using Mimisbrunnr.Favorites.Contracts;

namespace Mimisbrunnr.Favorites.Services;

public interface IFavoritesManager
{
     Task<IEnumerable<Favorite>> FindAllByUserId(string userId, CancellationToken token = default);
     Task<IEnumerable<Favorite>> FindByUserIdAndType(string userId, FavoriteType type, CancellationToken token = default);
     Task<bool> EnsureItemInFavorite(string userId, string favoriteItemId, FavoriteType type, CancellationToken token = default);
     Task<Favorite> Add(string userId, string favoriteItemId, FavoriteType type, CancellationToken token = default);
     Task Remove(Favorite favorite, CancellationToken token = default);
}