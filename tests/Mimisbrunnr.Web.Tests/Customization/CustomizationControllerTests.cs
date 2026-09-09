using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Customization;

namespace Mimisbrunnr.Web.Tests.Customization;

public class CustomizationControllerTests
{
    [Fact]
    public async Task Should_ReturnCssFile_WhenCustomCssExists()
    {
        var service = A.Fake<ICustomizationService>();
        A.CallTo(() => service.GetCustomCss()).Returns(Task.FromResult("body { color: red; }"));

        var result = await new CustomizationController(service).GetCustomCss();

        result.Should().BeOfType<FileContentResult>().Which.ContentType.Should().Be("text/css");
    }

    [Fact]
    public async Task Should_ReturnEmptyCssFile_WhenCustomCssIsMissing()
    {
        var service = A.Fake<ICustomizationService>();
        A.CallTo(() => service.GetCustomCss()).Returns(Task.FromResult<string>(null));

        var result = await new CustomizationController(service).GetCustomCss();

        result.Should().BeOfType<FileContentResult>().Which.FileContents.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_ReturnCustomHomepage_WhenRequested()
    {
        var service = A.Fake<ICustomizationService>();
        A.CallTo(() => service.GetCustomHomepage(A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult(new Mimisbrunnr.Web.Customization.CustomHomepageModel()));

        var result = await new CustomizationController(service).GetCustomHomepage();

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeOfType<Mimisbrunnr.Web.Customization.CustomHomepageModel>();
    }
}
