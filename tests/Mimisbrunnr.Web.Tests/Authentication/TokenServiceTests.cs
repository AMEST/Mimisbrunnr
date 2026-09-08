using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Authentication.Account;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Tests.Authentication;

public class TokenServiceTests
{
    private readonly ISecurityTokenService _tokens = A.Fake<ISecurityTokenService>();
    private readonly IUserManager _users = A.Fake<IUserManager>();
    private readonly TokenService _service;

    public TokenServiceTests() => _service = new(_tokens, _users);

    [Fact]
    public async Task Should_ReturnNull_WhenCreatingTokenForUnknownUser()
    {
        A.CallTo(() => _users.GetByEmail("unknown@example.test")).Returns(Task.FromResult<Mimisbrunnr.Users.User>(null));

        var result = await _service.CreateUserToken(new TokenCreateRequest(), new UserInfo { Email = "unknown@example.test" });

        result.Should().BeNull();
        A.CallTo(() => _tokens.GenerateAccessToken(A<Mimisbrunnr.Users.User>._, A<TimeSpan?>._, A<bool>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_RevokeToken_WhenRequestedByCurrentUser()
    {
        var user = new Mimisbrunnr.Users.User { Email = "user@example.test" };
        A.CallTo(() => _users.GetByEmail(user.Email)).Returns(Task.FromResult(user));

        await _service.Revoke("token", new UserInfo { Email = user.Email });

        A.CallTo(() => _tokens.RevokeToken("token", user)).MustHaveHappenedOnceExactly();
    }
}
