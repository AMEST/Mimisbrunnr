using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Web.Infrastructure;

namespace Mimisbrunnr.Web.Tests.Filters;

public class HandlePluginErrorsAttributeTests
{
    private static async Task<ExceptionContext> InvokeFilterAsync(Exception exception)
    {
        var context = new ExceptionContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
            [])
        {
            Exception = exception
        };
        await new HandlePluginErrorsAttribute().OnExceptionAsync(context);
        return context;
    }

    [Fact]
    public async Task Should_Return404_WhenArgumentOutOfRangeException()
    {
        var context = await InvokeFilterAsync(new ArgumentOutOfRangeException("idx"));

        context.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Should_Return404_WhenPluginNotFound()
    {
        var context = await InvokeFilterAsync(new PluginNotFoundException());

        context.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(404);
        context.ExceptionHandled.Should().BeTrue();
    }

    [Fact]
    public async Task Should_Return404_WhenMacroNotFound()
    {
        var context = await InvokeFilterAsync(new MacroNotFoundException());

        context.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(404);
        context.ExceptionHandled.Should().BeTrue();
    }

    [Fact]
    public async Task Should_Return500_WhenRemoteMacroRenderFails()
    {
        var context = await InvokeFilterAsync(new RemoteMacroRenderException());

        context.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(500);
        context.ExceptionHandled.Should().BeTrue();
    }
}
