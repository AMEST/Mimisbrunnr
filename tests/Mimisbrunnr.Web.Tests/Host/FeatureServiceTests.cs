using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Web.Host.Services.Features;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Infrastructure.Contracts;

namespace Mimisbrunnr.Web.Tests.Host;

public class FeatureServiceTests
{
    [Fact]
    public async Task Should_ReturnSwaggerState_WhenSwaggerFeatureIsRequested()
    {
        var configuration = A.Fake<IApplicationConfigurationManager>();
        A.CallTo(() => configuration.Get()).Returns(Task.FromResult(new ApplicationConfiguration { SwaggerEnabled = true }));

        var enabled = await new FeatureService(configuration).IsFeatureEnabled("appconfig__swagger");

        enabled.Should().BeTrue();
    }

    [Fact]
    public async Task Should_ReturnFalse_WhenSwaggerIsDisabled()
    {
        var configuration = A.Fake<IApplicationConfigurationManager>();
        A.CallTo(() => configuration.Get()).Returns(Task.FromResult(new ApplicationConfiguration { SwaggerEnabled = false }));

        var enabled = await new FeatureService(configuration).IsFeatureEnabled("appconfig__swagger");

        enabled.Should().BeFalse();
    }

    [Fact]
    public async Task Should_ReturnFalse_WhenConfigurationDoesNotExist()
    {
        var configuration = A.Fake<IApplicationConfigurationManager>();
        A.CallTo(() => configuration.Get()).Returns(Task.FromResult<ApplicationConfiguration>(null));

        var enabled = await new FeatureService(configuration).IsFeatureEnabled("appconfig__swagger");

        enabled.Should().BeFalse();
    }

    [Fact]
    public async Task Should_ThrowNotImplementedException_WhenRequestingUnknownFeature()
    {
        var service = new FeatureService(A.Fake<IApplicationConfigurationManager>());

        await service.Invoking(x => x.IsFeatureEnabled("custom_feature"))
            .Should().ThrowAsync<NotImplementedException>();
    }

    [Fact]
    public async Task Should_ThrowArgumentOutOfRangeException_WhenRequestingUnknownApplicationFeature()
    {
        var configuration = A.Fake<IApplicationConfigurationManager>();
        A.CallTo(() => configuration.Get()).Returns(Task.FromResult(new ApplicationConfiguration()));
        var service = new FeatureService(configuration);

        await service.Invoking(x => x.IsFeatureEnabled("appconfig__unknown"))
            .Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task Should_ThrowNotImplementedException_WhenSettingCustomFeatureState()
    {
        var service = new FeatureService(A.Fake<IApplicationConfigurationManager>());

        await service.Invoking(x => x.SetFeatureState("custom_feature", true))
            .Should().ThrowAsync<NotImplementedException>();
    }
}
