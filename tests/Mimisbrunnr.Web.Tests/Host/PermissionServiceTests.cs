using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Host.Services;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Infrastructure.Contracts;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

namespace Mimisbrunnr.Web.Tests.Host;

public class PermissionServiceTests
{
    private static (PermissionService service, ISpaceManager spaces, IUserManager users, IUserGroupManager groups, IApplicationConfigurationManager config) CreateSut()
    {
        var spaces = A.Fake<ISpaceManager>();
        var users = A.Fake<IUserManager>();
        var groups = A.Fake<IUserGroupManager>();
        var config = A.Fake<IApplicationConfigurationManager>();
        return (new PermissionService(config, spaces, users, groups), spaces, users, groups, config);
    }

    private static Space PrivateSpaceWithPermission(string userPermissionEmail, bool canView = false, bool canEdit = false, bool canRemove = false, bool isAdmin = false)
    {
        return new Space
        {
            Key = "PRIV",
            Type = SpaceType.Private,
            Permissions = [new Permission { User = new UserInfo { Email = userPermissionEmail }, CanView = canView, CanEdit = canEdit, CanRemove = canRemove, IsAdmin = isAdmin }]
        };
    }

    private static void SetupUserEmail(IUserManager userManager, string email, UserRole role = UserRole.Employee)
    {
        A.CallTo(() => userManager.GetByEmail(email)).Returns(new Users.User { Id = "u1", Email = email, Role = role });
    }

    #region EnsureViewPermission

    [Fact]
    public async Task Should_AllowView_WhenSpaceIsPublic()
    {
        var (service, spaces, _, _, _) = CreateSut();
        A.CallTo(() => spaces.GetByKey("PUB")).Returns(new Space { Key = "PUB", Type = SpaceType.Public });

        await service.EnsureViewPermission("PUB", null);
    }

    [Fact]
    public async Task ShouldThrow_SpaceNotFoundException_WhenSpaceDoesNotExist()
    {
        var (service, spaces, _, _, _) = CreateSut();
        A.CallTo(() => spaces.GetByKey("NOPE")).Returns((Space)null);

        await service.Invoking(s => s.EnsureViewPermission("NOPE", new UserInfo { Email = "a@b.c" }))
            .Should().ThrowAsync<SpaceNotFoundException>();
    }

    [Fact]
    public async Task ShouldThrow_UserHasNotPermissionException_WhenAnonymousOnPrivateSpace()
    {
        var (service, spaces, _, _, _) = CreateSut();
        A.CallTo(() => spaces.GetByKey("PRIV")).Returns(PrivateSpaceWithPermission("x@y.z"));

        await service.Invoking(s => s.EnsureViewPermission("PRIV", null))
            .Should().ThrowAsync<UserHasNotPermissionException>();
    }

    [Fact]
    public async Task Should_AllowView_WhenUserIsAdmin()
    {
        var (service, spaces, users, _, _) = CreateSut();
        A.CallTo(() => spaces.GetByKey("PRIV")).Returns(PrivateSpaceWithPermission("other@x.y"));
        SetupUserEmail(users, "admin@test.com", UserRole.Admin);

        await service.EnsureViewPermission("PRIV", new UserInfo { Email = "admin@test.com" });
    }

    [Fact]
    public async Task ShouldThrow_UserHasNotPermissionException_WhenUserHasNoViewPermission()
    {
        var (service, spaces, users, groups, _) = CreateSut();
        A.CallTo(() => spaces.GetByKey("PRIV")).Returns(PrivateSpaceWithPermission("other@x.y"));
        SetupUserEmail(users, "user@test.com");
        A.CallTo(() => groups.GetUserGroups(A<Users.User>._)).Returns([]);

        await service.Invoking(s => s.EnsureViewPermission("PRIV", new UserInfo { Email = "user@test.com" }))
            .Should().ThrowAsync<UserHasNotPermissionException>();
    }

    [Fact]
    public async Task Should_AllowView_WhenUserHasDirectPermission()
    {
        var (service, spaces, users, groups, _) = CreateSut();
        A.CallTo(() => spaces.GetByKey("PRIV")).Returns(PrivateSpaceWithPermission("user@test.com", canView: true));
        SetupUserEmail(users, "user@test.com");
        A.CallTo(() => groups.GetUserGroups(A<Users.User>._)).Returns([]);

        await service.EnsureViewPermission("PRIV", new UserInfo { Email = "user@test.com" });
    }

