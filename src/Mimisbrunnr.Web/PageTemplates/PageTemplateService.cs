using Mimisbrunnr.Integration.PageTemplates;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.PageTemplates.Contracts;
using Mimisbrunnr.PageTemplates.Services;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

namespace Mimisbrunnr.Web.PageTemplates;

internal class PageTemplateService : IPageTemplateService
{
    private readonly IPageTemplateManager _pageTemplateManager;
    private readonly ISpaceManager _spaceManager;
    private readonly IPermissionService _permissionService;
    private readonly IUserManager _userManager;
    private readonly ITemplateRenderer _templateRenderer;

    public PageTemplateService(
        IPageTemplateManager pageTemplateManager,
        ISpaceManager spaceManager,
        IPermissionService permissionService,
        IUserManager userManager,
        ITemplateRenderer templateRenderer)
    {
        _pageTemplateManager = pageTemplateManager;
        _spaceManager = spaceManager;
        _permissionService = permissionService;
        _userManager = userManager;
        _templateRenderer = templateRenderer;
    }

    public async Task<PageTemplateModel[]> GetAll(string type, string spaceKey, UserInfo user)
    {
        var query = _pageTemplateManager.GetAll();

        var templates = new List<PageTemplate>();

        if (string.IsNullOrEmpty(type) || type == TemplateType.System)
        {
            var systemTemplates = query.Where(x => x.Type == TemplateType.System).ToArray();
            templates.AddRange(systemTemplates);
        }

        if (string.IsNullOrEmpty(type) || type == TemplateType.User)
        {
            if (user != null)
            {
                var userTemplates = query.Where(x => x.Type == TemplateType.User && x.OwnerEmail == user.Email).ToArray();
                templates.AddRange(userTemplates);
            }
        }

        if (string.IsNullOrEmpty(type) || type == TemplateType.Space)
        {
            if (!string.IsNullOrEmpty(spaceKey))
            {
                try {
                    await _permissionService.EnsureEditPermission(spaceKey, user);
                    var space = await _spaceManager.GetByKey(spaceKey);
                    var spaceTemplates = query.Where(x => x.Type == TemplateType.Space && x.SpaceId == space.Id).ToArray();
                    templates.AddRange(spaceTemplates);
                }
                catch (UserHasNotPermissionException)
                {
                    //do nothing
                }
            }
        }

        return templates.Select(x => x.ToModel()).ToArray();
    }

    public async Task<PageTemplateModel> GetById(string id, UserInfo user)
    {
        var template = await _pageTemplateManager.GetById(id);
        await EnsureReadPermission(template, user);
        return template.ToModel();
    }

    public async Task<PageTemplateModel> Create(PageTemplateCreateModel model, UserInfo user)
    {
        await EnsureCreatePermission(model.Type, model.SpaceKey, user);

        var spaceId = default(string);
        var ownerEmail = user.Email;
        if (model.Type == TemplateType.Space)
        {
            var space = await _spaceManager.GetByKey(model.SpaceKey);
            if (space == null)
                throw new SpaceNotFoundException($"Space with key '{model.SpaceKey}' not found");
            spaceId = space.Id;
        }

        var template = new PageTemplate
        {
            Name = model.Name,
            Description = model.Description,
            Content = model.Content,
            Type = model.Type,
            OwnerEmail = ownerEmail,
            SpaceId = spaceId
        };

        var result = await _pageTemplateManager.Create(template);
        return result.ToModel();
    }

    public async Task Update(string id, PageTemplateUpdateModel model, UserInfo user)
    {
        var template = await _pageTemplateManager.GetById(id);
        await EnsureModifyPermission(template, user);

        await _pageTemplateManager.Update(id, model.Name, model.Description, model.Content, user);
    }

    public async Task Delete(string id, UserInfo user)
    {
        var template = await _pageTemplateManager.GetById(id);
        await EnsureModifyPermission(template, user);

        await _pageTemplateManager.Delete(id);
    }

    public async Task<PageTemplateRenderResponse> Render(string templateId, string spaceKey, UserInfo user)
    {
        var template = await _pageTemplateManager.GetById(templateId);
        await EnsureReadPermission(template, user);

        var space = await _spaceManager.GetByKey(spaceKey);
        if (space == null)
            throw new SpaceNotFoundException($"Space with key '{spaceKey}' not found");
        await _permissionService.EnsureViewPermission(spaceKey, user);

        var parameters = new Dictionary<string, object>
        {
            ["CurrentDate"] = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            ["CurrentTime"] = DateTime.UtcNow.ToString("HH:mm:ss"),
            ["CurrentDateTime"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            ["UserName"] = user?.Name ?? string.Empty,
            ["UserEmail"] = user?.Email ?? string.Empty,
            ["UserAvatarUrl"] = user?.AvatarUrl ?? string.Empty,
            ["SpaceName"] = space.Name,
            ["SpaceKey"] = space.Key
        };

        var rendered = await _templateRenderer.Render(template.Content, parameters);
        return new PageTemplateRenderResponse { Content = rendered };
    }

    private async Task EnsureCreatePermission(string type, string spaceKey, UserInfo user)
    {
        switch (type)
        {
            case TemplateType.System:
                await EnsureGlobalAdmin(user);
                break;
            case TemplateType.User:
                break;
            case TemplateType.Space:
                if (string.IsNullOrEmpty(spaceKey))
                    throw new InvalidOperationException("SpaceKey is required for Space templates");
                await _permissionService.EnsureAdminPermission(spaceKey, user);
                break;
            default:
                throw new InvalidOperationException($"Unknown template type: {type}");
        }
    }

    private async Task EnsureReadPermission(PageTemplate template, UserInfo user)
    {
        switch (template.Type)
        {
            case TemplateType.System:
                break;
            case TemplateType.User:
                if (user == null || template.OwnerEmail != user.Email)
                    throw new UserHasNotPermissionException();
                break;
            case TemplateType.Space:
                if (!string.IsNullOrEmpty(template.SpaceId))
                {
                    var space = await _spaceManager.GetById(template.SpaceId);
                    if (space != null)
                        await _permissionService.EnsureEditPermission(space.Key, user);
                }
                break;
        }
    }

    private async Task EnsureModifyPermission(PageTemplate template, UserInfo user)
    {
        switch (template.Type)
        {
            case TemplateType.System:
                await EnsureGlobalAdmin(user);
                break;
            case TemplateType.User:
                if (user == null || template.OwnerEmail != user.Email)
                    throw new UserHasNotPermissionException();
                break;
            case TemplateType.Space:
                if (!string.IsNullOrEmpty(template.SpaceId))
                {
                    var space = await _spaceManager.GetById(template.SpaceId);
                    if (space != null)
                        await _permissionService.EnsureAdminPermission(space.Key, user);
                }
                break;
        }
    }

    private async Task EnsureGlobalAdmin(UserInfo user)
    {
        if (user == null)
            throw new UserHasNotPermissionException();

        var dbUser = await _userManager.GetByEmail(user.Email);
        if (dbUser == null || dbUser.Role != UserRole.Admin)
            throw new UserHasNotPermissionException("Only global administrators can manage system templates");
    }
}
