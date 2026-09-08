using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Web.Tests.Managers;

public class CommentAndDraftManagerTests
{
    public CommentAndDraftManagerTests() => QueryableAsyncExtensions.EnableFallback();

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
    public async Task Should_RefreshAuditFields_WhenUpdatingDraft()
    {
        var repository = A.Fake<IRepository<Draft>>();
        var manager = new DraftManager(repository);
        var user = new UserInfo { Email = "user@example.test" };
        var draft = new Draft { Updated = DateTime.UtcNow.AddDays(-1) };

        await manager.Update(draft, user);

        draft.UpdatedBy.Should().BeSameAs(user);
        draft.Updated.Should().BeAfter(DateTime.UtcNow.AddMinutes(-1));
        A.CallTo(() => repository.Update(draft, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }
}
