using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Feed;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

namespace Mimisbrunnr.Web.Tests.Feed;

public class FeedServiceTests
{
    [Fact]
    public async Task Should_ReturnAllUpdates_WhenRequestedByGlobalAdmin()
    {
        var users = A.Fake<IUserManager>();
        var feed = A.Fake<IFeedManager>();
        var admin = new UserInfo { Email = "admin@example.test" };
        A.CallTo(() => users.GetByEmail(admin.Email)).Returns(Task.FromResult(new Mimisbrunnr.Users.User { Role = UserRole.Admin }));
        A.CallTo(() => feed.GetAllPageUpdates()).Returns(Task.FromResult(new[] { new PageUpdateEvent { PageId = "page" } }));
        var service = new FeedService(users, A.Fake<IPermissionService>(), feed, A.Fake<ISpaceDisplayService>());

        var result = await service.GetPageUpdates(admin);

        result.Should().ContainSingle().Which.PageId.Should().Be("page");
        A.CallTo(() => feed.GetAllPageUpdates()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_RequireAnonymousAccess_WhenRequestedAnonymously()
    {
        var permissions = A.Fake<IPermissionService>();
        var feed = A.Fake<IFeedManager>();
        A.CallTo(() => feed.GetPageUpdates(null, null, null)).Returns(Task.FromResult(Array.Empty<PageUpdateEvent>()));
        var service = new FeedService(A.Fake<IUserManager>(), permissions, feed, A.Fake<ISpaceDisplayService>());

        await service.GetPageUpdates(null);

        A.CallTo(() => permissions.EnsureAnonymousAllowed(null)).MustHaveHappenedOnceExactly();
    }
}
