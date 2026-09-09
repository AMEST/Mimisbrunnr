using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Integration.Plugin;
using Mimisbrunnr.Web.Plugin;

namespace Mimisbrunnr.Web.Tests.Plugin;

public class PluginControllerTests
{
    [Fact]
    public async Task Should_ForwardPagination_WhenMacrosAreRequested()
    {
        var service = A.Fake<IPluginService>();
        A.CallTo(() => service.GetAvailableMacroses(2, 10)).Returns(Task.FromResult<MacroModel[]>([]));

        var result = await new PluginController(service).GetAvailableMacroses(2, 10);

        result.Should().BeEmpty();
        A.CallTo(() => service.GetAvailableMacroses(2, 10)).MustHaveHappenedOnceExactly();
    }
}
