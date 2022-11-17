using Mimisbrunnr.Wiki.Contracts;
using MongoDB.Bson;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb.Mappings;

public class DraftMap : EntityMapClass<Draft>
{
    public DraftMap()
    {
        ToCollection("Drafts");
        MapId(x => x.Id, BsonType.String);
    }
}