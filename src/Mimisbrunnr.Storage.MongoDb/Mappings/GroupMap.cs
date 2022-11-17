using Mimisbrunnr.Users;
using MongoDB.Bson;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb.Mappings;

public class GroupMap : EntityMapClass<Group>
{
    public GroupMap()
    {
        ToCollection("Groups");
        MapId(x => x.Id, BsonType.String);
    }
}