using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;
using MongoDB.Driver;
using Skidbladnir.Repository.Abstractions;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb;

internal class PageSearcher : IPageSearcher
{
    private readonly BaseMongoDbContext _mongoDbContext;
    private readonly IMongoCollection<Page> _pageCollection;

    public PageSearcher(BaseMongoDbContext mongoDbContext)
    {
        _mongoDbContext = mongoDbContext;
        _pageCollection = mongoDbContext.GetCollection<Page>();
    }

    public async Task<IEnumerable<Page>> Search(string text)
    {
        var results = await _pageCollection
            .Find(Builders<Page>.Filter.Text(text))
            .Limit(100)
            .ToListAsync();
        return results;
    }
}