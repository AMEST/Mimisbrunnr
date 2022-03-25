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
        MapMember(x => x.Status).SetSerializer(new EnumSerializer<SpaceStatus>(BsonType.String));
        MapMember(x => x.Type).SetSerializer(new EnumSerializer<SpaceType>(BsonType.String));
    }
}