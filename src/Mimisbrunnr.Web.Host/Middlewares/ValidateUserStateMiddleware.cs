using Microsoft.AspNetCore.Authentication;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Host.Middlewares;

internal class ValidateUserStateMiddleware
{
    private readonly RequestDelegate _next;

    public ValidateUserStateMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserManager userManager, IApplicationConfigurationManager applicationConfigurationService)
    {
        var userInfo = context.User.ToInfo();
        if (userInfo is null)
        {
            await _next(context);
            return;
        }

        var user = await userManager.GetByEmail(userInfo?.Email);
        if (user is not null && user.Enable)
        {
            await SyncUser(userManager, user, userInfo);
            await _next(context);
            return;
        }

        var appConfig = await applicationConfigurationService.Get();        
        if ( user is null && appConfig.UserAutoCreation)
        {
            await _next(context);
            return;
        }

        await context.SignOutAsync();

        if (appConfig.AllowAnonymous)
        {
            await _next(context);
            return;
        }
        context.Response.Redirect("/error/account-disabled");
    }

    private static async Task SyncUser(IUserManager userManager, Users.User user, UserInfo userInfo)
    {
        var nameChanged = !string.IsNullOrEmpty(userInfo.Name) && user.Name != userInfo.Name;
        var avatarChanged = !string.IsNullOrEmpty(userInfo.AvatarUrl) && user.AvatarUrl != userInfo.AvatarUrl;
        // Nothing to sync: do not write the user document on every request.
        if (!nameChanged && !avatarChanged)
            return;

        if (nameChanged)
            user.Name = userInfo.Name;
        if (avatarChanged)
            user.AvatarUrl = userInfo.AvatarUrl;

        await userManager.UpdateUserInfo(user);
    }
}