using Mimisbrunnr.Wiki.Contracts;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb.Mappings;

public class PageUpdateEventMap : EntityMapClass<PageUpdateEvent>
{
    public PageUpdateEventMap()
    {
        ToCollection("PageUpdates");
        MapMember(x => x.SpaceType)
            .SetSerializer(new EnumSerializer<SpaceType>(BsonType.String))
            .SetIgnoreIfDefault(false);
    }
}