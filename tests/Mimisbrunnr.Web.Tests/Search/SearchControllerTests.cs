using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Web.Search;

namespace Mimisbrunnr.Web.Tests.Search;

public class SearchControllerTests
{
    [Fact]
    public async Task Should_ReturnEmpty_WhenSpaceSearchIsBlank()
    {
        var service = A.Fake<ISearchService>();

        var result = await new SearchController(service).SearchSpaces("");

        result.Should().BeEmpty();
        A.CallTo(() => service.SearchSpaces(A<string>._, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_ReturnEmpty_WhenPageSearchIsBlank()
    {
        var service = A.Fake<ISearchService>();

        var result = await new SearchController(service).SearchPages(null);

        result.Should().BeEmpty();
        A.CallTo(() => service.SearchPages(A<string>._, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._, A<int>._, A<int>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_ForwardSearch_WhenSearchingSpaces()
    {
        var service = A.Fake<ISearchService>();
        var expected = new[] { new SpaceModel { Key = "DOCS", Name = "Docs" } };
        A.CallTo(() => service.SearchSpaces("docs", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult<IEnumerable<SpaceModel>>(expected));

        var result = await new SearchController(service).SearchSpaces("docs");

        result.Should().BeEquivalentTo(expected);
        A.CallTo(() => service.SearchSpaces("docs", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(100, 0, null)]
    [InlineData(20, 20, 40)]
    public async Task Should_ExposeNextOffset_WhenSearchingPages(int limit, int offset, int? nextOffset)
    {
        var service = A.Fake<ISearchService>();
        var expected = new[] { new PageModel { Id = "p1", Name = "Title" } };
        A.CallTo(() => service.SearchPages("title", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._, limit, offset))
            .Returns(new PageSearchResult(expected, nextOffset));
        var controller = new SearchController(service)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var result = await controller.SearchPages("title", limit, offset);

        result.Should().BeEquivalentTo(expected);
        controller.Response.Headers["X-Next-Offset"].ToString().Should().Be(nextOffset?.ToString() ?? "");
        A.CallTo(() => service.SearchPages("title", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._, limit, offset)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ReturnEmptyResult_WhenUserSearchIsBlank()
    {
        var service = A.Fake<ISearchService>();

        var result = await new SearchController(service).SearchUsers("");

        result.Should().BeOfType<OkObjectResult>();
        A.CallTo(() => service.SearchUsers(A<string>._, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_ReturnFoundUsers_WhenUserSearchIsProvided()
    {
        var service = A.Fake<ISearchService>();
        var expected = new[] { new Mimisbrunnr.Integration.User.UserModel { Email = "user@example.test" } };
        A.CallTo(() => service.SearchUsers("user", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult<IEnumerable<Mimisbrunnr.Integration.User.UserModel>>(expected));

        var result = await new SearchController(service).SearchUsers("user");

        result.Should().BeOfType<OkObjectResult>();
        A.CallTo(() => service.SearchUsers("user", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._)).MustHaveHappenedOnceExactly();
    }
}
