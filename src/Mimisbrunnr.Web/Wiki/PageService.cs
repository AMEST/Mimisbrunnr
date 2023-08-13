using Microsoft.Extensions.Caching.Distributed;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;
using Mimisbrunnr.Integration.Wiki;

namespace Mimisbrunnr.Web.Wiki;

internal class PageService : IPageService
{
    private readonly TimeSpan _defaultCacheTime = TimeSpan.FromHours(12);
    private readonly IPageManager _pageManager;
    private readonly ISpaceManager _spaceManager;
    private readonly IFeedManager _feedManager;
    private readonly IPermissionService _permissionService;
    private readonly IDistributedCache _distributedCache;

    public PageService(IPageManager pageManager,
        ISpaceManager spaceManager,
        IFeedManager feedManager,
        IPermissionService permissionService,
        IDistributedCache distributedCache
    )
    {
        _pageManager = pageManager;
        _spaceManager = spaceManager;
        _feedManager = feedManager;
        _permissionService = permissionService;
        _distributedCache = distributedCache;
    }

    public async Task<PageModel> GetById(string pageId, UserInfo requestedBy)
    {
        await _permissionService.EnsureAnonymousAllowed(requestedBy);

        var page = await _pageManager.GetById(pageId) ?? throw new PageNotFoundException();
        var space = await _spaceManager.GetById(page.SpaceId);

        await _permissionService.EnsureViewPermission(space.Key, requestedBy);
        return page.ToModel(space.Key);
    }

