using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Repositories;
using MongoDB.Driver;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb.Repositories;

public class PageRepository : MongoRepository<Page, BaseMongoDbContext>, IPageRepository
{
    public PageRepository(BaseMongoDbContext context, MongoDbContextConfiguration<BaseMongoDbContext> configuration)
        : base(context, configuration)
    {
    }

    public async Task<Page[]> GetByIds(string[] ids)
    {
        if (ids is null || ids.Length == 0)
            return [];

        var pages = await Entities.Find(Builders<Page>.Filter.In(x => x.Id, ids)).ToListAsync();
        return [.. pages];
    }

    public Task MoveToSpace(string[] pageIds, string spaceId)
    {
        if (pageIds is null || pageIds.Length == 0)
            return Task.CompletedTask;

        return Entities.UpdateManyAsync(
            Builders<Page>.Filter.In(x => x.Id, pageIds),
            Builders<Page>.Update.Set(x => x.SpaceId, spaceId));
    }

    public Task Reparent(string pageId, string newParentId)
    {
        return Entities.UpdateOneAsync(
            Builders<Page>.Filter.Eq(x => x.Id, pageId),
            Builders<Page>.Update.Set(x => x.ParentId, newParentId));
    }

    public Task ReparentChildren(string parentId, string newParentId)
    {
        return Entities.UpdateManyAsync(
            Builders<Page>.Filter.Eq(x => x.ParentId, parentId),
            Builders<Page>.Update.Set(x => x.ParentId, newParentId));
    }

    public async Task ReplaceWithVersionCheck(Page page, long expectedVersion)
    {
        ArgumentNullException.ThrowIfNull(page);

        var filters = Builders<Page>.Filter;
        var versionFilter = filters.Eq(x => x.Version, expectedVersion);
        // Older documents without Version are read as version zero.
        if (expectedVersion == 0)
            versionFilter |= filters.Exists(x => x.Version, false);

        var filter = filters.Eq(x => x.Id, page.Id) & versionFilter;
        var result = await Entities.ReplaceOneAsync(filter, page);
        if (result.MatchedCount == 0)
            throw new PageConcurrencyException(page.Id);
    }
}
