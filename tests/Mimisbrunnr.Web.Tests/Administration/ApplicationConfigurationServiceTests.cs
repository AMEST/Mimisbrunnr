using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
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
}
