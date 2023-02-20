using Mimisbrunnr.Wiki.Contracts;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb.Mappings;

public class CommentMap : EntityMapClass<Comment>
{
    public CommentMap()
    {
        ToCollection("Comments");
        MapId(x => x.Id);
    }
}