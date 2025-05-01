using Mimisbrunnr.Wiki.Contracts;
using MongoDB.Bson;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb.Mappings;

public class PluginMap : EntityMapClass<Plugin>
{
    public PluginMap()
    {
        ToCollection("Plugin");
        MapId(x => x.Id, BsonType.String);
    }
}