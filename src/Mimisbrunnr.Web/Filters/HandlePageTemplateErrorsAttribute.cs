using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.PageTemplates.Contracts;
using Mimisbrunnr.Web.Infrastructure;

namespace Mimisbrunnr.Web.Filters;

public class HandlePageTemplateErrorsAttribute : ExceptionFilterAttribute
{
    public override Task OnExceptionAsync(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case PageTemplateNotFoundException notFoundEx:
                context.Result = new ObjectResult(new { message = notFoundEx.Message }) { StatusCode = 404 };
                context.ExceptionHandled = true;
                break;
            case UserHasNotPermissionException userHasNotPermissionEx:
                context.Result = new ObjectResult(new { message = userHasNotPermissionEx.Message }) { StatusCode = 403 };
                context.ExceptionHandled = true;
                break;
            case SpaceNotFoundException spaceNotFoundEx:
                context.Result = new ObjectResult(new { message = spaceNotFoundEx.Message }) { StatusCode = 404 };
                context.ExceptionHandled = true;
                break;
            case InvalidOperationException invalidOperationException:
                context.Result = new ObjectResult(new { message = invalidOperationException.Message }) { StatusCode = 400 };
                context.ExceptionHandled = true;
                break;
        }

        return base.OnExceptionAsync(context);
    }
}
