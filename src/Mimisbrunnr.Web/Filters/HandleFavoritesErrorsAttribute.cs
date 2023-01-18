using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Integration.Wiki;

namespace Mimisbrunnr.Web.Filters;

public class HandleFavoritesErrorsAttribute : ExceptionFilterAttribute
{
    /// <inheritdoc />
    public override Task OnExceptionAsync(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case UserHasNotPermissionException userHasNotPermissionEx:
                context.Result = new ObjectResult(new { message = userHasNotPermissionEx.Message })
                    { StatusCode = 403 };
                context.ExceptionHandled = true;
                break;
            case InvalidOperationException invalidOperationException:
                context.Result = new ObjectResult(new { message = invalidOperationException.Message })
                    { StatusCode = 400 };
                context.ExceptionHandled = true;
                break;
        }

        return base.OnExceptionAsync(context);
    }
}