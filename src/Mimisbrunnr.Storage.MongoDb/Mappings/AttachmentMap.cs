using Mimisbrunnr.Wiki.Contracts;
using MongoDB.Bson;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb.Mappings
{
    public class AttachmentMap : EntityMapClass<Attachment>
    {
        public AttachmentMap()
        {
            ToCollection("Attachments");
            MapId(x => x.Id, BsonType.String);
        }
    }
}