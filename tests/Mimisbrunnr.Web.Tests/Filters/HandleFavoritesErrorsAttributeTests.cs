using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Mimisbrunnr.Integration.User;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Web.Infrastructure;

namespace Mimisbrunnr.Web.Tests.Filters;

public class HandleFavoritesErrorsAttributeTests
{
    private static async Task<ExceptionContext> InvokeFilterAsync(Exception exception)
    {
        var context = new ExceptionContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
            [])
        {
            Exception = exception
        };
        await new HandleFavoritesErrorsAttribute().OnExceptionAsync(context);
        return context;
    }

    [Fact]
    public async Task Should_Return403_WhenUserHasNoPermission()
    {
        var context = await InvokeFilterAsync(new UserHasNotPermissionException());

        context.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(403);
        context.ExceptionHandled.Should().BeTrue();
    }

    [Fact]
    public async Task Should_Return400_WhenInvalidOperation()
    {
        var context = await InvokeFilterAsync(new InvalidOperationException("bad"));

        context.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(400);
        context.ExceptionHandled.Should().BeTrue();
    }

    [Fact]
    public async Task Should_Return404_WhenSpaceNotFound()
    {
        var context = await InvokeFilterAsync(new SpaceNotFoundException());

        context.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(404);
        context.ExceptionHandled.Should().BeTrue();
    }

    [Fact]
    public async Task Should_Return404_WhenPageNotFound()
    {
        var context = await InvokeFilterAsync(new PageNotFoundException());

        context.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(404);
        context.ExceptionHandled.Should().BeTrue();
    }

    [Fact]
    public async Task Should_Return404_WhenUserNotFound()
    {
        var context = await InvokeFilterAsync(new UserNotFoundException());

        context.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(404);
        context.ExceptionHandled.Should().BeTrue();
    }
}
