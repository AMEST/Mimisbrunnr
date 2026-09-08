using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Web.Wiki;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

namespace Mimisbrunnr.Web.Tests.Wiki;

public class PageServiceSecurityTests
{
    [Fact]
    public async Task ShouldThrow_UserHasNotPermissionException_WhenUnauthorizedUserReadsCachedTree()
    {
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        var permissions = A.Fake<IPermissionService>();
        var cache = A.Fake<IDistributedCache>();
        var requestedBy = new UserInfo { Email = "unauthorized@example.test" };
        A.CallTo(() => pages.GetById("page")).Returns(Task.FromResult(new Page { Id = "page", SpaceId = "private-space" }));
        A.CallTo(() => spaces.GetById("private-space")).Returns(Task.FromResult(new Space { Id = "private-space", Key = "PRIVATE" }));
        A.CallTo(() => permissions.EnsureViewPermission("PRIVATE", requestedBy))
            .ThrowsAsync(new UserHasNotPermissionException());
        var service = new PageService(pages, spaces, A.Fake<IFeedManager>(), permissions, cache);

        await service.Invoking(x => x.GetPageTreeByPageId("page", requestedBy))
            .Should().ThrowAsync<UserHasNotPermissionException>();

        A.CallTo(() => cache.GetAsync(A<string>._, A<CancellationToken>._)).MustNotHaveHappened();
    }
}
