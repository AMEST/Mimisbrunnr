using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Web.Tests.Managers;

public class PluginManagerTests
{
    public PluginManagerTests() => QueryableAsyncExtensions.EnableFallback();

    [Fact]
    public async Task ShouldThrow_InvalidOperationException_WhenChangingMacroStateIdentity()
    {
        var states = A.Fake<IRepository<MacroState>>();
        A.CallTo(() => states.GetAll()).Returns(new[] { new MacroState { Id = "state", PageId = "page", MacroIdentifierOnPage = "macro" } }.AsQueryable());
        var manager = new PluginManager(A.Fake<IRepository<Mimisbrunnr.Wiki.Contracts.Plugin>>(), states, NullLogger<PluginManager>.Instance);

        await manager.Invoking(x => x.CreateOrUpdateState(new MacroState { Id = "state", PageId = "other-page", MacroIdentifierOnPage = "macro" }))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_DisablePlugin_WhenPluginExists()
    {
        var repository = A.Fake<IRepository<Mimisbrunnr.Wiki.Contracts.Plugin>>();
        var plugin = new Mimisbrunnr.Wiki.Contracts.Plugin { PluginIdentifier = "plugin", Disabled = false };
        A.CallTo(() => repository.GetAll()).Returns(new[] { plugin }.AsQueryable());
        var manager = new PluginManager(repository, A.Fake<IRepository<MacroState>>(), NullLogger<PluginManager>.Instance);

        await manager.Disable("plugin");

        plugin.Disabled.Should().BeTrue();
        A.CallTo(() => repository.Update(plugin, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }
}
