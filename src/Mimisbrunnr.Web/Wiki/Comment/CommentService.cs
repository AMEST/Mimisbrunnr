using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

namespace Mimisbrunnr.Web.Wiki;

internal class CommentService : ICommentService
{
    private readonly ICommentManager _commentManager;
    private readonly ISpaceManager _spaceManager;
    private readonly IPageManager _pageManager;
    private readonly IUserManager _userManager;
    private readonly IPermissionService _permissionService;

    public CommentService(ICommentManager commentManager,
        ISpaceManager spaceManager,
        IPageManager pageManager,
        IUserManager userManager,
        IPermissionService permissionService)
    {
        _commentManager = commentManager;
        _spaceManager = spaceManager;
        _pageManager = pageManager;
        _userManager = userManager;
        _permissionService = permissionService;
    }
    
    public async Task<IEnumerable<CommentModel>> GetComments(string pageId, UserInfo requestedBy)
    {
        await _permissionService.EnsureAnonymousAllowed(requestedBy);
        
        var page = await EnsurePageAndPermissions(pageId, requestedBy);
        
        var comments = await _commentManager.GetComments(page);

        return comments.Select(x => x.ToModel());
    }

    public async Task<CommentModel> GetById(string pageId, string commentId, UserInfo requestedBy)
    {
        await _permissionService.EnsureAnonymousAllowed(requestedBy);
        
        var page = await EnsurePageAndPermissions(pageId, requestedBy);

        var comment = await _commentManager.GetById(commentId);
        if (comment is null)
            throw new CommentNotFoundException();

        if (comment.PageId != page.Id)
            throw new UserHasNotPermissionException("Requested comment not owned to this page");

        return comment.ToModel();
    }

    public async Task<CommentModel> Create(string pageId, CommentCreateModel model, UserInfo createdBy)
    {
        ArgumentNullException.ThrowIfNull(createdBy, nameof(createdBy));
        ArgumentNullException.ThrowIfNull(model, nameof(model));
        if (string.IsNullOrWhiteSpace(model.Message))
            throw new ArgumentNullException(nameof(model.Message), "Message can't be null or empty");
        
        var page = await EnsurePageAndPermissions(pageId, createdBy);

        var comment = await _commentManager.Create(page, model.Message, createdBy);

        return comment.ToModel();
    }
    
    public async Task Remove(string pageId, string commentId, UserInfo deletedBy)
    {
        ArgumentNullException.ThrowIfNull(deletedBy, nameof(deletedBy));
        var page = await EnsurePageAndPermissions(pageId, deletedBy);
        var comment = await _commentManager.GetById(commentId);
        
        if(comment.PageId != page.Id)
            throw new UserHasNotPermissionException("Requested comment not owned to this page");

        var currentUser = await _userManager.GetByEmail(deletedBy.Email);
        if(currentUser.Role != UserRole.Admin 
           && !comment.Author.Email.Equals(deletedBy.Email, StringComparison.OrdinalIgnoreCase))
            throw new UserHasNotPermissionException("Requested comment not owned to this user");

        await _commentManager.Remove(comment);
    }
    
    private async Task<Page> EnsurePageAndPermissions(string pageId, UserInfo createdBy)
    {
        var page = await _pageManager.GetById(pageId);
        if (page is null)
            throw new PageNotFoundException();

        var space = await _spaceManager.GetById(page.SpaceId);
        if (space is null)
            throw new SpaceNotFoundException();

        await _permissionService.EnsureViewPermission(space.Key, createdBy);

        return page;
    }
}