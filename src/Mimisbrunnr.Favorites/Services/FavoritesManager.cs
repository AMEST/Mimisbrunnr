using System.Linq.Expressions;
using Mimisbrunnr.Favorites.Contracts;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Favorites.Services;

internal class FavoritesManager : IFavoritesManager
{
    private readonly IFavoriteStore _repository;

    public FavoritesManager(IFavoriteStore repository)
    {
        _repository = repository;
    }

    public async Task<Favorite> Add(Favorite favorite, CancellationToken token = default)
    {
        await _repository.Create(favorite, token);
        return favorite;
    }

    public Task<bool> EnsureItemInFavorite<T>(string email, Expression<Func<T, bool>> expression, CancellationToken token = default)
        where T : Favorite
    {
        return _repository.GetAllByType<T>().Where( x=> x.OwnerEmail == email).AnyAsync(expression, token);
    }

    public async Task<IEnumerable<Favorite>> FindAllByUserEmail(string email, FavoriteFilter filter = null, CancellationToken token = default)
    {
        if (filter is null)
        {
            return await _repository.GetAll().Where(x => x.OwnerEmail == email).ToArrayAsync(token);
        }
        if (!filter.Type.HasValue)
            return await GetFiltered<Favorite>(email, filter, token);
        return filter.Type switch
        {
            FavoriteFilterType.User => await GetFiltered<FavoriteUser>(email, filter, token),
            FavoriteFilterType.Space => await GetFiltered<FavoriteSpace>(email, filter, token),
            FavoriteFilterType.Page => await GetFiltered<FavoritePage>(email, filter, token),
            _ => throw new NotImplementedException()
        };

    }

    public async Task<Favorite> FindById(string id, CancellationToken token = default)
    {
        return await _repository.GetAll().FirstOrDefaultAsync(x => x.Id == id, token);
    }

    public async Task<Favorite> GetByExpression<T>(string email, Expression<Func<T, bool>> expression, CancellationToken token = default)
         where T : Favorite
    {
        return await _repository.GetAllByType<T>().Where( x=> x.OwnerEmail == email).FirstOrDefaultAsync(expression, token);
    }

    public Task Remove(Favorite favorite, CancellationToken token = default)
    {
        return _repository.Delete(favorite, token);
    }

    private async Task<IEnumerable<Favorite>> GetFiltered<T>(string email, FavoriteFilter filter = null, CancellationToken token = default)
        where T : Favorite
    {
        var query = _repository.GetAllByType<T>().Where(x => x.OwnerEmail == email);

        if (filter.Skip.HasValue)
            query = query.Skip(filter.Skip.Value);
        if (filter.Count.HasValue)
            query = query.Take(filter.Count.Value);

        return await query.ToArrayAsync(token);
    }
}