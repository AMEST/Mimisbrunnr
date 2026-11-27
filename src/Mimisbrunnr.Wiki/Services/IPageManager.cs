using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Wiki.Services;

public interface IPageManager
{
    Task<Page[]> GetAllOnSpace(Space space);

    /// <summary>
    /// Returns versions of the page without content. Use <see cref="GetVersionByPageId"/> to load a version with content.
    /// </summary>
    Task<HistoricalPage[]> GetAllVersions(Page page);

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
