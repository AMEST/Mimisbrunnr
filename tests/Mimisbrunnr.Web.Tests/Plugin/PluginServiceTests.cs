using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Mimisbrunnr.Integration.Plugin;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

namespace Mimisbrunnr.Web.Tests.Plugin;

public class PluginServiceTests
{
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
        var service = new PluginService(permissions, plugins, pages, spaces, A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>(), A.Fake<ISecurityTokenService>(), NullLogger<PluginService>.Instance);

        await service.SaveMacroState(new MacroStateModel { PageId = "page", MacroIdentifier = "macro", MacroIdentifierOnPage = "macro-on-page" }, user);

        A.CallTo(() => permissions.EnsureEditPermission("SPACE", user)).MustHaveHappenedOnceExactly();
        A.CallTo(() => plugins.CreateOrUpdateState(A<MacroState>._)).MustHaveHappenedOnceExactly();
    }
}
