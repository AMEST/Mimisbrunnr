using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Web.Tests.Managers;

public class FeedManagerTests
{
    public FeedManagerTests() => QueryableAsyncExtensions.EnableFallback();

    [Fact]
    public async Task ShouldThrow_ArgumentNullException_WhenAuthenticatedRequestOmitsSpaces()
    {
        var manager = new FeedManager(A.Fake<IRepository<PageUpdateEvent>>());

        await manager.Invoking(x => x.GetPageUpdates(new UserInfo { Email = "user@example.test" }))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Should_CreatePageUpdate_WhenAddingUpdate()
    {
        var repository = A.Fake<IRepository<PageUpdateEvent>>();
        var manager = new FeedManager(repository);
        var space = new Space { Key = "DOCS", Type = SpaceType.Public };
        var page = new Page { Id = "page", Name = "Title" };
        var user = new UserInfo { Email = "user@example.test" };

        await manager.AddPageUpdate(space, page, user);

        A.CallTo(() => repository.Create(A<PageUpdateEvent>.That.Matches(x =>
            x.SpaceKey == "DOCS" && x.PageId == "page" && x.UpdatedBy == user), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ReturnOnlyPublicUpdates_WhenRequestedAnonymously()
    {
        var repository = A.Fake<IRepository<PageUpdateEvent>>();
        A.CallTo(() => repository.GetAll()).Returns(new[] { new PageUpdateEvent { SpaceType = SpaceType.Public, Updated = DateTime.UtcNow }, new PageUpdateEvent { SpaceType = SpaceType.Private, Updated = DateTime.UtcNow } }.AsQueryable());

        var updates = await new FeedManager(repository).GetPageUpdates(null);

        updates.Should().ContainSingle().Which.SpaceType.Should().Be(SpaceType.Public);
    }

    [Fact]
    public async Task Should_ReturnAllPageUpdates_OrderedByUpdatedDescending()
    {
        var repository = A.Fake<IRepository<PageUpdateEvent>>();
        A.CallTo(() => repository.GetAll()).Returns(new[]
        {
            new PageUpdateEvent { PageId = "old", Updated = DateTime.UtcNow.AddDays(-1) },
            new PageUpdateEvent { PageId = "new", Updated = DateTime.UtcNow }
        }.AsQueryable());

        var updates = await new FeedManager(repository).GetAllPageUpdates();

        updates.First().PageId.Should().Be("new");
    }

    [Fact]
    public async Task Should_ReturnOnlyUpdatesInUserSpaces_WhenGettingUpdatesWithSpaces()
    {
        var repository = A.Fake<IRepository<PageUpdateEvent>>();
        A.CallTo(() => repository.GetAll()).Returns(new[]
        {
            new PageUpdateEvent { SpaceKey = "DOCS", Updated = DateTime.UtcNow },
            new PageUpdateEvent { SpaceKey = "SALES", Updated = DateTime.UtcNow }
        }.AsQueryable());

        var updates = await new FeedManager(repository).GetPageUpdates(new UserInfo { Email = "user@example.test" }, userSpaces: [new Space { Key = "DOCS" }]);

        updates.Should().ContainSingle().Which.SpaceKey.Should().Be("DOCS");
    }

    [Fact]
    public async Task Should_ReturnOwnUpdates_WhenRequestedByAuthor()
    {
        var repository = A.Fake<IRepository<PageUpdateEvent>>();
        var user = new UserInfo { Email = "author@example.test" };
        A.CallTo(() => repository.GetAll()).Returns(new[]
        {
            new PageUpdateEvent { SpaceKey = "PRIVATE", SpaceType = SpaceType.Private, UpdatedBy = user, Updated = DateTime.UtcNow },
            new PageUpdateEvent { SpaceKey = "DOCS", UpdatedBy = new UserInfo { Email = "other@example.test" }, Updated = DateTime.UtcNow }
        }.AsQueryable());

        var updates = await new FeedManager(repository).GetPageUpdates(user, userSpaces: [new Space { Key = "PRIVATE" }], updatedBy: user);

        updates.Should().ContainSingle().Which.SpaceKey.Should().Be("PRIVATE");
    }

    [Fact]
    public async Task Should_ReturnOnlyPublicUpdatesOfUser_WhenRequestedByOther()
    {
        var repository = A.Fake<IRepository<PageUpdateEvent>>();
        var author = new UserInfo { Email = "author@example.test" };
        A.CallTo(() => repository.GetAll()).Returns(new[]
        {
            new PageUpdateEvent { SpaceKey = "PUB", SpaceType = SpaceType.Public, UpdatedBy = author, Updated = DateTime.UtcNow },
            new PageUpdateEvent { SpaceKey = "PRIVATE", SpaceType = SpaceType.Private, UpdatedBy = author, Updated = DateTime.UtcNow },
            new PageUpdateEvent { SpaceKey = "OTHER", SpaceType = SpaceType.Public, UpdatedBy = new UserInfo { Email = "x@example.test" }, Updated = DateTime.UtcNow }
        }.AsQueryable());

        var updates = await new FeedManager(repository).GetPageUpdates(new UserInfo { Email = "viewer@example.test" }, userSpaces: [new Space { Key = "PUB" }, new Space { Key = "PRIVATE" }], updatedBy: author);

        updates.Should().ContainSingle().Which.SpaceKey.Should().Be("PUB");
    }
}
