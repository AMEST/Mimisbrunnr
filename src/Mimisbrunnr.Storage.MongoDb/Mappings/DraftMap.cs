using Mimisbrunnr.Wiki.Contracts;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb.Mappings;

public class DraftMap : EntityMapClass<Draft>
{
    public DraftMap()
    {
        ToCollection("Drafts");
    }
}