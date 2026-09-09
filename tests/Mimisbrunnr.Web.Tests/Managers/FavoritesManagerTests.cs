using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Favorites.Contracts;
using Mimisbrunnr.Favorites.Services;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Web.Tests.Managers;

public class FavoritesManagerTests
{
    public FavoritesManagerTests() => QueryableAsyncExtensions.EnableFallback();

    [Fact]
    public async Task Should_PersistFavorite_WhenAddingItem()
    {
        var store = A.Fake<IFavoriteStore>();
        var manager = new FavoritesManager(store);
        var favorite = new FavoritePage { Id = "f1", OwnerEmail = "owner@example.test", PageId = "page" };

        var result = await manager.Add(favorite);

        result.Should().BeSameAs(favorite);
        A.CallTo(() => store.Create(favorite, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ApplyPagination_WhenFilteringFavorites()
    {
        var store = A.Fake<IFavoriteStore>();
        A.CallTo(() => store.GetAllByType<Favorite>()).Returns(new Favorite[]
        {
            new FavoritePage { Id = "1", OwnerEmail = "owner@example.test" },
            new FavoritePage { Id = "2", OwnerEmail = "owner@example.test" },
            new FavoritePage { Id = "3", OwnerEmail = "owner@example.test" }
        }.AsQueryable());
        var manager = new FavoritesManager(store);

        var favorites = await manager.FindAllByUserEmail("owner@example.test", new FavoriteFilter { Skip = 1, Count = 1 });

        favorites.Should().ContainSingle().Which.Id.Should().Be("2");
    }

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
    public async Task Should_ReturnTrue_WhenItemInFavorite()
    {
        var store = A.Fake<IFavoriteStore>();
        A.CallTo(() => store.GetAllByType<FavoritePage>()).Returns(new[]
        {
            new FavoritePage { Id = "f1", OwnerEmail = "owner@example.test", PageId = "page" }
        }.AsQueryable());
        var manager = new FavoritesManager(store);

        var result = await manager.EnsureItemInFavorite<FavoritePage>("owner@example.test", x => x.PageId == "page");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Should_ReturnFalse_WhenItemNotInFavorite()
    {
        var store = A.Fake<IFavoriteStore>();
        A.CallTo(() => store.GetAllByType<FavoritePage>()).Returns(new[]
        {
            new FavoritePage { Id = "f1", OwnerEmail = "owner@example.test", PageId = "other" }
        }.AsQueryable());
        var manager = new FavoritesManager(store);

        var result = await manager.EnsureItemInFavorite<FavoritePage>("owner@example.test", x => x.PageId == "page");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Should_ReturnFavorite_WhenFindingById()
    {
        var store = A.Fake<IFavoriteStore>();
        var favorite = new FavoritePage { Id = "f1", OwnerEmail = "owner@example.test" };
        A.CallTo(() => store.GetAll()).Returns(new[] { favorite }.AsQueryable());
        var manager = new FavoritesManager(store);

        var result = await manager.FindById("f1");

        result.Should().BeSameAs(favorite);
    }

    [Fact]
    public async Task Should_ReturnFavorite_WhenGettingByExpression()
    {
        var store = A.Fake<IFavoriteStore>();
        var favorite = new FavoritePage { Id = "f1", OwnerEmail = "owner@example.test", PageId = "page" };
        A.CallTo(() => store.GetAllByType<FavoritePage>()).Returns(new[] { favorite }.AsQueryable());
        var manager = new FavoritesManager(store);

        var result = await manager.GetByExpression<FavoritePage>("owner@example.test", x => x.PageId == "page");

        result.Should().BeSameAs(favorite);
    }

    [Fact]
    public async Task Should_ReturnNull_WhenNoMatchByExpression()
    {
        var store = A.Fake<IFavoriteStore>();
        A.CallTo(() => store.GetAllByType<FavoritePage>()).Returns(Array.Empty<FavoritePage>().AsQueryable());
        var manager = new FavoritesManager(store);

        var result = await manager.GetByExpression<FavoritePage>("owner@example.test", x => x.PageId == "page");

        result.Should().BeNull();
    }

    [Fact]
    public async Task Should_DeleteFavorite_WhenRemoving()
    {
        var store = A.Fake<IFavoriteStore>();
        var favorite = new FavoritePage { Id = "f1" };
        var manager = new FavoritesManager(store);

        await manager.Remove(favorite);

        A.CallTo(() => store.Delete(favorite, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_FilterPageFavorites_WhenTypeIsPage()
    {
        var store = A.Fake<IFavoriteStore>();
        A.CallTo(() => store.GetAllByType<FavoritePage>()).Returns(new[]
        {
            new FavoritePage { Id = "1", OwnerEmail = "owner@example.test", PageId = "p" },
            new FavoritePage { Id = "2", OwnerEmail = "other@example.test", PageId = "p" }
        }.AsQueryable());
        var manager = new FavoritesManager(store);

        var favorites = await manager.FindAllByUserEmail("owner@example.test", new FavoriteFilter { Type = FavoriteFilterType.Page });

        favorites.Should().ContainSingle().Which.Id.Should().Be("1");
    }

    [Fact]
    public async Task Should_FilterSpaceFavorites_WhenTypeIsSpace()
    {
        var store = A.Fake<IFavoriteStore>();
        A.CallTo(() => store.GetAllByType<FavoriteSpace>()).Returns(new[]
        {
            new FavoriteSpace { Id = "1", OwnerEmail = "owner@example.test", SpaceKey = "DOCS" },
            new FavoriteSpace { Id = "2", OwnerEmail = "other@example.test", SpaceKey = "SALES" }
        }.AsQueryable());
        var manager = new FavoritesManager(store);

        var favorites = await manager.FindAllByUserEmail("owner@example.test", new FavoriteFilter { Type = FavoriteFilterType.Space });

        favorites.Should().ContainSingle().Which.Id.Should().Be("1");
    }

    [Fact]
    public async Task Should_FilterUserFavorites_WhenTypeIsUser()
    {
        var store = A.Fake<IFavoriteStore>();
        A.CallTo(() => store.GetAllByType<FavoriteUser>()).Returns(new[]
        {
            new FavoriteUser { Id = "1", OwnerEmail = "owner@example.test", UserEmail = "x@example.test" },
            new FavoriteUser { Id = "2", OwnerEmail = "other@example.test", UserEmail = "x@example.test" }
        }.AsQueryable());
        var manager = new FavoritesManager(store);

        var favorites = await manager.FindAllByUserEmail("owner@example.test", new FavoriteFilter { Type = FavoriteFilterType.User });

        favorites.Should().ContainSingle().Which.Id.Should().Be("1");
    }

    [Fact]
    public async Task Should_ApplySkipAndCount_WhenFilteringTypedFavorites()
    {
        var store = A.Fake<IFavoriteStore>();
        A.CallTo(() => store.GetAllByType<FavoritePage>()).Returns(new[]
        {
            new FavoritePage { Id = "1", OwnerEmail = "owner@example.test", PageId = "a" },
            new FavoritePage { Id = "2", OwnerEmail = "owner@example.test", PageId = "b" },
            new FavoritePage { Id = "3", OwnerEmail = "owner@example.test", PageId = "c" }
        }.AsQueryable());
        var manager = new FavoritesManager(store);

        var favorites = await manager.FindAllByUserEmail("owner@example.test", new FavoriteFilter { Type = FavoriteFilterType.Page, Skip = 1, Count = 1 });

        favorites.Should().ContainSingle().Which.Id.Should().Be("2");
    }

    [Fact]
    public async Task ShouldThrow_NotImplementedException_WhenUnknownFavoriteType()
    {
        var store = A.Fake<IFavoriteStore>();
        var manager = new FavoritesManager(store);

        await manager.Invoking(x => x.FindAllByUserEmail("owner@example.test", new FavoriteFilter { Type = (FavoriteFilterType)999 }))
            .Should().ThrowAsync<NotImplementedException>();
    }
}
