using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Mimisbrunnr.Integration.Group;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Web.Infrastructure;

namespace Mimisbrunnr.Web.Tests.Filters;

public class HandleGroupErrorsAttributeTests
{
    private static async Task<ExceptionContext> InvokeFilterAsync(Exception exception)
    {
        var context = new ExceptionContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
            [])
        {
            Exception = exception
        };
        await new HandleGroupErrorsAttribute().OnExceptionAsync(context);
        return context;
    }

    [Fact]
    public async Task Should_Return404_WhenArgumentOutOfRangeException()
    {
        var context = await InvokeFilterAsync(new ArgumentOutOfRangeException("idx"));

        context.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Should_Return404_WhenGroupNotFound()
    {
        var context = await InvokeFilterAsync(new GroupNotFoundException());

        context.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Should_Return403_WhenUserHasNoPermission()
    {
        var context = await InvokeFilterAsync(new UserHasNotPermissionException());

        context.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(403);
        context.ExceptionHandled.Should().BeTrue();
    }
}
