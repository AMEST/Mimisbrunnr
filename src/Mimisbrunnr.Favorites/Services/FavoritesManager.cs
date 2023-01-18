using System.Linq.Expressions;
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

    public async Task<Favorite> Add(Favorite favorite, CancellationToken token = default)
    {
        await _repository.Create(favorite, token);
        return favorite;
    }

    public Task<bool> EnsureItemInFavorite<T>(string email, Expression<Func<T, bool>> expression, CancellationToken token = default)
        where T : Favorite
    {
        return _repository.GetAll().Where(x => x.OwnerEmail == email).OfType<T>().AnyAsync(expression, cancellationToken: token);
    }

    public async Task<IEnumerable<Favorite>> FindAllByUserEmail(string email, CancellationToken token = default)
    {
        return await _repository.GetAll().Where(x => x.OwnerEmail == email).ToArrayAsync(token);
    }


    public async Task<Favorite> FindById(string id, CancellationToken token = default)
    {
        return await _repository.GetAll().FirstOrDefaultAsync(x => x.Id == id, token);
    }

    public async Task<IEnumerable<Favorite>> FindByUserEmail<T>(string email, CancellationToken token = default)
        where T : Favorite
    {
        return await _repository.GetAll().Where(x => x.OwnerEmail == email).OfType<T>().ToArrayAsync(token);
    }

    public Task Remove(Favorite favorite, CancellationToken token = default)
    {
        return _repository.Delete(favorite, token);
    }
}