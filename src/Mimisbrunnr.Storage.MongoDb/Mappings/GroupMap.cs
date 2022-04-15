using Mimisbrunner.Users;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb.Mappings;

public class GroupMap : EntityMapClass<Group>
{
    public GroupMap()
    {
        ToCollection("Groups");
    }
}