using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Mimisbrunnr.Web.Infrastructure;

namespace Mimisbrunnr.Web.Filters;

public class HandleWikiErrorsAttribute: ExceptionFilterAttribute
{
    /// <inheritdoc />
    public override Task OnExceptionAsync(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case ArgumentOutOfRangeException notFoundEx:
                context.Result = new NotFoundObjectResult(new {message = notFoundEx.Message});
                break;

            case AnonymousNotAllowedException anonymousNotAllowedEx:
                context.Result = new ObjectResult(new { message = anonymousNotAllowedEx.Message }) { StatusCode = 401 };
                context.ExceptionHandled = true;
                break;
            case UserHasNotPermissionException userHasNotPermissionEx:
                context.Result =  new ObjectResult(new { message = userHasNotPermissionEx.Message }) { StatusCode = 403 };
                context.ExceptionHandled = true;
                break;
        }

        return base.OnExceptionAsync(context);
    }
}