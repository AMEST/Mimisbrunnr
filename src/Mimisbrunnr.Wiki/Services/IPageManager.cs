using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Wiki.Services;

public interface IPageManager
{
    Task<Page[]> GetAllOnSpace(Space space);

    Task<HistoricalPage[]> GetAllVersions(Page page); 

    Task<Page[]> FindByName(string name);

    Task<Page[]> GetAllChilds(Page page);

    Task<Page> GetById(string id);

    Task<HistoricalPage> GetVersionByPageId(string id, long version);

    Task<Page> Create(string spaceId, string name, string content, UserInfo createdBy, string parentPageId = null);

    Task<Page> RestoreVersion(Page page, long version, UserInfo restoredBy);

    Task Update(Page page, UserInfo updatedBy);

    Task<Page> Copy(Page source, Page destinationParentPage);

    Task<Page> Move(Page source, Page destinationParentPage);

    Task ChangeEditorType(Page page, PageEditorType editorType, UserInfo updatedBy);

    Task Remove(Page page, bool deleteChild = false);

    Task RemoveVersion(Page page, long version);
}