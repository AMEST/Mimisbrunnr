using System.Text.Json;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Host.Services;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Mcp.Tools;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Web.Wiki;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;
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
        var tools = CreateTools(pages);

        var json = await tools.GetPage("page-1");

        using var document = JsonDocument.Parse(json);
        document.RootElement.GetProperty("name").GetString().Should().Be("Home");
    }

    [Fact]
    public async Task Should_ReturnPageTree_WhenGetPageTreeInvoked()
    {
        var pages = A.Fake<IPageService>();
        A.CallTo(() => pages.GetPageTreeByPageId("page-1", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult(new PageTreeModel
            {
                Page = new PageModel { Id = "page-1", Name = "Home" },
                Childs = [new PageTreeModel { Page = new PageModel { Id = "page-2", Name = "Child" } }]
            }));
        var tools = CreateTools(pages);

        var json = await tools.GetPageTree("page-1");

        using var document = JsonDocument.Parse(json);
        document.RootElement.GetProperty("page").GetProperty("name").GetString().Should().Be("Home");
        document.RootElement.GetProperty("childs")[0].GetProperty("page").GetProperty("name").GetString().Should().Be("Child");
    }

    [Fact]
    public async Task Should_ReturnCreatedPage_WhenCreatePageInvoked()
    {
        var pages = A.Fake<IPageService>();
        var user = McpTestFactory.ExpectedUser();
        A.CallTo(() => pages.Create(A<PageCreateModel>.That.Matches(model =>
                model.SpaceKey == "DOCS" && model.ParentPageId == "home" &&
                model.Name == "New page" && model.Content == "# Markdown"), user))
            .Returns(new PageModel { Id = "new", SpaceKey = "DOCS", Name = "New page", Content = "# Markdown" });
        var tools = CreateTools(pages);

        var json = await tools.CreatePage("DOCS", "home", "New page", "# Markdown");

        using var document = JsonDocument.Parse(json);
        document.RootElement.GetProperty("id").GetString().Should().Be("new");
        document.RootElement.GetProperty("spaceKey").GetString().Should().Be("DOCS");
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task ShouldThrow_Forbidden_WhenCreatePageLacksSpacePermissions(bool canView, bool canEdit)
    {
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        var users = A.Fake<IUserManager>();
        var groups = A.Fake<IUserGroupManager>();
        var user = McpTestFactory.ExpectedUser();
        A.CallTo(() => spaces.GetByKey("DOCS")).Returns(new Space
        {
            Id = "docs",
            Key = "DOCS",
            Type = SpaceType.Private,
            Status = SpaceStatus.Actual,
            Permissions = [new Permission { User = user, CanView = canView, CanEdit = canEdit }]
        });
        A.CallTo(() => users.GetByEmail(user.Email)).Returns(new Users.User { Email = user.Email, Role = UserRole.Employee });
        A.CallTo(() => groups.GetUserGroups(A<Users.User>._)).Returns([]);
        var permissions = new PermissionService(A.Fake<IApplicationConfigurationManager>(), spaces, users, groups);
        var service = new PageService(pages, spaces, A.Fake<IFeedManager>(), permissions, A.Fake<IDistributedCache>());
        var tools = CreateTools(service, permissions);

        var exception = await Assert.ThrowsAsync<McpException>(() => tools.CreatePage("DOCS", "home", "New page"));

        exception.Message.Should().StartWith("forbidden");
        A.CallTo(() => pages.GetById(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => pages.Create(A<string>._, A<string>._, A<string>._, A<UserInfo>._, A<string>._)).MustNotHaveHappened();
    }

    [Theory]
    [InlineData("", "home", "New page")]
    [InlineData("DOCS", "", "New page")]
    [InlineData("DOCS", "home", " ")]
    public async Task ShouldThrow_ValidationFailed_WhenCreatePageMissesRequiredValue(string spaceKey, string parentPageId, string name)
    {
        var pages = A.Fake<IPageService>();
        var tools = CreateTools(pages);

        var exception = await Assert.ThrowsAsync<McpException>(() => tools.CreatePage(spaceKey, parentPageId, name));

        exception.Message.Should().StartWith("validation_failed");
        A.CallTo(() => pages.Create(A<PageCreateModel>._, A<UserInfo>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_MapPageNotFound_WhenGetPageTreeInvoked()
    {
        var pages = A.Fake<IPageService>();
        A.CallTo(() => pages.GetPageTreeByPageId("missing", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Throws(new PageNotFoundException());
        var tools = CreateTools(pages);

        var exception = await Assert.ThrowsAsync<McpException>(() => tools.GetPageTree("missing"));

        exception.Message.Should().StartWith("page_not_found");
    }

    [Fact]
    public async Task Should_MapPageNotFound_WhenGetPageInvoked()
    {
        var pages = A.Fake<IPageService>();
        A.CallTo(() => pages.GetById("missing", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Throws(new PageNotFoundException());
        var tools = CreateTools(pages);

        var exception = await Assert.ThrowsAsync<McpException>(() => tools.GetPage("missing"));

        exception.Message.Should().StartWith("page_not_found");
    }

    [Fact]
    public async Task Should_MapVersionNotFound_WhenGetPageVersionInvoked()
    {
        var pages = A.Fake<IPageService>();
        A.CallTo(() => pages.GetVersion("page-1", 7, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Throws(new PageVersionNotFoundException());
        var tools = CreateTools(pages);

        var exception = await Assert.ThrowsAsync<McpException>(() => tools.GetPageVersion("page-1", 7));

        exception.Message.Should().StartWith("version_not_found");
    }

    [Fact]
    public async Task Should_ReturnValidationFailed_WhenUpdatePageNameIsEmpty()
    {
        var pages = A.Fake<IPageService>();
        var tools = CreateTools(pages);

        var exception = await Assert.ThrowsAsync<McpException>(() => tools.UpdatePage("page-1", "", "content"));

        exception.Message.Should().StartWith("validation_failed");
        A.CallTo(() => pages.Update(A<string>._, A<PageUpdateModel>._, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .MustNotHaveHappened();
    }

    private static PageTools CreateTools(IPageService pages, IPermissionService permissions = null)
    {
        var context = McpTestFactory.CreateContext();
        return new PageTools(pages, permissions ?? A.Fake<IPermissionService>(), context, McpTestFactory.CreateExecutor(context));
    }
}
