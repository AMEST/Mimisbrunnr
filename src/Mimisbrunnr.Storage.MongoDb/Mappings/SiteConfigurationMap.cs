using Mimisbrunnr.Web.Infrastructure.Contracts;
using MongoDB.Bson;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb.Mappings;

public class ApplicationConfigurationMap : EntityMapClass<ApplicationConfiguration>
{
    public ApplicationConfigurationMap()
    {
        ToCollection("Configuration");
        MapId(x => x.Id, BsonType.String);
        MapProperty(x => x.AllowAnonymous).SetIgnoreIfDefault(false);
        MapProperty(x => x.SwaggerEnabled).SetIgnoreIfDefault(false);
        MapProperty(x => x.AllowHtml)
            .SetIgnoreIfDefault(false)
            .SetDefaultValue(true);
    }
}