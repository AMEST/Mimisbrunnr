using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Web.Mcp.Tools;
using Mimisbrunnr.Web.Wiki;
using ModelContextProtocol;

namespace Mimisbrunnr.Web.Tests.Mcp;

public class CommentToolsTests
{
    [Fact]
    public async Task Should_CreateComment_WhenMessageProvided()
    {
        var comments = A.Fake<ICommentService>();
        A.CallTo(() => comments.Create("page-1", A<CommentCreateModel>._, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult(new CommentModel { Id = "c1", Message = "hello" }));
        var tools = new CommentTools(comments, McpTestFactory.CreateContext(), McpTestFactory.CreateExecutor());

        var json = await tools.CreateComment("page-1", "hello");

        json.Should().Contain("hello");
        A.CallTo(() => comments.Create("page-1",
                A<CommentCreateModel>.That.Matches(m => m.Message == "hello"),
                A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ReturnValidationFailed_WhenCommentMessageIsEmpty()
    {
        var comments = A.Fake<ICommentService>();
        var tools = new CommentTools(comments, McpTestFactory.CreateContext(), McpTestFactory.CreateExecutor());

        var exception = await Assert.ThrowsAsync<McpException>(() => tools.CreateComment("page-1", ""));

        exception.Message.Should().StartWith("validation_failed");
    }

    [Fact]
    public async Task Should_DeleteComment_WhenIdsProvided()
    {
        var comments = A.Fake<ICommentService>();
        var tools = new CommentTools(comments, McpTestFactory.CreateContext(), McpTestFactory.CreateExecutor());

        var result = await tools.DeleteComment("page-1", "c1");

        result.Should().Contain("ok");
        A.CallTo(() => comments.Remove("page-1", "c1", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .MustHaveHappenedOnceExactly();
    }
}