    [Fact]
    public async Task Should_AllowView_WhenUserIsInGroupWithPermission()
    {
        var space = new Space
        {
            Key = "GRPS",
            Type = SpaceType.Private,
            Permissions = [new Permission { Group = new GroupInfo { Name = "devs" }, CanView = true }]
        };
        var (service, spaces, users, groups, _) = CreateSut();
        A.CallTo(() => spaces.GetByKey("GRPS")).Returns(space);
        SetupUserEmail(users, "user@test.com");
        A.CallTo(() => groups.GetUserGroups(A<Users.User>._)).Returns([new Users.Group { Name = "devs" }]);

        await service.EnsureViewPermission("GRPS", new UserInfo { Email = "user@test.com" });
    }

    [Fact]
    public async Task ShouldThrow_UserHasNotPermissionException_WhenUserHasAdminPermissionButNotView()
    {
        var (service, spaces, users, groups, _) = CreateSut();
        A.CallTo(() => spaces.GetByKey("PRIV")).Returns(PrivateSpaceWithPermission("user@test.com", isAdmin: true));
        SetupUserEmail(users, "user@test.com");
        A.CallTo(() => groups.GetUserGroups(A<Users.User>._)).Returns([]);

        await service.EnsureViewPermission("PRIV", new UserInfo { Email = "user@test.com" });
    }

    #endregion

    #region EnsureEditPermission

    [Fact]
    public async Task ShouldThrow_SpaceNotFoundException_WhenSpaceDoesNotExist_Editing()
    {
        var (service, spaces, _, _, _) = CreateSut();
        A.CallTo(() => spaces.GetByKey("NOPE")).Returns((Space)null);

        await service.Invoking(s => s.EnsureEditPermission("NOPE", new UserInfo { Email = "a@b.c" }))
            .Should().ThrowAsync<SpaceNotFoundException>();
    }

    [Fact]
    public async Task ShouldThrow_UserHasNotPermissionException_WhenAnonymousTriesToEdit()
    {
        var (service, spaces, _, _, _) = CreateSut();
        A.CallTo(() => spaces.GetByKey("PRIV")).Returns(PrivateSpaceWithPermission("x@y.z"));

        await service.Invoking(s => s.EnsureEditPermission("PRIV", null))
            .Should().ThrowAsync<UserHasNotPermissionException>();
    }

    [Fact]
    public async Task Should_AllowEdit_WhenUserIsAdmin()
    {
        var (service, spaces, users, _, _) = CreateSut();
        A.CallTo(() => spaces.GetByKey("PRIV")).Returns(PrivateSpaceWithPermission("other@x.y"));
        SetupUserEmail(users, "admin@test.com", UserRole.Admin);

        await service.EnsureEditPermission("PRIV", new UserInfo { Email = "admin@test.com" });
    }

    [Fact]
    public async Task ShouldThrow_UserHasNotPermissionException_WhenUserHasNoEditPermission()
    {
        var (service, spaces, users, groups, _) = CreateSut();
        A.CallTo(() => spaces.GetByKey("PRIV")).Returns(PrivateSpaceWithPermission("user@test.com", canView: true));
        SetupUserEmail(users, "user@test.com");
        A.CallTo(() => groups.GetUserGroups(A<Users.User>._)).Returns([]);

        await service.Invoking(s => s.EnsureEditPermission("PRIV", new UserInfo { Email = "user@test.com" }))
            .Should().ThrowAsync<UserHasNotPermissionException>();
    }

    [Fact]
    public async Task Should_AllowEdit_WhenUserHasDirectEditPermission()
    {
        var (service, spaces, users, groups, _) = CreateSut();
        A.CallTo(() => spaces.GetByKey("PRIV")).Returns(PrivateSpaceWithPermission("user@test.com", canEdit: true));
        SetupUserEmail(users, "user@test.com");
        A.CallTo(() => groups.GetUserGroups(A<Users.User>._)).Returns([]);

        await service.EnsureEditPermission("PRIV", new UserInfo { Email = "user@test.com" });
    }

