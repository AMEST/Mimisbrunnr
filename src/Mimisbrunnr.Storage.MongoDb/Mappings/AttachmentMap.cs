using Mimisbrunnr.Wiki.Contracts;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb.Mappings
{
    public class AttachmentMap : EntityMapClass<Attachment>
    {
        public AttachmentMap()
        {
            ToCollection("Attachments");
        }
    }
}