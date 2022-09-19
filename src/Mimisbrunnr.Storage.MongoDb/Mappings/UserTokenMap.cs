using Mimisbrunnr.Web.Infrastructure.Contracts;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb.Mappings;

public class UserTokenMap : EntityMapClass<UserToken>
{
    public UserTokenMap()
    {
        ToCollection("UserTokens");
        MapMember(x => x.Revoked)
            .SetIgnoreIfDefault(false)
            .SetIgnoreIfNull(false);
    }
}