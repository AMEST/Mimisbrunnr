using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Infrastructure.Contracts;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Web.Tests.Host;

public class ApplicationConfigurationManagerTests
{
    public ApplicationConfigurationManagerTests() => QueryableAsyncExtensions.EnableFallback();

    [Fact]
    public async Task ShouldInitialize_AndPromoteUserToAdmin_WhenConfigurationIsMissing()
    {
        var repository = A.Fake<IRepository<ApplicationConfiguration>>();
        A.CallTo(() => repository.GetAll()).Returns(Array.Empty<ApplicationConfiguration>().AsQueryable());
        var userManager = A.Fake<IUserManager>();
        var manager = CreateManager(repository, userManager);
        var user = new Mimisbrunnr.Users.User { Email = "admin@example.test" };
        var configuration = new ApplicationConfiguration();

        await manager.Initialize(configuration, user);

        A.CallTo(() => repository.Create(configuration, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => userManager.ChangeRole(user, UserRole.Admin)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ShouldThrow_InvalidOperationException_WhenInitializingTwice()
    {
        var repository = A.Fake<IRepository<ApplicationConfiguration>>();
        A.CallTo(() => repository.GetAll()).Returns(new[] { new ApplicationConfiguration() }.AsQueryable());
        var manager = CreateManager(repository, A.Fake<IUserManager>());

        await manager.Invoking(x => x.Initialize(new ApplicationConfiguration(), new Mimisbrunnr.Users.User()))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ShouldThrow_InvalidOperationException_WhenConfiguringBeforeInitialization()
    {
        var repository = A.Fake<IRepository<ApplicationConfiguration>>();
        A.CallTo(() => repository.GetAll()).Returns(Array.Empty<ApplicationConfiguration>().AsQueryable());
        var manager = CreateManager(repository, A.Fake<IUserManager>());

        await manager.Invoking(x => x.Configure(new ApplicationConfiguration()))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    private static ApplicationConfigurationManager CreateManager(IRepository<ApplicationConfiguration> repository, IUserManager users)
    {
        var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        return new ApplicationConfigurationManager(repository, users, cache);
    }
}
