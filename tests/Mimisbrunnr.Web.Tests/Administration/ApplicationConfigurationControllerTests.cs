using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Web.Administration;

namespace Mimisbrunnr.Web.Tests.Administration;

public class ApplicationConfigurationControllerTests
{
    [Fact]
    public async Task Should_ReturnConfiguration_WhenGetIsCalled()
    {
        var service = A.Fake<IApplicationConfigurationService>();
        var expected = new ApplicationConfigurationModel { Title = "Wiki" };
        A.CallTo(() => service.Get()).Returns(Task.FromResult(expected));

        var result = await new ApplicationConfigurationController(service).Get();

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task Should_ForwardUpdate_WhenUpdateIsCalled()
    {
        var service = A.Fake<IApplicationConfigurationService>();
        var model = new ApplicationConfigurationModel { Title = "Wiki" };

        await new ApplicationConfigurationController(service).Update(model);

        A.CallTo(() => service.Update(model, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .MustHaveHappenedOnceExactly();
    }
}
