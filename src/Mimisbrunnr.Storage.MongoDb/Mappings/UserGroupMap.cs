using Mimisbrunnr.Users;
using MongoDB.Bson;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb.Mappings;

public class UserGroupMap: EntityMapClass<UserGroup>
{
    public UserGroupMap()
    {
        ToCollection("UserGroups");
        MapId(x => x.Id, BsonType.String);
    }
}