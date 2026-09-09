using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Wiki;
using Mimisbrunnr.Integration.Wiki;

namespace Mimisbrunnr.Web.Tests.Wiki;

public class DraftControllerTests
{
    [Fact]
    public async Task Should_ReturnNotFound_WhenDraftDoesNotExist()
    {
        var service = A.Fake<IDraftService>();
        A.CallTo(() => service.GetByPageId("page", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult<DraftModel>(null));

        var result = await new DraftController(service).Get("page");

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Should_ReturnDraft_WhenDraftExists()
    {
        var service = A.Fake<IDraftService>();
        A.CallTo(() => service.GetByPageId("page", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult(new DraftModel { Name = "Draft" }));

        var result = await new DraftController(service).Get("page");

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeOfType<DraftModel>();
    }

    [Fact]
    public async Task Should_UpdateOrCreateDraft_WhenCalled()
    {
        var service = A.Fake<IDraftService>();
        var model = new DraftUpdateModel();

        var result = await new DraftController(service).UpdateOrCreate("page", model);

        result.Should().BeOfType<OkResult>();
        A.CallTo(() => service.Update("page", model, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_DeleteDraft_WhenCalled()
    {
        var service = A.Fake<IDraftService>();

        var result = await new DraftController(service).Delete("page");

        result.Should().BeOfType<OkResult>();
        A.CallTo(() => service.Delete("page", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._)).MustHaveHappenedOnceExactly();
    }
}