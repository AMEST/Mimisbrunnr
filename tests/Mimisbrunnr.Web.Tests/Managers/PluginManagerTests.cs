using FakeItEasy;
using FluentAssertions;
using System.Linq.Expressions;
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
    public async Task Should_ReturnNewState_WhenMacroStateDoesNotExist()
    {
        var states = A.Fake<IRepository<MacroState>>();
        A.CallTo(() => states.GetAll()).Returns(Array.Empty<MacroState>().AsQueryable());
        var manager = new PluginManager(A.Fake<IRepository<Mimisbrunnr.Wiki.Contracts.Plugin>>(), states, NullLogger<PluginManager>.Instance);

        var result = await manager.GetMacroState("page", "macro");

        result.PageId.Should().Be("page");
        result.MacroIdentifierOnPage.Should().Be("macro");
    }

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

    [Fact]
    public async Task Should_NotThrow_WhenDisablingMissingPlugin()
    {
        var repository = A.Fake<IRepository<Mimisbrunnr.Wiki.Contracts.Plugin>>();
        A.CallTo(() => repository.GetAll()).Returns(Array.Empty<Mimisbrunnr.Wiki.Contracts.Plugin>().AsQueryable());
        var manager = new PluginManager(repository, A.Fake<IRepository<MacroState>>(), NullLogger<PluginManager>.Instance);

        await manager.Disable("missing");

        A.CallTo(() => repository.Update(A<Mimisbrunnr.Wiki.Contracts.Plugin>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_EnablePlugin_WhenPluginExists()
    {
        var repository = A.Fake<IRepository<Mimisbrunnr.Wiki.Contracts.Plugin>>();
        var plugin = new Mimisbrunnr.Wiki.Contracts.Plugin { PluginIdentifier = "plugin", Disabled = true };
        A.CallTo(() => repository.GetAll()).Returns(new[] { plugin }.AsQueryable());
        var manager = new PluginManager(repository, A.Fake<IRepository<MacroState>>(), NullLogger<PluginManager>.Instance);

        await manager.Enable("plugin");

        plugin.Disabled.Should().BeFalse();
        A.CallTo(() => repository.Update(plugin, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ReturnExistingState_WhenGettingMacroState()
    {
        var states = A.Fake<IRepository<MacroState>>();
        var state = new MacroState { PageId = "page", MacroIdentifierOnPage = "macro", Params = new Dictionary<string, string> { ["k"] = "v" } };
        A.CallTo(() => states.GetAll()).Returns(new[] { state }.AsQueryable());
        var manager = new PluginManager(A.Fake<IRepository<Mimisbrunnr.Wiki.Contracts.Plugin>>(), states, NullLogger<PluginManager>.Instance);

        var result = await manager.GetMacroState("page", "macro");

        result.Should().BeSameAs(state);
    }

    [Fact]
    public async Task Should_CreateState_WhenStateWithPageAndMacroDoesNotExist()
    {
        var states = A.Fake<IRepository<MacroState>>();
        A.CallTo(() => states.GetAll()).Returns(Array.Empty<MacroState>().AsQueryable());
        var manager = new PluginManager(A.Fake<IRepository<Mimisbrunnr.Wiki.Contracts.Plugin>>(), states, NullLogger<PluginManager>.Instance);
        var macroState = new MacroState { PageId = "page", MacroIdentifierOnPage = "macro" };

        var result = await manager.CreateOrUpdateState(macroState);

        result.Should().BeSameAs(macroState);
        A.CallTo(() => states.Create(macroState, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_UpdateExistingState_WhenNoIdProvided()
    {
        var states = A.Fake<IRepository<MacroState>>();
        var existing = new MacroState { Id = "s1", PageId = "page", MacroIdentifierOnPage = "macro" };
        A.CallTo(() => states.GetAll()).Returns(new[] { existing }.AsQueryable());
        var manager = new PluginManager(A.Fake<IRepository<Mimisbrunnr.Wiki.Contracts.Plugin>>(), states, NullLogger<PluginManager>.Instance);

        var result = await manager.CreateOrUpdateState(new MacroState { PageId = "page", MacroIdentifierOnPage = "macro", Params = new Dictionary<string, string> { ["a"] = "b" } });

        result.Should().BeSameAs(existing);
        existing.Params.Should().ContainKey("a");
        A.CallTo(() => states.Create(A<MacroState>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_UpdateStateByExistingId_WhenPageAndMacroMatch()
    {
        var states = A.Fake<IRepository<MacroState>>();
        var existing = new MacroState { Id = "s1", PageId = "page", MacroIdentifierOnPage = "macro" };
        A.CallTo(() => states.GetAll()).Returns(new[] { existing }.AsQueryable());
        var manager = new PluginManager(A.Fake<IRepository<Mimisbrunnr.Wiki.Contracts.Plugin>>(), states, NullLogger<PluginManager>.Instance);

        var result = await manager.CreateOrUpdateState(new MacroState { Id = "s1", PageId = "page", MacroIdentifierOnPage = "macro", Params = new Dictionary<string, string> { ["x"] = "y" } });

        result.Should().BeSameAs(existing);
        existing.Params.Should().ContainKey("x");
        A.CallTo(() => states.Update(existing, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ShouldThrow_InvalidOperationException_WhenStateByIdNotFound()
    {
        var states = A.Fake<IRepository<MacroState>>();
        A.CallTo(() => states.GetAll()).Returns(Array.Empty<MacroState>().AsQueryable());
        var manager = new PluginManager(A.Fake<IRepository<Mimisbrunnr.Wiki.Contracts.Plugin>>(), states, NullLogger<PluginManager>.Instance);

        await manager.Invoking(x => x.CreateOrUpdateState(new MacroState { Id = "missing", PageId = "page", MacroIdentifierOnPage = "macro" }))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_DeleteAllStateInPage()
    {
        var states = A.Fake<IRepository<MacroState>>();
        var manager = new PluginManager(A.Fake<IRepository<Mimisbrunnr.Wiki.Contracts.Plugin>>(), states, NullLogger<PluginManager>.Instance);

        await manager.DeleteAllStateInPage("page");

        A.CallTo(() => states.DeleteAll(A<Expression<Func<MacroState, bool>>>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_DeleteState_WhenStateExists()
    {
        var states = A.Fake<IRepository<MacroState>>();
        var state = new MacroState { PageId = "page", MacroIdentifierOnPage = "macro" };
        A.CallTo(() => states.GetAll()).Returns(new[] { state }.AsQueryable());
        var manager = new PluginManager(A.Fake<IRepository<Mimisbrunnr.Wiki.Contracts.Plugin>>(), states, NullLogger<PluginManager>.Instance);

        await manager.DeleteState("page", "macro");

        A.CallTo(() => states.Delete(state, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_NotDeleteAnything_WhenStateDoesNotExist()
    {
        var states = A.Fake<IRepository<MacroState>>();
        A.CallTo(() => states.GetAll()).Returns(Array.Empty<MacroState>().AsQueryable());
        var manager = new PluginManager(A.Fake<IRepository<Mimisbrunnr.Wiki.Contracts.Plugin>>(), states, NullLogger<PluginManager>.Instance);

        await manager.DeleteState("page", "macro");

        A.CallTo(() => states.Delete(A<MacroState>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_ReturnPlugin_WhenGettingByPluginIdentifier()
    {
        var repository = A.Fake<IRepository<Mimisbrunnr.Wiki.Contracts.Plugin>>();
        var plugin = new Mimisbrunnr.Wiki.Contracts.Plugin { PluginIdentifier = "plugin" };
        A.CallTo(() => repository.GetAll()).Returns(new[] { plugin }.AsQueryable());
        var manager = new PluginManager(repository, A.Fake<IRepository<MacroState>>(), NullLogger<PluginManager>.Instance);

        var result = await manager.GetPlugin("plugin");

        result.Should().BeSameAs(plugin);
    }

    [Fact]
    public async Task Should_ReturnPlugin_WhenGettingByMacroIdentifier()
    {
        var repository = A.Fake<IRepository<Mimisbrunnr.Wiki.Contracts.Plugin>>();
        var plugin = new Mimisbrunnr.Wiki.Contracts.Plugin { Macros = [new Macro { MacroIdentifier = "macro" }] };
        A.CallTo(() => repository.GetAll()).Returns(new[] { plugin }.AsQueryable());
        var manager = new PluginManager(repository, A.Fake<IRepository<MacroState>>(), NullLogger<PluginManager>.Instance);

        var result = await manager.GetPluginByMacroIdentifier("macro");

        result.Should().BeSameAs(plugin);
    }

    [Fact]
    public async Task Should_ReturnMacro_WhenMacroExists()
    {
        var repository = A.Fake<IRepository<Mimisbrunnr.Wiki.Contracts.Plugin>>();
        var plugin = new Mimisbrunnr.Wiki.Contracts.Plugin { Macros = [new Macro { MacroIdentifier = "macro1" }, new Macro { MacroIdentifier = "macro2" }] };
        A.CallTo(() => repository.GetAll()).Returns(new[] { plugin }.AsQueryable());
        var manager = new PluginManager(repository, A.Fake<IRepository<MacroState>>(), NullLogger<PluginManager>.Instance);

        var result = await manager.GetMacro("macro2");

        result.Should().NotBeNull();
        result.MacroIdentifier.Should().Be("macro2");
    }

    [Fact]
    public async Task Should_ReturnNull_WhenMacroDoesNotExist()
    {
        var repository = A.Fake<IRepository<Mimisbrunnr.Wiki.Contracts.Plugin>>();
        A.CallTo(() => repository.GetAll()).Returns(Array.Empty<Mimisbrunnr.Wiki.Contracts.Plugin>().AsQueryable());
        var manager = new PluginManager(repository, A.Fake<IRepository<MacroState>>(), NullLogger<PluginManager>.Instance);

        var result = await manager.GetMacro("missing");

        result.Should().BeNull();
    }

    [Fact]
    public async Task Should_ApplySkipAndTop_WhenGettingPlugins()
    {
        var repository = A.Fake<IRepository<Mimisbrunnr.Wiki.Contracts.Plugin>>();
        A.CallTo(() => repository.GetAll()).Returns(new[]
        {
            new Mimisbrunnr.Wiki.Contracts.Plugin { PluginIdentifier = "a" },
            new Mimisbrunnr.Wiki.Contracts.Plugin { PluginIdentifier = "b" },
            new Mimisbrunnr.Wiki.Contracts.Plugin { PluginIdentifier = "c" }
        }.AsQueryable());
        var manager = new PluginManager(repository, A.Fake<IRepository<MacroState>>(), NullLogger<PluginManager>.Instance);

        var result = await manager.GetPlugins(top: 2, skip: 1);

        result.Select(x => x.PluginIdentifier).Should().BeEquivalentTo(["b", "c"]);
    }

    [Fact]
    public async Task Should_CreatePlugin_WhenNotInstalledBefore()
    {
        var repository = A.Fake<IRepository<Mimisbrunnr.Wiki.Contracts.Plugin>>();
        A.CallTo(() => repository.GetAll()).Returns(Array.Empty<Mimisbrunnr.Wiki.Contracts.Plugin>().AsQueryable());
        var manager = new PluginManager(repository, A.Fake<IRepository<MacroState>>(), NullLogger<PluginManager>.Instance);
        var plugin = new Mimisbrunnr.Wiki.Contracts.Plugin { PluginIdentifier = "plugin", Version = "1.0", Name = "Test" };
        var user = new UserInfo { Email = "admin@test.com" };

        await manager.InstallPlugin(plugin, user);

        plugin.InstalledBy.Should().BeSameAs(user);
        plugin.Installation.Should().NotBe(default);
        A.CallTo(() => repository.Create(plugin, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_SkipInstallation_WhenSameVersionInstalled()
    {
        var repository = A.Fake<IRepository<Mimisbrunnr.Wiki.Contracts.Plugin>>();
        var existing = new Mimisbrunnr.Wiki.Contracts.Plugin { PluginIdentifier = "plugin", Version = "1.0" };
        A.CallTo(() => repository.GetAll()).Returns(new[] { existing }.AsQueryable());
        var manager = new PluginManager(repository, A.Fake<IRepository<MacroState>>(), NullLogger<PluginManager>.Instance);

        await manager.InstallPlugin(new Mimisbrunnr.Wiki.Contracts.Plugin { PluginIdentifier = "plugin", Version = "1.0" }, new UserInfo { Email = "x@test.com" });

        A.CallTo(() => repository.Create(A<Mimisbrunnr.Wiki.Contracts.Plugin>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => repository.Update(A<Mimisbrunnr.Wiki.Contracts.Plugin>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_UpdatePlugin_WhenVersionDiffers()
    {
        var repository = A.Fake<IRepository<Mimisbrunnr.Wiki.Contracts.Plugin>>();
        var existing = new Mimisbrunnr.Wiki.Contracts.Plugin { PluginIdentifier = "plugin", Version = "1.0", Name = "Old" };
        A.CallTo(() => repository.GetAll()).Returns(new[] { existing }.AsQueryable());
        var manager = new PluginManager(repository, A.Fake<IRepository<MacroState>>(), NullLogger<PluginManager>.Instance);
        var user = new UserInfo { Email = "admin@test.com" };

        await manager.InstallPlugin(new Mimisbrunnr.Wiki.Contracts.Plugin { PluginIdentifier = "plugin", Version = "2.0", Macros = [new Macro { MacroIdentifier = "m" }] }, user);

        existing.Version.Should().Be("2.0");
        existing.Macros.Should().ContainSingle();
        A.CallTo(() => repository.Update(existing, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_DeleteStateAndPlugin_WhenUninstalling()
    {
        var repository = A.Fake<IRepository<Mimisbrunnr.Wiki.Contracts.Plugin>>();
        var states = A.Fake<IRepository<MacroState>>();
        var state1 = new MacroState { MacroIdentifier = "m1" };
        var state2 = new MacroState { MacroIdentifier = "m2" };
        var other = new MacroState { MacroIdentifier = "other" };
        A.CallTo(() => states.GetAll()).Returns(new[] { state1, state2, other }.AsQueryable());
        var manager = new PluginManager(repository, states, NullLogger<PluginManager>.Instance);
        var plugin = new Mimisbrunnr.Wiki.Contracts.Plugin { Macros = [new Macro { MacroIdentifier = "m1" }, new Macro { MacroIdentifier = "m2" }] };

        await manager.UnInstall(plugin, new UserInfo { Email = "x@test.com" });

        A.CallTo(() => states.Delete(state1, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => states.Delete(state2, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => states.Delete(other, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => repository.Delete(plugin, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }
}
