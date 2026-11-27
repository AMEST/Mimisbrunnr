using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Host.Configuration;
using Mimisbrunnr.Web.Host.Services;
using Mimisbrunnr.Web.Infrastructure.Contracts;
using Mimisbrunnr.Web.Tests.Host.CacheDecorators;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Web.Tests.Authentication;

public class SecurityTokenServiceTests
{
    private static BearerTokenConfiguration Configuration() => new() { SymmetricKey = "01234567890123456789012345678901" };

    private static SecurityTokenService CreateService(IRepository<UserToken> repository, IDistributedCache cache = null)
        => new(Configuration(), repository, cache ?? new FakeDistributedCache(), NullLogger<SecurityTokenService>.Instance);

    public SecurityTokenServiceTests() => QueryableAsyncExtensions.EnableFallback();

    [Fact]
    public async Task Should_CreatePersistentToken_WhenGeneratingUserToken()
    {
        var repository = A.Fake<IRepository<UserToken>>();
        var service = CreateService(repository);
        var user = new Mimisbrunnr.Users.User { Id = "user", Email = "user@example.test", Name = "User" };

        var token = await service.GenerateAccessToken(user, TimeSpan.FromMinutes(5));

        token.Should().NotBeNullOrWhiteSpace();
        A.CallTo(() => repository.Create(A<UserToken>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_NotPersistToken_WhenGeneratingSystemToken()
    {
        var repository = A.Fake<IRepository<UserToken>>();
        var service = CreateService(repository);
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
        var service = CreateService(repository);
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
        var service = CreateService(repository);
        var user = new Mimisbrunnr.Users.User { Id = "user", Email = "user@example.test" };

        var token = await service.GenerateAccessToken(user, TimeSpan.FromMinutes(5));

        var principal = service.GetPrincipal(token);
        principal.FindFirstValue("TokenId").Should().Be("repo-assigned-id");
    }

    [Fact]
    public async Task Should_ReturnFalse_WhenPrincipalIsNotAuthenticated()
    {
        var service = CreateService(A.Fake<IRepository<UserToken>>());
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        var result = await service.EnsureTokenNotRevoked(principal);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Should_ReturnTrue_WhenGeneratedTokenIsNotRevoked()
    {
        var repository = A.Fake<IRepository<UserToken>>();
        A.CallTo(() => repository.GetAll()).Returns(Array.Empty<UserToken>().AsQueryable());
        var service = CreateService(repository);
        var user = new Mimisbrunnr.Users.User { Id = "user", Email = "user@example.test" };
        var token = await service.GenerateAccessToken(user, TimeSpan.FromMinutes(5));

        var result = await service.EnsureTokenNotRevoked(service.GetPrincipal(token));

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Should_NotQueryRepository_WhenRevocationStateIsCached()
    {
        var repository = A.Fake<IRepository<UserToken>>();
        A.CallTo(() => repository.GetAll()).Returns(Array.Empty<UserToken>().AsQueryable());
        var cache = new FakeDistributedCache();
        var service = CreateService(repository, cache);
        var user = new Mimisbrunnr.Users.User { Id = "user", Email = "user@example.test" };
        var token = await service.GenerateAccessToken(user, TimeSpan.FromMinutes(5));
        var principal = service.GetPrincipal(token);

        await service.EnsureTokenNotRevoked(principal);
        await service.EnsureTokenNotRevoked(principal);

        A.CallTo(() => repository.GetAll()).MustHaveHappenedOnceExactly();
    }
}
