using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Integration.PageTemplates;
using Mimisbrunnr.Web.PageTemplates;

namespace Mimisbrunnr.Web.Tests.PageTemplates;

public class PageTemplateControllerTests
{
    [Fact]
    public async Task Should_ReturnTemplates_WhenFilteredBySpace()
    {
        var service = A.Fake<IPageTemplateService>();
        A.CallTo(() => service.GetAll("Space", "DOCS", null)).Returns(Task.FromResult<PageTemplateModel[]>([]));

        var result = await new PageTemplateController(service).GetAll("Space", "DOCS");

        result.Should().BeOfType<OkObjectResult>();
        A.CallTo(() => service.GetAll("Space", "DOCS", null)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ReturnOk_WhenTemplateIsUpdated()
    {
        var service = A.Fake<IPageTemplateService>();
        var model = new PageTemplateUpdateModel();

        var result = await new PageTemplateController(service).Update("template", model);

        result.Should().BeOfType<OkResult>();
        A.CallTo(() => service.Update("template", model, null)).MustHaveHappenedOnceExactly();
    }
}
