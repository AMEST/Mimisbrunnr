using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Mimisbrunnr.Web.Group;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Wiki;

namespace Mimisbrunnr.Web.Filters;

public class HandleGroupErrorsAttribute : ExceptionFilterAttribute
{
    /// <inheritdoc />
    public override Task OnExceptionAsync(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case ArgumentOutOfRangeException notFoundEx:
                context.Result = new NotFoundObjectResult(new { message = notFoundEx.Message });
                break;
            case GroupNotFoundException groupNotFoundEx:
                context.Result = new NotFoundObjectResult(new { message = groupNotFoundEx.Message });
                break;
            case UserHasNotPermissionException userHasNotPermissionEx:
                context.Result = new ObjectResult(new { message = userHasNotPermissionEx.Message })
                    { StatusCode = 403 };
                context.ExceptionHandled = true;
                break;
        }

        return base.OnExceptionAsync(context);
    }
}