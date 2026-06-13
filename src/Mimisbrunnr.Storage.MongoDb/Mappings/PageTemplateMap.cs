using Mimisbrunnr.PageTemplates.Contracts;
using MongoDB.Bson;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb.Mappings;

public class PageTemplateMap : EntityMapClass<PageTemplate>
{
    public PageTemplateMap()
    {
        ToCollection("PageTemplates");
        MapId(x => x.Id, BsonType.String);
    }
}
