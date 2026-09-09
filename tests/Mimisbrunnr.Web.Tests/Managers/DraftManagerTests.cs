using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Web.Tests.Managers;

public class DraftManagerTests
{
    [Fact]
    public async Task Should_CreateDraftWithContent_WhenCreatingDraft()
    {
        var repository = A.Fake<IRepository<Draft>>();
        var user = new UserInfo { Email = "user@example.test" };

        var draft = await new DraftManager(repository).Create("page", "Title", "Content", user);

        draft.OriginalPageId.Should().Be("page");
        draft.Content.Should().Be("Content");
        draft.UpdatedBy.Should().BeSameAs(user);
        A.CallTo(() => repository.Create(draft, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_RefreshAuditFields_WhenUpdatingDraft()
    {
        var repository = A.Fake<IRepository<Draft>>();
        var user = new UserInfo { Email = "user@example.test" };
        var draft = new Draft { Updated = DateTime.UtcNow.AddDays(-1) };

        await new DraftManager(repository).Update(draft, user);

        draft.UpdatedBy.Should().BeSameAs(user);
        draft.Updated.Should().BeAfter(DateTime.UtcNow.AddMinutes(-1));
        A.CallTo(() => repository.Update(draft, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ReturnDraft_WhenGettingByPageId()
    {
        var repository = A.Fake<IRepository<Draft>>();
        var draft = new Draft { OriginalPageId = "page", Name = "Title" };
        A.CallTo(() => repository.GetAll()).Returns(new[] { draft }.AsQueryable());

        var result = await new DraftManager(repository).GetByPageId("page");

        result.Should().BeSameAs(draft);
    }

    [Fact]
    public async Task Should_ReturnNull_WhenNoDraftForPage()
    {
        var repository = A.Fake<IRepository<Draft>>();
        A.CallTo(() => repository.GetAll()).Returns(Array.Empty<Draft>().AsQueryable());

        var result = await new DraftManager(repository).GetByPageId("missing");

        result.Should().BeNull();
    }

    [Fact]
    public async Task Should_DeleteDraft_WhenRemovingByDraft()
    {
        var repository = A.Fake<IRepository<Draft>>();
        var draft = new Draft { OriginalPageId = "page" };

        await new DraftManager(repository).Remove(draft);

        A.CallTo(() => repository.Delete(draft, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_DeleteDraft_WhenRemovingByPageId()
    {
        var repository = A.Fake<IRepository<Draft>>();
        var draft = new Draft { OriginalPageId = "page" };
        A.CallTo(() => repository.GetAll()).Returns(new[] { draft }.AsQueryable());

        await new DraftManager(repository).Remove("page");

        A.CallTo(() => repository.Delete(draft, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_NotDeleteAnything_WhenRemovingByUnknownPageId()
    {
        var repository = A.Fake<IRepository<Draft>>();
        A.CallTo(() => repository.GetAll()).Returns(Array.Empty<Draft>().AsQueryable());

        await new DraftManager(repository).Remove("missing");

        A.CallTo(() => repository.Delete(A<Draft>._, A<CancellationToken>._)).MustNotHaveHappened();
    }
}
