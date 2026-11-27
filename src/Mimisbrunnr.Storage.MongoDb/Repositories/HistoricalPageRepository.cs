using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Repositories;
using MongoDB.Driver;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb.Repositories;

public class HistoricalPageRepository : MongoRepository<HistoricalPage, BaseMongoDbContext>, IHistoricalPageRepository
{
    public HistoricalPageRepository(BaseMongoDbContext context, MongoDbContextConfiguration<BaseMongoDbContext> configuration)
        : base(context, configuration)
    {
    }

    public Task SaveVersion(HistoricalPage historicalPage)
    {
        ArgumentNullException.ThrowIfNull(historicalPage);

        var filter = Builders<HistoricalPage>.Filter.Eq(x => x.PageId, historicalPage.PageId)
                     & Builders<HistoricalPage>.Filter.Eq(x => x.Version, historicalPage.Version);

        // Upsert by version: a repeated save of the same version keeps the stored document id and does not fail.
        var update = Builders<HistoricalPage>.Update
            .Set(x => x.Name, historicalPage.Name)
            .Set(x => x.Content, historicalPage.Content)
            .Set(x => x.Updated, historicalPage.Updated)
            .Set(x => x.UpdatedBy, historicalPage.UpdatedBy)
            .SetOnInsert(x => x.Id, Guid.NewGuid().ToString());

        return Entities.UpdateOneAsync(filter, update, new UpdateOptions() { IsUpsert = true });
    }
}
