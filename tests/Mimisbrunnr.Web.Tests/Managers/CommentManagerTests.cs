using FakeItEasy;
using FluentAssertions;
using System.Linq.Expressions;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Web.Tests.Managers;

public class CommentManagerTests
{
    public CommentManagerTests() => QueryableAsyncExtensions.EnableFallback();

    [Fact]
    public async Task Should_ReturnOnlyPageComments_WhenGettingComments()
    {
        var repository = A.Fake<IRepository<Comment>>();
        A.CallTo(() => repository.GetAll()).Returns(new[]
        {
            new Comment { Id = "1", PageId = "page" },
            new Comment { Id = "2", PageId = "other" }
        }.AsQueryable());

        var result = await new CommentManager(repository).GetComments(new Page { Id = "page" });

        result.Should().ContainSingle().Which.Id.Should().Be("1");
    }

    [Fact]
    public async Task Should_EncodeHtml_WhenCreatingComment()
    {
        var repository = A.Fake<IRepository<Comment>>();
        var manager = new CommentManager(repository);

        var comment = await manager.Create(new Page { Id = "page" }, "<script>alert(1)</script>", new UserInfo());

        comment.Message.Should().Be("&lt;script&gt;alert(1)&lt;/script&gt;");
        A.CallTo(() => repository.Create(comment, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ReturnComment_WhenGettingById()
    {
        var repository = A.Fake<IRepository<Comment>>();
        var comment = new Comment { Id = "c1", PageId = "page" };
        A.CallTo(() => repository.GetAll()).Returns(new[] { comment }.AsQueryable());

        var result = await new CommentManager(repository).GetById("c1");

        result.Should().BeSameAs(comment);
    }

    [Fact]
    public async Task Should_ReturnNull_WhenCommentNotFoundById()
    {
        var repository = A.Fake<IRepository<Comment>>();
        A.CallTo(() => repository.GetAll()).Returns(Array.Empty<Comment>().AsQueryable());

        var result = await new CommentManager(repository).GetById("missing");

        result.Should().BeNull();
    }

    [Fact]
    public async Task Should_DeleteComment_WhenRemoving()
    {
        var repository = A.Fake<IRepository<Comment>>();
        var comment = new Comment { Id = "c1" };

        await new CommentManager(repository).Remove(comment);

        A.CallTo(() => repository.Delete(comment, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_DeleteAllPageComments_WhenRemovingAll()
    {
        var repository = A.Fake<IRepository<Comment>>();

        await new CommentManager(repository).RemoveAll(new Page { Id = "page" });

        A.CallTo(() => repository.DeleteAll(A<Expression<Func<Comment, bool>>>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

}
