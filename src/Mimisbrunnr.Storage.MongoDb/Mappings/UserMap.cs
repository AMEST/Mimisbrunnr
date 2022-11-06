using Mimisbrunnr.Users;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb.Mappings;

public class UserMap : EntityMapClass<User>
{
    public UserMap()
    {
        ToCollection("Users");
        MapId(x => x.Id, BsonType.String);
        MapMember(x => x.Role)
            .SetSerializer(new EnumSerializer<UserRole>(BsonType.String))
            .SetIgnoreIfDefault(false);
        MapMember(x => x.Enable)
            .SetIgnoreIfDefault(false)
            .SetIgnoreIfNull(false);
    }
}