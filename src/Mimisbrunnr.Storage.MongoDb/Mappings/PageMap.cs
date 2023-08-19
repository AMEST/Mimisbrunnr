using Mimisbrunnr.Wiki.Contracts;
using MongoDB.Bson;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb.Mappings;

public class PageMap : EntityMapClass<Page>
{
    public PageMap()
    {
        ToCollection("Pages");
        MapId(x => x.Id, BsonType.String);
        MapProperty(x => x.Version)
            .SetDefaultValue(0L)
            .SetIgnoreIfDefault(false);
    }
}