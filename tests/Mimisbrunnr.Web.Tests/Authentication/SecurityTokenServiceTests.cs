using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Host.Configuration;
using Mimisbrunnr.Web.Host.Services;
using Mimisbrunnr.Web.Infrastructure.Contracts;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Web.Tests.Authentication;

public class SecurityTokenServiceTests
{
    private static BearerTokenConfiguration Configuration() => new() { SymmetricKey = "01234567890123456789012345678901" };

    public SecurityTokenServiceTests() => QueryableAsyncExtensions.EnableFallback();

    [Fact]
    public async Task Should_CreatePersistentToken_WhenGeneratingUserToken()
    {
        var repository = A.Fake<IRepository<UserToken>>();
        var service = new SecurityTokenService(Configuration(), repository, NullLogger<SecurityTokenService>.Instance);
        var user = new Mimisbrunnr.Users.User { Id = "user", Email = "user@example.test", Name = "User" };

        var token = await service.GenerateAccessToken(user, TimeSpan.FromMinutes(5));

        token.Should().NotBeNullOrWhiteSpace();
        A.CallTo(() => repository.Create(A<UserToken>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_NotPersistToken_WhenGeneratingSystemToken()
    {
        var repository = A.Fake<IRepository<UserToken>>();
        var service = new SecurityTokenService(Configuration(), repository, NullLogger<SecurityTokenService>.Instance);
        var user = new Mimisbrunnr.Users.User { Id = "user", Email = "user@example.test" };

        await service.GenerateAccessToken(user, systemToken: true);

        A.CallTo(() => repository.Create(A<UserToken>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_NotFabricateId_WhenGeneratingUserToken()
    {
        var repository = A.Fake<IRepository<UserToken>>();
        UserToken persisted = null;
        A.CallTo(() => repository.Create(A<UserToken>._, A<CancellationToken>._))
            .Invokes((UserToken token, CancellationToken _) =>
            {
                persisted = token;
                token.Id.Should().BeNull();
            });
        var service = new SecurityTokenService(Configuration(), repository, NullLogger<SecurityTokenService>.Instance);
        var user = new Mimisbrunnr.Users.User { Id = "user", Email = "user@example.test" };

        await service.GenerateAccessToken(user, TimeSpan.FromMinutes(5));

        persisted.Should().NotBeNull();
        persisted!.Id.Should().BeNull();
    }

    [Fact]
    public async Task Should_UseRepositoryAssignedId_WhenTokenIsPersisted()
    {
        var repository = A.Fake<IRepository<UserToken>>();
        A.CallTo(() => repository.Create(A<UserToken>._, A<CancellationToken>._))
            .Invokes((UserToken token, CancellationToken _) => token.Id = "repo-assigned-id");
        var service = new SecurityTokenService(Configuration(), repository, NullLogger<SecurityTokenService>.Instance);
        var user = new Mimisbrunnr.Users.User { Id = "user", Email = "user@example.test" };

        var token = await service.GenerateAccessToken(user, TimeSpan.FromMinutes(5));

        var principal = service.GetPrincipal(token);
        principal.FindFirstValue("TokenId").Should().Be("repo-assigned-id");
    }

    [Fact]
    public async Task Should_ReturnFalse_WhenTokenIsInvalid()
    {
        var service = new SecurityTokenService(Configuration(), A.Fake<IRepository<UserToken>>(), NullLogger<SecurityTokenService>.Instance);

        var result = await service.EnsureTokenNotRevoked("not-a-jwt");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Should_ReturnTrue_WhenGeneratedTokenIsNotRevoked()
    {
        var repository = A.Fake<IRepository<UserToken>>();
        A.CallTo(() => repository.GetAll()).Returns(Array.Empty<UserToken>().AsQueryable());
        var service = new SecurityTokenService(Configuration(), repository, NullLogger<SecurityTokenService>.Instance);
        var user = new Mimisbrunnr.Users.User { Id = "user", Email = "user@example.test" };
        var token = await service.GenerateAccessToken(user, TimeSpan.FromMinutes(5));

        var result = await service.EnsureTokenNotRevoked(token);

        result.Should().BeTrue();
    }
}
