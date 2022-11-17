using Mimisbrunnr.Web.Infrastructure.Contracts;
using MongoDB.Bson;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb.Mappings;

public class UserTokenMap : EntityMapClass<UserToken>
{
    public UserTokenMap()
    {
        ToCollection("UserTokens");
        MapId(x => x.Id, BsonType.String);
        MapMember(x => x.Revoked)
            .SetIgnoreIfDefault(false)
            .SetIgnoreIfNull(false);
    }
}