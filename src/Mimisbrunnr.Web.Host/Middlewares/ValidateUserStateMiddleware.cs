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
        var userInfo = context.User?.ToEntity();
        if (userInfo == null)
        {
            await _next(context);
            return;
        }

        var user = await userManager.GetByEmail(userInfo?.Email);
        if (user == null || user.Enable)
        {
            await SyncUser(userManager, user, userInfo);
            await _next(context);
            return;
        }

        await context.SignOutAsync();

        var appConfig = await applicationConfigurationService.Get();
        if (appConfig.AllowAnonymous)
            await _next(context);
        else
            context.Response.Redirect("/error/account-disabled");
    }

    private async Task SyncUser(IUserManager userManager, Users.User user, UserInfo userInfo)
    {
        if (user.AvatarUrl == userInfo.AvatarUrl
            && user.Name != userInfo.Name)
            return;

        user.AvatarUrl = userInfo.AvatarUrl;
        user.Name = userInfo.Name;
        await userManager.UpdateUserInfo(user);
    }
}