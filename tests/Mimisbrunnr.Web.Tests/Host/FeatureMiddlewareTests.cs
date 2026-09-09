using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Mimisbrunnr.Web.Host.Middlewares;
using Mimisbrunnr.Web.Host.Services.Features;

namespace Mimisbrunnr.Web.Tests.Host;

public class FeatureMiddlewareTests
{
    [Fact]
    public async Task Should_InvokeNext_WhenFeatureIsEnabled()
    {
        var nextCalled = false;
        var middleware = new FeatureMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, "feature");
        var features = A.Fake<IFeatureService>();
        A.CallTo(() => features.IsFeatureEnabled("feature")).Returns(Task.FromResult(true));

        await middleware.InvokeAsync(new DefaultHttpContext(), features);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Should_ReturnNotFound_WhenFeatureIsDisabled()
    {
        var nextCalled = false;
        var middleware = new FeatureMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, "feature");
        var features = A.Fake<IFeatureService>();
        A.CallTo(() => features.IsFeatureEnabled("feature")).Returns(Task.FromResult(false));
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context, features);

        context.Response.StatusCode.Should().Be(404);
        nextCalled.Should().BeFalse();
    }
}
