using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Mimisbrunnr.Web.Infrastructure;

namespace Mimisbrunnr.Web.Filters;

public class HandleSearchErrorsAttribute : ExceptionFilterAttribute
{
    /// <inheritdoc />
    public override Task OnExceptionAsync(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case AnonymousNotAllowedException anonymousNotAllowedEx:
                context.Result = new ObjectResult(new { message = anonymousNotAllowedEx.Message }) { StatusCode = 401 };
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