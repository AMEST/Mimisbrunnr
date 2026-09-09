using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.User;

namespace Mimisbrunnr.Web.Tests.User;

public class UserControllerTests
{
    [Fact]
    public async Task Should_ReturnNotFound_WhenCurrentUserIsMissing()
    {
        var service = A.Fake<IUserService>();
        A.CallTo(() => service.GetCurrent(A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult<Mimisbrunnr.Integration.User.UserViewModel>(null));

        var result = await new UserController(service).GetCurrentUser();

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Should_ReturnNotFound_WhenRequestedUserDoesNotExist()
    {
        var service = A.Fake<IUserService>();
        A.CallTo(() => service.GetByEmail("missing@example.test", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult<Mimisbrunnr.Integration.User.UserProfileModel>(null));

        var result = await new UserController(service).Get("missing@example.test");

        result.Should().BeOfType<NotFoundResult>();
    }
}
