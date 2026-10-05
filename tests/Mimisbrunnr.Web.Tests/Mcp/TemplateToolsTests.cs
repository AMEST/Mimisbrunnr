using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Integration.PageTemplates;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Web.Mcp.Tools;
using Mimisbrunnr.Web.PageTemplates;
using Mimisbrunnr.Web.Wiki;

namespace Mimisbrunnr.Web.Tests.Mcp;

public class TemplateToolsTests
{
    [Fact]
    public async Task Should_RenderTemplateAndCreatePage_WhenCreatePageFromTemplateInvoked()
    {
        var templates = A.Fake<IPageTemplateService>();
        var pages = A.Fake<IPageService>();
        A.CallTo(() => templates.Render("tpl-1", "TEAM", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult(new PageTemplateRenderResponse { Content = "rendered content" }));
        A.CallTo(() => templates.GetById("tpl-1", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult(new PageTemplateModel { Id = "tpl-1", Name = "Onboarding" }));
        PageCreateModel captured = null;
        A.CallTo(() => pages.Create(A<PageCreateModel>._, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Invokes((PageCreateModel model, Mimisbrunnr.Wiki.Contracts.UserInfo _) => captured = model)
            .Returns(Task.FromResult(new PageModel { Id = "page-9", Name = "Onboarding" }));

        var tools = new TemplateTools(templates, pages, McpTestFactory.CreateContext(), McpTestFactory.CreateExecutor());

        var json = await tools.CreatePageFromTemplate("tpl-1", "TEAM", "parent-1");

        json.Should().Contain("page-9");
        captured.Should().NotBeNull();
        captured.Name.Should().Be("Onboarding");
        captured.Content.Should().Be("rendered content");
        captured.ParentPageId.Should().Be("parent-1");
    }

    [Fact]
    public async Task Should_UseProvidedName_WhenCreatePageFromTemplateInvoked()
    {
        var templates = A.Fake<IPageTemplateService>();
        var pages = A.Fake<IPageService>();
        A.CallTo(() => templates.Render("tpl-1", "TEAM", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult(new PageTemplateRenderResponse { Content = "x" }));
        PageCreateModel captured = null;
        A.CallTo(() => pages.Create(A<PageCreateModel>._, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Invokes((PageCreateModel model, Mimisbrunnr.Wiki.Contracts.UserInfo _) => captured = model)
            .Returns(Task.FromResult(new PageModel { Id = "page-9" }));

        var tools = new TemplateTools(templates, pages, McpTestFactory.CreateContext(), McpTestFactory.CreateExecutor());

        await tools.CreatePageFromTemplate("tpl-1", "TEAM", "parent-1", name: "Custom");

        captured.Name.Should().Be("Custom");
        A.CallTo(() => templates.GetById(A<string>._, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .MustNotHaveHappened();
    }
}
