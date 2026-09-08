using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Web.Wiki;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

namespace Mimisbrunnr.Web.Tests.Wiki;

public class DraftServiceTests
{
    private readonly IDraftManager _drafts = A.Fake<IDraftManager>();
    private readonly IPageManager _pages = A.Fake<IPageManager>();
    private readonly ISpaceManager _spaces = A.Fake<ISpaceManager>();
    private readonly IPermissionService _permissions = A.Fake<IPermissionService>();
    private readonly DraftService _service;

    public DraftServiceTests() => _service = new(_drafts, _pages, _spaces, _permissions);

    [Fact]
    public async Task Should_CreateDraft_WhenDraftDoesNotExist()
    {
        var user = new UserInfo { Email = "user@example.test" };
        ArrangePage(user);
        A.CallTo(() => _drafts.GetByPageId("page")).Returns(Task.FromResult<Draft>(null));
        var model = new DraftUpdateModel { Name = "Title", Content = "Content" };

        await _service.Update("page", model, user);

        A.CallTo(() => _drafts.Create("page", model.Name, model.Content, user)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_RequireEditPermission_WhenDeletingDraft()
    {
        var user = new UserInfo { Email = "user@example.test" };
        ArrangePage(user);

        await _service.Delete("page", user);

        A.CallTo(() => _permissions.EnsureEditPermission("SPACE", user)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _drafts.Remove("page")).MustHaveHappenedOnceExactly();
    }

    private void ArrangePage(UserInfo user)
    {
        A.CallTo(() => _pages.GetById("page")).Returns(Task.FromResult(new Page { Id = "page", SpaceId = "space" }));
        A.CallTo(() => _spaces.GetById("space")).Returns(Task.FromResult(new Space { Id = "space", Key = "SPACE" }));
    }
}
