using Mimisbrunnr.Wiki.Contracts;
using MongoDB.Bson;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb.Mappings;

public class MacroStateMap : EntityMapClass<MacroState>
{
    public MacroStateMap()
    {
        ToCollection("MacroState");
        MapId(x => x.Id, BsonType.String);

        MapField(x => x.PageId)
            .SetIsRequired(true);
        MapField(x => x.MacroIdentifierOnPage)
            .SetIsRequired(true);
    }
}