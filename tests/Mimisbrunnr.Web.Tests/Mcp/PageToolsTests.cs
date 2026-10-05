using System.Text.Json;
using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Web.Mcp.Tools;
using Mimisbrunnr.Web.Wiki;
using ModelContextProtocol;

namespace Mimisbrunnr.Web.Tests.Mcp;

public class PageToolsTests
{
    [Fact]
    public async Task Should_ReturnPage_WhenGetPageInvoked()
    {
        var pages = A.Fake<IPageService>();
        A.CallTo(() => pages.GetById("page-1", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult(new PageModel { Id = "page-1", Name = "Home", Content = "# Home" }));
        var tools = new PageTools(pages, McpTestFactory.CreateContext(), McpTestFactory.CreateExecutor());

        var json = await tools.GetPage("page-1");

        using var document = JsonDocument.Parse(json);
        document.RootElement.GetProperty("name").GetString().Should().Be("Home");
    }

    [Fact]
    public async Task Should_MapPageNotFound_WhenGetPageInvoked()
    {
        var pages = A.Fake<IPageService>();
        A.CallTo(() => pages.GetById("missing", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Throws(new PageNotFoundException());
        var tools = new PageTools(pages, McpTestFactory.CreateContext(), McpTestFactory.CreateExecutor());

        var exception = await Assert.ThrowsAsync<McpException>(() => tools.GetPage("missing"));

        exception.Message.Should().StartWith("page_not_found");
    }

    [Fact]
    public async Task Should_MapVersionNotFound_WhenGetPageVersionInvoked()
    {
        var pages = A.Fake<IPageService>();
        A.CallTo(() => pages.GetVersion("page-1", 7, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Throws(new PageVersionNotFoundException());
        var tools = new PageTools(pages, McpTestFactory.CreateContext(), McpTestFactory.CreateExecutor());

        var exception = await Assert.ThrowsAsync<McpException>(() => tools.GetPageVersion("page-1", 7));

        exception.Message.Should().StartWith("version_not_found");
    }

    [Fact]
    public async Task Should_ReturnValidationFailed_WhenUpdatePageNameIsEmpty()
    {
        var pages = A.Fake<IPageService>();
        var tools = new PageTools(pages, McpTestFactory.CreateContext(), McpTestFactory.CreateExecutor());

        var exception = await Assert.ThrowsAsync<McpException>(() => tools.UpdatePage("page-1", "", "content"));

        exception.Message.Should().StartWith("validation_failed");
        A.CallTo(() => pages.Update(A<string>._, A<PageUpdateModel>._, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .MustNotHaveHappened();
    }
}
