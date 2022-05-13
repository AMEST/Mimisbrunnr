using System.Security.Claims;
using Mimisbrunnr.Web.Authentication.Account;
using Mimisbrunnr.Web.User;
using Mimisbrunnr.Web.Infrastructure.Contracts;
using Mimisbrunnr.Web.Quickstart;
using Mimisbrunnr.Web.Wiki;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Web.Feed;

namespace Mimisbrunnr.Web.Mapping;

public static class MappingExtensions
{
    public static UserInfo ToEntity(this ClaimsPrincipal principal)
    {
        var user = new UserInfo
        {
            Email = principal.FindFirst(ClaimTypes.Email)?.Value?.ToLower() ?? principal.FindFirst("email")?.Value?.ToLower(),
            Name = principal.Identity?.Name,
            AvatarUrl = principal.FindFirst("picture")?.Value
        };
        if (string.IsNullOrEmpty(user.Email))
            return null;
        return user;
    }

    public static ApplicationConfiguration ToEntity(this QuickstartModel model)
    {
        return new ApplicationConfiguration()
        {
            Title = model.Title,
            AllowAnonymous = model.AllowAnonymous,
            SwaggerEnabled = model.SwaggerEnabled,
            AllowHtml = model.AllowHtml
        };
    }

    public static UserInfo ToEntity(this UserModel model)
    {
        return new UserInfo()
        {
            Email = model.Email.ToLower(),
            Name = model.Name,
            AvatarUrl = model.AvatarUrl
        };
    }

    public static GroupInfo ToEntity(this GroupModel model)
    {
        return new GroupInfo()
        {
            Name = model.Name
        };
    }

    public static Permission ToEntity(this SpacePermissionModel model)
    {
        return new Permission()
        {
            Group = model.Group?.ToEntity(),
            User = model.User?.ToEntity(),
            CanEdit = model.CanEdit,
            CanRemove = model.CanRemove,
            CanView = model.CanView,
            IsAdmin = model.IsAdmin
        };
    }

    public static QuickstartModel ToModel(this ApplicationConfiguration model)
    {
        return new QuickstartModel()
        {
            Title = model?.Title ?? "Mimisbrunnr",
            AllowAnonymous = model?.AllowAnonymous ?? false,
            AllowHtml = model?.AllowHtml ?? true,
            SwaggerEnabled = model?.SwaggerEnabled ?? false
        };
    }

    public static UserModel ToModel(this UserInfo user)
    {
        return new UserModel()
        {
            Email = user.Email.ToLower(),
            Name = user.Name,
            AvatarUrl = user.AvatarUrl
        };
    }

    public static UserModel ToModel(this Mimisbrunnr.Users.User user)
    {
        return new UserModel()
        {
            Email = user.Email.ToLower(),
            Name = user.Name,
            AvatarUrl = user.AvatarUrl
        };
    }

    public static GroupModel ToModel(this GroupInfo group)
    {
        return new GroupModel()
        {
            Name = group.Name,
        };
    }

    public static SpaceModel ToModel(this Space space)
    {
        return new SpaceModel()
        {
            Key = space.Key,
            Name = space.Name,
            Description = space.Description,
            Type = (SpaceTypeModel)space.Type,
            HomePageId = space.HomePageId
        };
    }

    public static PageModel ToModel(this Page page, string spaceKey = null)
    {
        return new PageModel()
        {
            Id = page.Id,
            SpaceKey = spaceKey,
            Name = page.Name,
            Content = page.Content,
            Created = page.Created,
            CreatedBy = page.CreatedBy?.ToModel(),
            Updated = page.Updated,
            UpdatedBy = page.UpdatedBy?.ToModel()
        };
    }

    public static PageTreeModel ToModel(this Page[] childs, Page rootPage, Space space = null)
    {
        var pageTree = new PageTreeModel
        {
            Page = rootPage?.ToModel(space.Key)
        };
        var childsPages = childs.Where(x => x.ParentId == rootPage?.Id).ToArray().Select(x => childs.ToModel(x, space)).ToList();
        pageTree.Childs = childsPages;
        return pageTree;
    }

    public static UserPermissionModel ToModel(this Permission permission)
    {
        return new UserPermissionModel()
        {
            CanEdit = permission.CanEdit,
            CanRemove = permission.CanRemove,
            CanView = permission.CanView,
            IsAdmin = permission.IsAdmin
        };
    }

    public static SpacePermissionModel ToSpacePermissionModel(this Permission permission)
    {
        return new SpacePermissionModel()
        {
            Group = permission.Group?.ToModel(),
            User = permission.User?.ToModel(),
            CanEdit = permission.CanEdit,
            CanRemove = permission.CanRemove,
            CanView = permission.CanView,
            IsAdmin = permission.IsAdmin
        };
    }

    public static PageUpdateEventModel ToModel(this PageUpdateEvent pageUpdateEvent)
    {
        return new PageUpdateEventModel()
        {
            PageTitle = pageUpdateEvent.PageTitle,
            PageId = pageUpdateEvent.PageId,
            SpaceKey = pageUpdateEvent.SpaceKey,
            Updated = pageUpdateEvent.Updated,
            UpdatedBy = pageUpdateEvent.UpdatedBy.ToModel()
        };
    }

    public static AttachmentModel ToModel(this Attachment attachment)
    {
        return new AttachmentModel(){
            Name = attachment.Name,
            Created = attachment.Created,
            CreatedBy = attachment.CreatedBy?.ToModel()
        };
    }
}