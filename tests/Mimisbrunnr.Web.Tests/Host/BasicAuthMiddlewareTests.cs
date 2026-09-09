using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Mimisbrunnr.Web.Host.Configuration;
using Mimisbrunnr.Web.Host.Middlewares;

namespace Mimisbrunnr.Web.Tests.Host;

public class BasicAuthMiddlewareTests
{
    [Fact]
    public void Should_AuthorizeCaseInsensitiveUsername_WhenCredentialsMatch()
    {
        var middleware = new BasicAuthMiddleware(_ => Task.CompletedTask,
            new MetricsConfiguration { Username = "Metrics", Password = "secret" });

        middleware.IsAuthorized("metrics", "secret").Should().BeTrue();
    }

    [Fact]
    public async Task Should_ReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        var middleware = new BasicAuthMiddleware(_ => Task.CompletedTask,
            new MetricsConfiguration { Username = "metrics", Password = "secret" });
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes("metrics:wrong"));

        await middleware.Invoke(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
        context.Response.Headers.WWWAuthenticate.ToString().Should().Contain("Basic");
    }
}