    public async Task<PageTreeModel> GetPageTreeByPageId(string pageId, UserInfo requestedBy)
    {
        await _permissionService.EnsureAnonymousAllowed(requestedBy);

        var cachedPageTree = await _distributedCache.GetAsync<PageTreeModel>(GetPageTreeCacheKey(pageId));
        if (cachedPageTree is not null)
            return cachedPageTree;

        var page = await _pageManager.GetById(pageId) ?? throw new PageNotFoundException();
        var space = await _spaceManager.GetById(page.SpaceId);

        await _permissionService.EnsureViewPermission(space.Key, requestedBy);
        var pageTree = await _pageManager.GetAllChilds(page);

        var pageTreeModel = pageTree.ToModel(page, space);
        if (pageTreeModel is not null)
            await _distributedCache.SetAsync(GetPageTreeCacheKey(pageId), pageTreeModel, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = _defaultCacheTime
            });
        return pageTreeModel;
    }

    public async Task<PageModel> Create(PageCreateModel createModel, UserInfo createdBy)
    {
        await _permissionService.EnsureEditPermission(createModel.SpaceKey, createdBy);
        var space = await _spaceManager.GetByKey(createModel.SpaceKey) ?? throw new SpaceNotFoundException();
        if (space.Status == SpaceStatus.Archived)
            throw new InvalidOperationException("Can't create page because space archived");

        var parentPage = await _pageManager.GetById(createModel.ParentPageId);
        if (parentPage == null || parentPage.SpaceId != space.Id)
            throw new PageNotFoundException();

        var page = await _pageManager.Create(space.Id, createModel.Name, createModel.Content, createdBy,
            createModel.ParentPageId);

        await _distributedCache.RemoveAsync(GetPageTreeCacheKey(space.HomePageId));

        return page.ToModel(space.Key);
    }

    public async Task Update(string pageId, PageUpdateModel updateModel, UserInfo updatedBy)
    {
        var page = await _pageManager.GetById(pageId) ?? throw new PageNotFoundException();
        var space = await _spaceManager.GetById(page.SpaceId) ?? throw new SpaceNotFoundException();
        await _permissionService.EnsureEditPermission(space.Key, updatedBy);

        if (space.Status == SpaceStatus.Archived)
            throw new InvalidOperationException("Can't update page because space archived");

        page.Name = updateModel.Name;
        page.Content = updateModel.Content;

        await _pageManager.Update(page, updatedBy);
        await _feedManager.AddPageUpdate(space, page, updatedBy);
        await _distributedCache.RemoveAsync(GetPageTreeCacheKey(space.HomePageId));
    }

    public async Task Delete(string pageId, UserInfo deletedBy, bool recursively)
    {
        var page = await _pageManager.GetById(pageId) ?? throw new PageNotFoundException();
        var space = await _spaceManager.GetById(page.SpaceId) ?? throw new SpaceNotFoundException();
        await _permissionService.EnsureRemovePermission(space.Key, deletedBy);

        if (space.Status == SpaceStatus.Archived)
            throw new InvalidOperationException("Can't remove page because space archived");

        if (space.HomePageId == pageId)
            throw new InvalidOperationException("Cannot remove home page of space");

        await _pageManager.Remove(page, recursively);
        await _distributedCache.RemoveAsync(GetPageTreeCacheKey(space.HomePageId));
    }

    public async Task<PageModel> Copy(string sourcePageId, string destinationParentPageId, UserInfo copiedBy)
    {
        var sourcePage = await _pageManager.GetById(sourcePageId) 
            ?? throw new PageNotFoundException($"Source page with id `{sourcePageId}` not found");
        var destinationParentPage = await _pageManager.GetById(destinationParentPageId) 
            ?? throw new PageNotFoundException($"Destination parent page with id `{destinationParentPageId}` not found");
        var sourceSpace = await _spaceManager.GetById(sourcePage.SpaceId);

        await _permissionService.EnsureEditPermission(sourceSpace.Key, copiedBy);

        var destinationSpace = sourcePage.SpaceId == destinationParentPage.SpaceId
            ? sourceSpace
            : await _spaceManager.GetById(destinationParentPage.SpaceId);

        if (destinationSpace.Status == SpaceStatus.Archived)
            throw new InvalidOperationException("Can't copy page because destination space archived");

        if (sourceSpace.Id != destinationSpace.Id)
            await _permissionService.EnsureEditPermission(destinationSpace.Key, copiedBy);

        var copiedPage = await _pageManager.Copy(sourcePage, destinationParentPage);

        await _distributedCache.RemoveAsync(GetPageTreeCacheKey(sourceSpace.HomePageId));
        if (!sourcePage.Id.Equals(destinationSpace.Id))
            await _distributedCache.RemoveAsync(GetPageTreeCacheKey(destinationSpace.HomePageId));

        return copiedPage.ToModel(destinationSpace.Key);
    }

    public async Task<PageModel> Move(string sourcePageId, string destinationParentPageId, UserInfo movedBy)
    {
        var sourcePage = await _pageManager.GetById(sourcePageId) 
            ?? throw new PageNotFoundException($"Source page with id `{sourcePageId}` not found");
        var destinationParentPage = await _pageManager.GetById(destinationParentPageId)
            ?? throw new PageNotFoundException($"Destination parent page with id `{destinationParentPageId}` not found");
        var sourceSpace = await _spaceManager.GetById(sourcePage.SpaceId);

        await _permissionService.EnsureEditPermission(sourceSpace.Key, movedBy);

        if (sourceSpace.HomePageId == sourcePage.Id)
            throw new InvalidOperationException("Cannot move home page of space. Only copy allowed");

        var destinationSpace = sourcePage.SpaceId == destinationParentPage.SpaceId
            ? sourceSpace
            : await _spaceManager.GetById(destinationParentPage.SpaceId);

        if (sourceSpace.Status == SpaceStatus.Archived)
            throw new InvalidOperationException("Can't move page because source space archived. Only copy allowed");

        if (destinationSpace.Status == SpaceStatus.Archived)
            throw new InvalidOperationException("Can't move page because destination space archived");

        if (sourceSpace.Id != destinationSpace.Id)
        {
            await _permissionService.EnsureRemovePermission(sourceSpace.Key, movedBy);
            await _permissionService.EnsureEditPermission(destinationSpace.Key, movedBy);
        }

        var movedPage = await _pageManager.Move(sourcePage, destinationParentPage);

        await _distributedCache.RemoveAsync(GetPageTreeCacheKey(sourceSpace.HomePageId));
        if (!sourcePage.Id.Equals(destinationSpace.Id))
            await _distributedCache.RemoveAsync(GetPageTreeCacheKey(destinationSpace.HomePageId));

        return movedPage.ToModel(destinationSpace.Key);
    }


    public async Task<PageVersionsListModel> GetPageVersions(string pageId, UserInfo requestedBy)
    {
        await _permissionService.EnsureAnonymousAllowed(requestedBy);

        var page = await _pageManager.GetById(pageId) ?? throw new PageNotFoundException();
        var space = await _spaceManager.GetById(page.SpaceId);

        await _permissionService.EnsureViewPermission(space.Key, requestedBy);

        var versions = await _pageManager.GetAllVersions(page);
        return new PageVersionsListModel()
        {
            Versions = versions.Select(x => x.ToModel()).ToArray(),
            Count = versions.Length,
            LatestVersion = page.Version,
            LatestVersionDate = page.Updated
        };
    }

    public async Task<PageModel> RestoreVersion(string pageId, long version, UserInfo restoredBy)
    {
        await _permissionService.EnsureAnonymousAllowed(restoredBy);

        var page = await _pageManager.GetById(pageId) ?? throw new PageNotFoundException();
        var space = await _spaceManager.GetById(page.SpaceId);

        await _permissionService.EnsureEditPermission(space.Key, restoredBy);
        await _pageManager.RestoreVersion(page, version, restoredBy);
        return page.ToModel(space.Key);
    }

    public async Task DeleteVersion(string pageId, long version, UserInfo deletedBy)
    {
        await _permissionService.EnsureAnonymousAllowed(deletedBy);

        var page = await _pageManager.GetById(pageId) ?? throw new PageNotFoundException();
        var space = await _spaceManager.GetById(page.SpaceId);

        await _permissionService.EnsureEditPermission(space.Key, deletedBy);

        await _pageManager.RemoveVersion(page, version);
    }

    private static string GetPageTreeCacheKey(string pageId) => $"page_tree_{pageId}";
}