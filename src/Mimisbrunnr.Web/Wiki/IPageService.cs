using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Integration.Wiki;

namespace Mimisbrunnr.Web.Wiki;

public interface IPageService
{
    Task<PageModel> GetById(string pageId, UserInfo requestedBy);

    Task<PageVersionsListModel> GetPageVersions(string pageId, UserInfo requestedBy);

    Task<PageTreeModel> GetPageTreeByPageId(string pageId, UserInfo requestedBy);

    Task<PageModel> Create(PageCreateModel createModel, UserInfo createdBy);

    Task<PageModel> RestoreVersion(string pageId, long version, UserInfo restoredBy);

    Task Update(string pageId, PageUpdateModel updateModel, UserInfo updatedBy);

    Task Delete(string pageId, UserInfo deletedBy, bool recursively);

    Task DeleteVersion(string pageId, long version, UserInfo deletedBy);

    Task<PageModel> Copy(string sourcePageId, string destinationParentPageId, UserInfo copiedBy);
    
    Task<PageModel> Move(string sourcePageId, string destinationParentPageId, UserInfo movedBy);

    Task UpdateEditorType(string pageId, PageEditorTypeUpdateModel model, UserInfo updatedBy);
}