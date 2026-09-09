using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Mimisbrunnr.Integration.User;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.User;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Tests.User;

public class UserServiceTests
{
    [Fact]
    public async Task Should_UpdateProfile_WhenUserUpdatesOwnProfile()
    {
        var users = A.Fake<IUserManager>();
        var currentUser = new Mimisbrunnr.Users.User { Email = "user@example.test" };
        A.CallTo(() => users.GetByEmail(currentUser.Email)).Returns(Task.FromResult(currentUser));
        var service = new UserService(users, A.Fake<IUserGroupManager>(), NullLogger<UserService>.Instance);
        var update = new UserProfileUpdateModel { Department = "Engineering", Website = "https://example.test" };

        await service.UpdateProfileInfo(currentUser.Email, update, new UserInfo { Email = currentUser.Email });

        currentUser.Department.Should().Be("Engineering");
        A.CallTo(() => users.UpdateUserInfo(currentUser)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ShouldThrow_UserHasNotPermissionException_WhenEmployeeUpdatesAnotherProfile()
    {
        var users = A.Fake<IUserManager>();
        A.CallTo(() => users.GetByEmail("target@example.test")).Returns(Task.FromResult(new Mimisbrunnr.Users.User { Email = "target@example.test" }));
        A.CallTo(() => users.GetByEmail("other@example.test")).Returns(Task.FromResult(new Mimisbrunnr.Users.User { Email = "other@example.test", Role = UserRole.Employee }));
        var service = new UserService(users, A.Fake<IUserGroupManager>(), NullLogger<UserService>.Instance);

        await service.Invoking(x => x.UpdateProfileInfo("target@example.test", new UserProfileUpdateModel(), new UserInfo { Email = "other@example.test" }))
            .Should().ThrowAsync<UserHasNotPermissionException>();
    }

    [Fact]
    public async Task Should_AllowAdminToUpdateAnotherProfile()
    {
        var users = A.Fake<IUserManager>();
        var target = new Mimisbrunnr.Users.User { Email = "target@example.test" };
        var admin = new Mimisbrunnr.Users.User { Email = "admin@example.test", Role = UserRole.Admin };
        A.CallTo(() => users.GetByEmail("target@example.test")).Returns(Task.FromResult(target));
        A.CallTo(() => users.GetByEmail("admin@example.test")).Returns(Task.FromResult(admin));
        var service = new UserService(users, A.Fake<IUserGroupManager>(), NullLogger<UserService>.Instance);
        var update = new UserProfileUpdateModel { Website = "https://w.test", Post = "Dev", Organization = "Org", Location = "City" };

        await service.UpdateProfileInfo("target@example.test", update, new UserInfo { Email = "admin@example.test" });

        target.Website.Should().Be("https://w.test");
        target.Post.Should().Be("Dev");
        target.Organization.Should().Be("Org");
        target.Location.Should().Be("City");
        A.CallTo(() => users.UpdateUserInfo(target)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ReturnViewModels_WhenAdminGetsUsers()
    {
        var users = A.Fake<IUserManager>();
        var admin = new Mimisbrunnr.Users.User { Email = "admin@example.test", Role = UserRole.Admin };
        var regular = new Mimisbrunnr.Users.User { Email = "user@example.test", Role = UserRole.Employee };
        A.CallTo(() => users.GetByEmail("admin@example.test")).Returns(Task.FromResult(admin));
        A.CallTo(() => users.GetUsers(A<int?>._)).Returns(Task.FromResult(new[] { regular }));
        var service = new UserService(users, A.Fake<IUserGroupManager>(), NullLogger<UserService>.Instance);

        var result = await service.GetUsers(new UserInfo { Email = "admin@example.test" });

        result.Should().ContainSingle().Which.Should().BeOfType<UserViewModel>();
    }

    [Fact]
    public async Task Should_ReturnModels_WhenEmployeeGetsUsers()
    {
        var users = A.Fake<IUserManager>();
        var employee = new Mimisbrunnr.Users.User { Email = "employee@example.test", Role = UserRole.Employee };
        var regular = new Mimisbrunnr.Users.User { Email = "user@example.test", Role = UserRole.Employee };
        A.CallTo(() => users.GetByEmail("employee@example.test")).Returns(Task.FromResult(employee));
        A.CallTo(() => users.GetUsers(A<int?>._)).Returns(Task.FromResult(new[] { regular }));
        var service = new UserService(users, A.Fake<IUserGroupManager>(), NullLogger<UserService>.Instance);

        var result = await service.GetUsers(new UserInfo { Email = "employee@example.test" });

        result.Should().ContainSingle().Which.Should().BeOfType<UserModel>();
        result.Should().NotContain(x => x is UserViewModel);
    }

    [Fact]
    public async Task Should_ReturnCurrentUserViewModel_WhenGettingCurrent()
    {
        var users = A.Fake<IUserManager>();
        var user = new Mimisbrunnr.Users.User { Email = "me@example.test", Role = UserRole.Admin };
        A.CallTo(() => users.GetByEmail("me@example.test")).Returns(Task.FromResult(user));
        var service = new UserService(users, A.Fake<IUserGroupManager>(), NullLogger<UserService>.Instance);

        var result = await service.GetCurrent(new UserInfo { Email = "me@example.test" });

        result.Email.Should().Be("me@example.test");
        result.IsAdmin.Should().BeTrue();
    }

    [Fact]
    public async Task Should_ReturnProfile_WhenGettingUserByEmail()
    {
        var users = A.Fake<IUserManager>();
        var user = new Mimisbrunnr.Users.User { Email = "x@example.test", Name = "X" };
        A.CallTo(() => users.GetByEmail("x@example.test")).Returns(Task.FromResult(user));
        var service = new UserService(users, A.Fake<IUserGroupManager>(), NullLogger<UserService>.Instance);

        var result = await service.GetByEmail("x@example.test", new UserInfo { Email = "x@example.test" });

        result.Should().BeOfType<UserProfileModel>();
        result.Email.Should().Be("x@example.test");
    }

    [Fact]
    public async Task Should_ReturnEmptyGroups_WhenEmailIsEmpty()
    {
        var users = A.Fake<IUserManager>();
        var service = new UserService(users, A.Fake<IUserGroupManager>(), NullLogger<UserService>.Instance);

        var result = await service.GetUserGroups("", new UserInfo { Email = "x@example.test" });

        result.Should().BeEmpty();
        A.CallTo(() => users.GetByEmail(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_ReturnOwnGroups_WhenRequestingOwnGroups()
    {
        var users = A.Fake<IUserManager>();
        var groups = A.Fake<IUserGroupManager>();
        var user = new Mimisbrunnr.Users.User { Email = "u@example.test" };
        var group = new Mimisbrunnr.Users.Group { Name = "DEV" };
        A.CallTo(() => users.GetByEmail("u@example.test")).Returns(Task.FromResult(user));
        A.CallTo(() => groups.GetUserGroups(user)).Returns(Task.FromResult(new[] { group }));
        var service = new UserService(users, groups, NullLogger<UserService>.Instance);

        var result = await service.GetUserGroups("u@example.test", new UserInfo { Email = "u@example.test" });

        result.Should().ContainSingle().Which.Name.Should().Be("DEV");
    }

    [Fact]
    public async Task Should_ReturnEmptyGroups_WhenNonAdminRequestsOthersGroups()
    {
        var users = A.Fake<IUserManager>();
        var groups = A.Fake<IUserGroupManager>();
        var target = new Mimisbrunnr.Users.User { Email = "target@example.test" };
        var employee = new Mimisbrunnr.Users.User { Email = "other@example.test", Role = UserRole.Employee };
        A.CallTo(() => users.GetByEmail("target@example.test")).Returns(Task.FromResult(target));
        A.CallTo(() => users.GetByEmail("other@example.test")).Returns(Task.FromResult(employee));
        var service = new UserService(users, groups, NullLogger<UserService>.Instance);

        var result = await service.GetUserGroups("target@example.test", new UserInfo { Email = "other@example.test" });

        result.Should().BeEmpty();
        A.CallTo(() => groups.GetUserGroups(A<Mimisbrunnr.Users.User>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_CreateUser_WhenRequestedByAdmin()
    {
        var users = A.Fake<IUserManager>();
        var admin = new Mimisbrunnr.Users.User { Email = "admin@example.test", Role = UserRole.Admin };
        var created = new Mimisbrunnr.Users.User { Email = "new@example.test", Name = "New" };
        A.CallTo(() => users.GetByEmail("admin@example.test")).Returns(Task.FromResult(admin));
        A.CallTo(() => users.GetByEmail("new@example.test")).Returns(Task.FromResult<Mimisbrunnr.Users.User>(null));
        A.CallTo(() => users.Add("new@example.test", "New", "avatar", Mimisbrunnr.Users.UserRole.Employee)).Returns(Task.FromResult(created));
        var service = new UserService(users, A.Fake<IUserGroupManager>(), NullLogger<UserService>.Instance);

        var result = await service.CreateUser(new UserCreateModel { Email = "new@example.test", Name = "New", AvatarUrl = "avatar" }, new UserInfo { Email = "admin@example.test" });

        result.Email.Should().Be("new@example.test");
    }

    [Fact]
    public async Task ShouldThrow_UserHasNotPermissionException_WhenEmployeeCreatesUser()
    {
        var users = A.Fake<IUserManager>();
        var employee = new Mimisbrunnr.Users.User { Email = "employee@example.test", Role = UserRole.Employee };
        A.CallTo(() => users.GetByEmail("employee@example.test")).Returns(Task.FromResult(employee));
        var service = new UserService(users, A.Fake<IUserGroupManager>(), NullLogger<UserService>.Instance);

        await service.Invoking(x => x.CreateUser(new UserCreateModel { Email = "new@example.test" }, new UserInfo { Email = "employee@example.test" }))
            .Should().ThrowAsync<UserHasNotPermissionException>();
    }

    [Fact]
    public async Task ShouldThrow_InvalidOperationException_WhenCreatingExistingUser()
    {
        var users = A.Fake<IUserManager>();
        var admin = new Mimisbrunnr.Users.User { Email = "admin@example.test", Role = UserRole.Admin };
        A.CallTo(() => users.GetByEmail("admin@example.test")).Returns(Task.FromResult(admin));
        A.CallTo(() => users.GetByEmail("existing@example.test")).Returns(Task.FromResult(new Mimisbrunnr.Users.User { Email = "existing@example.test" }));
        var service = new UserService(users, A.Fake<IUserGroupManager>(), NullLogger<UserService>.Instance);

        await service.Invoking(x => x.CreateUser(new UserCreateModel { Email = "existing@example.test" }, new UserInfo { Email = "admin@example.test" }))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_DisableUser_WhenDisabling()
    {
        var users = A.Fake<IUserManager>();
        var user = new Mimisbrunnr.Users.User { Email = "u@example.test" };
        A.CallTo(() => users.GetByEmail("u@example.test")).Returns(Task.FromResult(user));
        var service = new UserService(users, A.Fake<IUserGroupManager>(), NullLogger<UserService>.Instance);

        await service.Disable("u@example.test", new UserInfo { Email = "admin@example.test" });

        A.CallTo(() => users.Disable(user)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_EnableUser_WhenEnabling()
    {
        var users = A.Fake<IUserManager>();
        var user = new Mimisbrunnr.Users.User { Email = "u@example.test" };
        A.CallTo(() => users.GetByEmail("u@example.test")).Returns(Task.FromResult(user));
        var service = new UserService(users, A.Fake<IUserGroupManager>(), NullLogger<UserService>.Instance);

        await service.Enable("u@example.test", new UserInfo { Email = "admin@example.test" });

        A.CallTo(() => users.Enable(user)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_PromoteToAdmin_WhenPromoting()
    {
        var users = A.Fake<IUserManager>();
        var user = new Mimisbrunnr.Users.User { Email = "u@example.test", Role = UserRole.Employee };
        A.CallTo(() => users.GetByEmail("u@example.test")).Returns(Task.FromResult(user));
        var service = new UserService(users, A.Fake<IUserGroupManager>(), NullLogger<UserService>.Instance);

        await service.Promote("u@example.test", new UserInfo { Email = "admin@example.test" });

        A.CallTo(() => users.ChangeRole(user, UserRole.Admin)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_DemoteToEmployee_WhenDemoting()
    {
        var users = A.Fake<IUserManager>();
        var user = new Mimisbrunnr.Users.User { Email = "u@example.test", Role = UserRole.Admin };
        A.CallTo(() => users.GetByEmail("u@example.test")).Returns(Task.FromResult(user));
        var service = new UserService(users, A.Fake<IUserGroupManager>(), NullLogger<UserService>.Instance);

        await service.Demote("u@example.test", new UserInfo { Email = "admin@example.test" });

        A.CallTo(() => users.ChangeRole(user, UserRole.Employee)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_PassOffset_WhenGettingUsers()
    {
        var users = A.Fake<IUserManager>();
        var admin = new Mimisbrunnr.Users.User { Email = "admin@example.test", Role = UserRole.Admin };
        A.CallTo(() => users.GetByEmail("admin@example.test")).Returns(Task.FromResult(admin));
        A.CallTo(() => users.GetUsers(10)).Returns(Task.FromResult(Array.Empty<Mimisbrunnr.Users.User>()));
        var service = new UserService(users, A.Fake<IUserGroupManager>(), NullLogger<UserService>.Instance);

        await service.GetUsers(new UserInfo { Email = "admin@example.test" }, 10);

        A.CallTo(() => users.GetUsers(10)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ReturnAnnotations_WhenGettingUserByEmail()
    {
        var users = A.Fake<IUserManager>();

        var result = await new UserService(users, A.Fake<IUserGroupManager>(), NullLogger<UserService>.Instance).GetByEmail("missing@example.test", new UserInfo { Email = "x@example.test" });

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_ReturnGroups_WhenAdminRequestsOthersGroups()
    {
        var users = A.Fake<IUserManager>();
        var groups = A.Fake<IUserGroupManager>();
        var target = new Mimisbrunnr.Users.User { Email = "target@example.test" };
        var admin = new Mimisbrunnr.Users.User { Email = "admin@example.test", Role = UserRole.Admin };
        var group = new Mimisbrunnr.Users.Group { Name = "DEV" };
        A.CallTo(() => users.GetByEmail("target@example.test")).Returns(Task.FromResult(target));
        A.CallTo(() => users.GetByEmail("admin@example.test")).Returns(Task.FromResult(admin));
        A.CallTo(() => groups.GetUserGroups(target)).Returns(Task.FromResult(new[] { group }));
        var service = new UserService(users, groups, NullLogger<UserService>.Instance);

        var result = await service.GetUserGroups("target@example.test", new UserInfo { Email = "admin@example.test" });

        result.Should().ContainSingle().Which.Name.Should().Be("DEV");
    }
}
