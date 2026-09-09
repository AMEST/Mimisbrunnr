using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.PageTemplates.Services;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Infrastructure.Contracts;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Web.Wiki;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

namespace Mimisbrunnr.Web.Tests.Wiki;

public class SpaceServiceTests
{
    private static SpaceService BuildService(IPermissionService permissions,
        ISpaceManager spaces,
        IUserManager users,
        IUserGroupManager groups,
        IApplicationConfigurationManager configuration,
        IPageTemplateManager pageTemplates = null)
    {
        return new SpaceService(
            permissions ?? A.Fake<IPermissionService>(),
            spaces ?? A.Fake<ISpaceManager>(),
            users ?? A.Fake<IUserManager>(),
            groups ?? A.Fake<IUserGroupManager>(),
            configuration ?? A.Fake<IApplicationConfigurationManager>(),
            NullLogger<SpaceService>.Instance,
            pageTemplates);
    }

    [Fact]
    public async Task Should_UseCurrentUserIdentity_WhenCreatingPersonalSpace()
    {
        var spaces = A.Fake<ISpaceManager>();
        var user = new UserInfo { Email = "user@example.test", Name = "User" };
        A.CallTo(() => spaces.FindPersonalSpace(user)).Returns(Task.FromResult<Space>(null));
        A.CallTo(() => spaces.GetByKey(user.Email)).Returns(Task.FromResult<Space>(null));
        A.CallTo(() => spaces.Create(user.Email.ToUpper(), user.Name, A<string>._, SpaceType.Personal, user))
            .Returns(Task.FromResult(new Space { Key = user.Email.ToUpper(), Name = user.Name, Type = SpaceType.Personal }));
        var service = BuildService(A.Fake<IPermissionService>(), spaces, A.Fake<IUserManager>(), A.Fake<IUserGroupManager>(), A.Fake<IApplicationConfigurationManager>(), A.Fake<IPageTemplateManager>());

        var result = await service.Create(new SpaceCreateModel { Key = "ignored", Name = "ignored", Type = SpaceTypeModel.Personal }, user);

        result.Key.Should().Be(user.Email.ToUpper());
        A.CallTo(() => spaces.Create(user.Email.ToUpper(), user.Name, A<string>._, SpaceType.Personal, user)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ShouldThrow_AnonymousNotAllowedException_WhenAnonymousUserCreatesSpace()
    {
        var service = BuildService(A.Fake<IPermissionService>(), A.Fake<ISpaceManager>(), A.Fake<IUserManager>(), A.Fake<IUserGroupManager>(), A.Fake<IApplicationConfigurationManager>(), A.Fake<IPageTemplateManager>());

        await service.Invoking(x => x.Create(new SpaceCreateModel(), null)).Should().ThrowAsync<AnonymousNotAllowedException>();
    }

    [Fact]
    public async Task Should_ReturnPublicSpaces_WhenAnonymousRequestsAllSpaces()
    {
        var permissions = A.Fake<IPermissionService>();
        var spaces = A.Fake<ISpaceManager>();
        A.CallTo(() => spaces.GetPublicSpaces(null, null)).Returns(Task.FromResult(new[] { new Space { Key = "PUBLIC", Type = SpaceType.Public } }));
        var service = BuildService(permissions, spaces, null, null, null, null);

        var result = await service.GetAll(null);

        result.Should().ContainSingle().Which.Key.Should().Be("PUBLIC");
        A.CallTo(() => permissions.EnsureAnonymousAllowed(null)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ReturnAllSpaces_WhenAdminRequestsAllSpaces()
    {
        var spaces = A.Fake<ISpaceManager>();
        var users = A.Fake<IUserManager>();
        var user = new UserInfo { Email = "admin@example.test" };
        A.CallTo(() => users.GetByEmail(user.Email)).Returns(Task.FromResult(new Mimisbrunnr.Users.User { Role = UserRole.Admin }));
        A.CallTo(() => spaces.GetAll(null, null)).Returns(Task.FromResult(new[] { new Space { Key = "A" }, new Space { Key = "B" } }));
        var service = BuildService(null, spaces, users, null, null, null);

        var result = await service.GetAll(user);

        result.Should().HaveCount(2);
        A.CallTo(() => spaces.GetAll(null, null)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ReturnSpacesWithPermissions_WhenEmployeeRequestsAllSpaces()
    {
        var spaces = A.Fake<ISpaceManager>();
        var users = A.Fake<IUserManager>();
        var groups = A.Fake<IUserGroupManager>();
        var user = new UserInfo { Email = "user@example.test" };
        var dbUser = new Mimisbrunnr.Users.User { Email = user.Email, Role = UserRole.Employee };
        A.CallTo(() => users.GetByEmail(user.Email)).Returns(Task.FromResult(dbUser));
        A.CallTo(() => groups.GetUserGroups(dbUser)).Returns(Task.FromResult(new[] { new Mimisbrunnr.Users.Group { Name = "developers" } }));
        A.CallTo(() => spaces.GetAllWithPermissions(user, new[] { "developers" }, null, null)).Returns(Task.FromResult(new[] { new Space { Key = "VISIBLE" } }));
        var service = BuildService(null, spaces, users, groups, null, null);

        var result = await service.GetAll(user);

        result.Should().ContainSingle().Which.Key.Should().Be("VISIBLE");
    }

    [Fact]
    public async Task Should_RequireViewPermission_WhenReadingSpaceByKey()
    {
        var permissions = A.Fake<IPermissionService>();
        var spaces = A.Fake<ISpaceManager>();
        var user = new UserInfo { Email = "user@example.test" };
        A.CallTo(() => spaces.GetByKey("DOCS")).Returns(Task.FromResult(new Space { Key = "DOCS", Type = SpaceType.Public }));
        var service = BuildService(permissions, spaces, null, null, null, null);

        var result = await service.GetByKey("DOCS", user);

        result.Key.Should().Be("DOCS");
        A.CallTo(() => permissions.EnsureViewPermission("DOCS", user)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ShouldThrow_SpaceNotFoundException_WhenReadingMissingSpace()
    {
        var spaces = A.Fake<ISpaceManager>();
        A.CallTo(() => spaces.GetByKey("MISSING")).Returns(Task.FromResult<Space>(null));
        var service = BuildService(null, spaces, null, null, null, null);

        await service.Invoking(x => x.GetByKey("MISSING", new UserInfo { Email = "user@example.test" }))
            .Should().ThrowAsync<SpaceNotFoundException>();
    }

    [Fact]
    public async Task Should_AllowAnonymousView_WhenGettingPermissionAnonymously()
    {
        var permissions = A.Fake<IPermissionService>();
        var users = A.Fake<IUserManager>();
        var service = BuildService(permissions, null, users, null, null, null);

        var result = await service.GetPermission("DOCS", null);

        result.CanView.Should().BeTrue();
        A.CallTo(() => users.GetByEmail(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_GrantAllPermissions_WhenGettingPermissionForAdmin()
    {
        var users = A.Fake<IUserManager>();
        var spaces = A.Fake<ISpaceManager>();
        var user = new UserInfo { Email = "admin@example.test" };
        A.CallTo(() => users.GetByEmail(user.Email)).Returns(Task.FromResult(new Mimisbrunnr.Users.User { Role = UserRole.Admin }));
        var service = BuildService(null, spaces, users, null, null, null);

        var result = await service.GetPermission("DOCS", user);

        result.CanView.Should().BeTrue();
        result.CanEdit.Should().BeTrue();
        result.CanRemove.Should().BeTrue();
        result.IsAdmin.Should().BeTrue();
        A.CallTo(() => spaces.GetByKey(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_GrantViewOnlyForPublicSpace_WhenNoPermissionMatches()
    {
        var users = A.Fake<IUserManager>();
        var groups = A.Fake<IUserGroupManager>();
        var spaces = A.Fake<ISpaceManager>();
        var user = new UserInfo { Email = "user@example.test" };
        var dbUser = new Mimisbrunnr.Users.User { Email = user.Email, Role = UserRole.Employee };
        A.CallTo(() => users.GetByEmail(user.Email)).Returns(Task.FromResult(dbUser));
        A.CallTo(() => groups.GetUserGroups(dbUser)).Returns(Task.FromResult(Array.Empty<Mimisbrunnr.Users.Group>()));
        A.CallTo(() => spaces.GetByKey("PUBLIC")).Returns(Task.FromResult(new Space
        {
            Key = "PUBLIC",
            Type = SpaceType.Public,
            Permissions = [new Permission { User = new UserInfo { Email = "other@example.test" }, CanEdit = true }]
        }));
        var service = BuildService(null, spaces, users, groups, null, null);

        var result = await service.GetPermission("PUBLIC", user);

        result.CanView.Should().BeTrue();
        result.CanEdit.Should().BeFalse();
    }

    [Fact]
    public async Task Should_ReturnUserPermission_WhenPermissionMatchesUser()
    {
        var users = A.Fake<IUserManager>();
        var groups = A.Fake<IUserGroupManager>();
        var spaces = A.Fake<ISpaceManager>();
        var user = new UserInfo { Email = "user@example.test" };
        var dbUser = new Mimisbrunnr.Users.User { Email = user.Email, Role = UserRole.Employee };
        A.CallTo(() => users.GetByEmail(user.Email)).Returns(Task.FromResult(dbUser));
        A.CallTo(() => groups.GetUserGroups(dbUser)).Returns(Task.FromResult(Array.Empty<Mimisbrunnr.Users.Group>()));
        A.CallTo(() => spaces.GetByKey("DOCS")).Returns(Task.FromResult(new Space
        {
            Key = "DOCS",
            Permissions = [new Permission { User = user, CanView = true, CanEdit = true }]
        }));
        var service = BuildService(null, spaces, users, groups, null, null);

        var result = await service.GetPermission("DOCS", user);

        result.CanView.Should().BeTrue();
        result.CanEdit.Should().BeTrue();
    }

    [Fact]
    public async Task Should_ReturnGroupPermission_WhenPermissionMatchesUserGroup()
    {
        var users = A.Fake<IUserManager>();
        var groups = A.Fake<IUserGroupManager>();
        var spaces = A.Fake<ISpaceManager>();
        var user = new UserInfo { Email = "user@example.test" };
        var dbUser = new Mimisbrunnr.Users.User { Email = user.Email, Role = UserRole.Employee };
        A.CallTo(() => users.GetByEmail(user.Email)).Returns(Task.FromResult(dbUser));
        A.CallTo(() => groups.GetUserGroups(dbUser)).Returns(Task.FromResult(new[] { new Mimisbrunnr.Users.Group { Name = "developers" } }));
        A.CallTo(() => spaces.GetByKey("DOCS")).Returns(Task.FromResult(new Space
        {
            Key = "DOCS",
            Permissions = [new Permission { Group = new GroupInfo { Name = "developers" }, CanView = true, CanEdit = true }]
        }));
        var service = BuildService(null, spaces, users, groups, null, null);

        var result = await service.GetPermission("DOCS", user);

        result.CanView.Should().BeTrue();
        result.CanEdit.Should().BeTrue();
    }

    [Fact]
    public async Task Should_RequireAdminPermission_WhenGettingSpacePermissions()
    {
        var permissions = A.Fake<IPermissionService>();
        var spaces = A.Fake<ISpaceManager>();
        var user = new UserInfo { Email = "admin@example.test" };
        A.CallTo(() => spaces.GetByKey("DOCS")).Returns(Task.FromResult(new Space
        {
            Key = "DOCS",
            Permissions = [new Permission { User = new UserInfo { Email = "user@example.test" }, CanView = true }]
        }));
        var service = BuildService(permissions, spaces, null, null, null, null);

        var result = await service.GetSpacePermissions("DOCS", user);

        result.Should().ContainSingle();
        A.CallTo(() => permissions.EnsureAdminPermission("DOCS", user)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ShouldThrow_AnonymousNotAllowedException_WhenAnonymousGetsSpacePermissions()
    {
        var service = BuildService(null, null, null, null, null, null);

        await service.Invoking(x => x.GetSpacePermissions("DOCS", null)).Should().ThrowAsync<AnonymousNotAllowedException>();
    }

    [Fact]
    public async Task Should_AddPermission_WhenAddingSpacePermission()
    {
        var permissions = A.Fake<IPermissionService>();
        var spaces = A.Fake<ISpaceManager>();
        var user = new UserInfo { Email = "admin@example.test" };
        var space = new Space { Key = "DOCS" };
        A.CallTo(() => spaces.GetByKey("DOCS")).Returns(Task.FromResult(space));
        var service = BuildService(permissions, spaces, null, null, null, null);
        var model = new SpacePermissionModel { User = new Mimisbrunnr.Integration.User.UserModel { Email = "user@example.test" }, CanView = true };

        var result = await service.AddPermission("DOCS", model, user);

        result.Should().BeSameAs(model);
        A.CallTo(() => spaces.AddPermission(space, A<Permission>.That.Matches(x => x.CanView))).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ShouldThrow_SpaceNotFoundException_WhenUpdatingPermissionOnMissingSpace()
    {
        var spaces = A.Fake<ISpaceManager>();
        A.CallTo(() => spaces.GetByKey("MISSING")).Returns(Task.FromResult<Space>(null));
        var service = BuildService(null, spaces, null, null, null, null);

        await service.Invoking(x => x.UpdatePermission("MISSING", new SpacePermissionModel(), new UserInfo { Email = "admin@example.test" }))
            .Should().ThrowAsync<SpaceNotFoundException>();
    }

    [Fact]
    public async Task Should_RemovePermission_WhenRemovingSpacePermission()
    {
        var permissions = A.Fake<IPermissionService>();
        var spaces = A.Fake<ISpaceManager>();
        var user = new UserInfo { Email = "admin@example.test" };
        var space = new Space { Key = "DOCS" };
        A.CallTo(() => spaces.GetByKey("DOCS")).Returns(Task.FromResult(space));
        var service = BuildService(permissions, spaces, null, null, null, null);
        var model = new SpacePermissionModel { User = new Mimisbrunnr.Integration.User.UserModel { Email = "user@example.test" }, CanView = true };

        await service.RemovePermission("DOCS", model, user);

        A.CallTo(() => spaces.RemovePermission(space, A<Permission>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ShouldThrow_InvalidOperationException_WhenCreatingSpaceWithExistingKey()
    {
        var spaces = A.Fake<ISpaceManager>();
        var user = new UserInfo { Email = "user@example.test" };
        A.CallTo(() => spaces.GetByKey("DOCS")).Returns(Task.FromResult(new Space { Key = "DOCS" }));
        var service = BuildService(null, spaces, null, null, null, null);

        await service.Invoking(x => x.Create(new SpaceCreateModel { Key = "DOCS", Type = SpaceTypeModel.Private }, user))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ShouldThrow_InvalidOperationException_WhenCreatingSecondPersonalSpace()
    {
        var spaces = A.Fake<ISpaceManager>();
        var user = new UserInfo { Email = "user@example.test", Name = "User" };
        A.CallTo(() => spaces.FindPersonalSpace(user)).Returns(Task.FromResult(new Space { Key = user.Email }));
        var service = BuildService(null, spaces, null, null, null, null);

        await service.Invoking(x => x.Create(new SpaceCreateModel { Key = "other", Type = SpaceTypeModel.Personal }, user))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_PersistAvatar_WhenCreatingPersonalSpace()
    {
        var spaces = A.Fake<ISpaceManager>();
        var user = new UserInfo { Email = "user@example.test", Name = "User", AvatarUrl = "/api/attachment/avatar" };
        A.CallTo(() => spaces.FindPersonalSpace(user)).Returns(Task.FromResult<Space>(null));
        A.CallTo(() => spaces.GetByKey(A<string>._)).Returns(Task.FromResult<Space>(null));
        A.CallTo(() => spaces.Create(A<string>._, A<string>._, A<string>._, SpaceType.Personal, user))
            .Returns(Task.FromResult(new Space { Key = user.Email, Type = SpaceType.Personal }));
        var service = BuildService(null, spaces, null, null, null, null);

        await service.Create(new SpaceCreateModel { Key = "ignored", Type = SpaceTypeModel.Personal }, user);

        A.CallTo(() => spaces.Update(A<Space>.That.Matches(x => x.AvatarUrl == user.AvatarUrl))).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_MakeSpacePrivate_WhenUpdatingWithPublicFlagOff()
    {
        var spaces = A.Fake<ISpaceManager>();
        var configuration = A.Fake<IApplicationConfigurationManager>();
        var user = new UserInfo { Email = "admin@example.test" };
        var space = new Space { Key = "DOCS", Type = SpaceType.Public };
        A.CallTo(() => spaces.GetByKey("DOCS")).Returns(Task.FromResult(space));
        A.CallTo(() => configuration.Get()).Returns(Task.FromResult(new ApplicationConfiguration()));

        await BuildService(null, spaces, null, null, configuration, null).Update("DOCS", new SpaceUpdateModel { Name = "Docs", Description = "D", Public = false }, user);

        space.Type.Should().Be(SpaceType.Private);
        A.CallTo(() => spaces.Update(space)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ShouldThrow_InvalidOperationException_WhenMakingHomepageSpacePrivate()
    {
        var spaces = A.Fake<ISpaceManager>();
        var configuration = A.Fake<IApplicationConfigurationManager>();
        var user = new UserInfo { Email = "admin@example.test" };
        var space = new Space { Key = "DOCS", Type = SpaceType.Public };
        A.CallTo(() => spaces.GetByKey("DOCS")).Returns(Task.FromResult(space));
        A.CallTo(() => configuration.Get()).Returns(Task.FromResult(new ApplicationConfiguration { CustomHomepageEnabled = true, CustomHomepageSpaceKey = "docs" }));
        var service = BuildService(null, spaces, null, null, configuration, null);

        await service.Invoking(x => x.Update("DOCS", new SpaceUpdateModel { Name = "Docs", Description = "D", Public = false }, user))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_IgnoreAvatarForPersonalSpace_WhenUpdating()
    {
        var spaces = A.Fake<ISpaceManager>();
        var configuration = A.Fake<IApplicationConfigurationManager>();
        var user = new UserInfo { Email = "user@example.test" };
        var space = new Space { Key = "DOCS", Type = SpaceType.Personal, AvatarUrl = "old" };
        A.CallTo(() => spaces.GetByKey("DOCS")).Returns(Task.FromResult(space));
        A.CallTo(() => configuration.Get()).Returns(Task.FromResult(new ApplicationConfiguration()));
        var service = BuildService(null, spaces, null, null, configuration, null);

        await service.Update("DOCS", new SpaceUpdateModel { Name = "Docs", Description = "D", AvatarUrl = "/api/attachment/new" }, user);

        space.AvatarUrl.Should().Be("old");
        A.CallTo(() => spaces.Update(space)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ArchiveSpace_WhenArchiving()
    {
        var permissions = A.Fake<IPermissionService>();
        var spaces = A.Fake<ISpaceManager>();
        var user = new UserInfo { Email = "admin@example.test" };
        var space = new Space { Key = "DOCS" };
        A.CallTo(() => spaces.GetByKey("DOCS")).Returns(Task.FromResult(space));
        var service = BuildService(permissions, spaces, null, null, null, null);

        await service.Archive("DOCS", user);

        A.CallTo(() => permissions.EnsureAdminPermission("DOCS", user)).MustHaveHappenedOnceExactly();
        A.CallTo(() => spaces.Archive(space)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ShouldThrow_InvalidOperationException_WhenRemovingNonArchivedSpace()
    {
        var spaces = A.Fake<ISpaceManager>();
        var configuration = A.Fake<IApplicationConfigurationManager>();
        var user = new UserInfo { Email = "admin@example.test" };
        A.CallTo(() => spaces.GetByKey("DOCS")).Returns(Task.FromResult(new Space { Key = "DOCS", Type = SpaceType.Private, Status = SpaceStatus.Actual }));
        A.CallTo(() => configuration.Get()).Returns(Task.FromResult(new ApplicationConfiguration()));
        var service = BuildService(null, spaces, null, null, configuration, null);

        await service.Invoking(x => x.Remove("DOCS", user)).Should().ThrowAsync<InvalidOperationException>();
        A.CallTo(() => spaces.Remove(A<Space>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ShouldThrow_InvalidOperationException_WhenRemovingHomepageSpace()
    {
        var spaces = A.Fake<ISpaceManager>();
        var configuration = A.Fake<IApplicationConfigurationManager>();
        var user = new UserInfo { Email = "admin@example.test" };
        A.CallTo(() => spaces.GetByKey("DOCS")).Returns(Task.FromResult(new Space { Key = "DOCS", Type = SpaceType.Public, Status = SpaceStatus.Actual }));
        A.CallTo(() => configuration.Get()).Returns(Task.FromResult(new ApplicationConfiguration { CustomHomepageEnabled = true, CustomHomepageSpaceKey = "DOCS" }));
        var service = BuildService(null, spaces, null, null, configuration, null);

        await service.Invoking(x => x.Remove("DOCS", user)).Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_DeleteTemplatesAndSpace_WhenRemovingArchivedSpace()
    {
        var spaces = A.Fake<ISpaceManager>();
        var configuration = A.Fake<IApplicationConfigurationManager>();
        var pageTemplates = A.Fake<IPageTemplateManager>();
        var user = new UserInfo { Email = "admin@example.test" };
        var space = new Space { Id = "s1", Key = "DOCS", Type = SpaceType.Private, Status = SpaceStatus.Archived };
        A.CallTo(() => spaces.GetByKey("DOCS")).Returns(Task.FromResult(space));
        A.CallTo(() => configuration.Get()).Returns(Task.FromResult(new ApplicationConfiguration()));
        var service = BuildService(null, spaces, null, null, configuration, pageTemplates);

        await service.Remove("DOCS", user);

        A.CallTo(() => pageTemplates.DeleteBySpaceId("s1")).MustHaveHappenedOnceExactly();
        A.CallTo(() => spaces.Remove(space)).MustHaveHappenedOnceExactly();
    }
}
