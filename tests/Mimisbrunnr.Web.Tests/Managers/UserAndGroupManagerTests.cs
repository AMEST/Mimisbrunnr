using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Users;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Web.Tests.Managers;

public class UserAndGroupManagerTests
{
    public UserAndGroupManagerTests() => QueryableAsyncExtensions.EnableFallback();

    [Fact]
    public async Task Should_NormalizeEmail_WhenAddingUser()
    {
        var repository = A.Fake<IRepository<Mimisbrunnr.Users.User>>();
        var manager = new UserManager(repository);

        var user = await manager.Add("USER@Example.Test", "User", null, UserRole.Employee);

        user.Email.Should().Be("user@example.test");
        A.CallTo(() => repository.Create(A<Mimisbrunnr.Users.User>.That.Matches(x => x.Email == "user@example.test"), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_SearchCaseInsensitively_WhenFindingUsers()
    {
        var repository = A.Fake<IRepository<Mimisbrunnr.Users.User>>();
        A.CallTo(() => repository.GetAll()).Returns(new[]
        {
            new Mimisbrunnr.Users.User { Name = "Alice", Email = "alice@example.test" },
            new Mimisbrunnr.Users.User { Name = "Bob", Email = "bob@example.test" }
        }.AsQueryable());
        var manager = new UserManager(repository);

        var result = await manager.Search("ALICE");

        result.Should().ContainSingle().Which.Email.Should().Be("alice@example.test");
    }

    [Fact]
    public async Task ShouldNot_CreateMembership_WhenMembershipAlreadyExists()
    {
        var groups = A.Fake<IRepository<Mimisbrunnr.Users.Group>>();
        var memberships = A.Fake<IRepository<UserGroup>>();
        A.CallTo(() => memberships.GetAll()).Returns(new[] { new UserGroup { GroupId = "group", UserId = "user" } }.AsQueryable());
        var manager = new UserGroupManager(groups, memberships, A.Fake<IUserManager>());

        await manager.AddToGroup(new Mimisbrunnr.Users.Group { Id = "group" }, new Mimisbrunnr.Users.User { Id = "user" });

        A.CallTo(() => memberships.Create(A<UserGroup>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_RemoveMemberships_WhenRemovingGroup()
    {
        var groups = A.Fake<IRepository<Mimisbrunnr.Users.Group>>();
        var memberships = A.Fake<IRepository<UserGroup>>();
        var group = new Mimisbrunnr.Users.Group { Id = "group" };
        A.CallTo(() => groups.GetAll()).Returns(new[] { group }.AsQueryable());
        var manager = new UserGroupManager(groups, memberships, A.Fake<IUserManager>());

        await manager.Remove(group);

        A.CallTo(() => memberships.DeleteAll(A<System.Linq.Expressions.Expression<Func<UserGroup, bool>>>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => groups.Delete(group, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }
}
