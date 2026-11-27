using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;
using MongoDB.Driver;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb;

internal class PageSearcher : IPageSearcher
{
    private const int DefaultLimit = 100;

    private readonly IMongoCollection<Page> _collection;

    public PageSearcher(BaseMongoDbContext mongoDbContext)
    {
        _collection = mongoDbContext.GetCollection<Page>()
            .WithReadPreference(ReadPreference.SecondaryPreferred);
    }

    public Task<Page[]> Search(string text) => Search(text, null);

    public async Task<Page[]> Search(string text, string[] spaceIds, int limit = DefaultLimit)
    {
        // An empty set of visible spaces means the user can see nothing at all.
        if (spaceIds is { Length: 0 })
            return [];

        var filter = Builders<Page>.Filter.Text(text);
        if (spaceIds is not null)
            filter &= Builders<Page>.Filter.In(x => x.SpaceId, spaceIds);

        // Page content is part of the result: the search UI builds a snippet around the match.
        var results = await _collection
            .Find(filter)
            .Limit(limit > 0 ? limit : DefaultLimit)
            .ToListAsync();
        return [.. results];
    }
}
