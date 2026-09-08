using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Favorites.Contracts;
using Mimisbrunnr.Favorites.Services;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Favorites;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Wiki;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Tests.Favorites;

public class FavoriteServiceTests
{
    [Fact]
    public async Task ShouldThrow_UserHasNotPermissionException_WhenFavoriteBelongsToAnotherUser()
    {
        var favorites = A.Fake<IFavoritesManager>();
        A.CallTo(() => favorites.FindById("favorite", A<CancellationToken>._)).Returns(Task.FromResult<Favorite>(new FavoritePage { Id = "favorite", OwnerEmail = "owner@example.test" }));
        var service = new FavoriteService(favorites, A.Fake<IUserManager>(), A.Fake<IPageService>(), A.Fake<ISpaceService>());

        await service.Invoking(x => x.Remove("favorite", new UserInfo { Email = "other@example.test" }))
            .Should().ThrowAsync<UserHasNotPermissionException>();
    }
}
