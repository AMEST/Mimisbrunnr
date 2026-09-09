using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Web.Infrastructure;

namespace Mimisbrunnr.Web.Tests.Filters;

public class HandleSearchErrorsAttributeTests
{
    private static async Task<ExceptionContext> InvokeFilterAsync(Exception exception)
    {
        var context = new ExceptionContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
            [])
        {
            Exception = exception
        };
        await new HandleSearchErrorsAttribute().OnExceptionAsync(context);
        return context;
    }

    [Fact]
    public async Task Should_Return401_WhenAnonymousNotAllowed()
    {
        var context = await InvokeFilterAsync(new AnonymousNotAllowedException());

        context.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(401);
        context.ExceptionHandled.Should().BeTrue();
    }

    [Fact]
    public async Task Should_Return400_WhenInvalidOperation()
    {
        var context = await InvokeFilterAsync(new InvalidOperationException("bad"));

        context.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(400);
        context.ExceptionHandled.Should().BeTrue();
    }
}
