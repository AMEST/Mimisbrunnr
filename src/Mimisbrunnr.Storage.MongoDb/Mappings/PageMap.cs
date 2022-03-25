using Mimisbrunnr.Wiki.Contracts;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb.Mappings;

public class PageMap : EntityMapClass<Page>
{
    public PageMap()
    {
        ToCollection("Pages");
    }
}