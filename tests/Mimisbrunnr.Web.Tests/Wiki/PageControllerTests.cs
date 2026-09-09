using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Wiki;
using Mimisbrunnr.Integration.Wiki;

namespace Mimisbrunnr.Web.Tests.Wiki;

public class PageControllerTests
{
    [Fact]
    public async Task Should_UseRecursiveMoveByDefault_WhenMoveIsCalled()
    {
        var service = A.Fake<IPageService>();
        A.CallTo(() => service.Move("source", "destination", true, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult<PageModel>(null));

        var result = await new PageController(service).Move("source", "destination");

        result.Should().BeOfType<OkObjectResult>();
        A.CallTo(() => service.Move("source", "destination", true, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_UseProvidedRecursiveFlag_WhenMoveFlagIsExplicit()
    {
        var service = A.Fake<IPageService>();
        A.CallTo(() => service.Move("source", "destination", false, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult<PageModel>(null));

        var result = await new PageController(service).Move("source", "destination", false);

        result.Should().BeOfType<OkObjectResult>();
        A.CallTo(() => service.Move("source", "destination", false, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ReturnPage_WhenGettingById()
    {
        var service = A.Fake<IPageService>();
        A.CallTo(() => service.GetById("page", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult(new PageModel { Id = "page" }));

        var result = await new PageController(service).Get("page");

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeOfType<PageModel>();
    }

    [Fact]
    public async Task Should_ReturnTree_WhenGettingTree()
    {
        var service = A.Fake<IPageService>();
        A.CallTo(() => service.GetPageTreeByPageId("page", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult(new PageTreeModel()));

        var result = await new PageController(service).GetTree("page");

        result.Should().BeOfType<OkObjectResult>();
        A.CallTo(() => service.GetPageTreeByPageId("page", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ReturnVersions_WhenGettingVersions()
    {
        var service = A.Fake<IPageService>();
        A.CallTo(() => service.GetPageVersions("page", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult(new PageVersionsListModel()));

        var result = await new PageController(service).GetVersions("page");

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Should_ReturnCreatedPage_WhenCreating()
    {
        var service = A.Fake<IPageService>();
        var model = new PageCreateModel();
        A.CallTo(() => service.Create(model, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult(new PageModel()));

        var result = await new PageController(service).Create(model);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Should_ReturnOk_WhenUpdating()
    {
        var service = A.Fake<IPageService>();
        var model = new PageUpdateModel();

        var result = await new PageController(service).Update("page", model);

        result.Should().BeOfType<OkResult>();
        A.CallTo(() => service.Update("page", model, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ReturnOk_WhenRestoringVersion()
    {
        var service = A.Fake<IPageService>();
        A.CallTo(() => service.RestoreVersion("page", 3, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult(new PageModel()));

        var result = await new PageController(service).RestoreVersion("page", 3);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Should_NotDeleteRecursively_WhenFlagIsFalse()
    {
        var service = A.Fake<IPageService>();

        var result = await new PageController(service).Delete("page", false);

        result.Should().BeOfType<OkResult>();
        A.CallTo(() => service.Delete("page", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._, false)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_DeleteRecursively_WhenFlagIsTrue()
    {
        var service = A.Fake<IPageService>();

        var result = await new PageController(service).Delete("page", true);

        result.Should().BeOfType<OkResult>();
        A.CallTo(() => service.Delete("page", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._, true)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ReturnOk_WhenDeletingVersion()
    {
        var service = A.Fake<IPageService>();

        var result = await new PageController(service).DeleteVersion("page", 2);

        result.Should().BeOfType<OkResult>();
        A.CallTo(() => service.DeleteVersion("page", 2, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ReturnCopiedPage_WhenCopying()
    {
        var service = A.Fake<IPageService>();
        A.CallTo(() => service.Copy("source", "destination", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult(new PageModel { Id = "copied" }));

        var result = await new PageController(service).Copy("source", "destination");

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeOfType<PageModel>();
    }
}