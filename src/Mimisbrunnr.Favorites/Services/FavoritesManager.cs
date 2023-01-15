using Mimisbrunnr.Favorites.Contracts;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Favorites.Services;

internal class FavoritesManager : IFavoritesManager
{
    private readonly IRepository<Favorite> _repository;

    public FavoritesManager(IRepository<Favorite> repository)
    {
        _repository = repository;
    }

    public async Task<Favorite> Add(string userId, string favoriteItemId, FavoriteType type, CancellationToken token = default)
    {
        var favorite = new Favorite()
        {
            UserId = userId,
            FavoriteItemId = favoriteItemId,
            Type = type
        };
        await _repository.Create(favorite, token);
        return favorite;
    }

    public Task<bool> EnsureItemInFavorite(string userId, string favoriteItemId, FavoriteType type, CancellationToken token = default)
    {
        return _repository.GetAll().AnyAsync(x => x.UserId == userId && x.FavoriteItemId == favoriteItemId && x.Type == type, cancellationToken: token);
    }

    public async Task<IEnumerable<Favorite>> FindAllByUserId(string userId, CancellationToken token = default)
    {
        return await _repository.GetAll().Where( x=> x.UserId == userId).ToArrayAsync(token);
    }

    public async Task<IEnumerable<Favorite>> FindByUserIdAndType(string userId, FavoriteType type, CancellationToken token = default)
    {
        return await _repository.GetAll().Where( x=> x.UserId == userId && x.Type == type).ToArrayAsync(token);
    }

    public Task Remove(Favorite favorite, CancellationToken token = default)
    {
        return _repository.Delete(favorite, token);
    }
}