using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Mimisbrunnr.Integration.Plugin;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Plugin;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

namespace Mimisbrunnr.Web.Tests.Plugin;

public class PluginServiceTests
{
    private static PluginService BuildService(IPermissionService permissions, IPluginManager plugins, IPageManager pages, ISpaceManager spaces, IUserManager users, ITemplateRenderer renderer, ISecurityTokenService tokens)
    {
        return new PluginService(permissions, plugins, pages, spaces, users, renderer, tokens, NullLogger<PluginService>.Instance);
    }

    [Fact]
    public async Task Should_RequireEditPermission_WhenSavingMacroState()
    {
        var plugins = A.Fake<IPluginManager>();
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        var permissions = A.Fake<IPermissionService>();
        var user = new UserInfo { Email = "user@example.test" };
        A.CallTo(() => pages.GetById("page")).Returns(Task.FromResult(new Page { Id = "page", SpaceId = "space" }));
        A.CallTo(() => spaces.GetById("space")).Returns(Task.FromResult(new Space { Id = "space", Key = "SPACE" }));
        var service = BuildService(permissions, plugins, pages, spaces, A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>(), A.Fake<ISecurityTokenService>());

        await service.SaveMacroState(new MacroStateModel { PageId = "page", MacroIdentifier = "macro", MacroIdentifierOnPage = "macro-on-page" }, user);

        A.CallTo(() => permissions.EnsureEditPermission("SPACE", user)).MustHaveHappenedOnceExactly();
        A.CallTo(() => plugins.CreateOrUpdateState(A<MacroState>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ReturnOnlyEnabledMacros_WhenGettingAvailableMacrosesWithoutPaging()
    {
        var plugins = A.Fake<IPluginManager>();
        A.CallTo(() => plugins.GetPlugins(null, null)).Returns(Task.FromResult(new[]
        {
            new Mimisbrunnr.Wiki.Contracts.Plugin { PluginIdentifier = "p1", Macros = new[] { new Macro { MacroIdentifier = "m1" }, new Macro { MacroIdentifier = "m-disabled", Disabled = true } } },
            new Mimisbrunnr.Wiki.Contracts.Plugin { PluginIdentifier = "p2", Disabled = true, Macros = new[] { new Macro { MacroIdentifier = "m-in-disabled-plugin" } } }
        }));
        var service = BuildService(A.Fake<IPermissionService>(), plugins, A.Fake<IPageManager>(), A.Fake<ISpaceManager>(), A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>(), A.Fake<ISecurityTokenService>());

        var result = await service.GetAvailableMacroses();

        result.Should().ContainSingle().Which.MacroIdentifier.Should().Be("m1");
    }

    [Fact]
    public async Task Should_ApplySkipAndTake_WhenGettingAvailableMacroses()
    {
        var plugins = A.Fake<IPluginManager>();
        A.CallTo(() => plugins.GetPlugins(null, null)).Returns(Task.FromResult(new[]
        {
            new Mimisbrunnr.Wiki.Contracts.Plugin { PluginIdentifier = "p1", Macros = new[] { new Macro { MacroIdentifier = "m1" }, new Macro { MacroIdentifier = "m2" }, new Macro { MacroIdentifier = "m3" } } }
        }));
        var service = BuildService(A.Fake<IPermissionService>(), plugins, A.Fake<IPageManager>(), A.Fake<ISpaceManager>(), A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>(), A.Fake<ISecurityTokenService>());

        var result = await service.GetAvailableMacroses(1, 1);

        result.Should().ContainSingle().Which.MacroIdentifier.Should().Be("m2");
    }

    [Fact]
    public async Task Should_ReturnMacroInfo_WhenMacroExists()
    {
        var plugins = A.Fake<IPluginManager>();
        A.CallTo(() => plugins.GetMacro("m1")).Returns(Task.FromResult(new Macro { MacroIdentifier = "m1", Name = "Macro" }));
        var service = BuildService(A.Fake<IPermissionService>(), plugins, A.Fake<IPageManager>(), A.Fake<ISpaceManager>(), A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>(), A.Fake<ISecurityTokenService>());

        var result = await service.GetMacroInfo("m1");

        result.MacroIdentifier.Should().Be("m1");
    }

    [Fact]
    public async Task Should_ThrowPluginNotFoundException_WhenMacroInfoNotFound()
    {
        var plugins = A.Fake<IPluginManager>();
        A.CallTo(() => plugins.GetMacro("m1")).Returns(Task.FromResult<Macro>(null));
        var service = BuildService(A.Fake<IPermissionService>(), plugins, A.Fake<IPageManager>(), A.Fake<ISpaceManager>(), A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>(), A.Fake<ISecurityTokenService>());

        var action = () => service.GetMacroInfo("m1");

        await action.Should().ThrowAsync<PluginNotFoundException>();
    }

    [Fact]
    public async Task Should_ThrowPageNotFoundException_WhenGettingMacroStateOnMissingPage()
    {
        var pages = A.Fake<IPageManager>();
        A.CallTo(() => pages.GetById("missing")).Returns(Task.FromResult<Page>(null));
        var service = BuildService(A.Fake<IPermissionService>(), A.Fake<IPluginManager>(), pages, A.Fake<ISpaceManager>(), A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>(), A.Fake<ISecurityTokenService>());

        var action = () => service.GetMacroState("missing", "m-id", new UserInfo { Email = "user@example.test" });

        await action.Should().ThrowAsync<PageNotFoundException>();
    }

    [Fact]
    public async Task Should_ThrowSpaceNotFoundException_WhenGettingMacroStateOnMissingSpace()
    {
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        A.CallTo(() => pages.GetById("page")).Returns(Task.FromResult(new Page { Id = "page", SpaceId = "missing-space" }));
        A.CallTo(() => spaces.GetById("missing-space")).Returns(Task.FromResult<Space>(null));
        var service = BuildService(A.Fake<IPermissionService>(), A.Fake<IPluginManager>(), pages, spaces, A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>(), A.Fake<ISecurityTokenService>());

        var action = () => service.GetMacroState("page", "m-id", new UserInfo { Email = "user@example.test" });

        await action.Should().ThrowAsync<SpaceNotFoundException>();
    }

    [Fact]
    public async Task Should_ReturnMacroState_WhenGettingMacroState()
    {
        var plugins = A.Fake<IPluginManager>();
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        var permissions = A.Fake<IPermissionService>();
        var user = new UserInfo { Email = "user@example.test" };
        A.CallTo(() => pages.GetById("page")).Returns(Task.FromResult(new Page { Id = "page", SpaceId = "space" }));
        A.CallTo(() => spaces.GetById("space")).Returns(Task.FromResult(new Space { Id = "space", Key = "SPACE" }));
        A.CallTo(() => plugins.GetMacroState("page", "m-id")).Returns(Task.FromResult(new MacroState { MacroIdentifier = "m1" }));
        var service = BuildService(permissions, plugins, pages, spaces, A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>(), A.Fake<ISecurityTokenService>());

        var result = await service.GetMacroState("page", "m-id", user);

        result.MacroIdentifier.Should().Be("m1");
        A.CallTo(() => permissions.EnsureViewPermission("SPACE", user)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ThrowPluginNotFoundException_WhenUninstallingNotInstalledPlugin()
    {
        var plugins = A.Fake<IPluginManager>();
        A.CallTo(() => plugins.GetPlugin("p1")).Returns(Task.FromResult<Mimisbrunnr.Wiki.Contracts.Plugin>(null));
        var service = BuildService(A.Fake<IPermissionService>(), plugins, A.Fake<IPageManager>(), A.Fake<ISpaceManager>(), A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>(), A.Fake<ISecurityTokenService>());

        var action = () => service.UnInstallPlugin("p1", new UserInfo { Email = "user@example.test" });

        await action.Should().ThrowAsync<PluginNotFoundException>();
    }

    [Fact]
    public async Task Should_UninstallInstalledPlugin()
    {
        var plugins = A.Fake<IPluginManager>();
        var plugin = new Mimisbrunnr.Wiki.Contracts.Plugin { PluginIdentifier = "p1" };
        var user = new UserInfo { Email = "user@example.test" };
        A.CallTo(() => plugins.GetPlugin("p1")).Returns(Task.FromResult(plugin));
        var service = BuildService(A.Fake<IPermissionService>(), plugins, A.Fake<IPageManager>(), A.Fake<ISpaceManager>(), A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>(), A.Fake<ISecurityTokenService>());

        await service.UnInstallPlugin("p1", user);

        A.CallTo(() => plugins.UnInstall(plugin, user)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ThrowMacroNotFoundException_WhenRenderingMissingMacro()
    {
        var plugins = A.Fake<IPluginManager>();
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        A.CallTo(() => pages.GetById("page")).Returns(Task.FromResult(new Page { Id = "page", SpaceId = "space" }));
        A.CallTo(() => spaces.GetById("space")).Returns(Task.FromResult(new Space { Id = "space", Key = "SPACE" }));
        A.CallTo(() => plugins.GetMacroState("page", "m-id")).Returns(Task.FromResult(new MacroState { MacroIdentifier = "m1" }));
        A.CallTo(() => plugins.GetMacro("m1")).Returns(Task.FromResult<Macro>(null));
        var service = BuildService(A.Fake<IPermissionService>(), plugins, pages, spaces, A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>(), A.Fake<ISecurityTokenService>());

        var action = () => service.Render("page", "m-id", new MacroRenderUserRequest(), new UserInfo { Email = "user@example.test" });

        await action.Should().ThrowAsync<MacroNotFoundException>();
    }

    [Fact]
    public async Task Should_ThrowMacroNotFoundException_WhenRenderingMacroOfDisabledPlugin()
    {
        var plugins = A.Fake<IPluginManager>();
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        A.CallTo(() => pages.GetById("page")).Returns(Task.FromResult(new Page { Id = "page", SpaceId = "space" }));
        A.CallTo(() => spaces.GetById("space")).Returns(Task.FromResult(new Space { Id = "space", Key = "SPACE" }));
        A.CallTo(() => plugins.GetMacroState("page", "m-id")).Returns(Task.FromResult(new MacroState { MacroIdentifier = "m1" }));
        A.CallTo(() => plugins.GetMacro("m1")).Returns(Task.FromResult(new Macro { MacroIdentifier = "m1" }));
        A.CallTo(() => plugins.GetPluginByMacroIdentifier("m1")).Returns(Task.FromResult(new Mimisbrunnr.Wiki.Contracts.Plugin { PluginIdentifier = "p1", Disabled = true }));
        var service = BuildService(A.Fake<IPermissionService>(), plugins, pages, spaces, A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>(), A.Fake<ISecurityTokenService>());

        var action = () => service.Render("page", "m-id", new MacroRenderUserRequest(), new UserInfo { Email = "user@example.test" });

        await action.Should().ThrowAsync<MacroNotFoundException>();
    }

    [Fact]
    public async Task Should_RenderLocalMacroWithMergedAndSystemParams_WhenRendering()
    {
        var plugins = A.Fake<IPluginManager>();
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        var users = A.Fake<IUserManager>();
        var renderer = A.Fake<ITemplateRenderer>();
        var user = new UserInfo { Email = "user@example.test", Name = "User" };
        A.CallTo(() => pages.GetById("page")).Returns(Task.FromResult(new Page { Id = "page", SpaceId = "space", Name = "PageName" }));
        A.CallTo(() => spaces.GetById("space")).Returns(Task.FromResult(new Space { Id = "space", Key = "SPACE", Name = "SpaceName" }));
        A.CallTo(() => plugins.GetMacroState("page", "m-id")).Returns(Task.FromResult(new MacroState { MacroIdentifier = "m1", Params = new Dictionary<string, string> { { "StateKey", "StateValue" } } }));
        A.CallTo(() => plugins.GetMacro("m1")).Returns(Task.FromResult(new Macro { MacroIdentifier = "m1", Template = "{{PageName}}", DefaultValues = new Dictionary<string, string> { { "DefKey", "DefValue" } } }));
        A.CallTo(() => plugins.GetPluginByMacroIdentifier("m1")).Returns(Task.FromResult(new Mimisbrunnr.Wiki.Contracts.Plugin { PluginIdentifier = "p1" }));
        A.CallTo(() => users.GetByEmail(user.Email)).Returns(Task.FromResult(new Mimisbrunnr.Users.User { Email = user.Email, Role = UserRole.Employee }));
        A.CallTo(() => renderer.Render(A<string>._, A<IDictionary<string, object>>._)).Returns("rendered");
        var service = BuildService(A.Fake<IPermissionService>(), plugins, pages, spaces, users, renderer, A.Fake<ISecurityTokenService>());

        var result = await service.Render("page", "m-id", new MacroRenderUserRequest { Params = new Dictionary<string, string> { { "UserKey", "UserValue" } } }, user);

        result.Html.Should().Be("rendered");
        A.CallTo(() => renderer.Render("{{PageName}}", A<IDictionary<string, object>>.That.Matches(d =>
            d.ContainsKey("UserKey") && d.ContainsKey("StateKey") && d.ContainsKey("DefKey") &&
            d.ContainsKey("MacroIdOnPage") && d["MacroIdOnPage"].Equals("m-id") &&
            d.ContainsKey("PageId") && d.ContainsKey("SpaceKey") && d["SpaceKey"].Equals("SPACE") &&
            d.ContainsKey("UserEmail") && d["UserEmail"].Equals(user.Email) &&
            d.ContainsKey("UserRole") && d["UserRole"].Equals(UserRole.Employee.ToString())))).MustHaveHappenedOnceExactly();
        A.CallTo(() => users.GetByEmail(user.Email)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_NotLookupUser_WhenRenderingForAnonymous()
    {
        var plugins = A.Fake<IPluginManager>();
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        var users = A.Fake<IUserManager>();
        var renderer = A.Fake<ITemplateRenderer>();
        A.CallTo(() => pages.GetById("page")).Returns(Task.FromResult(new Page { Id = "page", SpaceId = "space", Name = "PageName" }));
        A.CallTo(() => spaces.GetById("space")).Returns(Task.FromResult(new Space { Id = "space", Key = "SPACE", Name = "SpaceName" }));
        A.CallTo(() => plugins.GetMacroState("page", "m-id")).Returns(Task.FromResult(new MacroState { MacroIdentifier = "m1" }));
        A.CallTo(() => plugins.GetMacro("m1")).Returns(Task.FromResult(new Macro { MacroIdentifier = "m1", Template = "x" }));
        A.CallTo(() => plugins.GetPluginByMacroIdentifier("m1")).Returns(Task.FromResult(new Mimisbrunnr.Wiki.Contracts.Plugin { PluginIdentifier = "p1" }));
        A.CallTo(() => renderer.Render(A<string>._, A<IDictionary<string, object>>._)).Returns("rendered");
        var service = BuildService(A.Fake<IPermissionService>(), plugins, pages, spaces, users, renderer, A.Fake<ISecurityTokenService>());

        var result = await service.Render("page", "m-id", new MacroRenderUserRequest(), null);

        result.Html.Should().Be("rendered");
        A.CallTo(() => users.GetByEmail(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => renderer.Render(A<string>._, A<IDictionary<string, object>>.That.Matches(d => d.ContainsKey("UserRole") && d["UserRole"].Equals(string.Empty)))).MustHaveHappenedOnceExactly();
    }
}
