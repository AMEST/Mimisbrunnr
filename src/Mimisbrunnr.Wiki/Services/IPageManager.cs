using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Wiki.Services;

public interface IPageManager
{
    Task<Page[]> GetAllOnSpace(Space space);

    /// <summary>
    /// Returns versions of the page with optional content.
    /// </summary>
    /// <param name="page">The page whose history is requested</param>
    /// <param name="includeContent">Whether to include page content</param>
    /// <param name="contentVersion">Limit included content to this version when specified</param>
    Task<HistoricalPage[]> GetAllVersions(Page page, bool includeContent = true, long? contentVersion = null);

    Task<Page[]> FindByName(string name);

    Task<Page[]> GetAllChilds(Page page, bool lightContract = true);

    Task<Page> GetById(string id);

    Task<Page[]> GetByIds(params string[] ids);

    Task<HistoricalPage> GetVersionByPageId(string id, long version);

    Task<Page> Create(string spaceId, string name, string content, UserInfo createdBy, string parentPageId = null);

    Task<Page> RestoreVersion(Page page, long version, UserInfo restoredBy);

    /// <summary>
    /// Saves new page content. <paramref name="page"/> must contain the state that is currently stored.
    /// </summary>
    /// <exception cref="PageConcurrencyException">The page was modified concurrently</exception>
    Task Update(Page page, string name, string content, UserInfo updatedBy);

    Task<Page> Copy(Page source, Page destinationParentPage);

    Task<Page> Move(Page source, Page destinationParentPage, bool withChilds = true);

    Task Remove(Page page, bool deleteChild = false);

    Task RemoveVersion(Page page, long version);
}
