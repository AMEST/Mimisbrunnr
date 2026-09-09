using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Web.Administration;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Infrastructure.Contracts;
using Mimisbrunnr.Web.Wiki;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Tests.Administration;

public class ApplicationConfigurationServiceTests
{
    [Fact]
    public async Task Should_PersistConfigurationValues_WhenUpdatingConfiguration()
    {
        var configurationManager = A.Fake<IApplicationConfigurationManager>();
        var configuration = new ApplicationConfiguration { Title = "Old", AllowAnonymous = false };
        A.CallTo(() => configurationManager.Get()).Returns(Task.FromResult(configuration));
        var service = new ApplicationConfigurationService(configurationManager, A.Fake<ISpaceService>(), NullLogger<ApplicationConfigurationService>.Instance);
        var model = new ApplicationConfigurationModel { Title = "New", AllowAnonymous = true, AllowHtml = true, UserAutoCreation = true };

        await service.Update(model, new UserInfo { Email = "admin@example.test" });

        configuration.Title.Should().Be("New");
        configuration.AllowAnonymous.Should().BeTrue();
        A.CallTo(() => configurationManager.Configure(configuration)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ThrowInvalidOperationException_WhenEnablingCustomHomepageWithoutSpaceKey()
    {
        var configurationManager = A.Fake<IApplicationConfigurationManager>();
        A.CallTo(() => configurationManager.Get()).Returns(Task.FromResult(new ApplicationConfiguration()));
        var service = new ApplicationConfigurationService(configurationManager, A.Fake<ISpaceService>(), NullLogger<ApplicationConfigurationService>.Instance);

        await service.Invoking(x => x.Update(new ApplicationConfigurationModel { Title = "T", CustomHomepageEnabled = true }, new UserInfo { Email = "admin@example.test" }))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_ThrowInvalidOperationException_WhenCustomHomepageSpaceIsNotPublic()
    {
        var configurationManager = A.Fake<IApplicationConfigurationManager>();
        var spaces = A.Fake<ISpaceService>();
        var user = new UserInfo { Email = "admin@example.test" };
        A.CallTo(() => configurationManager.Get()).Returns(Task.FromResult(new ApplicationConfiguration()));
        A.CallTo(() => spaces.GetByKey("PRIVATE", user)).Returns(Task.FromResult(new SpaceModel { Key = "PRIVATE", Type = SpaceTypeModel.Personal }));
        var service = new ApplicationConfigurationService(configurationManager, spaces, NullLogger<ApplicationConfigurationService>.Instance);

        await service.Invoking(x => x.Update(new ApplicationConfigurationModel { Title = "T", CustomHomepageEnabled = true, CustomHomepageSpaceKey = "PRIVATE" }, user))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_ClearSpaceKey_WhenDisablingCustomHomepage()
    {
        var configurationManager = A.Fake<IApplicationConfigurationManager>();
        var spaces = A.Fake<ISpaceService>();
        var user = new UserInfo { Email = "admin@example.test" };
        var configuration = new ApplicationConfiguration { CustomHomepageEnabled = true, CustomHomepageSpaceKey = "PUBLIC" };
        A.CallTo(() => configurationManager.Get()).Returns(Task.FromResult(configuration));
        var service = new ApplicationConfigurationService(configurationManager, spaces, NullLogger<ApplicationConfigurationService>.Instance);

        await service.Update(new ApplicationConfigurationModel { Title = "T", CustomHomepageEnabled = false }, user);

        configuration.CustomHomepageEnabled.Should().BeFalse();
        configuration.CustomHomepageSpaceKey.Should().BeNull();
        A.CallTo(() => spaces.GetByKey(A<string>._, user)).MustNotHaveHappened();
        A.CallTo(() => configurationManager.Configure(configuration)).MustHaveHappenedOnceExactly();
    }
}
