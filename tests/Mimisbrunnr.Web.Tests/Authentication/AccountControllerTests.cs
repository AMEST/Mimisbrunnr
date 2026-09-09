using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Mimisbrunnr.Web.Authentication.Account;

namespace Mimisbrunnr.Web.Tests.Authentication;

public class AccountControllerTests
{
    [Fact]
    public async Task Should_ReturnBadRequest_WhenTokenCreationFails()
    {
        var service = A.Fake<ITokenService>();
        A.CallTo(() => service.CreateUserToken(A<TokenCreateRequest>._, null))
            .Returns(Task.FromResult<TokenCreateResult>(null));
        var controller = new AccountController(service);

        var result = await controller.CreateToken(new TokenCreateRequest());

        result.Should().BeOfType<BadRequestResult>();
    }

    [Fact]
    public async Task Should_ReturnTokens_WhenTokensAreRequested()
    {
        var service = A.Fake<ITokenService>();
        A.CallTo(() => service.GetUserTokens(null)).Returns(Task.FromResult<IEnumerable<TokenModel>>([]));

        var result = await new AccountController(service).GetTokens();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void Should_UseRootRedirect_WhenProtocolRelativeRedirectIsProvided()
    {
        var url = A.Fake<IUrlHelper>();
        A.CallTo(() => url.IsLocalUrl("//attacker.example")).Returns(false);
        var controller = new AccountController(A.Fake<ITokenService>())
        {
            Url = url,
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var result = controller.Login("//attacker.example");

        result.Should().BeOfType<ChallengeResult>()
            .Which.Properties.RedirectUri.Should().Be("/");
    }

    [Fact]
    public void Should_UseProvidedRedirect_WhenLocalRedirectIsProvided()
    {
        var url = A.Fake<IUrlHelper>();
        A.CallTo(() => url.IsLocalUrl("/space/SPACE")).Returns(true);
        var controller = new AccountController(A.Fake<ITokenService>())
        {
            Url = url,
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var result = controller.Login("/space/SPACE");

        result.Should().BeOfType<ChallengeResult>()
            .Which.Properties.RedirectUri.Should().Be("/space/SPACE");
    }
}
