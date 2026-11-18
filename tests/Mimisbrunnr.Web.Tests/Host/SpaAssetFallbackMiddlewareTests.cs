using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Mimisbrunnr.Web.Host.Middlewares;

namespace Mimisbrunnr.Web.Tests.Host;

public class SpaAssetFallbackMiddlewareTests
{
    [Fact]
    public async Task Should_InvokeNext_WhenStaticLookingPathHasMatchedEndpoint()
    {
        var nextCalled = false;
        var middleware = new SpaAssetFallbackMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        context.Request.Path = "/assets/generated.js";
        context.SetEndpoint(new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(), "generated asset"));

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Theory]
    [InlineData("/js/profile.12345678.js")]
    [InlineData("/css/app.12345678.css")]
    [InlineData("/JS/profile.12345678.js")]
    [InlineData("/img/missing.svg")]
    [InlineData("/assets/missing.html")]
    [InlineData("/service-worker.js")]
    [InlineData("/asset-recovery.js")]
    [InlineData("/workbox-12345678.js")]
    [InlineData("/manifest.json")]
    [InlineData("/favicon.ico")]
    [InlineData("/robots.txt")]
    public async Task Should_ReturnUncachedNotFound_WhenStaticAssetIsMissing(string path)
    {
        var nextCalled = false;
        var middleware = new SpaAssetFallbackMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        context.Request.Path = path;

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        context.Response.Headers.CacheControl.ToString().Should().Be("no-store");
        nextCalled.Should().BeFalse();
    }

    [Theory]
    [InlineData("/")]
    [InlineData("")]
    [InlineData("/profile/person@example.com")]
    [InlineData("/space/KEY/123/edit")]
    [InlineData("/jsmith")]
    [InlineData("/css-guide")]
    public async Task Should_InvokeSpaFallback_WhenPathIsAnApplicationRoute(string path)
    {
        var nextCalled = false;
        var middleware = new SpaAssetFallbackMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        context.Request.Path = path;

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }
}
