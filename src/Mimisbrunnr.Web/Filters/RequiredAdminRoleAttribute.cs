using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Mapping;

namespace Mimisbrunnr.Web.Filters;

public class RequiredAdminRoleAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var userManager = context.HttpContext.RequestServices.GetService<IUserManager>();
        var userInfo = context.HttpContext?.User.ToInfo();
        var user = await userManager.GetByEmail(userInfo?.Email);
        if(user is null || user.Role != UserRole.Admin)
            context.Result = new ForbidResult();
    }
}