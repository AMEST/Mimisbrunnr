﻿using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

namespace Mimisbrunnr.Web.Wiki;

internal class PageService : IPageService
{
    private readonly IPageManager _pageManager;
    private readonly ISpaceManager _spaceManager;
    private readonly IPermissionService _permissionService;

    public PageService(IPageManager pageManager,
        ISpaceManager spaceManager,
        IPermissionService permissionService
    )
    {
        _pageManager = pageManager;
        _spaceManager = spaceManager;
        _permissionService = permissionService;
    }

    public async Task<PageModel> GetById(string pageId, UserInfo requestedBy)
    {
        await _permissionService.EnsureAnonymousAllowed(requestedBy);
        
        var page = await _pageManager.GetById(pageId);
        if (page == null)
            throw new PageNotFountException();
        var space = await _spaceManager.GetById(page.SpaceId);

        await _permissionService.EnsureViewPermission(space.Key, requestedBy);
        return page.ToModel(space.Key);
    }

    public async Task<PageTreeModel> GetPageTreeByPageId(string pageId, UserInfo requestedBy)
    {
        await _permissionService.EnsureAnonymousAllowed(requestedBy);
        
        var page = await _pageManager.GetById(pageId);
        if (page == null)
            throw new PageNotFountException();
        var space = await _spaceManager.GetById(page.SpaceId);

        await _permissionService.EnsureViewPermission(space.Key, requestedBy);
        var pageTree = await _pageManager.GetAllChilds(page);
        
        return pageTree?.ToModel(page, space);
    }

    public async Task<PageModel> Create(PageCreateModel createModel, UserInfo createdBy)
    {
        await _permissionService.EnsureEditPermission(createModel.SpaceKey, createdBy);
        var space = await _spaceManager.GetByKey(createModel.SpaceKey);
        if (space == null)
            throw new SpaceNotFoundException();

        var parentPage = await _pageManager.GetById(createModel.ParentPageId);
        if (parentPage == null || parentPage.SpaceId != space.Id)
            throw new PageNotFountException();

        var page = await _pageManager.Create(space.Id, createModel.Name, createModel.Content, createdBy,
            createModel.ParentPageId);
        
        return page.ToModel(space.Key);
    }

    public async Task Update(string pageId, PageUpdateModel updateModel, UserInfo updatedBy)
    {
        var page = await _pageManager.GetById(pageId);
        if (page == null)
            throw new PageNotFountException();
        
        var space = await _spaceManager.GetById(page.SpaceId);
        if (space == null)
            throw new SpaceNotFoundException();

        await _permissionService.EnsureEditPermission(space.Key, updatedBy);

        page.Name = updateModel.Name;
        page.Content = updateModel.Content;

        await _pageManager.Update(page, updatedBy);
    }

    public async Task Delete(string pageId, UserInfo deletedBy, bool recursively)
    {
        var page = await _pageManager.GetById(pageId);
        if (page == null)
            throw new PageNotFountException();
        
        var space = await _spaceManager.GetById(page.SpaceId);
        if (space == null)
            throw new SpaceNotFoundException();

        await _permissionService.EnsureRemovePermission(space.Key, deletedBy);

        if (space.HomePageId == pageId)
            throw new InvalidOperationException("Cannot remove home page of space");

        await _pageManager.Remove(page, recursively);
    }

    public async Task<PageModel> Copy(string sourcePageId, string destinationParentPageId, UserInfo copiedBy)
    {
        var sourcePage = await _pageManager.GetById(sourcePageId);
        if (sourcePage == null)
            throw new PageNotFountException($"Source page with id `{sourcePageId}` not found");
        
        var destinationParentPage = await _pageManager.GetById(destinationParentPageId);
        if (destinationParentPage == null)
            throw new PageNotFountException($"Destination parent page with id `{destinationParentPageId}` not found");
        
        var sourceSpace = await _spaceManager.GetById(sourcePage.SpaceId);
        
        await _permissionService.EnsureEditPermission(sourceSpace.Key, copiedBy);
        
        var destinationSpace = sourcePage.SpaceId == destinationParentPage.SpaceId
            ? sourceSpace
            : await _spaceManager.GetById(sourcePage.SpaceId);
        if (sourceSpace.Id != destinationSpace.Id)
            await _permissionService.EnsureEditPermission(destinationSpace.Id, copiedBy);

        var copiedPage = await _pageManager.Copy(sourcePage, destinationParentPage);
        
        return copiedPage.ToModel(destinationSpace.Key);
    }

    public async Task<PageModel> Move(string sourcePageId, string destinationParentPageId, UserInfo movedBy)
    {
        var sourcePage = await _pageManager.GetById(sourcePageId);
        if (sourcePage == null)
            throw new PageNotFountException($"Source page with id `{sourcePageId}` not found");
        
        var destinationParentPage = await _pageManager.GetById(destinationParentPageId);
        if (destinationParentPage == null)
            throw new PageNotFountException($"Destination parent page with id `{destinationParentPageId}` not found");
        
        var sourceSpace = await _spaceManager.GetById(sourcePage.SpaceId);
        
        await _permissionService.EnsureEditPermission(sourceSpace.Key, movedBy);
        
        var destinationSpace = sourcePage.SpaceId == destinationParentPage.SpaceId
            ? sourceSpace
            : await _spaceManager.GetById(sourcePage.SpaceId);
        if (sourceSpace.Id != destinationSpace.Id)
        {
            await _permissionService.EnsureRemovePermission(sourceSpace.Key, movedBy);
            await _permissionService.EnsureEditPermission(destinationSpace.Id, movedBy);
        }

        var movedPage = await _pageManager.Move(sourcePage, destinationParentPage);

        return movedPage.ToModel(destinationSpace.Key);
    }
}