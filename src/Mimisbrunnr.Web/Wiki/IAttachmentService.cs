using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Integration.Wiki;

namespace Mimisbrunnr.Web.Wiki;

/// <summary>
/// Service for managing page attachments
/// </summary>
public interface IAttachmentService
{
    /// <summary>
    /// Get all attachments for a page
    /// </summary>
    /// <param name="pageId">Page identifier</param>
    /// <param name="requestedBy">User requesting the attachments</param>
    /// <returns>List of attachments</returns>
    Task<AttachmentModel[]> GetAttachments(string pageId, UserInfo requestedBy);

    /// <summary>
    /// Get the content of an attachment
    /// </summary>
    /// <param name="pageId">Page identifier</param>
    /// <param name="name">Attachment name</param>
    /// <param name="requestedBy">User requesting the attachment content</param>
    /// <returns>Stream with the attachment content</returns>
    Task<Stream> GetAttachmentContent(string pageId, string name, UserInfo requestedBy);

    /// <summary>
    /// Get attachment content after checking its size in storage, before downloading it.
    /// </summary>
    /// <param name="pageId">Page identifier</param>
    /// <param name="name">Attachment name</param>
    /// <param name="requestedBy">User requesting the attachment content</param>
    /// <param name="maxBytes">Maximum permitted file size in bytes</param>
    /// <returns>Stream with the attachment content</returns>
    Task<Stream> GetAttachmentContent(string pageId, string name, UserInfo requestedBy, long maxBytes);

    /// <summary>
    /// Upload an attachment to a page
    /// </summary>
    /// <param name="pageId">Page identifier</param>
    /// <param name="content">Attachment content stream</param>
    /// <param name="name">Attachment name</param>
    /// <param name="uploadedBy">User uploading the attachment</param>
    Task Upload(string pageId, Stream content, string name, UserInfo uploadedBy);

    /// <summary>
    /// Remove an attachment from a page
    /// </summary>
    /// <param name="pageId">Page identifier</param>
    /// <param name="name">Attachment name</param>
    /// <param name="removedBy">User removing the attachment</param>
    Task Remove(string pageId, string name, UserInfo removedBy);
}
