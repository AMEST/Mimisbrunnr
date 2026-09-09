using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Mimisbrunnr.Web.Host.Middlewares;
using Mimisbrunnr.Web.Infrastructure;

namespace Mimisbrunnr.Web.Tests.Host;

public class ValidateTokenNotRevokedMiddlewareTests
{
    [Fact]
    public async Task Should_ReturnUnauthorized_WhenBearerTokenIsRevoked()
    {
        var nextCalled = false;
        var middleware = new ValidateTokenNotRevokedMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var tokenService = A.Fake<ISecurityTokenService>();
        A.CallTo(() => tokenService.EnsureTokenNotRevoked("token")).Returns(Task.FromResult(false));
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = "Bearer token";

        await middleware.InvokeAsync(context, tokenService);

        context.Response.StatusCode.Should().Be(401);
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Should_InvokeNext_WhenAuthorizationHeaderIsMissing()
    {
        var nextCalled = false;
        var middleware = new ValidateTokenNotRevokedMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(new DefaultHttpContext(), A.Fake<ISecurityTokenService>());

        nextCalled.Should().BeTrue();
    }
}
