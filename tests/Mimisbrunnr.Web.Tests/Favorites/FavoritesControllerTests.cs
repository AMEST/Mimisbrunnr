using System.Security.Claims;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Integration.Favorites;
using Mimisbrunnr.Web.Favorites;

namespace Mimisbrunnr.Web.Tests.Favorites;

public class FavoritesControllerTests
{
    private static FavoritesController CreateController(IFavoriteService service)
    {
        var controller = new FavoritesController(service)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        [new Claim(ClaimTypes.Email, "user@example.test")]))
                }
            }
        };
        return controller;
    }

    [Fact]
    public async Task Should_ReturnNotFound_WhenFavoriteDoesNotExist()
    {
        var service = A.Fake<IFavoriteService>();
        A.CallTo(() => service.GetFavorite(A<FavoriteFindModel>._, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult<FavoriteModel>(null));

        var result = await CreateController(service).FindOne(new FavoritePageFindModel { PageId = "page" });

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Should_ReturnNotFound_WhenFavoriteIsAbsent()
    {
        var service = A.Fake<IFavoriteService>();
        A.CallTo(() => service.EnsureInFavorites(A<FavoriteFindModel>._, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult(false));

        var result = await CreateController(service).Exists(new FavoritePageFindModel { PageId = "page" });

        result.Should().BeOfType<NotFoundResult>();
    }
}
