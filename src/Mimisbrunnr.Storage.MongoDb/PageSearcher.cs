using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;
using MongoDB.Driver;
using Skidbladnir.Repository.Abstractions;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb;

internal class PageSearcher : IPageSearcher
{
    private readonly IMongoCollection<Page> _collection;

    public PageSearcher(BaseMongoDbContext mongoDbContext)
    {
        _collection = mongoDbContext.GetCollection<Page>()
            .WithReadPreference(ReadPreference.SecondaryPreferred);
    }

    public async Task<Page[]> Search(string text)
    {
        var results = await _collection
            .Find(Builders<Page>.Filter.Text(text))
            .Limit(100)
            .ToListAsync();
        return [.. results];
    }
}