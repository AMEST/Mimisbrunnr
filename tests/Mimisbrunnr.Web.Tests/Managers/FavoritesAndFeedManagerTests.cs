using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Favorites.Contracts;
using Mimisbrunnr.Favorites.Services;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Web.Tests.Managers;

public class FavoritesAndFeedManagerTests
{
    public FavoritesAndFeedManagerTests() => QueryableAsyncExtensions.EnableFallback();

    [Fact]
    public async Task Should_ReturnOwnersFavorites_WhenFindingFavorites()
    {
        var store = A.Fake<IFavoriteStore>();
        A.CallTo(() => store.GetAllByType<Favorite>()).Returns(new Favorite[]
        {
            new FavoritePage { OwnerEmail = "owner@example.test", PageId = "page" },
            new FavoritePage { OwnerEmail = "other@example.test", PageId = "other" }
        }.AsQueryable());
        var manager = new FavoritesManager(store);

        var favorites = await manager.FindAllByUserEmail("owner@example.test");

        favorites.Should().ContainSingle().Which.OwnerEmail.Should().Be("owner@example.test");
    }

    [Fact]
    public async Task Should_ReturnOnlyPublicUpdates_WhenRequestedAnonymously()
    {
        var repository = A.Fake<IRepository<PageUpdateEvent>>();
        A.CallTo(() => repository.GetAll()).Returns(new[]
        {
            new PageUpdateEvent { SpaceType = SpaceType.Public, Updated = DateTime.UtcNow },
            new PageUpdateEvent { SpaceType = SpaceType.Private, Updated = DateTime.UtcNow }
        }.AsQueryable());
        var manager = new FeedManager(repository);

        var updates = await manager.GetPageUpdates(null);

        updates.Should().ContainSingle().Which.SpaceType.Should().Be(SpaceType.Public);
    }
}
