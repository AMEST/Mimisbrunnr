using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Integration.User;
using Mimisbrunnr.Integration.Group;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Group;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

namespace Mimisbrunnr.Web.Tests.Group;

public class GroupServiceTests
{
    [Fact]
    public async Task Should_CreateGroup_WhenValidRequestIsProvided()
    {
        var groups = A.Fake<IUserGroupManager>();
        var created = new Mimisbrunnr.Users.Group { Name = "team", Description = "Team", OwnerEmails = ["owner@example.test"] };
        A.CallTo(() => groups.Add("team", "Team", "owner@example.test")).Returns(Task.FromResult(created));
        var service = new GroupService(A.Fake<IUserManager>(), groups, A.Fake<ISpaceManager>());

        var result = await service.Create(new GroupCreateModel { Name = "team", Description = "Team" }, new UserInfo { Email = "owner@example.test" });

        result.Name.Should().Be("team");
    }

    [Fact]
    public async Task ShouldThrow_GroupNotFoundException_WhenReadingUnknownGroup()
    {
        var groups = A.Fake<IUserGroupManager>();
        A.CallTo(() => groups.FindByName("missing")).Returns(Task.FromResult<Mimisbrunnr.Users.Group>(null));
        var service = new GroupService(A.Fake<IUserManager>(), groups, A.Fake<ISpaceManager>());

        await service.Invoking(x => x.Get("missing", new UserInfo { Email = "user@example.test" }))
            .Should().ThrowAsync<GroupNotFoundException>();
    }

    [Fact]
    public async Task ShouldThrow_UserHasNotPermissionException_WhenNonOwnerAddsGroupMember()
    {
        var users = A.Fake<IUserManager>();
        var groups = A.Fake<IUserGroupManager>();
        var service = new GroupService(users, groups, A.Fake<ISpaceManager>());
        var group = new Mimisbrunnr.Users.Group { Name = "team", OwnerEmails = ["owner@example.test"] };
        A.CallTo(() => groups.FindByName("team")).Returns(Task.FromResult(group));
        A.CallTo(() => users.GetByEmail("member@example.test")).Returns(Task.FromResult(new Mimisbrunnr.Users.User { Role = UserRole.Employee }));

        await service.Invoking(x => x.AddUserToGroup("team", new UserInfo { Email = "target@example.test" }, new UserInfo { Email = "member@example.test" }))
            .Should().ThrowAsync<UserHasNotPermissionException>();
    }

