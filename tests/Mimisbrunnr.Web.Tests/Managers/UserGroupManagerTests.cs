using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Users;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Web.Tests.Managers;

public class UserGroupManagerTests
{
    public UserGroupManagerTests() => QueryableAsyncExtensions.EnableFallback();

    [Fact]
    public async Task Should_FindGroupCaseInsensitively_WhenSearchingByName()
    {
        var groups = A.Fake<IRepository<Mimisbrunnr.Users.Group>>();
        var group = new Mimisbrunnr.Users.Group { Name = "Developers" };
        A.CallTo(() => groups.GetAll()).Returns(new[] { group }.AsQueryable());

        var result = await new UserGroupManager(groups, A.Fake<IRepository<UserGroup>>(), A.Fake<IUserManager>()).FindByName("developers");

        result.Should().BeSameAs(group);
    }

    [Fact]
    public async Task ShouldNot_CreateMembership_WhenMembershipAlreadyExists()
    {
        var groups = A.Fake<IRepository<Mimisbrunnr.Users.Group>>();
        var memberships = A.Fake<IRepository<UserGroup>>();
        A.CallTo(() => memberships.GetAll()).Returns(new[] { new UserGroup { GroupId = "group", UserId = "user" } }.AsQueryable());

        await new UserGroupManager(groups, memberships, A.Fake<IUserManager>()).AddToGroup(new Mimisbrunnr.Users.Group { Id = "group" }, new Mimisbrunnr.Users.User { Id = "user" });

        A.CallTo(() => memberships.Create(A<UserGroup>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_RemoveMemberships_WhenRemovingGroup()
    {
        var groups = A.Fake<IRepository<Mimisbrunnr.Users.Group>>();
        var memberships = A.Fake<IRepository<UserGroup>>();
        var group = new Mimisbrunnr.Users.Group { Id = "group" };
        A.CallTo(() => groups.GetAll()).Returns(new[] { group }.AsQueryable());

        await new UserGroupManager(groups, memberships, A.Fake<IUserManager>()).Remove(group);

        A.CallTo(() => memberships.DeleteAll(A<System.Linq.Expressions.Expression<Func<UserGroup, bool>>>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => groups.Delete(group, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ApplyOffsetAndMaxCount_WhenGettingAllGroups()
    {
        var groups = A.Fake<IRepository<Mimisbrunnr.Users.Group>>();
        A.CallTo(() => groups.GetAll()).Returns(new[]
        {
            new Mimisbrunnr.Users.Group { Name = "1" },
            new Mimisbrunnr.Users.Group { Name = "2" },
            new Mimisbrunnr.Users.Group { Name = "3" }
        }.AsQueryable());

        var result = await new UserGroupManager(groups, A.Fake<IRepository<UserGroup>>(), A.Fake<IUserManager>()).GetAll(offset: 1);

        result.Length.Should().Be(2);
        result[0].Name.Should().Be("2");
    }

    [Fact]
    public async Task Should_AddGroupWithOwner_WhenAddingNewGroup()
    {
        var groups = A.Fake<IRepository<Mimisbrunnr.Users.Group>>();

        var group = await new UserGroupManager(groups, A.Fake<IRepository<UserGroup>>(), A.Fake<IUserManager>()).Add("Developers", "desc", "owner@example.test");

        group.Name.Should().Be("Developers");
        group.OwnerEmails.Should().BeEquivalentTo(["owner@example.test"]);
        A.CallTo(() => groups.Create(group, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_UpdateGroup_WhenUpdatingGroup()
    {
        var groups = A.Fake<IRepository<Mimisbrunnr.Users.Group>>();
        var group = new Mimisbrunnr.Users.Group { Id = "group", Name = "New" };

        await new UserGroupManager(groups, A.Fake<IRepository<UserGroup>>(), A.Fake<IUserManager>()).Update(group);

        A.CallTo(() => groups.Update(group, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_CreateMembership_WhenUserAddedToGroup()
    {
        var groups = A.Fake<IRepository<Mimisbrunnr.Users.Group>>();
        var memberships = A.Fake<IRepository<UserGroup>>();
        A.CallTo(() => memberships.GetAll()).Returns(Array.Empty<UserGroup>().AsQueryable());

        await new UserGroupManager(groups, memberships, A.Fake<IUserManager>()).AddToGroup(new Mimisbrunnr.Users.Group { Id = "group" }, new Mimisbrunnr.Users.User { Id = "user" });

        A.CallTo(() => memberships.Create(A<UserGroup>.That.Matches(x => x.GroupId == "group" && x.UserId == "user"), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_DeleteMembership_WhenUserRemovedFromGroup()
    {
        var groups = A.Fake<IRepository<Mimisbrunnr.Users.Group>>();
        var memberships = A.Fake<IRepository<UserGroup>>();
        var membership = new UserGroup { GroupId = "group", UserId = "user" };
        A.CallTo(() => memberships.GetAll()).Returns(new[] { membership }.AsQueryable());

        await new UserGroupManager(groups, memberships, A.Fake<IUserManager>()).RemoveFromGroup(new Mimisbrunnr.Users.Group { Id = "group" }, new Mimisbrunnr.Users.User { Id = "user" });

        A.CallTo(() => memberships.Delete(membership, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ShouldNot_DeleteAnything_WhenRemovingMissingGroup()
    {
        var groups = A.Fake<IRepository<Mimisbrunnr.Users.Group>>();
        var memberships = A.Fake<IRepository<UserGroup>>();
        A.CallTo(() => groups.GetAll()).Returns(Array.Empty<Mimisbrunnr.Users.Group>().AsQueryable());

        await new UserGroupManager(groups, memberships, A.Fake<IUserManager>()).Remove(new Mimisbrunnr.Users.Group { Id = "missing" });

        A.CallTo(() => memberships.DeleteAll(A<System.Linq.Expressions.Expression<Func<UserGroup, bool>>>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => groups.Delete(A<Mimisbrunnr.Users.Group>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_ResolveUserGroups_WhenGettingUserGroups()
    {
        var groups = A.Fake<IRepository<Mimisbrunnr.Users.Group>>();
        var memberships = A.Fake<IRepository<UserGroup>>();
        var group1 = new Mimisbrunnr.Users.Group { Id = "g1", Name = "One" };
        var group2 = new Mimisbrunnr.Users.Group { Id = "g2", Name = "Two" };
        A.CallTo(() => memberships.GetAll()).Returns(new[]
        {
            new UserGroup { GroupId = "g1", UserId = "u1" },
            new UserGroup { GroupId = "g3", UserId = "u1" }
        }.AsQueryable());
        A.CallTo(() => groups.GetAll()).Returns(new[] { group1, group2 }.AsQueryable());

        var result = await new UserGroupManager(groups, memberships, A.Fake<IUserManager>()).GetUserGroups(new Mimisbrunnr.Users.User { Id = "u1" });

        result.Should().ContainSingle().Which.Should().BeSameAs(group1);
    }

    [Fact]
    public async Task Should_ResolveUsersInGroup_WhenGettingUsersInGroup()
    {
        var groups = A.Fake<IRepository<Mimisbrunnr.Users.Group>>();
        var memberships = A.Fake<IRepository<UserGroup>>();
        var userManager = A.Fake<IUserManager>();
        var user1 = new Mimisbrunnr.Users.User { Id = "u1", Name = "Bob" };
        A.CallTo(() => memberships.GetAll()).Returns(new[]
        {
            new UserGroup { GroupId = "g1", UserId = "u1" },
            new UserGroup { GroupId = "g1", UserId = "u2" }
        }.AsQueryable());
        A.CallTo(() => userManager.GetById("u1")).Returns(user1);
        A.CallTo(() => userManager.GetById("u2")).Returns((Mimisbrunnr.Users.User)null);

        var result = await new UserGroupManager(groups, memberships, userManager).GetUsersInGroup(new Mimisbrunnr.Users.Group { Id = "g1" });

        result.Should().ContainSingle().Which.Should().BeSameAs(user1);
    }
}
