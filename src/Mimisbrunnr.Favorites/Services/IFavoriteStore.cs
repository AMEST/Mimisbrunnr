using Mimisbrunnr.Favorites.Contracts;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Favorites.Services;

public interface IFavoriteStore : IRepository<Favorite>
{
    IQueryable<T> GetAllByType<T>() where T : Favorite;
}