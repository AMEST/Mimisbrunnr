using Microsoft.AspNetCore.Authorization;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Mapping;

namespace Mimisbrunnr.Web.Host.Services;

internal class EnsureUserAuthorizationHandler : IAuthorizationHandler
{
    private readonly IUserManager _userManager;
    private readonly IApplicationConfigurationManager _applicationConfigurationManager;
    private readonly ILogger<EnsureUserAuthorizationHandler> _logger;

    public EnsureUserAuthorizationHandler(IUserManager userManager,
        IApplicationConfigurationManager applicationConfigurationManager,
        ILogger<EnsureUserAuthorizationHandler> logger)
    {
        _userManager = userManager;
        _applicationConfigurationManager = applicationConfigurationManager;
        _logger = logger;
    }

    public async Task HandleAsync(AuthorizationHandlerContext context)
    {
        if (context.User == null || context.User.ToInfo() == null)
            return;

        var userInfo = context.User.ToInfo();
        var applicationUser = await _userManager.GetByEmail(userInfo.Email);
        if (applicationUser is not null)
            return;

        var appConfiguration = await _applicationConfigurationManager.Get();
        if (!appConfiguration.UserAutoCreation)
        {
            _logger.LogWarning("New user authorized but automatic user creation disabled. Need create user manual (via api) or ignore. \nEmail: {Email}\nName: {Name}\nAvatar: {Avatar}",
                userInfo?.Email, userInfo?.Name, userInfo?.AvatarUrl);
            return;
        }
        _logger.LogDebug("Add new user `{Email}`", userInfo.Email);
        await _userManager.Add(userInfo.Email, userInfo.Name, userInfo.AvatarUrl, UserRole.Employee);
    }
}