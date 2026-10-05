using System.Text.Json;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Mimisbrunnr.Integration.User;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Mcp.Internal;
using Mimisbrunnr.Web.Mcp.Tools;
using Mimisbrunnr.Web.Search;
using Mimisbrunnr.Wiki.Contracts;
using ModelContextProtocol;

namespace Mimisbrunnr.Web.Tests.Mcp;

public class SearchToolsTests
{
    [Theory]
    [InlineData("SearchPages", "id", "page-1")]
    [InlineData("SearchSpaces", "key", "DOCS")]
    [InlineData("SearchUsers", "email", "alice@example.com")]
    public async Task Should_ReturnResults_WhenSearchInvoked(string method, string property, string expected)
    {
        var search = A.Fake<ISearchService>();
        A.CallTo(() => search.SearchPages("query", A<UserInfo>._))
            .Returns(Task.FromResult<IEnumerable<PageModel>>([new PageModel { Id = "page-1", SpaceKey = "DOCS", Name = "Page" }]));
        A.CallTo(() => search.SearchSpaces("query", A<UserInfo>._))
            .Returns(Task.FromResult<IEnumerable<SpaceModel>>([new SpaceModel { Key = "DOCS", Name = "Docs" }]));
        A.CallTo(() => search.SearchUsers("query", A<UserInfo>._))
            .Returns(Task.FromResult<IEnumerable<UserModel>>([new UserModel { Email = "alice@example.com", Name = "Alice" }]));
        var tools = CreateTools(search);

        var json = await Invoke(tools, method, "query");

        using var document = JsonDocument.Parse(json);
        document.RootElement.GetArrayLength().Should().Be(1);
        document.RootElement[0].GetProperty(property).GetString().Should().Be(expected);
        var call = Fake.GetCalls(search).Should().ContainSingle().Which;
        call.Method.Name.Should().Be(method);
        call.Arguments[0].Should().Be("query");
        call.Arguments[1].Should().BeOfType<UserInfo>().Which.Email.Should().Be(McpTestFactory.UserEmail);
    }

    [Theory]
    [InlineData("SearchPages")]
    [InlineData("SearchSpaces")]
    [InlineData("SearchUsers")]
    public async Task Should_ReturnEmptyArray_WhenNoResultsMatch(string method)
    {
        var search = A.Fake<ISearchService>();
        A.CallTo(() => search.SearchPages("query", A<UserInfo>._))
            .Returns(Task.FromResult<IEnumerable<PageModel>>([]));
        A.CallTo(() => search.SearchSpaces("query", A<UserInfo>._))
            .Returns(Task.FromResult<IEnumerable<SpaceModel>>([]));
        A.CallTo(() => search.SearchUsers("query", A<UserInfo>._))
            .Returns(Task.FromResult<IEnumerable<UserModel>>([]));

        var json = await Invoke(CreateTools(search), method, "query");

        json.Should().Be("[]");
    }

    [Theory]
    [InlineData("SearchPages", null)]
    [InlineData("SearchPages", "")]
    [InlineData("SearchPages", "   ")]
    [InlineData("SearchSpaces", null)]
    [InlineData("SearchSpaces", "")]
    [InlineData("SearchSpaces", "   ")]
    [InlineData("SearchUsers", null)]
    [InlineData("SearchUsers", "")]
    [InlineData("SearchUsers", "   ")]
    public async Task ShouldThrow_ValidationFailed_WhenQueryIsEmpty(string method, string text)
    {
        var search = A.Fake<ISearchService>();

        var exception = await Assert.ThrowsAsync<McpException>(() => Invoke(CreateTools(search), method, text));

        exception.Message.Should().StartWith("validation_failed:");
        Fake.GetCalls(search).Should().BeEmpty();
    }

    [Theory]
    [InlineData("SearchPages")]
    [InlineData("SearchSpaces")]
    [InlineData("SearchUsers")]
    public async Task ShouldThrow_Forbidden_WhenRequestIsNotAuthenticated(string method)
    {
        var search = A.Fake<ISearchService>();
        var context = new ToolContext(new HttpContextAccessor { HttpContext = new DefaultHttpContext() });

        var exception = await Assert.ThrowsAsync<McpException>(() => Invoke(CreateTools(search, context), method, "query"));

        exception.Message.Should().StartWith("forbidden:");
        Fake.GetCalls(search).Should().BeEmpty();
    }

    [Theory]
    [InlineData("SearchPages")]
    [InlineData("SearchSpaces")]
    [InlineData("SearchUsers")]
    public async Task ShouldThrow_Forbidden_WhenSearchServiceDeniesAccess(string method)
    {
        var search = A.Fake<ISearchService>();
        A.CallTo(() => search.SearchPages("query", A<UserInfo>._)).Throws(new UserHasNotPermissionException());
        A.CallTo(() => search.SearchSpaces("query", A<UserInfo>._)).Throws(new UserHasNotPermissionException());
        A.CallTo(() => search.SearchUsers("query", A<UserInfo>._)).Throws(new UserHasNotPermissionException());

        var exception = await Assert.ThrowsAsync<McpException>(() => Invoke(CreateTools(search), method, "query"));

        exception.Message.Should().StartWith("forbidden:");
    }

    private static SearchTools CreateTools(ISearchService search, ToolContext context = null)
    {
        context ??= McpTestFactory.CreateContext();
        return new SearchTools(search, context, McpTestFactory.CreateExecutor(context));
    }

    private static Task<string> Invoke(SearchTools tools, string method, string text) => method switch
    {
        "SearchPages" => tools.SearchPages(text),
        "SearchSpaces" => tools.SearchSpaces(text),
        "SearchUsers" => tools.SearchUsers(text),
        _ => throw new ArgumentException("Unknown search method.", nameof(method))
    };
}
