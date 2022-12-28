using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;
using Mimisbrunnr.Integration.Wiki;

namespace Mimisbrunnr.Web.Wiki;

internal class AttachmentService : IAttachmentService
{
    private readonly IAttachmentManager _attachmentManager;
    private readonly ISpaceManager _spaceManager;
    private readonly IPageManager _pageManager;
    private readonly IPermissionService _permissionService;

    public AttachmentService(IAttachmentManager attachmentManager, ISpaceManager spaceManager, IPageManager pageManager, IPermissionService permissionService)
    {
        _attachmentManager = attachmentManager;
        _spaceManager = spaceManager;
        _pageManager = pageManager;
        _permissionService = permissionService;
    }

    public async Task<AttachmentModel[]> GetAttachments(string pageId, UserInfo requestedBy)
    {
        var page = await _pageManager.GetById(pageId);
        if (page is null)
            throw new PageNotFoundException();

        var space = await _spaceManager.GetById(page.SpaceId);
        if (space is null)
            throw new SpaceNotFoundException();

        await _permissionService.EnsureViewPermission(space.Key, requestedBy);

        var attachments = await _attachmentManager.GetAttachments(page);

        return attachments?.Select(WikiMapper.Instance.ToModel).ToArray();
    }

    public async Task<Stream> GetAttachmentContent(string pageId, string name, UserInfo requestedBy)
    {
        await _permissionService.EnsureAnonymousAllowed(requestedBy);
        var page = await _pageManager.GetById(pageId);
        if (page is null)
            throw new PageNotFoundException();

        var space = await _spaceManager.GetById(page.SpaceId);
        if (space is null)
            throw new SpaceNotFoundException();

        await _permissionService.EnsureViewPermission(space.Key, requestedBy);

        return await _attachmentManager.GetAttachmentContent(page, name);
    }

    public async Task Remove(string pageId, string name, UserInfo removedBy)
    {
        var page = await _pageManager.GetById(pageId);
        if (page is null)
            throw new PageNotFoundException();

        var space = await _spaceManager.GetById(page.SpaceId);
        if (space is null)
            throw new SpaceNotFoundException();

        await _permissionService.EnsureRemovePermission(space.Key, removedBy);

        await _attachmentManager.Remove(page, name);
    }

    public async Task Upload(string pageId, Stream content, string name, UserInfo uploadedBy)
    {
        var page = await _pageManager.GetById(pageId);
        if (page is null)
            throw new PageNotFoundException();

        var space = await _spaceManager.GetById(page.SpaceId);
        if (space is null)
            throw new SpaceNotFoundException();

        await _permissionService.EnsureEditPermission(space.Key, uploadedBy);

        await _attachmentManager.Upload(page, content, name, uploadedBy);
    }
}
