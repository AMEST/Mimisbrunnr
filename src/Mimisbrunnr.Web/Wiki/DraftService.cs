using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;
using Mimisbrunnr.Integration.Wiki;

namespace Mimisbrunnr.Web.Wiki;

internal class DraftService : IDraftService
{
    private readonly IDraftManager _draftManager;
    private readonly IPageManager _pageManager;
    private readonly ISpaceManager _spaceManager;
    private readonly IPermissionService _permissionService;

    public DraftService(
        IDraftManager draftManager,
        IPageManager pageManager,
        ISpaceManager spaceManager,
        IPermissionService permissionService
    )
    {
        _draftManager = draftManager;
        _pageManager = pageManager;
        _spaceManager = spaceManager;
        _permissionService = permissionService;
    }

    public async Task Delete(string pageId, UserInfo deletedBy)
    {
        await EnsureDraftPermission(pageId, deletedBy);

        await _draftManager.Remove(pageId);
    }

    public async Task<DraftModel> GetByPageId(string pageId, UserInfo requestedBy)
    {
        await EnsureDraftPermission(pageId, requestedBy);
        var draft = await _draftManager.GetByPageId(pageId);
        return WikiMapper.Instance.ToModel(draft);
    }

    public async Task Update(string pageId, DraftUpdateModel updateModel, UserInfo updatedBy)
    {
        await EnsureDraftPermission(pageId, updatedBy);
        var draft = await _draftManager.GetByPageId(pageId);
        if (draft is null)
        {
            await _draftManager.Create(pageId, updateModel.Name, updateModel.Content, updatedBy);
            return;
        }

        draft.Name = updateModel.Name;
        draft.Content = updateModel.Content;
        await _draftManager.Update(draft, updatedBy);
    }

    private async Task EnsureDraftPermission(string pageId, UserInfo requestedBy)
    {
        await _permissionService.EnsureAnonymousAllowed(requestedBy);

        var page = await _pageManager.GetById(pageId);
        if (page is null)
            throw new PageNotFoundException();

        var space = await _spaceManager.GetById(page.SpaceId);
        if (space is null)
            throw new SpaceNotFoundException();

        await _permissionService.EnsureEditPermission(space.Key, requestedBy);
    }
}