using Mimisbrunner.Users;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb.Mappings;

public class UserMap : EntityMapClass<User>
{
    public UserMap()
    {
        ToCollection("Users");
        MapMember(x => x.Role).SetSerializer(new EnumSerializer<UserRole>(BsonType.String));
    }
}