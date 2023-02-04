using System.Linq.Expressions;
using Mimisbrunnr.Favorites.Contracts;

namespace Mimisbrunnr.Favorites.Services;

public interface IFavoritesManager
{
    Task<IEnumerable<Favorite>> FindAllByUserEmail(string email, FavoriteFilter filter = null, CancellationToken token = default);
    Task<Favorite> FindById(string id, CancellationToken token = default);
     Task<bool> EnsureItemInFavorite<T>(string email, Expression<Func<T, bool>> expression, CancellationToken token = default)
        where T : Favorite;
    Task<Favorite> Add(Favorite favorite, CancellationToken token = default);
    Task Remove(Favorite favorite, CancellationToken token = default);
}