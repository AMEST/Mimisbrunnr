using Mimisbrunnr.Wiki.Contracts;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb.Mappings;

public class SpaceMap : EntityMapClass<Space>
{
    public SpaceMap()
    {
        ToCollection("Spaces");
        MapId(x => x.Id);
        MapField(x => x.Status).SetSerializer(new EnumSerializer<SpaceStatus>(BsonType.String));
        MapField(x => x.Type).SetSerializer(new EnumSerializer<SpaceType>(BsonType.String));
    }
}