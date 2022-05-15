using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Wiki;

public interface IAttachmentService
{
    Task<AttachmentModel[]> GetAttachments(string pageId, UserInfo requestedBy);

     Task<Stream> GetAttachmentContent(string pageId, string name, UserInfo requestedBy);

     Task Upload(string pageId, Stream content, string name, UserInfo uploadedBy);

     Task Remove(string pageId, string name, UserInfo removedBy);
}
