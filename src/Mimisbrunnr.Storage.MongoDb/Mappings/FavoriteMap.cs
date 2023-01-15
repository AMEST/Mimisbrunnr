using Mimisbrunnr.Favorites.Contracts;
using Mimisbrunnr.Wiki.Contracts;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb.Mappings;

public class FavoriteMap : EntityMapClass<Favorite>
{
    public FavoriteMap()
    {
        ToCollection("Favorites");
        MapId(x => x.Id);
        MapMember(x => x.Type)
            .SetSerializer(new EnumSerializer<FavoriteType>(BsonType.String))
            .SetIgnoreIfDefault(false);
    }
}