using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Web.Wiki;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

namespace Mimisbrunnr.Web.Tests.Wiki;

public class PageServiceTests
{
    [Fact]
    public async Task ShouldThrow_UserHasNotPermissionException_WhenUnauthorizedUserReadsCachedTree()
    {
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        var permissions = A.Fake<IPermissionService>();
        var cache = A.Fake<IDistributedCache>();
        var requestedBy = new UserInfo { Email = "unauthorized@example.test" };
        A.CallTo(() => pages.GetById("page")).Returns(Task.FromResult(new Page { Id = "page", SpaceId = "private-space" }));
        A.CallTo(() => spaces.GetById("private-space")).Returns(Task.FromResult(new Space { Id = "private-space", Key = "PRIVATE" }));
        A.CallTo(() => permissions.EnsureViewPermission("PRIVATE", requestedBy))
            .ThrowsAsync(new UserHasNotPermissionException());
        var service = new PageService(pages, spaces, A.Fake<IFeedManager>(), permissions, cache);

        await service.Invoking(x => x.GetPageTreeByPageId("page", requestedBy))
            .Should().ThrowAsync<UserHasNotPermissionException>();

        A.CallTo(() => cache.GetAsync(A<string>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_ReturnPageModel_WhenGettingPageById()
    {
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        var permissions = A.Fake<IPermissionService>();
        var user = new UserInfo { Email = "user@example.test" };
        var page = new Page { Id = "page", SpaceId = "space", Name = "Title" };
        A.CallTo(() => pages.GetById("page")).Returns(Task.FromResult(page));
        A.CallTo(() => spaces.GetById("space")).Returns(Task.FromResult(new Space { Id = "space", Key = "DOCS" }));
        var service = new PageService(pages, spaces, A.Fake<IFeedManager>(), permissions, A.Fake<IDistributedCache>());

        var result = await service.GetById("page", user);

        result.Id.Should().Be("page");
        result.SpaceKey.Should().Be("DOCS");
        result.Name.Should().Be("Title");
        A.CallTo(() => permissions.EnsureAnonymousAllowed(user)).MustHaveHappenedOnceExactly();
        A.CallTo(() => permissions.EnsureViewPermission("DOCS", user)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ShouldThrow_PageNotFoundException_WhenGettingMissingPage()
    {
        var pages = A.Fake<IPageManager>();
        A.CallTo(() => pages.GetById("missing")).Returns(Task.FromResult<Page>(null));
        var service = new PageService(pages, A.Fake<ISpaceManager>(), A.Fake<IFeedManager>(), A.Fake<IPermissionService>(), A.Fake<IDistributedCache>());

        await service.Invoking(x => x.GetById("missing", new UserInfo { Email = "user@example.test" }))
            .Should().ThrowAsync<PageNotFoundException>();
    }

    [Fact]
    public async Task Should_ReturnCachedTree_WhenTreeIsCached()
    {
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        var cache = A.Fake<IDistributedCache>();
        var page = new Page { Id = "page", SpaceId = "space" };
        A.CallTo(() => pages.GetById("page")).Returns(Task.FromResult(page));
        A.CallTo(() => spaces.GetById("space")).Returns(Task.FromResult(new Space { Id = "space", Key = "DOCS" }));
        var cachedTree = new Mimisbrunnr.Integration.Wiki.PageTreeModel { Page = new Mimisbrunnr.Integration.Wiki.PageModel { Id = "page" }, Childs = [] };
        A.CallTo(() => cache.GetAsync("page_tree_page", A<CancellationToken>._)).Returns(Task.FromResult(System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(cachedTree))));
        var service = new PageService(pages, spaces, A.Fake<IFeedManager>(), A.Fake<IPermissionService>(), cache);

        var result = await service.GetPageTreeByPageId("page", new UserInfo { Email = "user@example.test" });

        result.Page.Id.Should().Be("page");
        A.CallTo(() => pages.GetAllChilds(A<Page>._, true)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_BuildAndCacheTree_WhenTreeIsNotCached()
    {
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        var cache = A.Fake<IDistributedCache>();
        var page = new Page { Id = "page", SpaceId = "space" };
        A.CallTo(() => pages.GetById("page")).Returns(Task.FromResult(page));
        A.CallTo(() => spaces.GetById("space")).Returns(Task.FromResult(new Space { Id = "space", Key = "DOCS" }));
        A.CallTo(() => pages.GetAllChilds(page, true)).Returns(Task.FromResult(new[] { new Page { Id = "child", SpaceId = "space", ParentId = "page" } }));
        var service = new PageService(pages, spaces, A.Fake<IFeedManager>(), A.Fake<IPermissionService>(), cache);

        var result = await service.GetPageTreeByPageId("page", new UserInfo { Email = "user@example.test" });

        result.Childs.Should().ContainSingle().Which.Page.Id.Should().Be("child");
        A.CallTo(() => cache.SetAsync("page_tree_page", A<byte[]>._, A<DistributedCacheEntryOptions>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ShouldThrow_SpaceNotFoundException_WhenSpaceMissingForPageTree()
    {
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        A.CallTo(() => pages.GetById("page")).Returns(Task.FromResult(new Page { Id = "page", SpaceId = "space" }));
        A.CallTo(() => spaces.GetById("space")).Returns(Task.FromResult<Space>(null));
        var service = new PageService(pages, spaces, A.Fake<IFeedManager>(), A.Fake<IPermissionService>(), A.Fake<IDistributedCache>());

        await service.Invoking(x => x.GetPageTreeByPageId("page", new UserInfo { Email = "user@example.test" }))
            .Should().ThrowAsync<SpaceNotFoundException>();
    }

    [Fact]
    public async Task ShouldThrow_InvalidOperationException_WhenCreatingPageInArchivedSpace()
    {
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        A.CallTo(() => spaces.GetByKey("DOCS")).Returns(Task.FromResult(new Space { Id = "space", Key = "DOCS", Status = SpaceStatus.Archived }));
        var service = new PageService(pages, spaces, A.Fake<IFeedManager>(), A.Fake<IPermissionService>(), A.Fake<IDistributedCache>());

        await service.Invoking(x => x.Create(new Mimisbrunnr.Integration.Wiki.PageCreateModel { SpaceKey = "DOCS" }, new UserInfo()))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ShouldThrow_PageNotFoundException_WhenParentPageMissing()
    {
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        A.CallTo(() => spaces.GetByKey("DOCS")).Returns(Task.FromResult(new Space { Id = "space", Key = "DOCS", Status = SpaceStatus.Actual }));
        A.CallTo(() => pages.GetById("parent")).Returns(Task.FromResult<Page>(null));
        var service = new PageService(pages, spaces, A.Fake<IFeedManager>(), A.Fake<IPermissionService>(), A.Fake<IDistributedCache>());

        await service.Invoking(x => x.Create(new Mimisbrunnr.Integration.Wiki.PageCreateModel { SpaceKey = "DOCS", ParentPageId = "parent" }, new UserInfo()))
            .Should().ThrowAsync<PageNotFoundException>();
    }

    [Fact]
    public async Task Should_CreatePageAndInvalidateHomeTree_WhenParentExists()
    {
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        var cache = A.Fake<IDistributedCache>();
        var user = new UserInfo { Email = "creator@example.test" };
        var space = new Space { Id = "space", Key = "DOCS", Status = SpaceStatus.Actual, HomePageId = "home" };
        A.CallTo(() => spaces.GetByKey("DOCS")).Returns(Task.FromResult(space));
        A.CallTo(() => pages.GetById("parent")).Returns(Task.FromResult(new Page { Id = "parent", SpaceId = "space" }));
        A.CallTo(() => pages.Create("space", "Title", "Content", user, "parent")).Returns(Task.FromResult(new Page { Id = "new", SpaceId = "space" }));
        var service = new PageService(pages, spaces, A.Fake<IFeedManager>(), A.Fake<IPermissionService>(), cache);

        var result = await service.Create(new Mimisbrunnr.Integration.Wiki.PageCreateModel { SpaceKey = "DOCS", ParentPageId = "parent", Name = "Title", Content = "Content" }, user);

        result.Id.Should().Be("new");
        result.SpaceKey.Should().Be("DOCS");
        A.CallTo(() => cache.RemoveAsync("page_tree_home", A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_UpdatePage_WhenEditing()
    {
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        var feed = A.Fake<IFeedManager>();
        var cache = A.Fake<IDistributedCache>();
        var user = new UserInfo { Email = "editor@example.test" };
        var page = new Page { Id = "page", SpaceId = "space" };
        var space = new Space { Id = "space", Key = "DOCS", Status = SpaceStatus.Actual, HomePageId = "home" };
        A.CallTo(() => pages.GetById("page")).Returns(Task.FromResult(page));
        A.CallTo(() => spaces.GetById("space")).Returns(Task.FromResult(space));
        var service = new PageService(pages, spaces, feed, A.Fake<IPermissionService>(), cache);

        await service.Update("page", new Mimisbrunnr.Integration.Wiki.PageUpdateModel { Name = "New", Content = "New content" }, user);

        page.Name.Should().Be("New");
        page.Content.Should().Be("New content");
        A.CallTo(() => pages.Update(page, user)).MustHaveHappenedOnceExactly();
        A.CallTo(() => feed.AddPageUpdate(space, page, user)).MustHaveHappenedOnceExactly();
        A.CallTo(() => cache.RemoveAsync("page_tree_home", A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ShouldThrow_InvalidOperationException_WhenUpdatingArchivedSpace()
    {
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        A.CallTo(() => pages.GetById("page")).Returns(Task.FromResult(new Page { Id = "page", SpaceId = "space" }));
        A.CallTo(() => spaces.GetById("space")).Returns(Task.FromResult(new Space { Id = "space", Key = "DOCS", Status = SpaceStatus.Archived }));
        var service = new PageService(pages, spaces, A.Fake<IFeedManager>(), A.Fake<IPermissionService>(), A.Fake<IDistributedCache>());

        await service.Invoking(x => x.Update("page", new Mimisbrunnr.Integration.Wiki.PageUpdateModel(), new UserInfo()))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ShouldThrow_InvalidOperationException_WhenRemovingHomePage()
    {
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        A.CallTo(() => pages.GetById("page")).Returns(Task.FromResult(new Page { Id = "page", SpaceId = "space" }));
        A.CallTo(() => spaces.GetById("space")).Returns(Task.FromResult(new Space { Id = "space", Key = "DOCS", Status = SpaceStatus.Actual, HomePageId = "page" }));
        var service = new PageService(pages, spaces, A.Fake<IFeedManager>(), A.Fake<IPermissionService>(), A.Fake<IDistributedCache>());

        await service.Invoking(x => x.Delete("page", new UserInfo(), true))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_DeletePage_WhenRemoving()
    {
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        var cache = A.Fake<IDistributedCache>();
        var page = new Page { Id = "page", SpaceId = "space" };
        A.CallTo(() => pages.GetById("page")).Returns(Task.FromResult(page));
        A.CallTo(() => spaces.GetById("space")).Returns(Task.FromResult(new Space { Id = "space", Key = "DOCS", Status = SpaceStatus.Actual, HomePageId = "home" }));
        var service = new PageService(pages, spaces, A.Fake<IFeedManager>(), A.Fake<IPermissionService>(), cache);

        await service.Delete("page", new UserInfo { Email = "remover@example.test" }, true);

        A.CallTo(() => pages.Remove(page, true)).MustHaveHappenedOnceExactly();
        A.CallTo(() => cache.RemoveAsync("page_tree_home", A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_CopyPage_WhenCopying()
    {
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        var cache = A.Fake<IDistributedCache>();
        var user = new UserInfo { Email = "copier@example.test" };
        var sourcePage = new Page { Id = "src", SpaceId = "space" };
        var destParent = new Page { Id = "dest", SpaceId = "space" };
        var sourceSpace = new Space { Id = "space", Key = "DOCS", Status = SpaceStatus.Actual, HomePageId = "home" };
        A.CallTo(() => pages.GetById("src")).Returns(Task.FromResult(sourcePage));
        A.CallTo(() => pages.GetById("dest")).Returns(Task.FromResult(destParent));
        A.CallTo(() => spaces.GetById("space")).Returns(Task.FromResult(sourceSpace));
        A.CallTo(() => pages.Copy(sourcePage, destParent)).Returns(Task.FromResult(new Page { Id = "copy", SpaceId = "space" }));
        var service = new PageService(pages, spaces, A.Fake<IFeedManager>(), A.Fake<IPermissionService>(), cache);

        var result = await service.Copy("src", "dest", user);

        result.Id.Should().Be("copy");
        result.SpaceKey.Should().Be("DOCS");
        A.CallTo(() => pages.Copy(sourcePage, destParent)).MustHaveHappenedOnceExactly();
        A.CallTo(() => cache.RemoveAsync("page_tree_home", A<CancellationToken>._)).MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task ShouldThrow_InvalidOperationException_WhenMovingHomePage()
    {
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        A.CallTo(() => pages.GetById("page")).Returns(Task.FromResult(new Page { Id = "page", SpaceId = "space" }));
        A.CallTo(() => pages.GetById("dest")).Returns(Task.FromResult(new Page { Id = "dest", SpaceId = "space" }));
        A.CallTo(() => spaces.GetById("space")).Returns(Task.FromResult(new Space { Id = "space", Key = "DOCS", Status = SpaceStatus.Actual, HomePageId = "page" }));
        var service = new PageService(pages, spaces, A.Fake<IFeedManager>(), A.Fake<IPermissionService>(), A.Fake<IDistributedCache>());

        await service.Invoking(x => x.Move("page", "dest", true, new UserInfo()))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_MovePage_WhenMoving()
    {
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        var cache = A.Fake<IDistributedCache>();
        var user = new UserInfo { Email = "mover@example.test" };
        var sourcePage = new Page { Id = "src", SpaceId = "space" };
        var destParent = new Page { Id = "dest", SpaceId = "space" };
        var sourceSpace = new Space { Id = "space", Key = "DOCS", Status = SpaceStatus.Actual, HomePageId = "home" };
        A.CallTo(() => pages.GetById("src")).Returns(Task.FromResult(sourcePage));
        A.CallTo(() => pages.GetById("dest")).Returns(Task.FromResult(destParent));
        A.CallTo(() => spaces.GetById("space")).Returns(Task.FromResult(sourceSpace));
        A.CallTo(() => pages.Move(sourcePage, destParent, true)).Returns(Task.FromResult(new Page { Id = "src", SpaceId = "space" }));
        var service = new PageService(pages, spaces, A.Fake<IFeedManager>(), A.Fake<IPermissionService>(), cache);

        var result = await service.Move("src", "dest", true, user);

        result.Id.Should().Be("src");
        A.CallTo(() => pages.Move(sourcePage, destParent, true)).MustHaveHappenedOnceExactly();
        A.CallTo(() => cache.RemoveAsync("page_tree_home", A<CancellationToken>._)).MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_ReturnVersions_WhenGettingPageVersions()
    {
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        var updated = DateTime.UtcNow;
        var page = new Page { Id = "page", SpaceId = "space", Version = 3, Updated = updated };
        A.CallTo(() => pages.GetById("page")).Returns(Task.FromResult(page));
        A.CallTo(() => spaces.GetById("space")).Returns(Task.FromResult(new Space { Id = "space", Key = "DOCS" }));
        A.CallTo(() => pages.GetAllVersions(page)).Returns(Task.FromResult(new[]
        {
            new HistoricalPage { Id = "h1", Version = 1 },
            new HistoricalPage { Id = "h2", Version = 2 }
        }));
        var service = new PageService(pages, spaces, A.Fake<IFeedManager>(), A.Fake<IPermissionService>(), A.Fake<IDistributedCache>());

        var result = await service.GetPageVersions("page", new UserInfo { Email = "user@example.test" });

        result.Count.Should().Be(2);
        result.LatestVersion.Should().Be(3);
        result.LatestVersionDate.Should().Be(updated);
    }

    [Fact]
    public async Task Should_RestoreVersion_WhenRestoring()
    {
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        var permissions = A.Fake<IPermissionService>();
        var user = new UserInfo { Email = "user@example.test" };
        var page = new Page { Id = "page", SpaceId = "space" };
        A.CallTo(() => pages.GetById("page")).Returns(Task.FromResult(page));
        A.CallTo(() => spaces.GetById("space")).Returns(Task.FromResult(new Space { Id = "space", Key = "DOCS" }));
        var service = new PageService(pages, spaces, A.Fake<IFeedManager>(), permissions, A.Fake<IDistributedCache>());

        await service.RestoreVersion("page", 5, user);

        A.CallTo(() => permissions.EnsureEditPermission("DOCS", user)).MustHaveHappenedOnceExactly();
        A.CallTo(() => pages.RestoreVersion(page, 5, user)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_DeleteVersion_WhenRemovingVersion()
    {
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        var user = new UserInfo { Email = "user@example.test" };
        var page = new Page { Id = "page", SpaceId = "space" };
        A.CallTo(() => pages.GetById("page")).Returns(Task.FromResult(page));
        A.CallTo(() => spaces.GetById("space")).Returns(Task.FromResult(new Space { Id = "space", Key = "DOCS" }));
        var service = new PageService(pages, spaces, A.Fake<IFeedManager>(), A.Fake<IPermissionService>(), A.Fake<IDistributedCache>());

        await service.DeleteVersion("page", 5, user);

        A.CallTo(() => pages.RemoveVersion(page, 5)).MustHaveHappenedOnceExactly();
    }
}
