using System.Security.Claims;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Host.Middlewares;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Infrastructure.Contracts;

namespace Mimisbrunnr.Web.Tests.Host;

public class ValidateUserStateMiddlewareTests
{
    private static DefaultHttpContext Context(string email, string name = "User")
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, name)
            ], "test"))
        };
        return context;
    }

    [Fact]
    public async Task Should_InvokeNext_WhenUserIsEnabled()
    {
        var nextCalled = false;
        var users = A.Fake<IUserManager>();
        var user = new Mimisbrunnr.Users.User { Email = "user@example.test", Name = "User", Enable = true };
        A.CallTo(() => users.GetByEmail(user.Email)).Returns(Task.FromResult(user));
        var middleware = new ValidateUserStateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(Context(user.Email), users, A.Fake<IApplicationConfigurationManager>());

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Should_InvokeNext_WhenUnknownUserAndAutoCreationIsEnabled()
    {
        var nextCalled = false;
        var users = A.Fake<IUserManager>();
        A.CallTo(() => users.GetByEmail("new@example.test")).Returns(Task.FromResult<Mimisbrunnr.Users.User>(null));
        var config = A.Fake<IApplicationConfigurationManager>();
        A.CallTo(() => config.Get()).Returns(Task.FromResult(new ApplicationConfiguration { UserAutoCreation = true }));
        var middleware = new ValidateUserStateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(Context("new@example.test"), users, config);

        nextCalled.Should().BeTrue();
    }
}
