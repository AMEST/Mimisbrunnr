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
}
