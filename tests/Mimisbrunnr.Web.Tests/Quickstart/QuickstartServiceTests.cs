using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Quickstart;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Tests.Quickstart;

public class QuickstartServiceTests
{
    private readonly IApplicationConfigurationManager _configuration = A.Fake<IApplicationConfigurationManager>();
    private readonly IUserManager _users = A.Fake<IUserManager>();
    private readonly QuickstartService _service;

    public QuickstartServiceTests() => _service = new(_configuration, _users);

    [Fact]
    public async Task ShouldThrow_InitializeException_WhenInitializingWithoutUser()
    {
        await _service.Invoking(x => x.Initialize(new QuickstartModel(), null))
            .Should().ThrowAsync<InitializeException>();
    }

    [Fact]
    public async Task Should_EnableAutoCreation_WhenInitializingApplication()
    {
        var userInfo = new UserInfo { Email = "admin@example.test" };
        var user = new Mimisbrunnr.Users.User { Email = userInfo.Email };
        A.CallTo(() => _configuration.IsInitialized()).Returns(Task.FromResult(false));
        A.CallTo(() => _users.GetByEmail(userInfo.Email)).Returns(Task.FromResult(user));

        await _service.Initialize(new QuickstartModel { Title = "Wiki" }, userInfo);

        A.CallTo(() => _configuration.Initialize(A<Mimisbrunnr.Web.Infrastructure.Contracts.ApplicationConfiguration>.That.Matches(x => x.Title == "Wiki" && x.UserAutoCreation), user)).MustHaveHappenedOnceExactly();
    }
}
