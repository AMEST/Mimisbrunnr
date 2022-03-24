using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Wiki;

public interface IPageService
{
    Task<PageModel> GetById(string pageId, UserInfo requestedBy);

    Task<PageTreeModel> GetPageTreeByPageId(string pageId, UserInfo requestedBy);

    Task<PageModel> Create(PageCreateModel createModel, UserInfo createdBy);

    Task Update(PageUpdateModel updateModel, UserInfo updatedBy);

    Task Delete(string pageId, UserInfo deletedBy, bool recursively);

    Task<PageModel> Copy(string sourcePageId, string destinationParentPageId, UserInfo copiedBy);
    
    Task<PageModel> Move(string sourcePageId, string destinationParentPageId, UserInfo copiedBy);
}