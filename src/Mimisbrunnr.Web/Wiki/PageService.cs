using Mimisbrunner.Users;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

namespace Mimisbrunnr.Web.Wiki;

internal class PageService : IPageService
{
    private readonly IPageManager _pageManager;
    private readonly IUserManager _userManager;

    public PageService(IPageManager pageManager,
        IUserManager userManager
    )
    {
        _pageManager = pageManager;
        _userManager = userManager;
    }

    public Task<PageModel> GetById(string pageId, UserInfo requestedBy)
    {
        throw new NotImplementedException();
    }

    public Task<PageTreeModel> GetPageTreeByPageId(string pageId, UserInfo requestedBy)
    {
        throw new NotImplementedException();
    }

    public Task<PageModel> Create(PageCreateModel createModel, UserInfo createdBy)
    {
        throw new NotImplementedException();
    }

    public Task Update(PageUpdateModel updateModel, UserInfo updatedBy)
    {
        throw new NotImplementedException();
    }

    public Task Delete(string pageId, UserInfo deletedBy, bool recursively)
    {
        throw new NotImplementedException();
    }

    public Task<PageModel> Copy(string sourcePageId, string destinationParentPageId, UserInfo copiedBy)
    {
        throw new NotImplementedException();
    }

    public Task<PageModel> Move(string sourcePageId, string destinationParentPageId, UserInfo copiedBy)
    {
        throw new NotImplementedException();
    }
}