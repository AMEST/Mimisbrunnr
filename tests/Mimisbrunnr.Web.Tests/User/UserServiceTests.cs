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
}
