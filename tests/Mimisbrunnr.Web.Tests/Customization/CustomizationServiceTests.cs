using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Web.Customization;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Infrastructure.Contracts;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Web.Wiki;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Tests.Customization;

public class CustomizationServiceTests
{
    private readonly IPermissionService _permissions = A.Fake<IPermissionService>();
    private readonly IApplicationConfigurationManager _configuration = A.Fake<IApplicationConfigurationManager>();
    private readonly ISpaceService _spaces = A.Fake<ISpaceService>();
    private readonly CustomizationService _service;

    public CustomizationServiceTests() => _service = new(_permissions, _configuration, _spaces);

    [Fact]
    public async Task Should_ReturnEmptyString_WhenConfigurationDoesNotExist()
    {
        A.CallTo(() => _configuration.Get()).Returns(Task.FromResult<ApplicationConfiguration>(null));

        (await _service.GetCustomCss()).Should().BeEmpty();
    }

    [Fact]
    public async Task Should_ReturnConfiguredHomepage_WhenAnonymousAccessIsAllowed()
    {
        var user = new UserInfo { Email = "user@example.test" };
        A.CallTo(() => _configuration.Get()).Returns(Task.FromResult(new ApplicationConfiguration { CustomHomepageEnabled = true, CustomHomepageSpaceKey = "PUBLIC" }));
        A.CallTo(() => _spaces.GetByKey("PUBLIC", user)).Returns(Task.FromResult(new Mimisbrunnr.Integration.Wiki.SpaceModel { HomePageId = "home" }));

        var result = await _service.GetCustomHomepage(user);

        result.HomepageId.Should().Be("home");
        A.CallTo(() => _permissions.EnsureAnonymousAllowed(user)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ReturnNull_WhenCustomHomepageIsDisabled()
    {
        var user = new UserInfo { Email = "user@example.test" };
        A.CallTo(() => _configuration.Get()).Returns(Task.FromResult(new ApplicationConfiguration { CustomHomepageEnabled = false }));

        var result = await _service.GetCustomHomepage(user);

        result.Should().BeNull();
        A.CallTo(() => _spaces.GetByKey(A<string>._, user)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_ReturnCustomCss_WhenConfigurationExists()
    {
        A.CallTo(() => _configuration.Get()).Returns(Task.FromResult(new ApplicationConfiguration { CustomCss = "body { color: red }" }));

        var result = await _service.GetCustomCss();

        result.Should().Be("body { color: red }");
    }
}
