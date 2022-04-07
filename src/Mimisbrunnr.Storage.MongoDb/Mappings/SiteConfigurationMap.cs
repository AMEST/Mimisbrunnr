using Mimisbrunnr.Web.Infrastructure.Contracts;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb.Mappings;

public class ApplicationConfigurationMap : EntityMapClass<ApplicationConfiguration>
{
    public ApplicationConfigurationMap()
    {
        ToCollection("Configuration");
        MapProperty(x => x.AllowAnonymous).SetIgnoreIfDefault(false);
        MapProperty(x => x.SwaggerEnabled).SetIgnoreIfDefault(false);
    }
}