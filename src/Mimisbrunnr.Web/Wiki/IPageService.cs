using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Integration.Wiki;

namespace Mimisbrunnr.Web.Wiki;

/// <summary>
/// Service for managing wiki pages
/// </summary>
public interface IPageService
{
    /// <summary>
    /// Get a page by its identifier
    /// </summary>
    /// <param name="pageId">Page identifier</param>
    /// <param name="requestedBy">User requesting the page</param>
    /// <returns>The requested page</returns>
    Task<PageModel> GetById(string pageId, UserInfo requestedBy);

    /// <summary>
    /// Get the version history of a page
    /// </summary>
    /// <param name="pageId">Page identifier</param>
    /// <param name="requestedBy">User requesting the versions</param>
    /// <returns>List of page versions</returns>
    Task<PageVersionsListModel> GetPageVersions(string pageId, UserInfo requestedBy);

    /// <summary>
    /// Get a specific historical version of a page
    /// </summary>
    /// <param name="pageId">Page identifier</param>
    /// <param name="version">Version number</param>
    /// <param name="requestedBy">User requesting the version</param>
    /// <returns>The requested historical page version</returns>
    Task<HistoricalPageModel> GetVersion(string pageId, long version, UserInfo requestedBy);

    /// <summary>
    /// Get the page tree structure starting from the specified page
    /// </summary>
    /// <param name="pageId">Identifier of the root page</param>
    /// <param name="requestedBy">User requesting the page tree</param>
    /// <returns>The page tree structure</returns>
    Task<PageTreeModel> GetPageTreeByPageId(string pageId, UserInfo requestedBy);

    /// <summary>
    /// Create a new page
    /// </summary>
    /// <param name="createModel">Page creation parameters</param>
    /// <param name="createdBy">User creating the page</param>
    /// <returns>The created page</returns>
    Task<PageModel> Create(PageCreateModel createModel, UserInfo createdBy);

    /// <summary>
    /// Restore a specific version of a page
    /// </summary>
    /// <param name="pageId">Page identifier</param>
    /// <param name="version">Version number to restore</param>
    /// <param name="restoredBy">User restoring the version</param>
    /// <returns>The restored page</returns>
    Task<PageModel> RestoreVersion(string pageId, long version, UserInfo restoredBy);

    /// <summary>
    /// Update an existing page
    /// </summary>
    /// <param name="pageId">Page identifier</param>
    /// <param name="updateModel">Page update parameters</param>
    /// <param name="updatedBy">User updating the page</param>
    Task Update(string pageId, PageUpdateModel updateModel, UserInfo updatedBy);

    /// <summary>
    /// Delete a page
    /// </summary>
    /// <param name="pageId">Page identifier</param>
    /// <param name="deletedBy">User deleting the page</param>
    /// <param name="recursively">Whether to delete child pages</param>
    Task Delete(string pageId, UserInfo deletedBy, bool recursively);

    /// <summary>
    /// Delete a specific version of a page
    /// </summary>
    /// <param name="pageId">Page identifier</param>
    /// <param name="version">Version number to delete</param>
    /// <param name="deletedBy">User deleting the version</param>
    Task DeleteVersion(string pageId, long version, UserInfo deletedBy);

    /// <summary>
    /// Copy a page to a new location
    /// </summary>
    /// <param name="sourcePageId">Identifier of the page to copy</param>
    /// <param name="destinationParentPageId">Identifier of the destination parent page</param>
    /// <param name="copiedBy">User copying the page</param>
    /// <returns>The copied page</returns>
    Task<PageModel> Copy(string sourcePageId, string destinationParentPageId, UserInfo copiedBy);
    
    /// <summary>
    /// Move a page to a new location
    /// </summary>
    /// <param name="sourcePageId">Identifier of the page to move</param>
    /// <param name="destinationParentPageId">Identifier of the destination parent page</param>
    /// <param name="withChilds">Whether to move child pages</param>
    /// <param name="movedBy">User moving the page</param>
    /// <returns>The moved page</returns>
    Task<PageModel> Move(string sourcePageId, string destinationParentPageId, bool withChilds, UserInfo movedBy);
}