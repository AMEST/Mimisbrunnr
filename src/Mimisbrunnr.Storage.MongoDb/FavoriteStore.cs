using System.Linq.Expressions;
using Mimisbrunnr.Favorites.Contracts;
using Mimisbrunnr.Favorites.Services;
using MongoDB.Driver;
using Skidbladnir.Repository.Abstractions;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb;

public class FavoriteStore : IFavoriteStore
{
    private readonly BaseMongoDbContext _context;
    private readonly IRepository<Favorite> _favoriteRepository;

    public FavoriteStore(BaseMongoDbContext context, IRepository<Favorite> favoriteRepository)
    {
        _context = context;
        _favoriteRepository = favoriteRepository;
    }

    public Task Create(Favorite obj, CancellationToken cancellationToken = default)
    {
        return _favoriteRepository.Create(obj, cancellationToken);
    }

    public Task CreateAll(IEnumerable<Favorite> objs, CancellationToken cancellationToken = default)
    {
        return _favoriteRepository.CreateAll(objs, cancellationToken);
    }

    public Task Delete(Favorite obj, CancellationToken cancellationToken = default)
    {
        return _favoriteRepository.Delete(obj, cancellationToken);
    }

    public Task DeleteAll(IEnumerable<Favorite> objs, CancellationToken cancellationToken = default)
    {
        return _favoriteRepository.DeleteAll(objs, cancellationToken);
    }

    public Task DeleteAll(Expression<Func<Favorite, bool>> filter, CancellationToken cancellationToken = default)
    {
        return _favoriteRepository.DeleteAll(filter, cancellationToken);
    }

    public IQueryable<Favorite> GetAll()
    {
        return _favoriteRepository.GetAll();
    }

    public IQueryable<T> GetAllByType<T>() where T : Favorite
    {
        var collection = _context.GetCollection<Favorite>();
        return collection.OfType<T>().AsQueryable();
    }

    public Task Update(Favorite obj, CancellationToken cancellationToken = default)
    {
        return _favoriteRepository.Update(obj, cancellationToken);
    }
}