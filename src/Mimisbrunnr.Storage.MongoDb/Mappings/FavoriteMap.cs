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
        SetIsRootClass(true);
    }
}

public class FavoriteUserMap : EntityMapClass<FavoriteUser>
{
    public FavoriteUserMap()
    {
        AutoMap();
    }
}

public class FavoriteSpaceMap : EntityMapClass<FavoriteSpace>
{
    public FavoriteSpaceMap()
    {
        AutoMap();
    }
}

public class FavoritePageMap : EntityMapClass<FavoritePage>
{
    public FavoritePageMap()
    {
        AutoMap();
    }
}