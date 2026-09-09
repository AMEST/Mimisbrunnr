using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Users;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Web.Tests.Managers;

public class UserManagerTests
{
    public UserManagerTests() => QueryableAsyncExtensions.EnableFallback();

    [Fact]
    public async Task Should_ReturnNull_WhenEmailIsEmpty()
    {
        var repository = A.Fake<IRepository<Mimisbrunnr.Users.User>>();
        var manager = new UserManager(repository);

        var result = await manager.GetByEmail("");

        result.Should().BeNull();
        A.CallTo(() => repository.GetAll()).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_DisableUser_WhenDisableIsCalled()
    {
        var repository = A.Fake<IRepository<Mimisbrunnr.Users.User>>();
        var manager = new UserManager(repository);
        var user = new Mimisbrunnr.Users.User { Enable = true };

        await manager.Disable(user);

        user.Enable.Should().BeFalse();
        A.CallTo(() => repository.Update(user, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

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
    public async Task Should_ReturnNull_WhenEmailIsNull()
    {
        var repository = A.Fake<IRepository<Mimisbrunnr.Users.User>>();
        var manager = new UserManager(repository);

        var result = await manager.GetByEmail(null);

        result.Should().BeNull();
        A.CallTo(() => repository.GetAll()).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_FindUserCaseInsensitively_WhenGettingByEmail()
    {
        var repository = A.Fake<IRepository<Mimisbrunnr.Users.User>>();
        var user = new Mimisbrunnr.Users.User { Email = "user@example.test" };
        A.CallTo(() => repository.GetAll()).Returns(new[] { user }.AsQueryable());
        var manager = new UserManager(repository);

        var result = await manager.GetByEmail("USER@Example.Test");

        result.Should().BeSameAs(user);
    }

    [Fact]
    public async Task Should_FindUserById_LowercasingId()
    {
        var repository = A.Fake<IRepository<Mimisbrunnr.Users.User>>();
        var user = new Mimisbrunnr.Users.User { Id = "abc123" };
        A.CallTo(() => repository.GetAll()).Returns(new[] { user }.AsQueryable());
        var manager = new UserManager(repository);

        var result = await manager.GetById("ABC123");

        result.Should().BeSameAs(user);
    }

    [Fact]
    public async Task Should_ApplyOffsetAndMaxCount_WhenGettingUsers()
    {
        var repository = A.Fake<IRepository<Mimisbrunnr.Users.User>>();
        A.CallTo(() => repository.GetAll()).Returns(new[]
        {
            new Mimisbrunnr.Users.User { Id = "1" },
            new Mimisbrunnr.Users.User { Id = "2" },
            new Mimisbrunnr.Users.User { Id = "3" }
        }.AsQueryable());
        var manager = new UserManager(repository);

        var result = await manager.GetUsers(offset: 1);

        result.Length.Should().Be(2);
        result[0].Id.Should().Be("2");
    }

    [Fact]
    public async Task Should_EnableUser_WhenEnableIsCalled()
    {
        var repository = A.Fake<IRepository<Mimisbrunnr.Users.User>>();
        var manager = new UserManager(repository);
        var user = new Mimisbrunnr.Users.User { Enable = false };

        await manager.Enable(user);

        user.Enable.Should().BeTrue();
        A.CallTo(() => repository.Update(user, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ChangeRole_WhenRoleChanges()
    {
        var repository = A.Fake<IRepository<Mimisbrunnr.Users.User>>();
        var manager = new UserManager(repository);
        var user = new Mimisbrunnr.Users.User { Role = UserRole.Employee };

        await manager.ChangeRole(user, UserRole.Admin);

        user.Role.Should().Be(UserRole.Admin);
        A.CallTo(() => repository.Update(user, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_UpdateUserInfo_WhenCalled()
    {
        var repository = A.Fake<IRepository<Mimisbrunnr.Users.User>>();
        var manager = new UserManager(repository);
        var user = new Mimisbrunnr.Users.User { Id = "u1", Name = "New Name" };

        await manager.UpdateUserInfo(user);

        A.CallTo(() => repository.Update(user, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }
}
