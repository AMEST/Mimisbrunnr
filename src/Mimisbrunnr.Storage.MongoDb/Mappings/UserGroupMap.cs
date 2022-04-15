using Mimisbrunnr.Users;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb.Mappings;

public class UserGroupMap: EntityMapClass<UserGroup>
{
    public UserGroupMap()
    {
        ToCollection("UserGroups");
    }
}