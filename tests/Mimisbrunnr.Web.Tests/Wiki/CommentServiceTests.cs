using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Web.Wiki;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

namespace Mimisbrunnr.Web.Tests.Wiki;

public class CommentServiceTests
{
    [Fact]
    public async Task ShouldThrow_ArgumentNullException_WhenMessageIsEmpty()
    {
        var comments = A.Fake<ICommentManager>();
        var service = new CommentService(comments, A.Fake<ISpaceManager>(), A.Fake<IPageManager>(), A.Fake<IUserManager>(), A.Fake<IPermissionService>());

        await service.Invoking(x => x.Create("page", new CommentCreateModel { Message = "  " }, new UserInfo { Email = "user@example.test" }))
            .Should().ThrowAsync<ArgumentNullException>();

        A.CallTo(() => comments.Create(A<Page>._, A<string>._, A<UserInfo>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ShouldThrow_UserHasNotPermissionException_WhenCommentBelongsToAnotherPage()
    {
        var comments = A.Fake<ICommentManager>();
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        A.CallTo(() => pages.GetById("page")).Returns(Task.FromResult(new Page { Id = "page", SpaceId = "space" }));
        A.CallTo(() => spaces.GetById("space")).Returns(Task.FromResult(new Space { Key = "SPACE" }));
        A.CallTo(() => comments.GetById("comment")).Returns(Task.FromResult(new Comment { PageId = "other-page" }));
        var service = new CommentService(comments, spaces, pages, A.Fake<IUserManager>(), A.Fake<IPermissionService>());

        await service.Invoking(x => x.Remove("page", "comment", new UserInfo { Email = "user@example.test" }))
            .Should().ThrowAsync<UserHasNotPermissionException>();
    }
}
