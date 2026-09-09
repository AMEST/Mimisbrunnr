using FakeItEasy;
using FluentAssertions;
using System.Linq.Expressions;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Web.Tests.Managers;

public class PageManagerTests
{
    public PageManagerTests() => QueryableAsyncExtensions.EnableFallback();

    [Fact]
    public async Task Should_ReturnOnlyPagesInSpace_WhenGettingSpacePages()
    {
        var repository = A.Fake<IRepository<Page>>();
        A.CallTo(() => repository.GetAll()).Returns(new[]
        {
            new Page { Id = "1", SpaceId = "space" },
            new Page { Id = "2", SpaceId = "other" }
        }.AsQueryable());
        var manager = new PageManager(repository, A.Fake<IRepository<HistoricalPage>>(), A.Fake<IAttachmentManager>(), A.Fake<IDraftManager>(), A.Fake<ICommentManager>(), A.Fake<IPluginManager>());

        var result = await manager.GetAllOnSpace(new Space { Id = "space" });

        result.Should().ContainSingle().Which.Id.Should().Be("1");
    }

    [Fact]
    public async Task Should_SetAuditFields_WhenCreatingPage()
    {
        var repository = A.Fake<IRepository<Page>>();
        var manager = new PageManager(repository, A.Fake<IRepository<HistoricalPage>>(), A.Fake<IAttachmentManager>(), A.Fake<IDraftManager>(), A.Fake<ICommentManager>(), A.Fake<IPluginManager>());
        var author = new UserInfo { Email = "user@example.test" };

        var page = await manager.Create("space", "Title", "Content", author, "parent");

        page.SpaceId.Should().Be("space");
        page.ParentId.Should().Be("parent");
        page.CreatedBy.Should().BeSameAs(author);
        A.CallTo(() => repository.Create(page, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ReturnPage_WhenGettingById()
    {
        var repository = A.Fake<IRepository<Page>>();
        A.CallTo(() => repository.GetAll()).Returns(new[]
        {
            new Page { Id = "p1", Name = "One" },
            new Page { Id = "p2", Name = "Two" }
        }.AsQueryable());
        var manager = new PageManager(repository, A.Fake<IRepository<HistoricalPage>>(), A.Fake<IAttachmentManager>(), A.Fake<IDraftManager>(), A.Fake<ICommentManager>(), A.Fake<IPluginManager>());

        var result = await manager.GetById("p2");

        result.Should().NotBeNull();
        result.Name.Should().Be("Two");
    }

    [Fact]
    public async Task Should_ReturnNull_WhenPageNotFoundById()
    {
        var repository = A.Fake<IRepository<Page>>();
        A.CallTo(() => repository.GetAll()).Returns(Array.Empty<Page>().AsQueryable());
        var manager = new PageManager(repository, A.Fake<IRepository<HistoricalPage>>(), A.Fake<IAttachmentManager>(), A.Fake<IDraftManager>(), A.Fake<ICommentManager>(), A.Fake<IPluginManager>());

        var result = await manager.GetById("missing");

        result.Should().BeNull();
    }

    [Fact]
    public async Task Should_ReturnOnlyMatchingPages_WhenFindingByName()
    {
        var repository = A.Fake<IRepository<Page>>();
        A.CallTo(() => repository.GetAll()).Returns(new[]
        {
            new Page { Id = "p1", Name = "Meeting notes" },
            new Page { Id = "p2", Name = "Other" }
        }.AsQueryable());
        var manager = new PageManager(repository, A.Fake<IRepository<HistoricalPage>>(), A.Fake<IAttachmentManager>(), A.Fake<IDraftManager>(), A.Fake<ICommentManager>(), A.Fake<IPluginManager>());

        var result = await manager.FindByName("eting");

        result.Should().ContainSingle().Which.Id.Should().Be("p1");
    }

    [Fact]
    public async Task Should_IncludeChildsAtAllLevels_WhenGettingChilds()
    {
        var repository = A.Fake<IRepository<Page>>();
        A.CallTo(() => repository.GetAll()).Returns(new[]
        {
            new Page { Id = "p1", ParentId = null },
            new Page { Id = "p2", ParentId = "p1" },
            new Page { Id = "p3", ParentId = "p2" },
            new Page { Id = "p4", ParentId = "other" }
        }.AsQueryable());
        var manager = new PageManager(repository, A.Fake<IRepository<HistoricalPage>>(), A.Fake<IAttachmentManager>(), A.Fake<IDraftManager>(), A.Fake<ICommentManager>(), A.Fake<IPluginManager>());

        var result = await manager.GetAllChilds(new Page { Id = "p1" });

        result.Should().Contain(x => x.Id == "p2").And.Contain(x => x.Id == "p3");
        result.Should().NotContain(x => x.Id == "p4");
    }

    [Fact]
    public async Task Should_ReturnEmpty_WhenPageHasNoChilds()
    {
        var repository = A.Fake<IRepository<Page>>();
        A.CallTo(() => repository.GetAll()).Returns(Array.Empty<Page>().AsQueryable());
        var manager = new PageManager(repository, A.Fake<IRepository<HistoricalPage>>(), A.Fake<IAttachmentManager>(), A.Fake<IDraftManager>(), A.Fake<ICommentManager>(), A.Fake<IPluginManager>());

        var result = await manager.GetAllChilds(new Page { Id = "p1" });

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_SaveCurrentVersionAndIncrement_WhenUpdatingPage()
    {
        var repository = A.Fake<IRepository<Page>>();
        var historical = A.Fake<IRepository<HistoricalPage>>();
        var drafts = A.Fake<IDraftManager>();
        var page = new Page { Id = "p1", Name = "Old", Content = "Old content", Version = 3, UpdatedBy = A.Dummy<UserInfo>(), Updated = DateTime.UtcNow };
        A.CallTo(() => repository.GetAll()).Returns(new[] { page }.AsQueryable());
        var manager = new PageManager(repository, historical, A.Fake<IAttachmentManager>(), drafts, A.Fake<ICommentManager>(), A.Fake<IPluginManager>());
        var user = new UserInfo { Email = "editor@test.com" };

        await manager.Update(page, user);

        page.Version.Should().Be(4);
        page.UpdatedBy.Should().BeSameAs(user);
        A.CallTo(() => historical.Create(A<HistoricalPage>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => drafts.Remove("p1")).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_CreateCopyInDestination_WhenCopyingPage()
    {
        var repository = A.Fake<IRepository<Page>>();
        var source = new Page { Id = "p1", SpaceId = "s1", ParentId = "parent1", Name = "Source", Content = "Body" };
        var destination = new Page { Id = "p2", SpaceId = "s2" };
        var manager = new PageManager(repository, A.Fake<IRepository<HistoricalPage>>(), A.Fake<IAttachmentManager>(), A.Fake<IDraftManager>(), A.Fake<ICommentManager>(), A.Fake<IPluginManager>());

        var result = await manager.Copy(source, destination);

        result.SpaceId.Should().Be("s2");
        result.ParentId.Should().Be("p2");
        result.Id.Should().NotBe("p1");
        A.CallTo(() => repository.Create(A<Page>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_MovePageWithChildsToDestination_WhenMovingPage()
    {
        var repository = A.Fake<IRepository<Page>>();
        var source = new Page { Id = "p1", SpaceId = "s1", ParentId = "old" };
        var destination = new Page { Id = "p2", SpaceId = "s2" };
        A.CallTo(() => repository.GetAll()).Returns(new[]
        {
            source,
            new Page { Id = "child", SpaceId = "s1", ParentId = "p1" }
        }.AsQueryable());
        var manager = new PageManager(repository, A.Fake<IRepository<HistoricalPage>>(), A.Fake<IAttachmentManager>(), A.Fake<IDraftManager>(), A.Fake<ICommentManager>(), A.Fake<IPluginManager>());

        var result = await manager.Move(source, destination);

        result.SpaceId.Should().Be("s2");
        result.ParentId.Should().Be("p2");
        A.CallTo(() => repository.Update(A<Page>._, A<CancellationToken>._)).MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_ReparentFirstLevelChildsToOriginalParent_WhenMovingWithoutChilds()
    {
        var repository = A.Fake<IRepository<Page>>();
        var source = new Page { Id = "p1", SpaceId = "s1", ParentId = "old" };
        var firstLevel = new Page { Id = "c1", SpaceId = "s1", ParentId = "p1" };
        var nested = new Page { Id = "c2", SpaceId = "s1", ParentId = "c1" };
        A.CallTo(() => repository.GetAll()).Returns(new[] { source, firstLevel, nested }.AsQueryable());
        var manager = new PageManager(repository, A.Fake<IRepository<HistoricalPage>>(), A.Fake<IAttachmentManager>(), A.Fake<IDraftManager>(), A.Fake<ICommentManager>(), A.Fake<IPluginManager>());
        var destination = new Page { Id = "p2", SpaceId = "s2" };

        await manager.Move(source, destination, withChilds: false);

        firstLevel.ParentId.Should().Be("old");
        firstLevel.SpaceId.Should().Be("s1");
        nested.ParentId.Should().Be("c1");
    }

    [Fact]
    public async Task Should_RemovePageAndReparentChilds_WhenRemovingSinglePage()
    {
        var repository = A.Fake<IRepository<Page>>();
        var attachments = A.Fake<IAttachmentManager>();
        var historical = A.Fake<IRepository<HistoricalPage>>();
        var drafts = A.Fake<IDraftManager>();
        var comments = A.Fake<ICommentManager>();
        var plugins = A.Fake<IPluginManager>();
        var page = new Page { Id = "p1", ParentId = "root" };
        var child = new Page { Id = "c1", ParentId = "p1" };
        A.CallTo(() => repository.GetAll()).Returns(new[] { page, child }.AsQueryable());
        var manager = new PageManager(repository, historical, attachments, drafts, comments, plugins);

        await manager.Remove(page, deleteChild: false);

        child.ParentId.Should().Be("root");
        A.CallTo(() => repository.Delete(page, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => attachments.RemoveAll(page)).MustHaveHappenedOnceExactly();
        A.CallTo(() => drafts.Remove("p1")).MustHaveHappenedOnceExactly();
        A.CallTo(() => comments.RemoveAll(page)).MustHaveHappenedOnceExactly();
        A.CallTo(() => plugins.DeleteAllStateInPage("p1")).MustHaveHappenedOnceExactly();
        A.CallTo(() => historical.DeleteAll(A<Expression<Func<HistoricalPage, bool>>>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_RemoveAllChildsAndPage_WhenRemovingRecursively()
    {
        var repository = A.Fake<IRepository<Page>>();
        var historical = A.Fake<IRepository<HistoricalPage>>();
        var page = new Page { Id = "p1", ParentId = null };
        var child = new Page { Id = "c1", ParentId = "p1" };
        A.CallTo(() => repository.GetAll()).Returns(new[] { page, child }.AsQueryable());
        var manager = new PageManager(repository, historical, A.Fake<IAttachmentManager>(), A.Fake<IDraftManager>(), A.Fake<ICommentManager>(), A.Fake<IPluginManager>());

        await manager.Remove(page, deleteChild: true);

        A.CallTo(() => repository.Delete(A<Page>._, A<CancellationToken>._)).MustHaveHappenedTwiceExactly();
        A.CallTo(() => historical.DeleteAll(A<Expression<Func<HistoricalPage, bool>>>._, A<CancellationToken>._)).MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_ReturnAllVersionsOfPage_WhenGettingAllVersions()
    {
        var historical = A.Fake<IRepository<HistoricalPage>>();
        A.CallTo(() => historical.GetAll()).Returns(new[]
        {
            new HistoricalPage { Id = "h1", PageId = "p1", Version = 1 },
            new HistoricalPage { Id = "h2", PageId = "p1", Version = 2 },
            new HistoricalPage { Id = "h3", PageId = "other", Version = 1 }
        }.AsQueryable());
        var manager = new PageManager(A.Fake<IRepository<Page>>(), historical, A.Fake<IAttachmentManager>(), A.Fake<IDraftManager>(), A.Fake<ICommentManager>(), A.Fake<IPluginManager>());

        var result = await manager.GetAllVersions(new Page { Id = "p1" });

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Should_ReturnVersion_WhenMatchingPageIdAndVersion()
    {
        var historical = A.Fake<IRepository<HistoricalPage>>();
        A.CallTo(() => historical.GetAll()).Returns(new[]
        {
            new HistoricalPage { PageId = "p1", Version = 2, Name = "v2" },
            new HistoricalPage { PageId = "p1", Version = 3, Name = "v3" }
        }.AsQueryable());
        var manager = new PageManager(A.Fake<IRepository<Page>>(), historical, A.Fake<IAttachmentManager>(), A.Fake<IDraftManager>(), A.Fake<ICommentManager>(), A.Fake<IPluginManager>());

        var result = await manager.GetVersionByPageId("p1", 3);

        result.Should().NotBeNull();
        result.Name.Should().Be("v3");
    }

    [Fact]
    public async Task Should_RestoreNameAndContent_WhenRestoringVersion()
    {
        var historical = A.Fake<IRepository<HistoricalPage>>();
        var repository = A.Fake<IRepository<Page>>();
        var page = new Page { Id = "p1", Name = "Current", Content = "Current content", Version = 5 };
        A.CallTo(() => repository.GetAll()).Returns(new[] { page }.AsQueryable());
        A.CallTo(() => historical.GetAll()).Returns(new[]
        {
            new HistoricalPage { PageId = "p1", Version = 2, Name = "Old", Content = "Old content" }
        }.AsQueryable());
        var manager = new PageManager(repository, historical, A.Fake<IAttachmentManager>(), A.Fake<IDraftManager>(), A.Fake<ICommentManager>(), A.Fake<IPluginManager>());
        var user = new UserInfo { Email = "admin@test.com" };

        await manager.RestoreVersion(page, 2, user);

        page.Name.Should().Be("Old");
        page.Content.Should().Be("Old content");
        A.CallTo(() => historical.Create(A<HistoricalPage>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_DeleteVersion_WhenRemovingExistingVersion()
    {
        var historical = A.Fake<IRepository<HistoricalPage>>();
        var version = new HistoricalPage { PageId = "p1", Version = 1 };
        A.CallTo(() => historical.GetAll()).Returns(new[] { version }.AsQueryable());
        var manager = new PageManager(A.Fake<IRepository<Page>>(), historical, A.Fake<IAttachmentManager>(), A.Fake<IDraftManager>(), A.Fake<ICommentManager>(), A.Fake<IPluginManager>());

        await manager.RemoveVersion(new Page { Id = "p1" }, 1);

        A.CallTo(() => historical.Delete(version, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_NotDeleteAnything_WhenRemovingNonexistentVersion()
    {
        var historical = A.Fake<IRepository<HistoricalPage>>();
        A.CallTo(() => historical.GetAll()).Returns(Array.Empty<HistoricalPage>().AsQueryable());
        var manager = new PageManager(A.Fake<IRepository<Page>>(), historical, A.Fake<IAttachmentManager>(), A.Fake<IDraftManager>(), A.Fake<ICommentManager>(), A.Fake<IPluginManager>());

        await manager.RemoveVersion(new Page { Id = "p1" }, 99);

        A.CallTo(() => historical.Delete(A<HistoricalPage>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

}
