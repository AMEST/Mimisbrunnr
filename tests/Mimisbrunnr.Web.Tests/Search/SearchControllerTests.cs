using FakeItEasy;
using FluentAssertions;
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
        A.CallTo(() => service.SearchPages(A<string>._, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._)).MustNotHaveHappened();
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

    [Fact]
    public async Task Should_ForwardSearch_WhenSearchingPages()
    {
        var service = A.Fake<ISearchService>();
        var expected = new[] { new PageModel { Id = "p1", Name = "Title" } };
        A.CallTo(() => service.SearchPages("title", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult<IEnumerable<PageModel>>(expected));

        var result = await new SearchController(service).SearchPages("title");

        result.Should().BeEquivalentTo(expected);
        A.CallTo(() => service.SearchPages("title", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._)).MustHaveHappenedOnceExactly();
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
