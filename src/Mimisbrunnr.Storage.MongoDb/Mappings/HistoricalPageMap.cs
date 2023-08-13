using Mimisbrunnr.Wiki.Contracts;
using MongoDB.Bson;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb.Mappings;

public class HistoricalPageMap : EntityMapClass<HistoricalPage>
{
    public HistoricalPageMap()
    {
        ToCollection("PageHistory");
        MapId(x => x.Id, BsonType.String);
    }
}