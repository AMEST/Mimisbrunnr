using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Web.Feed;

namespace Mimisbrunnr.Web.Tests.Feed;

public class FeedControllerTests
{
    [Fact]
    public async Task Should_ReturnOk_WhenFeedIsRequested()
    {
        var service = A.Fake<IFeedService>();
        A.CallTo(() => service.GetPageUpdates(null, null)).Returns(Task.FromResult<PageUpdateEventModel[]>([]));

        var result = await new FeedController(service).Get();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Should_ReturnMappedUpdates_WhenFeedIsRequested()
    {
        var service = A.Fake<IFeedService>();
        A.CallTo(() => service.GetPageUpdates(null, null)).Returns(Task.FromResult(new[] { new PageUpdateEventModel { PageId = "page" } }));

        var result = await new FeedController(service).Get();

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeOfType<PageUpdateEventModel[]>();
    }

    [Fact]
    public async Task Should_FilterByEmail_WhenEmailFilterIsProvided()
    {
        var service = A.Fake<IFeedService>();
        A.CallTo(() => service.GetPageUpdates(null, "author@example.test")).Returns(Task.FromResult(new[] { new PageUpdateEventModel { PageId = "page" } }));

        var result = await new FeedController(service).Get("author@example.test");

        result.Should().BeOfType<OkObjectResult>();
        A.CallTo(() => service.GetPageUpdates(null, "author@example.test")).MustHaveHappenedOnceExactly();
    }
}