    [Fact]
    public async Task Should_AllowEdit_WhenUserIsInGroupWithEditPermission()
    {
        var space = new Space
        {
            Key = "GRPS",
            Type = SpaceType.Private,
            Permissions = [new Permission { Group = new GroupInfo { Name = "editors" }, CanEdit = true }]
        };
        var (service, spaces, users, groups, _) = CreateSut();
        A.CallTo(() => spaces.GetByKey("GRPS")).Returns(space);
        SetupUserEmail(users, "user@test.com");
        A.CallTo(() => groups.GetUserGroups(A<Users.User>._)).Returns([new Users.Group { Name = "editors" }]);

        await service.EnsureEditPermission("GRPS", new UserInfo { Email = "user@test.com" });
    }

    #endregion

    #region EnsureRemovePermission

    [Fact]
    public async Task ShouldThrow_SpaceNotFoundException_WhenSpaceDoesNotExist_Removing()
    {
        var (service, spaces, _, _, _) = CreateSut();
        A.CallTo(() => spaces.GetByKey("NOPE")).Returns((Space)null);

        await service.Invoking(s => s.EnsureRemovePermission("NOPE", new UserInfo { Email = "a@b.c" }))
            .Should().ThrowAsync<SpaceNotFoundException>();
    }

    [Fact]
    public async Task ShouldThrow_UserHasNotPermissionException_WhenAnonymousTriesToRemove()
    {
        var (service, spaces, _, _, _) = CreateSut();
        A.CallTo(() => spaces.GetByKey("PRIV")).Returns(PrivateSpaceWithPermission("x@y.z"));

        await service.Invoking(s => s.EnsureRemovePermission("PRIV", null))
            .Should().ThrowAsync<UserHasNotPermissionException>();
    }

    [Fact]
    public async Task Should_AllowRemove_WhenUserIsAdmin()
    {
        var (service, spaces, users, _, _) = CreateSut();
        A.CallTo(() => spaces.GetByKey("PRIV")).Returns(PrivateSpaceWithPermission("other@x.y"));
        SetupUserEmail(users, "admin@test.com", UserRole.Admin);

        await service.EnsureRemovePermission("PRIV", new UserInfo { Email = "admin@test.com" });
    }

    [Fact]
    public async Task ShouldThrow_UserHasNotPermissionException_WhenUserHasNoRemovePermission()
    {
        var (service, spaces, users, groups, _) = CreateSut();
        A.CallTo(() => spaces.GetByKey("PRIV")).Returns(PrivateSpaceWithPermission("user@test.com", canEdit: true));
        SetupUserEmail(users, "user@test.com");
        A.CallTo(() => groups.GetUserGroups(A<Users.User>._)).Returns([]);

        await service.Invoking(s => s.EnsureRemovePermission("PRIV", new UserInfo { Email = "user@test.com" }))
            .Should().ThrowAsync<UserHasNotPermissionException>();
    }

    [Fact]
    public async Task Should_AllowRemove_WhenUserHasDirectRemovePermission()
    {
        var (service, spaces, users, groups, _) = CreateSut();
        A.CallTo(() => spaces.GetByKey("PRIV")).Returns(PrivateSpaceWithPermission("user@test.com", canRemove: true));
        SetupUserEmail(users, "user@test.com");
        A.CallTo(() => groups.GetUserGroups(A<Users.User>._)).Returns([]);

        await service.EnsureRemovePermission("PRIV", new UserInfo { Email = "user@test.com" });
    }

    [Fact]
    public async Task Should_AllowRemove_WhenUserIsInGroupWithRemovePermission()
    {
        var space = new Space
        {
            Key = "GRPS",
            Type = SpaceType.Private,
            Permissions = [new Permission { Group = new GroupInfo { Name = "admins" }, CanRemove = true }]
        };
        var (service, spaces, users, groups, _) = CreateSut();
        A.CallTo(() => spaces.GetByKey("GRPS")).Returns(space);
        SetupUserEmail(users, "user@test.com");
        A.CallTo(() => groups.GetUserGroups(A<Users.User>._)).Returns([new Users.Group { Name = "admins" }]);

        await service.EnsureRemovePermission("GRPS", new UserInfo { Email = "user@test.com" });
    }

    #endregion

    #region EnsureAdminPermission

    [Fact]
    public async Task ShouldThrow_SpaceNotFoundException_WhenSpaceDoesNotExist_AdminCheck()
    {
        var (service, spaces, _, _, _) = CreateSut();
        A.CallTo(() => spaces.GetByKey("NOPE")).Returns((Space)null);

        await service.Invoking(s => s.EnsureAdminPermission("NOPE", new UserInfo { Email = "a@b.c" }))
            .Should().ThrowAsync<SpaceNotFoundException>();
    }

