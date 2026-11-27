using System.Linq.Expressions;
using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Favorites.Contracts;
using Mimisbrunnr.Favorites.Services;
using Mimisbrunnr.Integration.Favorites;
using Mimisbrunnr.Integration.User;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Favorites;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Web.Wiki;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

namespace Mimisbrunnr.Web.Tests.Favorites;

public class FavoriteServiceTests
{
    private static FavoriteService CreateService(
        IFavoritesManager favorites,
        IUserManager users = null,
        IPageService pages = null,
        ISpaceService spaces = null,
        IPageManager pageManager = null,
        ISpaceManager spaceManager = null,
        ISpaceDisplayService spaceDisplay = null)
    {
        return new FavoriteService(
            favorites,
            users ?? A.Fake<IUserManager>(),
            pages ?? A.Fake<IPageService>(),
            spaces ?? A.Fake<ISpaceService>(),
            pageManager ?? A.Fake<IPageManager>(),
            spaceManager ?? A.Fake<ISpaceManager>(),
            spaceDisplay ?? A.Fake<ISpaceDisplayService>());
    }

    [Fact]
    public async Task ShouldThrow_UserHasNotPermissionException_WhenFavoriteBelongsToAnotherUser()
    {
        var favorites = A.Fake<IFavoritesManager>();
        A.CallTo(() => favorites.FindById("favorite", A<CancellationToken>._)).Returns(Task.FromResult<Favorite>(new FavoritePage { Id = "favorite", OwnerEmail = "owner@example.test" }));
        var service = CreateService(favorites);

        await service.Invoking(x => x.Remove("favorite", new UserInfo { Email = "other@example.test" }))
            .Should().ThrowAsync<UserHasNotPermissionException>();
    }

