using System.Security.Claims;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging.Abstractions;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Host.Services;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Infrastructure.Contracts;

namespace Mimisbrunnr.Web.Tests.Host;

public class EnsureUserAuthorizationHandlerTests
{
    private static ClaimsPrincipal Principal(string email) => new(new ClaimsIdentity([
        new Claim(ClaimTypes.Email, email),
        new Claim(ClaimTypes.Name, "New User")
    ], "test"));

    [Fact]
    public async Task Should_CreateEmployee_WhenUnknownUserAndAutoCreationEnabled()
    {
        var users = A.Fake<IUserManager>();
        A.CallTo(() => users.GetByEmail("new@example.test")).Returns(Task.FromResult<Mimisbrunnr.Users.User>(null));
        var config = A.Fake<IApplicationConfigurationManager>();
        A.CallTo(() => config.Get()).Returns(Task.FromResult(new ApplicationConfiguration { UserAutoCreation = true }));
        var handler = new EnsureUserAuthorizationHandler(users, config, NullLogger<EnsureUserAuthorizationHandler>.Instance);
        var context = new AuthorizationHandlerContext(Array.Empty<IAuthorizationRequirement>(), Principal("new@example.test"), null);

        await handler.HandleAsync(context);

        A.CallTo(() => users.Add("new@example.test", "New User", null, UserRole.Employee)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_NotCreateUser_WhenExistingUserIsFound()
    {
        var users = A.Fake<IUserManager>();
        A.CallTo(() => users.GetByEmail("existing@example.test")).Returns(Task.FromResult(new Mimisbrunnr.Users.User()));
        var config = A.Fake<IApplicationConfigurationManager>();
        var handler = new EnsureUserAuthorizationHandler(users, config, NullLogger<EnsureUserAuthorizationHandler>.Instance);
        var context = new AuthorizationHandlerContext(Array.Empty<IAuthorizationRequirement>(), Principal("existing@example.test"), null);

        await handler.HandleAsync(context);

        A.CallTo(() => users.Add(A<string>._, A<string>._, A<string>._, A<UserRole>._)).MustNotHaveHappened();
        A.CallTo(() => config.Get()).MustNotHaveHappened();
    }
}