    [Fact]
    public async Task ShouldThrow_UserHasNotPermissionException_WhenAnonymousTriesAdminAction()
    {
        var (service, spaces, _, _, _) = CreateSut();
        A.CallTo(() => spaces.GetByKey("PRIV")).Returns(PrivateSpaceWithPermission("x@y.z"));

        await service.Invoking(s => s.EnsureAdminPermission("PRIV", null))
            .Should().ThrowAsync<UserHasNotPermissionException>();
    }

    [Fact]
    public async Task Should_AllowAdmin_WhenUserIsSystemAdmin()
    {
        var (service, spaces, users, _, _) = CreateSut();
        A.CallTo(() => spaces.GetByKey("PRIV")).Returns(PrivateSpaceWithPermission("other@x.y"));
        SetupUserEmail(users, "admin@test.com", UserRole.Admin);

        await service.EnsureAdminPermission("PRIV", new UserInfo { Email = "admin@test.com" });
    }

    [Fact]
    public async Task ShouldThrow_UserHasNotPermissionException_WhenUserIsNotSpaceAdmin()
    {
        var (service, spaces, users, groups, _) = CreateSut();
        A.CallTo(() => spaces.GetByKey("PRIV")).Returns(PrivateSpaceWithPermission("user@test.com", canEdit: true));
        SetupUserEmail(users, "user@test.com");
        A.CallTo(() => groups.GetUserGroups(A<Users.User>._)).Returns([]);

        await service.Invoking(s => s.EnsureAdminPermission("PRIV", new UserInfo { Email = "user@test.com" }))
            .Should().ThrowAsync<UserHasNotPermissionException>();
    }

    [Fact]
    public async Task Should_AllowAdmin_WhenUserHasDirectAdminPermission()
    {
        var (service, spaces, users, groups, _) = CreateSut();
        A.CallTo(() => spaces.GetByKey("PRIV")).Returns(PrivateSpaceWithPermission("user@test.com", isAdmin: true));
        SetupUserEmail(users, "user@test.com");
        A.CallTo(() => groups.GetUserGroups(A<Users.User>._)).Returns([]);

        await service.EnsureAdminPermission("PRIV", new UserInfo { Email = "user@test.com" });
    }

    [Fact]
    public async Task ShouldThrow_UserHasNotPermissionException_WhenGroupHasEditButNotAdmin()
    {
        var space = new Space
        {
            Key = "GRPS",
            Type = SpaceType.Private,
            Permissions = [new Permission { Group = new GroupInfo { Name = "editors" }, CanEdit = true }]
        };
        var (service, spaces, users, groups, _) = CreateSut();
        A.CallTo(() => spaces.GetByKey("GRPS")).Returns(space);
        SetupUserEmail(users, "user@test.com");
        A.CallTo(() => groups.GetUserGroups(A<Users.User>._)).Returns([new Users.Group { Name = "editors" }]);

        await service.Invoking(s => s.EnsureAdminPermission("GRPS", new UserInfo { Email = "user@test.com" }))
            .Should().ThrowAsync<UserHasNotPermissionException>();
    }

    #endregion

    #region EnsureAnonymousAllowed

    [Fact]
    public async Task Should_AllowAnonymous_WhenUserIsNotNull()
    {
        var (service, _, _, _, _) = CreateSut();

        await service.EnsureAnonymousAllowed(new UserInfo { Email = "a@b.c" });
    }

    [Fact]
    public async Task Should_AllowAnonymous_WhenConfigurationAllowsIt()
    {
        var (service, _, _, _, config) = CreateSut();
        A.CallTo(() => config.Get()).Returns(new ApplicationConfiguration { AllowAnonymous = true });

        await service.EnsureAnonymousAllowed(null);
    }

    [Fact]
    public async Task ShouldThrow_AnonymousNotAllowedException_WhenConfigurationDisallowsIt()
    {
        var (service, _, _, _, config) = CreateSut();
        A.CallTo(() => config.Get()).Returns(new ApplicationConfiguration { AllowAnonymous = false });

        await service.Invoking(s => s.EnsureAnonymousAllowed(null))
            .Should().ThrowAsync<AnonymousNotAllowedException>();
    }

    #endregion
}
