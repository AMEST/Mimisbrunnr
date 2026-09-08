using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Web.Tests.Managers;

public class PageAndSpaceManagerTests
{
    public PageAndSpaceManagerTests() => QueryableAsyncExtensions.EnableFallback();

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
    public async Task ShouldThrow_InvalidOperationException_WhenAddingPermissionWithUserAndGroup()
    {
        var manager = new SpaceManager(A.Fake<IRepository<Space>>(), A.Fake<IPageManager>());
        var space = new Space { Permissions = [] };
        var permission = new Permission { User = new UserInfo(), Group = new GroupInfo() };

        await manager.Invoking(x => x.AddPermission(space, permission))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_ReturnOnlyPublicSpaces_WhenGettingPublicSpaces()
    {
        var repository = A.Fake<IRepository<Space>>();
        A.CallTo(() => repository.GetAll()).Returns(new[]
        {
            new Space { Key = "PUBLIC", Type = SpaceType.Public },
            new Space { Key = "PRIVATE", Type = SpaceType.Private }
        }.AsQueryable());
        var manager = new SpaceManager(repository, A.Fake<IPageManager>());

        var spaces = await manager.GetPublicSpaces();

        spaces.Should().ContainSingle().Which.Key.Should().Be("PUBLIC");
    }
}
