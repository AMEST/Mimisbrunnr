using Mimisbrunnr.Web.Infrastructure.Contracts;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb.Mappings;

public class ApplicationConfigurationMap : EntityMapClass<ApplicationConfiguration>
{
    public ApplicationConfigurationMap()
    {
        ToCollection("Configuration");
        MapId(x => x.Id);
    }
}