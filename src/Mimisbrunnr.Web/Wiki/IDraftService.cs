using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Wiki;

/// <summary>
/// Service for managing page drafts
/// </summary>
public interface IDraftService
{
    /// <summary>
    /// Get a draft by page identifier
    /// </summary>
    /// <param name="pageId">Page identifier</param>
    /// <param name="requestedBy">User requesting the draft</param>
    /// <returns>The draft for the page</returns>
    Task<DraftModel> GetByPageId(string pageId, UserInfo requestedBy);

    /// <summary>
    /// Update or create a draft for a page
    /// </summary>
    /// <param name="pageId">Page identifier</param>
    /// <param name="updateModel">Draft update parameters</param>
    /// <param name="updatedBy">User updating the draft</param>
    Task Update(string pageId, DraftUpdateModel updateModel, UserInfo updatedBy);

    /// <summary>
    /// Delete a draft
    /// </summary>
    /// <param name="pageId">Page identifier</param>
    /// <param name="deletedBy">User deleting the draft</param>
    Task Delete(string pageId, UserInfo deletedBy);
}