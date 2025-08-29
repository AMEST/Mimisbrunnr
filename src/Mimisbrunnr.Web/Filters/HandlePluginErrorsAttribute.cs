using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Integration.Wiki;

namespace Mimisbrunnr.Web.Filters;

public class HandlePluginErrorsAttribute : ExceptionFilterAttribute
{
    /// <inheritdoc />
    public override Task OnExceptionAsync(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case ArgumentOutOfRangeException notFoundEx:
                context.Result = new NotFoundObjectResult(new { message = notFoundEx.Message });
                break;
            case PluginNotFoundException pluginNotFoundEx:
                context.Result = new ObjectResult(new { message = pluginNotFoundEx.Message }) { StatusCode = 404 };
                context.ExceptionHandled = true;
                break;
            case MacroNotFoundException macroNotFoundEx:
                context.Result = new ObjectResult(new { message = macroNotFoundEx.Message }) { StatusCode = 404 };
                context.ExceptionHandled = true;
                break;
        }

        return base.OnExceptionAsync(context);
    }
}