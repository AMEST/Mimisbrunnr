using Microsoft.AspNetCore.Authentication;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Mapping;

namespace Mimisbrunnr.Web.Host.Middlewares;

internal class ValidateUserStateMiddleware
{
    private readonly RequestDelegate _next;

    public ValidateUserStateMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserManager userManager, IApplicationConfigurationService applicationConfigurationService)
    {
        var userInfo = context.User?.ToEntity();
        if ( userInfo == null ){
            await _next(context);
            return;
        }

        var user = await userManager.GetByEmail(userInfo?.Email); 
        if ( user == null || user.Enable ){
            await _next(context);
            return;
        }

        await context.SignOutAsync();

        var appConfig = await applicationConfigurationService.Get();
        if(appConfig.AllowAnonymous)
            await _next(context);
        else
            context.Response.Redirect("/error/account-disabled");
    }
}