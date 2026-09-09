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
        var service = new PageTemplateService(manager, A.Fake<ISpaceManager>(), A.Fake<IPermissionService>(), A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>());

        await service.Invoking(x => x.GetById("template", new UserInfo { Email = "other@example.test" }))
            .Should().ThrowAsync<UserHasNotPermissionException>();
    }

    [Fact]
    public async Task Should_PersistCurrentUserAsOwner_WhenCreatingUserTemplate()
    {
        var manager = A.Fake<IPageTemplateManager>();
        A.CallTo(() => manager.Create(A<PageTemplate>._)).ReturnsLazily(call => Task.FromResult(call.GetArgument<PageTemplate>(0)));
        var service = new PageTemplateService(manager, A.Fake<ISpaceManager>(), A.Fake<IPermissionService>(), A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>());
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
        var service = new PageTemplateService(manager, spaces, permissions, A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>());

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
        var service = new PageTemplateService(manager, A.Fake<ISpaceManager>(), A.Fake<IPermissionService>(), A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>());

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
        var service = new PageTemplateService(manager, A.Fake<ISpaceManager>(), permissions, A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>());

        var result = await service.GetAll(TemplateType.Space, "KEY", user);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_AllowReadingSystemTemplate_WhenGettingById()
    {
        var manager = A.Fake<IPageTemplateManager>();
        A.CallTo(() => manager.GetById("system")).Returns(Task.FromResult(new PageTemplate { Id = "system", Type = TemplateType.System }));
        var service = new PageTemplateService(manager, A.Fake<ISpaceManager>(), A.Fake<IPermissionService>(), A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>());

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
        var service = new PageTemplateService(manager, spaces, permissions, A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>());

        var result = await service.GetById("template", user);

        result.Id.Should().Be("template");
        A.CallTo(() => permissions.EnsureEditPermission("KEY", user)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ThrowInvalidOperationException_WhenCreatingSpaceTemplateWithoutSpaceKey()
    {
        var manager = A.Fake<IPageTemplateManager>();
        var service = new PageTemplateService(manager, A.Fake<ISpaceManager>(), A.Fake<IPermissionService>(), A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>());

        await service.Invoking(x => x.Create(new PageTemplateCreateModel { Name = "T", Type = TemplateType.Space }, new UserInfo { Email = "user@example.test" }))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_ThrowInvalidOperationException_WhenCreatingUnknownTemplateType()
    {
        var manager = A.Fake<IPageTemplateManager>();
        var service = new PageTemplateService(manager, A.Fake<ISpaceManager>(), A.Fake<IPermissionService>(), A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>());

        await service.Invoking(x => x.Create(new PageTemplateCreateModel { Name = "T", Type = "Fake" }, new UserInfo { Email = "user@example.test" }))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_ThrowUserHasNotPermissionException_WhenNonAdminCreatesSystemTemplate()
    {
        var users = A.Fake<IUserManager>();
        var user = new UserInfo { Email = "user@example.test" };
        A.CallTo(() => users.GetByEmail(user.Email)).Returns(Task.FromResult(new Mimisbrunnr.Users.User { Role = UserRole.Employee }));
        var service = new PageTemplateService(A.Fake<IPageTemplateManager>(), A.Fake<ISpaceManager>(), A.Fake<IPermissionService>(), users, A.Fake<ITemplateRenderer>());

        await service.Invoking(x => x.Create(new PageTemplateCreateModel { Name = "T", Type = TemplateType.System }, user))
            .Should().ThrowAsync<UserHasNotPermissionException>();
    }

    [Fact]
    public async Task Should_ThrowSpaceNotFoundException_WhenCreatingSpaceTemplateInMissingSpace()
    {
        var permissions = A.Fake<IPermissionService>();
        var spaces = A.Fake<ISpaceManager>();
        A.CallTo(() => spaces.GetByKey("KEY")).Returns(Task.FromResult<Space>(null));
        var service = new PageTemplateService(A.Fake<IPageTemplateManager>(), spaces, permissions, A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>());

        await service.Invoking(x => x.Create(new PageTemplateCreateModel { Name = "T", Type = TemplateType.Space, SpaceKey = "KEY" }, new UserInfo { Email = "user@example.test" }))
            .Should().ThrowAsync<SpaceNotFoundException>();
    }

    [Fact]
    public async Task Should_ThrowUserHasNotPermissionException_WhenUpdatingAnotherUsersTemplate()
    {
        var manager = A.Fake<IPageTemplateManager>();
        A.CallTo(() => manager.GetById("template")).Returns(Task.FromResult(new PageTemplate { Id = "template", Type = TemplateType.User, OwnerEmail = "owner@example.test" }));
        var service = new PageTemplateService(manager, A.Fake<ISpaceManager>(), A.Fake<IPermissionService>(), A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>());

        await service.Invoking(x => x.Update("template", new PageTemplateUpdateModel(), new UserInfo { Email = "other@example.test" }))
            .Should().ThrowAsync<UserHasNotPermissionException>();
    }

    [Fact]
    public async Task Should_UpdateTemplate_WhenOwnerUpdatesOwnTemplate()
    {
        var manager = A.Fake<IPageTemplateManager>();
        var user = new UserInfo { Email = "owner@example.test" };
        A.CallTo(() => manager.GetById("template")).Returns(Task.FromResult(new PageTemplate { Id = "template", Type = TemplateType.User, OwnerEmail = user.Email }));
        var service = new PageTemplateService(manager, A.Fake<ISpaceManager>(), A.Fake<IPermissionService>(), A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>());

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
        var service = new PageTemplateService(manager, spaces, A.Fake<IPermissionService>(), A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>());

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
        var service = new PageTemplateService(manager, spaces, A.Fake<IPermissionService>(), A.Fake<IUserManager>(), renderer);

        var result = await service.Render("template", "KEY", user);

        result.Content.Should().Be("Hello User");
        A.CallTo(() => renderer.Render("Hello {{UserName}}", A<IDictionary<string, object>>.That.Matches(d =>
            d.ContainsKey("UserName") && d["UserName"].Equals("User") &&
            d.ContainsKey("UserEmail") && d["UserEmail"].Equals(user.Email) &&
            d.ContainsKey("SpaceKey") && d["SpaceKey"].Equals("KEY") &&
            d.ContainsKey("SpaceName") && d["SpaceName"].Equals("SpaceName") &&
            d.ContainsKey("CurrentDate")))).MustHaveHappenedOnceExactly();
    }
}
