using Mimisbrunner.Users;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb.Mappings;

public class UserMap : EntityMapClass<User>
{
    public UserMap()
    {
        ToCollection("Users");
        MapId(x => x.Id);
    }
}