    [Fact]
    public async Task Should_ReturnEmpty_WhenNonAdminFiltersAnotherOwnersGroups()
    {
        var users = A.Fake<IUserManager>();
        var groups = A.Fake<IUserGroupManager>();
        var service = new GroupService(users, groups, A.Fake<ISpaceManager>());
        var user = new UserInfo { Email = "member@example.test" };
        A.CallTo(() => users.GetByEmail(user.Email)).Returns(Task.FromResult(new Mimisbrunnr.Users.User { Role = UserRole.Employee }));

        var result = await service.GetAll(new GroupFilterModel { OwnerEmail = "other@example.test" }, user);

        result.Should().BeEmpty();
        A.CallTo(() => groups.GetAll(A<int?>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_FilterGroupsByOwnerEmail_WhenOwnerOrAdminRequested()
    {
        var users = A.Fake<IUserManager>();
        var groups = A.Fake<IUserGroupManager>();
        var service = new GroupService(users, groups, A.Fake<ISpaceManager>());
        var user = new UserInfo { Email = "owner@example.test" };
        A.CallTo(() => users.GetByEmail(user.Email)).Returns(Task.FromResult(new Mimisbrunnr.Users.User { Role = UserRole.Admin }));
        A.CallTo(() => groups.GetAll(null)).Returns(Task.FromResult(new[]
        {
            new Mimisbrunnr.Users.Group { Name = "mine", OwnerEmails = [user.Email] },
            new Mimisbrunnr.Users.Group { Name = "other", OwnerEmails = ["other@example.test"] }
        }));

        var result = await service.GetAll(new GroupFilterModel { OwnerEmail = user.Email }, user);

        result.Should().ContainSingle().Which.Name.Should().Be("mine");
    }

    [Fact]
    public async Task ShouldThrow_UserHasNotPermissionException_WhenNonOwnerReadsGroupUsers()
    {
        var users = A.Fake<IUserManager>();
        var groups = A.Fake<IUserGroupManager>();
        var service = new GroupService(users, groups, A.Fake<ISpaceManager>());
        var group = new Mimisbrunnr.Users.Group { Name = "team", OwnerEmails = ["owner@example.test"] };
        var user = new UserInfo { Email = "member@example.test" };
        A.CallTo(() => groups.FindByName("team")).Returns(Task.FromResult(group));
        A.CallTo(() => users.GetByEmail(user.Email)).Returns(Task.FromResult(new Mimisbrunnr.Users.User { Role = UserRole.Employee }));

        await service.Invoking(x => x.GetUsers("team", user)).Should().ThrowAsync<UserHasNotPermissionException>();
    }

    [Fact]
    public async Task ShouldThrow_GroupNotFoundException_WhenAddingUserToMissingGroup()
    {
        var groups = A.Fake<IUserGroupManager>();
        A.CallTo(() => groups.FindByName("missing")).Returns(Task.FromResult<Mimisbrunnr.Users.Group>(null));
        var service = new GroupService(A.Fake<IUserManager>(), groups, A.Fake<ISpaceManager>());

        await service.Invoking(x => x.AddUserToGroup("missing", new UserInfo { Email = "target@example.test" }, new UserInfo { Email = "admin@example.test" }))
            .Should().ThrowAsync<GroupNotFoundException>();
    }

    [Fact]
    public async Task ShouldThrow_ArgumentOutOfRangeException_WhenAddingUnknownUserToGroup()
    {
        var users = A.Fake<IUserManager>();
        var groups = A.Fake<IUserGroupManager>();
        var service = new GroupService(users, groups, A.Fake<ISpaceManager>());
        var group = new Mimisbrunnr.Users.Group { Name = "team", OwnerEmails = ["owner@example.test"] };
        var target = new UserInfo { Email = "missing@example.test" };
        A.CallTo(() => groups.FindByName("team")).Returns(Task.FromResult(group));
        A.CallTo(() => users.GetByEmail("owner@example.test")).Returns(Task.FromResult(new Mimisbrunnr.Users.User { Role = UserRole.Admin }));
        A.CallTo(() => users.GetByEmail(target.Email)).Returns(Task.FromResult<Mimisbrunnr.Users.User>(null));

        await service.Invoking(x => x.AddUserToGroup("team", target, new UserInfo { Email = "owner@example.test" }))
            .Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task Should_RemoveSpacePermissionsAndGroup_WhenRemovingGroup()
    {
        var users = A.Fake<IUserManager>();
        var groups = A.Fake<IUserGroupManager>();
        var spaces = A.Fake<ISpaceManager>();
        var service = new GroupService(users, groups, spaces);
        var group = new Mimisbrunnr.Users.Group { Name = "team", OwnerEmails = ["owner@example.test"] };
        var removedBy = new UserInfo { Email = "owner@example.test" };
        var space = new Space { Permissions = [new Permission { Group = new GroupInfo { Name = "team" } }] };
        A.CallTo(() => groups.FindByName("team")).Returns(Task.FromResult(group));
        A.CallTo(() => users.GetByEmail(removedBy.Email)).Returns(Task.FromResult(new Mimisbrunnr.Users.User { Role = UserRole.Admin }));
        A.CallTo(() => spaces.GetAll(null, null)).Returns(Task.FromResult(new[] { space }));

        await service.Remove("team", removedBy);

        A.CallTo(() => spaces.RemovePermission(space, A<Permission>.That.Matches(x => x.Group.Name == "team"))).MustHaveHappenedOnceExactly();
        A.CallTo(() => groups.Remove(group)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_UpdateDescription_WhenOwnerUpdatesGroup()
    {
        var users = A.Fake<IUserManager>();
        var groups = A.Fake<IUserGroupManager>();
        var service = new GroupService(users, groups, A.Fake<ISpaceManager>());
        var group = new Mimisbrunnr.Users.Group { Name = "team", OwnerEmails = ["owner@example.test"], Description = "Old" };
        var updatedBy = new UserInfo { Email = "owner@example.test" };
        A.CallTo(() => groups.FindByName("team")).Returns(Task.FromResult(group));
        A.CallTo(() => users.GetByEmail(updatedBy.Email)).Returns(Task.FromResult(new Mimisbrunnr.Users.User { Role = UserRole.Admin }));

        await service.Update("team", new GroupUpdateModel { Description = "New" }, updatedBy);

        group.Description.Should().Be("New");
        A.CallTo(() => groups.Update(group)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ShouldThrow_UserHasNotPermissionException_WhenNonOwnerUpdatesGroup()
    {
        var users = A.Fake<IUserManager>();
        var groups = A.Fake<IUserGroupManager>();
        var service = new GroupService(users, groups, A.Fake<ISpaceManager>());
        var group = new Mimisbrunnr.Users.Group { Name = "team", OwnerEmails = ["owner@example.test"] };
        var updatedBy = new UserInfo { Email = "member@example.test" };
        A.CallTo(() => groups.FindByName("team")).Returns(Task.FromResult(group));
        A.CallTo(() => users.GetByEmail(updatedBy.Email)).Returns(Task.FromResult(new Mimisbrunnr.Users.User { Role = UserRole.Employee }));

        await service.Invoking(x => x.Update("team", new GroupUpdateModel(), updatedBy)).Should().ThrowAsync<UserHasNotPermissionException>();
    }
}
