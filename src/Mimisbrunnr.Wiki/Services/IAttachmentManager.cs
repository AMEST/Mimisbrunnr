using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Wiki.Services;

public interface IAttachmentManager
{
    Task<Attachment[]> GetAttachments(Page page);

    Task<Stream> GetAttachmentContent(Page page, string name);

    Task Upload(Page page, Stream content, string name, UserInfo uploadedBy);

    Task Remove(Page page, string name);

    Task RemoveAll(Page page);
}
