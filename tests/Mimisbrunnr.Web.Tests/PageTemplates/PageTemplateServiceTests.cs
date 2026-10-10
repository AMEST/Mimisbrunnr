using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Integration.PageTemplates;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.PageTemplates.Contracts;
using Mimisbrunnr.PageTemplates.Services;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.PageTemplates;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

namespace Mimisbrunnr.Web.Tests.PageTemplates;

public class PageTemplateServiceTests
{
    [Fact]
    public async Task ShouldThrow_UserHasNotPermissionException_WhenUserReadsAnotherUsersTemplate()
    {
        var manager = A.Fake<IPageTemplateManager>();
        A.CallTo(() => manager.GetById("template")).Returns(Task.FromResult(new PageTemplate { Id = "template", Type = TemplateType.User, OwnerEmail = "owner@example.test" }));
        var service = new PageTemplateService(manager, A.Fake<ISpaceManager>(), A.Fake<IPermissionService>(), A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>(), A.Fake<IPluginManager>());

        await service.Invoking(x => x.GetById("template", new UserInfo { Email = "other@example.test" }))
            .Should().ThrowAsync<UserHasNotPermissionException>();
    }

    [Fact]
    public async Task Should_PersistCurrentUserAsOwner_WhenCreatingUserTemplate()
    {
        var manager = A.Fake<IPageTemplateManager>();
        A.CallTo(() => manager.Create(A<PageTemplate>._)).ReturnsLazily(call => Task.FromResult(call.GetArgument<PageTemplate>(0)));
        var service = new PageTemplateService(manager, A.Fake<ISpaceManager>(), A.Fake<IPermissionService>(), A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>(), A.Fake<IPluginManager>());
        var user = new UserInfo { Email = "user@example.test" };

        await service.Create(new PageTemplateCreateModel { Name = "Template", Type = TemplateType.User }, user);

        A.CallTo(() => manager.Create(A<PageTemplate>.That.Matches(x => x.OwnerEmail == user.Email && x.Type == TemplateType.User))).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ReturnSystemUserAndSpaceTemplates_WhenGettingAllWithoutType()
    {
        var manager = A.Fake<IPageTemplateManager>();
        var spaces = A.Fake<ISpaceManager>();
        var permissions = A.Fake<IPermissionService>();
        var user = new UserInfo { Email = "user@example.test" };
        A.CallTo(() => manager.GetAll()).Returns(new[]
        {
            new PageTemplate { Id = "system", Type = TemplateType.System },
            new PageTemplate { Id = "mine", Type = TemplateType.User, OwnerEmail = user.Email },
            new PageTemplate { Id = "other", Type = TemplateType.User, OwnerEmail = "other@example.test" },
            new PageTemplate { Id = "space-template", Type = TemplateType.Space, SpaceId = "s1" }
        }.AsQueryable());
        A.CallTo(() => spaces.GetByKey("KEY")).Returns(Task.FromResult(new Space { Id = "s1", Key = "KEY" }));
        var service = new PageTemplateService(manager, spaces, permissions, A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>(), A.Fake<IPluginManager>());

        var result = await service.GetAll(null, "KEY", user);

        result.Should().HaveCount(3);
        result.Should().Contain(x => x.Id == "system");
        result.Should().Contain(x => x.Id == "mine");
        result.Should().Contain(x => x.Id == "space-template");
        A.CallTo(() => permissions.EnsureEditPermission("KEY", user)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ReturnOnlySystemTemplates_WhenGettingAllWithSystemType()
    {
        var manager = A.Fake<IPageTemplateManager>();
        A.CallTo(() => manager.GetAll()).Returns(new[]
        {
            new PageTemplate { Id = "system", Type = TemplateType.System },
            new PageTemplate { Id = "mine", Type = TemplateType.User, OwnerEmail = "user@example.test" }
        }.AsQueryable());
        var service = new PageTemplateService(manager, A.Fake<ISpaceManager>(), A.Fake<IPermissionService>(), A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>(), A.Fake<IPluginManager>());

        var result = await service.GetAll(TemplateType.System, null, new UserInfo { Email = "user@example.test" });

        result.Should().ContainSingle().Which.Id.Should().Be("system");
    }

    [Fact]
    public async Task Should_SkipSpaceTemplates_WhenUserHasNoEditPermission()
    {
        var manager = A.Fake<IPageTemplateManager>();
        var permissions = A.Fake<IPermissionService>();
        var user = new UserInfo { Email = "user@example.test" };
        A.CallTo(() => manager.GetAll()).Returns(new[]
        {
            new PageTemplate { Id = "space-template", Type = TemplateType.Space, SpaceId = "s1" }
        }.AsQueryable());
        A.CallTo(() => permissions.EnsureEditPermission("KEY", user)).ThrowsAsync(new UserHasNotPermissionException());
        var service = new PageTemplateService(manager, A.Fake<ISpaceManager>(), permissions, A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>(), A.Fake<IPluginManager>());

        var result = await service.GetAll(TemplateType.Space, "KEY", user);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_AllowReadingSystemTemplate_WhenGettingById()
    {
        var manager = A.Fake<IPageTemplateManager>();
        A.CallTo(() => manager.GetById("system")).Returns(Task.FromResult(new PageTemplate { Id = "system", Type = TemplateType.System }));
        var service = new PageTemplateService(manager, A.Fake<ISpaceManager>(), A.Fake<IPermissionService>(), A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>(), A.Fake<IPluginManager>());

        var result = await service.GetById("system", new UserInfo { Email = "user@example.test" });

        result.Id.Should().Be("system");
    }

    [Fact]
    public async Task Should_RequireEditPermission_WhenReadingSpaceTemplate()
    {
        var manager = A.Fake<IPageTemplateManager>();
        var spaces = A.Fake<ISpaceManager>();
        var permissions = A.Fake<IPermissionService>();
        var user = new UserInfo { Email = "user@example.test" };
        A.CallTo(() => manager.GetById("template")).Returns(Task.FromResult(new PageTemplate { Id = "template", Type = TemplateType.Space, SpaceId = "s1" }));
        A.CallTo(() => spaces.GetById("s1")).Returns(Task.FromResult(new Space { Id = "s1", Key = "KEY" }));
        var service = new PageTemplateService(manager, spaces, permissions, A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>(), A.Fake<IPluginManager>());

        var result = await service.GetById("template", user);

        result.Id.Should().Be("template");
        A.CallTo(() => permissions.EnsureEditPermission("KEY", user)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ThrowInvalidOperationException_WhenCreatingSpaceTemplateWithoutSpaceKey()
    {
        var manager = A.Fake<IPageTemplateManager>();
        var service = new PageTemplateService(manager, A.Fake<ISpaceManager>(), A.Fake<IPermissionService>(), A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>(), A.Fake<IPluginManager>());

        await service.Invoking(x => x.Create(new PageTemplateCreateModel { Name = "T", Type = TemplateType.Space }, new UserInfo { Email = "user@example.test" }))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_ThrowInvalidOperationException_WhenCreatingUnknownTemplateType()
    {
        var manager = A.Fake<IPageTemplateManager>();
        var service = new PageTemplateService(manager, A.Fake<ISpaceManager>(), A.Fake<IPermissionService>(), A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>(), A.Fake<IPluginManager>());

        await service.Invoking(x => x.Create(new PageTemplateCreateModel { Name = "T", Type = "Fake" }, new UserInfo { Email = "user@example.test" }))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_ThrowUserHasNotPermissionException_WhenNonAdminCreatesSystemTemplate()
    {
        var users = A.Fake<IUserManager>();
        var user = new UserInfo { Email = "user@example.test" };
        A.CallTo(() => users.GetByEmail(user.Email)).Returns(Task.FromResult(new Mimisbrunnr.Users.User { Role = UserRole.Employee }));
        var service = new PageTemplateService(A.Fake<IPageTemplateManager>(), A.Fake<ISpaceManager>(), A.Fake<IPermissionService>(), users, A.Fake<ITemplateRenderer>(), A.Fake<IPluginManager>());

        await service.Invoking(x => x.Create(new PageTemplateCreateModel { Name = "T", Type = TemplateType.System }, user))
            .Should().ThrowAsync<UserHasNotPermissionException>();
    }

    [Fact]
    public async Task Should_ThrowSpaceNotFoundException_WhenCreatingSpaceTemplateInMissingSpace()
    {
        var permissions = A.Fake<IPermissionService>();
        var spaces = A.Fake<ISpaceManager>();
        A.CallTo(() => spaces.GetByKey("KEY")).Returns(Task.FromResult<Space>(null));
        var service = new PageTemplateService(A.Fake<IPageTemplateManager>(), spaces, permissions, A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>(), A.Fake<IPluginManager>());

        await service.Invoking(x => x.Create(new PageTemplateCreateModel { Name = "T", Type = TemplateType.Space, SpaceKey = "KEY" }, new UserInfo { Email = "user@example.test" }))
            .Should().ThrowAsync<SpaceNotFoundException>();
    }

    [Fact]
    public async Task Should_ThrowUserHasNotPermissionException_WhenUpdatingAnotherUsersTemplate()
    {
        var manager = A.Fake<IPageTemplateManager>();
        A.CallTo(() => manager.GetById("template")).Returns(Task.FromResult(new PageTemplate { Id = "template", Type = TemplateType.User, OwnerEmail = "owner@example.test" }));
        var service = new PageTemplateService(manager, A.Fake<ISpaceManager>(), A.Fake<IPermissionService>(), A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>(), A.Fake<IPluginManager>());

        await service.Invoking(x => x.Update("template", new PageTemplateUpdateModel(), new UserInfo { Email = "other@example.test" }))
            .Should().ThrowAsync<UserHasNotPermissionException>();
    }

    [Fact]
    public async Task Should_UpdateTemplate_WhenOwnerUpdatesOwnTemplate()
    {
        var manager = A.Fake<IPageTemplateManager>();
        var user = new UserInfo { Email = "owner@example.test" };
        A.CallTo(() => manager.GetById("template")).Returns(Task.FromResult(new PageTemplate { Id = "template", Type = TemplateType.User, OwnerEmail = user.Email }));
        var service = new PageTemplateService(manager, A.Fake<ISpaceManager>(), A.Fake<IPermissionService>(), A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>(), A.Fake<IPluginManager>());

        await service.Update("template", new PageTemplateUpdateModel { Name = "New", Description = "Desc", Content = "Content" }, user);

        A.CallTo(() => manager.Update("template", "New", "Desc", "Content", user)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ThrowSpaceNotFoundException_WhenRenderingTemplateInMissingSpace()
    {
        var manager = A.Fake<IPageTemplateManager>();
        var spaces = A.Fake<ISpaceManager>();
        A.CallTo(() => manager.GetById("template")).Returns(Task.FromResult(new PageTemplate { Id = "template", Type = TemplateType.System }));
        A.CallTo(() => spaces.GetByKey("KEY")).Returns(Task.FromResult<Space>(null));
        var service = new PageTemplateService(manager, spaces, A.Fake<IPermissionService>(), A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>(), A.Fake<IPluginManager>());

        await service.Invoking(x => x.Render("template", "KEY", new UserInfo { Email = "user@example.test" }))
            .Should().ThrowAsync<SpaceNotFoundException>();
    }

    [Fact]
    public async Task Should_RenderTemplateWithParameters_WhenRendering()
    {
        var manager = A.Fake<IPageTemplateManager>();
        var spaces = A.Fake<ISpaceManager>();
        var renderer = A.Fake<ITemplateRenderer>();
        var user = new UserInfo { Email = "user@example.test", Name = "User" };
        A.CallTo(() => manager.GetById("template")).Returns(Task.FromResult(new PageTemplate { Id = "template", Type = TemplateType.System, Content = "Hello {{UserName}}" }));
        A.CallTo(() => spaces.GetByKey("KEY")).Returns(Task.FromResult(new Space { Id = "s1", Key = "KEY", Name = "SpaceName" }));
        A.CallTo(() => renderer.Render(A<string>._, A<IDictionary<string, object>>._)).Returns("Hello User");
        var service = new PageTemplateService(manager, spaces, A.Fake<IPermissionService>(), A.Fake<IUserManager>(), renderer, A.Fake<IPluginManager>());

        var result = await service.Render("template", "KEY", user);

        result.Content.Should().Be("Hello User");
        A.CallTo(() => renderer.Render("Hello {{UserName}}", A<IDictionary<string, object>>.That.Matches(d =>
            d.ContainsKey("UserName") && d["UserName"].Equals("User") &&
            d.ContainsKey("UserEmail") && d["UserEmail"].Equals(user.Email) &&
            d.ContainsKey("SpaceKey") && d["SpaceKey"].Equals("KEY") &&
            d.ContainsKey("SpaceName") && d["SpaceName"].Equals("SpaceName") &&
            d.ContainsKey("CurrentDate")))).MustHaveHappenedOnceExactly();
    }

    private static (PageTemplateService Service, IPageTemplateManager Templates, IPluginManager Plugins,
        ISpaceManager Spaces, IPermissionService Permissions, ITemplateRenderer Renderer) PluginTemplateService(
        params Mimisbrunnr.Wiki.Contracts.Plugin[] plugins)
    {
        var templates = A.Fake<IPageTemplateManager>();
        A.CallTo(() => templates.GetAll()).Returns(Array.Empty<PageTemplate>().AsQueryable());
        var pluginManager = A.Fake<IPluginManager>();
        A.CallTo(() => pluginManager.GetPlugins(null, null)).Returns(plugins);
        A.CallTo(() => pluginManager.GetPlugin(A<string>._)).ReturnsLazily(call =>
            Task.FromResult(plugins.FirstOrDefault(x => x.PluginIdentifier == call.GetArgument<string>(0))));
        var spaces = A.Fake<ISpaceManager>();
        var permissions = A.Fake<IPermissionService>();
        var renderer = A.Fake<ITemplateRenderer>();
        return (new PageTemplateService(templates, spaces, permissions, A.Fake<IUserManager>(), renderer, pluginManager),
            templates, pluginManager, spaces, permissions, renderer);
    }

    private static Mimisbrunnr.Wiki.Contracts.Plugin TemplatePlugin(string identifier = "plugin") => new()
    {
        PluginIdentifier = identifier, Name = "Template plugin", Version = "1.0",
        PageTemplates = [new PluginPageTemplate
        {
            TemplateIdentifier = "meeting", Name = "Meeting", Description = "Meeting notes",
            Content = "# {{SpaceName}} {{UserName}} {{CurrentDate}}"
        }]
    };

    [Theory]
    [InlineData(null)]
    [InlineData("System")]
    public async Task Should_ReturnGlobalPluginTemplatesWithSource_WhenListingSystemOrAllTemplates(string type)
    {
        var plugin = TemplatePlugin();
        var fixture = PluginTemplateService(plugin);
        A.CallTo(() => fixture.Templates.GetAll()).Returns(new[] { new PageTemplate { Id = "normal", Type = "System" } }.AsQueryable());

        var result = await fixture.Service.GetAll(type, null, new UserInfo());

        result.Should().HaveCount(2);
        var template = result.Single(x => x.IsReadOnly);
        template.PluginIdentifier.Should().Be(plugin.PluginIdentifier);
        template.PluginName.Should().Be(plugin.Name);
        template.Type.Should().Be(TemplateType.System);
        template.Content.Should().Be(plugin.PageTemplates[0].Content);
        var fetched = await fixture.Service.GetById(template.Id, new UserInfo());
        fetched.Should().BeEquivalentTo(template);
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Space")]
    public async Task Should_NotIncludePluginTemplates_WhenFilteringOtherTypes(string type)
    {
        var fixture = PluginTemplateService(TemplatePlugin());
        (await fixture.Service.GetAll(type, null, new UserInfo())).Should().BeEmpty();
    }

    [Fact]
    public async Task Should_KeepDistinctStableIds_WhenPluginsUseSameTemplateIdentifierAndUpdateContent()
    {
        var first = TemplatePlugin("плагин/a.b");
        var second = TemplatePlugin("плагин/a");
        var fixture = PluginTemplateService(first, second);
        var before = await fixture.Service.GetAll(null, null, new UserInfo());
        before.Select(x => x.Id).Distinct().Should().HaveCount(2);
        foreach (var template in before)
            (await fixture.Service.GetById(template.Id, new UserInfo())).PluginIdentifier.Should().Be(template.PluginIdentifier);

        first.Version = "2.0";
        first.PageTemplates[0].Content = "Updated";
        var after = await fixture.Service.GetAll(null, null, new UserInfo());
        after.Select(x => x.Id).Should().Equal(before.Select(x => x.Id));
        (await fixture.Service.GetById(before[0].Id, new UserInfo())).Content.Should().Be("Updated");
    }

    [Fact]
    public async Task Should_HideAndRestoreTemplates_WhenPluginIsDisabledAndEnabled()
    {
        var plugin = TemplatePlugin();
        var fixture = PluginTemplateService(plugin);
        var template = (await fixture.Service.GetAll(null, null, new UserInfo())).Single();
        plugin.Disabled = true;

        (await fixture.Service.GetAll(null, null, new UserInfo())).Should().BeEmpty();
        await fixture.Service.Invoking(x => x.GetById(template.Id, new UserInfo())).Should().ThrowAsync<PageTemplateNotFoundException>();
        await fixture.Service.Invoking(x => x.Render(template.Id, "SPACE", new UserInfo())).Should().ThrowAsync<PageTemplateNotFoundException>();
        A.CallTo(() => fixture.Renderer.Render(A<string>._, A<IDictionary<string, object>>._)).MustNotHaveHappened();

        plugin.Disabled = false;
        (await fixture.Service.GetAll(null, null, new UserInfo())).Should().ContainSingle().Which.Id.Should().Be(template.Id);
    }

    [Fact]
    public async Task Should_NotResolveRemovedTemplates_WhenPluginOrTemplateIsRemoved()
    {
        var plugin = TemplatePlugin();
        var fixture = PluginTemplateService(plugin);
        var template = (await fixture.Service.GetAll(null, null, new UserInfo())).Single();
        plugin.PageTemplates = [];
        await fixture.Service.Invoking(x => x.GetById(template.Id, new UserInfo())).Should().ThrowAsync<PageTemplateNotFoundException>();
        A.CallTo(() => fixture.Plugins.GetPlugin(plugin.PluginIdentifier)).Returns(Task.FromResult<Mimisbrunnr.Wiki.Contracts.Plugin>(null));
        await fixture.Service.Invoking(x => x.Render(template.Id, "SPACE", new UserInfo())).Should().ThrowAsync<PageTemplateNotFoundException>();
    }

    [Fact]
    public async Task Should_RejectChangesWithoutWriting_WhenEditingPluginTemplate()
    {
        var fixture = PluginTemplateService(TemplatePlugin());
        var template = (await fixture.Service.GetAll(null, null, new UserInfo())).Single();
        await fixture.Service.Invoking(x => x.Update(template.Id, new PageTemplateUpdateModel { Name = "Changed" }, new UserInfo()))
            .Should().ThrowAsync<UserHasNotPermissionException>();
        await fixture.Service.Invoking(x => x.Delete(template.Id, new UserInfo())).Should().ThrowAsync<UserHasNotPermissionException>();
        A.CallTo(() => fixture.Templates.Update(A<string>._, A<string>._, A<string>._, A<string>._, A<UserInfo>._)).MustNotHaveHappened();
        A.CallTo(() => fixture.Templates.Delete(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_RenderPluginTemplateWithSpaceAndUserContext_WhenCreatingPage()
    {
        var fixture = PluginTemplateService(TemplatePlugin());
        var user = new UserInfo { Name = "Alice", Email = "alice@test.com" };
        A.CallTo(() => fixture.Spaces.GetByKey("SPACE")).Returns(new Space { Key = "SPACE", Name = "Knowledge" });
        A.CallTo(() => fixture.Renderer.Render(A<string>._, A<IDictionary<string, object>>._)).Returns("Rendered content");
        var template = (await fixture.Service.GetAll(null, null, user)).Single();

        var result = await fixture.Service.Render(template.Id, "SPACE", user);

        result.Content.Should().Be("Rendered content");
        A.CallTo(() => fixture.Permissions.EnsureViewPermission("SPACE", user)).MustHaveHappenedOnceExactly();
        A.CallTo(() => fixture.Renderer.Render(template.Content, A<IDictionary<string, object>>.That.Matches(x =>
            (string)x["SpaceName"] == "Knowledge" && (string)x["UserName"] == "Alice" && x.ContainsKey("CurrentDate"))))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_NotRenderPluginTemplate_WhenUserCannotViewTargetSpace()
    {
        var fixture = PluginTemplateService(TemplatePlugin());
        var user = new UserInfo();
        A.CallTo(() => fixture.Spaces.GetByKey("SPACE")).Returns(new Space { Key = "SPACE" });
        A.CallTo(() => fixture.Permissions.EnsureViewPermission("SPACE", user)).ThrowsAsync(new UserHasNotPermissionException());
        var template = (await fixture.Service.GetAll(null, null, user)).Single();
        await fixture.Service.Invoking(x => x.Render(template.Id, "SPACE", user)).Should().ThrowAsync<UserHasNotPermissionException>();
        A.CallTo(() => fixture.Renderer.Render(A<string>._, A<IDictionary<string, object>>._)).MustNotHaveHappened();
    }

    [Theory]
    [InlineData("plugin.bad")]
    [InlineData("plugin.ZZ.00")]
    [InlineData("plugin.00.00")]
    public async Task ShouldThrow_NotFound_WhenPluginTemplateIdIsInvalidOrMissing(string id)
    {
        var fixture = PluginTemplateService();
        await fixture.Service.Invoking(x => x.GetById(id, new UserInfo())).Should().ThrowAsync<PageTemplateNotFoundException>();
    }

    [Fact]
    public async Task Should_ReturnEmptyList_WhenLegacyPluginHasNoTemplates()
    {
        var plugin = TemplatePlugin();
        plugin.PageTemplates = null;
        var fixture = PluginTemplateService(plugin);
        (await fixture.Service.GetAll(null, null, new UserInfo())).Should().BeEmpty();
    }
}
