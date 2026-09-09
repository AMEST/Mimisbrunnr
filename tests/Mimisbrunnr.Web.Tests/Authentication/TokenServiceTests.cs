using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Authentication.Account;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Infrastructure.Contracts;
using Mimisbrunnr.Web.Mapping;
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

    [Fact]
    public async Task Should_DoNothing_WhenRevokingTokenForUnknownUser()
    {
        A.CallTo(() => _users.GetByEmail("unknown@example.test")).Returns(Task.FromResult<Mimisbrunnr.Users.User>(null));

        await _service.Revoke("token", new UserInfo { Email = "unknown@example.test" });

        A.CallTo(() => _tokens.RevokeToken("token", A<Mimisbrunnr.Users.User>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_ReturnGeneratedToken_WhenCreatingTokenForKnownUser()
    {
        var user = new Mimisbrunnr.Users.User { Email = "user@example.test" };
        A.CallTo(() => _users.GetByEmail(user.Email)).Returns(Task.FromResult(user));
        A.CallTo(() => _tokens.GenerateAccessToken(user, TimeSpan.FromMinutes(30), false)).Returns(Task.FromResult("generated-token"));

        var result = await _service.CreateUserToken(new TokenCreateRequest { Lifetime = TimeSpan.FromMinutes(30) }, new UserInfo { Email = user.Email });

        result.Token.Should().Be("generated-token");
        A.CallTo(() => _tokens.GenerateAccessToken(user, TimeSpan.FromMinutes(30), false)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ReturnEmpty_WhenGettingTokensForUnknownUser()
    {
        A.CallTo(() => _users.GetByEmail("unknown@example.test")).Returns(Task.FromResult<Mimisbrunnr.Users.User>(null));

        var result = await _service.GetUserTokens(new UserInfo { Email = "unknown@example.test" });

        result.Should().BeEmpty();
        A.CallTo(() => _tokens.GetUserTokens(A<Mimisbrunnr.Users.User>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_ReturnMappedTokens_WhenGettingTokensForKnownUser()
    {
        var user = new Mimisbrunnr.Users.User { Email = "user@example.test" };
        A.CallTo(() => _users.GetByEmail(user.Email)).Returns(Task.FromResult(user));
        A.CallTo(() => _tokens.GetUserTokens(user)).Returns(Task.FromResult<IEnumerable<UserToken>>(new[] { new UserToken { Id = "t1" } }));

        var result = await _service.GetUserTokens(new UserInfo { Email = user.Email });

        result.Should().ContainSingle().Which.Id.Should().Be("t1");
    }
}
