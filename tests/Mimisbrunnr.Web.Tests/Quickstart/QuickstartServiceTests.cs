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

    [Fact]
    public async Task ShouldThrow_InitializeException_WhenInitializingAlreadyInitializedApplication()
    {
        var user = new UserInfo { Email = "admin@example.test" };
        A.CallTo(() => _configuration.IsInitialized()).Returns(Task.FromResult(true));

        await _service.Invoking(x => x.Initialize(new QuickstartModel { Title = "Wiki" }, user))
            .Should().ThrowAsync<InitializeException>();
        A.CallTo(() => _configuration.Initialize(A<Mimisbrunnr.Web.Infrastructure.Contracts.ApplicationConfiguration>._, A<Mimisbrunnr.Users.User>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_ReturnConfiguration_WhenGettingQuickstart()
    {
        A.CallTo(() => _configuration.Get()).Returns(Task.FromResult(new Mimisbrunnr.Web.Infrastructure.Contracts.ApplicationConfiguration { Title = "Wiki", UserAutoCreation = true }));

        var result = await _service.Get();

        result.Title.Should().Be("Wiki");
    }

    [Fact]
    public async Task Should_ReturnInitializedFlag_WhenCheckingInitialization()
    {
        A.CallTo(() => _configuration.IsInitialized()).Returns(Task.FromResult(true));

        (await _service.IsInitialized()).Should().BeTrue();
    }
}
