using Microsoft.AspNetCore.Authorization;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Mapping;

namespace Mimisbrunnr.Web.Host.Services;

internal class EnsureUserAuthorizationHandler : IAuthorizationHandler
{
    private readonly IUserManager _userManager;
    private readonly ILogger<EnsureUserAuthorizationHandler> _logger;

    public EnsureUserAuthorizationHandler(IUserManager userManager, ILogger<EnsureUserAuthorizationHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task HandleAsync(AuthorizationHandlerContext context)
    {
        if (context.User == null || context.User.ToEntity() == null)
            return;

        var userInfo = context.User.ToEntity();
        var applicationUser = await _userManager.GetByEmail(userInfo.Email);
        if (applicationUser == null)
        {
            _logger.LogDebug("Add new user `{Email}`", userInfo.Email);
            await _userManager.Add(userInfo.Email, userInfo.Name, userInfo.AvatarUrl, UserRole.Employee);
        }
    }
}