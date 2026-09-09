using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Wiki;
using Mimisbrunnr.Integration.Wiki;

namespace Mimisbrunnr.Web.Tests.Wiki;

public class CommentControllerTests
{
    [Fact]
    public async Task Should_ReturnEmptyCollection_WhenCommentsAreMissing()
    {
        var service = A.Fake<ICommentService>();
        A.CallTo(() => service.GetComments("page", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult<IEnumerable<CommentModel>>(null));

        var result = await new CommentController(service).GetAll("page");

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeAssignableTo<CommentModel[]>();
    }

    [Fact]
    public async Task Should_ReturnComments_WhenGettingAll()
    {
        var service = A.Fake<ICommentService>();
        A.CallTo(() => service.GetComments("page", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult<IEnumerable<CommentModel>>(new[] { new CommentModel { Id = "c1" } }));
        var result = await new CommentController(service).GetAll("page");

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeOfType<CommentModel[]>();
    }

    [Fact]
    public async Task Should_ReturnComment_WhenGettingById()
    {
        var service = A.Fake<ICommentService>();
        A.CallTo(() => service.GetById("page", "comment", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult(new CommentModel { Id = "comment" }));

        var result = await new CommentController(service).Get("page", "comment");

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeOfType<CommentModel>();
    }

    [Fact]
    public async Task Should_ReturnCreatedComment_WhenCreating()
    {
        var service = A.Fake<ICommentService>();
        var model = new CommentCreateModel { Message = "Hello" };
        A.CallTo(() => service.Create("page", model, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult(new CommentModel { Id = "c1" }));

        var result = await new CommentController(service).Create("page", model);

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeOfType<CommentModel>();
    }

    [Fact]
    public async Task Should_ReturnBadRequest_WhenCreatingNullAuthorComment()
    {
        var service = A.Fake<ICommentService>();
        A.CallTo(() => service.Create("page", A<CommentCreateModel>._, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .ThrowsAsync(new ArgumentNullException());

        var result = await new CommentController(service).Create("page", new CommentCreateModel());

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Should_ReturnOk_WhenRemovingComment()
    {
        var service = A.Fake<ICommentService>();

        var result = await new CommentController(service).Remove("page", "comment");

        result.Should().BeOfType<OkResult>();
        A.CallTo(() => service.Remove("page", "comment", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ReturnBadRequest_WhenRemovingNullAuthorComment()
    {
        var service = A.Fake<ICommentService>();
        A.CallTo(() => service.Remove("page", "comment", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .ThrowsAsync(new ArgumentNullException());

        var result = await new CommentController(service).Remove("page", "comment");

        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
