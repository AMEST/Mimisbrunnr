using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Integration.User;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Group;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

namespace Mimisbrunnr.Web.Tests.Group;

public class GroupServiceTests
{
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
}
