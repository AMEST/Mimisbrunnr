using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Web.Wiki;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

namespace Mimisbrunnr.Web.Tests.Wiki;

public class AttachmentServiceTests
{
    private readonly IAttachmentManager _attachments = A.Fake<IAttachmentManager>();
    private readonly ISpaceManager _spaces = A.Fake<ISpaceManager>();
    private readonly IPageManager _pages = A.Fake<IPageManager>();
    private readonly IPermissionService _permissions = A.Fake<IPermissionService>();
    private readonly AttachmentService _service;

    public AttachmentServiceTests() => _service = new(_attachments, _spaces, _pages, _permissions);

    [Fact]
    public async Task Should_RequireAnonymousAndViewPermissions_WhenGettingAttachmentContent()
    {
        var page = new Page { Id = "page", SpaceId = "space" };
        var space = new Space { Id = "space", Key = "SPACE" };
        var user = new UserInfo { Email = "user@example.test" };
        var content = new MemoryStream([1, 2, 3]);
        A.CallTo(() => _pages.GetById(page.Id)).Returns(Task.FromResult(page));
        A.CallTo(() => _spaces.GetById(space.Id)).Returns(Task.FromResult(space));
        A.CallTo(() => _attachments.GetAttachmentContent(page, "file.png")).Returns(Task.FromResult<Stream>(content));

        var result = await _service.GetAttachmentContent(page.Id, "file.png", user);

        result.Should().BeSameAs(content);
        A.CallTo(() => _permissions.EnsureAnonymousAllowed(user)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _permissions.EnsureViewPermission(space.Key, user)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ShouldThrow_PageNotFoundException_WhenUploadingToMissingPage()
    {
        A.CallTo(() => _pages.GetById("missing")).Returns(Task.FromResult<Page>(null));

        await _service.Invoking(x => x.Upload("missing", new MemoryStream(), "file.txt", new UserInfo()))
            .Should().ThrowAsync<PageNotFoundException>();
    }

    [Fact]
    public async Task Should_ReturnAttachments_WhenGettingAttachments()
    {
        var page = new Page { Id = "page", SpaceId = "space" };
        var space = new Space { Id = "space", Key = "SPACE" };
        A.CallTo(() => _pages.GetById(page.Id)).Returns(Task.FromResult(page));
        A.CallTo(() => _spaces.GetById(space.Id)).Returns(Task.FromResult(space));
        A.CallTo(() => _attachments.GetAttachments(page)).Returns(Task.FromResult(new[] { new Attachment { Name = "file.txt" } }));

        var result = await _service.GetAttachments(page.Id, new UserInfo { Email = "user@example.test" });

        result.Should().ContainSingle().Which.Name.Should().Be("file.txt");
        A.CallTo(() => _permissions.EnsureViewPermission("SPACE", A<UserInfo>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ShouldThrow_PageNotFoundException_WhenGettingContentOfMissingPage()
    {
        A.CallTo(() => _pages.GetById("missing")).Returns(Task.FromResult<Page>(null));

        await _service.Invoking(x => x.GetAttachmentContent("missing", "file.txt", new UserInfo { Email = "user@example.test" }))
            .Should().ThrowAsync<PageNotFoundException>();
    }

    [Fact]
    public async Task Should_RequireRemovePermission_WhenRemovingAttachment()
    {
        var page = new Page { Id = "page", SpaceId = "space" };
        var space = new Space { Id = "space", Key = "SPACE" };
        var user = new UserInfo { Email = "user@example.test" };
        A.CallTo(() => _pages.GetById(page.Id)).Returns(Task.FromResult(page));
        A.CallTo(() => _spaces.GetById(space.Id)).Returns(Task.FromResult(space));

        await _service.Remove(page.Id, "file.txt", user);

        A.CallTo(() => _permissions.EnsureRemovePermission("SPACE", user)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _attachments.Remove(page, "file.txt")).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_RequireEditPermission_WhenUploadingAttachment()
    {
        var page = new Page { Id = "page", SpaceId = "space" };
        var space = new Space { Id = "space", Key = "SPACE" };
        var user = new UserInfo { Email = "user@example.test" };
        A.CallTo(() => _pages.GetById(page.Id)).Returns(Task.FromResult(page));
        A.CallTo(() => _spaces.GetById(space.Id)).Returns(Task.FromResult(space));

        await _service.Upload(page.Id, new MemoryStream(), "file.txt", user);

        A.CallTo(() => _permissions.EnsureEditPermission("SPACE", user)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _attachments.Upload(page, A<Stream>._, "file.txt", user)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ShouldThrow_SpaceNotFoundException_WhenUploadingInMissingSpace()
    {
        A.CallTo(() => _pages.GetById("page")).Returns(Task.FromResult(new Page { Id = "page", SpaceId = "space" }));
        A.CallTo(() => _spaces.GetById("space")).Returns(Task.FromResult<Space>(null));

        await _service.Invoking(x => x.Upload("page", new MemoryStream(), "file.txt", new UserInfo()))
            .Should().ThrowAsync<SpaceNotFoundException>();
    }
}