    [Fact]
    public async Task Should_PersistUserFavorite_WhenAddingUserFavorite()
    {
        var favorites = A.Fake<IFavoritesManager>();
        var users = A.Fake<IUserManager>();
        var user = new UserInfo { Email = "owner@example.test" };
        A.CallTo(() => users.GetByEmail("target@example.test")).Returns(Task.FromResult(new Mimisbrunnr.Users.User { Email = "target@example.test", Name = "Target" }));
        A.CallTo(() => favorites.Add(A<Favorite>._, A<CancellationToken>._)).ReturnsLazily(call => Task.FromResult(call.GetArgument<Favorite>(0)));
        var service = CreateService(favorites, users);

        var result = await service.Add(new FavoriteUserCreateModel { UserEmail = "target@example.test" }, user);

        A.CallTo(() => favorites.Add(A<FavoriteUser>.That.Matches(x => x.OwnerEmail == "owner@example.test" && x.UserEmail == "target@example.test" && x.Created != default), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        (result as FavoriteUserModel).User.Email.Should().Be("target@example.test");
    }

    [Fact]
    public async Task ShouldThrow_UserNotFoundException_WhenFavoriteTargetUserMissing()
    {
        var favorites = A.Fake<IFavoritesManager>();
        var users = A.Fake<IUserManager>();
        A.CallTo(() => users.GetByEmail("missing@example.test")).Returns(Task.FromResult<Mimisbrunnr.Users.User>(null));
        var service = CreateService(favorites, users);

        await service.Invoking(x => x.Add(new FavoriteUserCreateModel { UserEmail = "missing@example.test" }, new UserInfo { Email = "owner@example.test" }))
            .Should().ThrowAsync<UserNotFoundException>();
    }

    [Fact]
    public async Task ShouldThrow_SpaceNotFoundException_WhenFavoriteTargetSpaceMissing()
    {
        var favorites = A.Fake<IFavoritesManager>();
        var spaces = A.Fake<ISpaceService>();
        var user = new UserInfo { Email = "owner@example.test" };
        A.CallTo(() => spaces.GetByKey("NOPE", user)).Returns(Task.FromResult<SpaceModel>(null));
        var service = CreateService(favorites, spaces: spaces);

        await service.Invoking(x => x.Add(new FavoriteSpaceCreateModel { SpaceKey = "NOPE" }, user))
            .Should().ThrowAsync<SpaceNotFoundException>();
    }

    [Fact]
    public async Task ShouldThrow_PageNotFoundException_WhenFavoriteTargetPageMissing()
    {
        var favorites = A.Fake<IFavoritesManager>();
        var pages = A.Fake<IPageService>();
        var user = new UserInfo { Email = "owner@example.test" };
        A.CallTo(() => pages.GetById("missing", user)).Returns(Task.FromResult<PageModel>(null));
        var service = CreateService(favorites, pages: pages);

        await service.Invoking(x => x.Add(new FavoritePageCreateModel { PageId = "missing" }, user))
            .Should().ThrowAsync<PageNotFoundException>();
    }

    [Fact]
    public async Task ShouldThrow_ArgumentOutOfRangeException_WhenUnknownFavoriteCreateType()
    {
        var favorites = A.Fake<IFavoritesManager>();
        var service = CreateService(favorites);

        await service.Invoking(x => x.Add(A.Fake<FavoriteCreateModel>(), new UserInfo { Email = "owner@example.test" }))
            .Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task Should_CheckUserFavoriteInList_WhenCheckingInFavorites()
    {
        var favorites = A.Fake<IFavoritesManager>();
        A.CallTo(() => favorites.EnsureItemInFavorite<FavoriteUser>(A<string>._, A<Expression<Func<FavoriteUser, bool>>>._, A<CancellationToken>._)).Returns(Task.FromResult(true));
        var service = CreateService(favorites);

        var result = await service.EnsureInFavorites(new FavoriteUserFindModel { UserEmail = "target@example.test" }, new UserInfo { Email = "owner@example.test" });

        result.Should().BeTrue();
        A.CallTo(() => favorites.EnsureItemInFavorite<FavoriteUser>("owner@example.test", A<Expression<Func<FavoriteUser, bool>>>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ShouldThrow_ArgumentOutOfRangeException_WhenUnknownFavoriteFindType()
    {
        var favorites = A.Fake<IFavoritesManager>();
        var service = CreateService(favorites);

        await service.Invoking(x => x.EnsureInFavorites(A.Fake<FavoriteFindModel>(), new UserInfo { Email = "owner@example.test" }))
            .Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task Should_ReturnNull_WhenUserFavoriteNotFound()
    {
        var favorites = A.Fake<IFavoritesManager>();
        A.CallTo(() => favorites.GetByExpression<FavoriteUser>(A<string>._, A<Expression<Func<FavoriteUser, bool>>>._, A<CancellationToken>._)).Returns(Task.FromResult<Favorite>(null));
        var service = CreateService(favorites);

        var result = await service.GetFavorite(new FavoriteUserFindModel { UserEmail = "target@example.test" }, new UserInfo { Email = "owner@example.test" });

        result.Should().BeNull();
    }

    [Fact]
    public async Task Should_ReturnSpaceFavorite_WhenGettingSpaceFavorite()
    {
        var favorites = A.Fake<IFavoritesManager>();
        var spaces = A.Fake<ISpaceService>();
        var user = new UserInfo { Email = "owner@example.test" };
        var favorite = new FavoriteSpace { Id = "f1", OwnerEmail = user.Email, SpaceKey = "DOCS" };
        A.CallTo(() => favorites.GetByExpression<FavoriteSpace>(A<string>._, A<Expression<Func<FavoriteSpace, bool>>>._, A<CancellationToken>._)).Returns(Task.FromResult<Favorite>(favorite));
        A.CallTo(() => spaces.GetByKey("DOCS", user)).Returns(Task.FromResult(new SpaceModel { Key = "DOCS" }));
        var service = CreateService(favorites, spaces: spaces);

        var result = await service.GetFavorite(new FavoriteSpaceFindModel { SpaceKey = "DOCS" }, user);

        (result as FavoriteSpaceModel).Space.Key.Should().Be("DOCS");
    }

    [Fact]
    public async Task Should_ReturnMappedFavorites_WhenGettingFavorites()
    {
        var favorites = A.Fake<IFavoritesManager>();
        var users = A.Fake<IUserManager>();
        var user = new UserInfo { Email = "owner@example.test" };
        A.CallTo(() => favorites.FindAllByUserEmail(user.Email, A<FavoriteFilter>._, A<CancellationToken>._))
            .Returns(Task.FromResult<IEnumerable<Favorite>>(new[] { new FavoriteUser { Id = "f1", OwnerEmail = user.Email, UserEmail = "target@example.test" } }));
        A.CallTo(() => users.GetByEmails(A<string[]>._))
            .Returns(Task.FromResult(new[] { new Mimisbrunnr.Users.User { Email = "target@example.test", Name = "Target" } }));
        var service = CreateService(favorites, users);

        var result = await service.GetFavorites(new FavoriteFilterModel(), user);

        result.Should().ContainSingle().Which.As<FavoriteUserModel>().User.Email.Should().Be("target@example.test");
    }

    [Fact]
    public async Task Should_LoadFavoriteTargetsInBatches_WhenGettingFavorites()
    {
        var favorites = A.Fake<IFavoritesManager>();
        var userManager = A.Fake<IUserManager>();
        var pageManager = A.Fake<IPageManager>();
        var spaceManager = A.Fake<ISpaceManager>();
        var spaceDisplay = A.Fake<ISpaceDisplayService>();
        var user = new UserInfo { Email = "owner@example.test" };
        A.CallTo(() => favorites.FindAllByUserEmail(user.Email, A<FavoriteFilter>._, A<CancellationToken>._))
            .Returns(Task.FromResult<IEnumerable<Favorite>>(new Favorite[]
            {
                new FavoritePage { Id = "f1", OwnerEmail = user.Email, PageId = "p1" },
                new FavoritePage { Id = "f2", OwnerEmail = user.Email, PageId = "p2" },
                new FavoriteSpace { Id = "f3", OwnerEmail = user.Email, SpaceKey = "DOCS" }
            }));
        A.CallTo(() => pageManager.GetByIds(A<string[]>._)).Returns(Task.FromResult(new[]
        {
            new Page { Id = "p1", SpaceId = "s1", Name = "One" },
            new Page { Id = "p2", SpaceId = "s1", Name = "Two" }
        }));
        A.CallTo(() => spaceManager.GetByKeys(A<string[]>._)).Returns(Task.FromResult(new[] { new Space { Id = "s1", Key = "DOCS" } }));
        A.CallTo(() => spaceDisplay.FindUserVisibleSpaces(user, A<int?>._, A<int?>._)).Returns(Task.FromResult<IEnumerable<Space>>(new[] { new Space { Id = "s1", Key = "DOCS" } }));
        var service = CreateService(favorites, userManager, pageManager: pageManager, spaceManager: spaceManager, spaceDisplay: spaceDisplay);

        var result = await service.GetFavorites(new FavoriteFilterModel(), user);

        result.Should().HaveCount(3);
        A.CallTo(() => pageManager.GetByIds(A<string[]>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => spaceManager.GetByKeys(A<string[]>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_SkipFavoritesWhoseTargetIsNotVisible_WhenGettingFavorites()
    {
        var favorites = A.Fake<IFavoritesManager>();
        var pageManager = A.Fake<IPageManager>();
        var spaceDisplay = A.Fake<ISpaceDisplayService>();
        var user = new UserInfo { Email = "owner@example.test" };
        A.CallTo(() => favorites.FindAllByUserEmail(user.Email, A<FavoriteFilter>._, A<CancellationToken>._))
            .Returns(Task.FromResult<IEnumerable<Favorite>>(new[] { new FavoritePage { Id = "f1", OwnerEmail = user.Email, PageId = "hidden" } }));
        A.CallTo(() => pageManager.GetByIds(A<string[]>._)).Returns(Task.FromResult(new[] { new Page { Id = "hidden", SpaceId = "private-space" } }));
        A.CallTo(() => spaceDisplay.FindUserVisibleSpaces(user, A<int?>._, A<int?>._)).Returns(Task.FromResult<IEnumerable<Space>>(Array.Empty<Space>()));
        var service = CreateService(favorites, pageManager: pageManager, spaceDisplay: spaceDisplay);

        var result = await service.GetFavorites(new FavoriteFilterModel(), user);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_DeleteFavorite_WhenRemovingOwnFavorite()
    {
        var favorites = A.Fake<IFavoritesManager>();
        var user = new UserInfo { Email = "owner@example.test" };
        var favorite = new FavoritePage { Id = "f1", OwnerEmail = user.Email };
        A.CallTo(() => favorites.FindById("f1", A<CancellationToken>._)).Returns(Task.FromResult<Favorite>(favorite));
        var service = CreateService(favorites);

        await service.Remove("f1", user);

        A.CallTo(() => favorites.Remove(favorite, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_NotDeleteAnything_WhenRemovingMissingFavorite()
    {
        var favorites = A.Fake<IFavoritesManager>();
        A.CallTo(() => favorites.FindById("missing", A<CancellationToken>._)).Returns(Task.FromResult<Favorite>(null));
        var service = CreateService(favorites);

        await service.Remove("missing", new UserInfo { Email = "owner@example.test" });

        A.CallTo(() => favorites.Remove(A<Favorite>._, A<CancellationToken>._)).MustNotHaveHappened();
    }
}
