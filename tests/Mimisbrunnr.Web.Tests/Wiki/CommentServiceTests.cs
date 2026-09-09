using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Web.Wiki;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;
using Mimisbrunnr.Web.Infrastructure;

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

    [Fact]
    public async Task Should_ReturnComments_WhenGettingComments()
    {
        var comments = A.Fake<ICommentManager>();
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        var permissions = A.Fake<IPermissionService>();
        var user = new UserInfo { Email = "user@example.test" };
        var comment = new Comment { Id = "comment", Message = "Hello", Author = new UserInfo { Email = "author@example.test" } };
        A.CallTo(() => pages.GetById("page")).Returns(Task.FromResult(new Page { Id = "page", SpaceId = "space" }));
        A.CallTo(() => spaces.GetById("space")).Returns(Task.FromResult(new Space { Id = "space", Key = "SPACE" }));
        A.CallTo(() => comments.GetComments(A<Page>._)).Returns(Task.FromResult(new[] { comment }));
        var service = new CommentService(comments, spaces, pages, A.Fake<IUserManager>(), permissions);

        var result = await service.GetComments("page", user);

        result.Should().ContainSingle().Which.Message.Should().Be("Hello");
        A.CallTo(() => permissions.EnsureViewPermission("SPACE", user)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ShouldThrow_PageNotFoundException_WhenGettingCommentsOfMissingPage()
    {
        var pages = A.Fake<IPageManager>();
        A.CallTo(() => pages.GetById("missing")).Returns(Task.FromResult<Page>(null));
        var service = new CommentService(A.Fake<ICommentManager>(), A.Fake<ISpaceManager>(), pages, A.Fake<IUserManager>(), A.Fake<IPermissionService>());

        await service.Invoking(x => x.GetComments("missing", new UserInfo { Email = "user@example.test" }))
            .Should().ThrowAsync<PageNotFoundException>();
    }

    [Fact]
    public async Task ShouldThrow_CommentNotFoundException_WhenCommentMissing()
    {
        var comments = A.Fake<ICommentManager>();
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        A.CallTo(() => pages.GetById("page")).Returns(Task.FromResult(new Page { Id = "page", SpaceId = "space" }));
        A.CallTo(() => spaces.GetById("space")).Returns(Task.FromResult(new Space { Id = "space", Key = "SPACE" }));
        A.CallTo(() => comments.GetById("missing")).Returns(Task.FromResult<Comment>(null));
        var service = new CommentService(comments, spaces, pages, A.Fake<IUserManager>(), A.Fake<IPermissionService>());

        await service.Invoking(x => x.GetById("page", "missing", new UserInfo { Email = "user@example.test" }))
            .Should().ThrowAsync<CommentNotFoundException>();
    }

    [Fact]
    public async Task Should_ReturnComment_WhenGettingCommentById()
    {
        var comments = A.Fake<ICommentManager>();
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        var permissions = A.Fake<IPermissionService>();
        var comment = new Comment { Id = "comment", PageId = "page", Message = "Hi" };
        A.CallTo(() => pages.GetById("page")).Returns(Task.FromResult(new Page { Id = "page", SpaceId = "space" }));
        A.CallTo(() => spaces.GetById("space")).Returns(Task.FromResult(new Space { Id = "space", Key = "SPACE" }));
        A.CallTo(() => comments.GetById("comment")).Returns(Task.FromResult(comment));
        var service = new CommentService(comments, spaces, pages, A.Fake<IUserManager>(), permissions);

        var result = await service.GetById("page", "comment", new UserInfo { Email = "user@example.test" });

        result.Id.Should().Be("comment");
        A.CallTo(() => permissions.EnsureAnonymousAllowed(A<UserInfo>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ShouldThrow_ArgumentNullException_WhenCreatedByIsNull()
    {
        var comments = A.Fake<ICommentManager>();
        var service = new CommentService(comments, A.Fake<ISpaceManager>(), A.Fake<IPageManager>(), A.Fake<IUserManager>(), A.Fake<IPermissionService>());

        await service.Invoking(x => x.Create("page", new CommentCreateModel { Message = "Hi" }, null))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Should_CreateComment_WhenValidRequestIsProvided()
    {
        var comments = A.Fake<ICommentManager>();
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        var user = new UserInfo { Email = "author@example.test" };
        var page = new Page { Id = "page", SpaceId = "space" };
        A.CallTo(() => pages.GetById("page")).Returns(Task.FromResult(page));
        A.CallTo(() => spaces.GetById("space")).Returns(Task.FromResult(new Space { Id = "space", Key = "SPACE" }));
        A.CallTo(() => comments.Create(page, "Hello", user)).Returns(Task.FromResult(new Comment { Id = "comment", Message = "Hello" }));
        var service = new CommentService(comments, spaces, pages, A.Fake<IUserManager>(), A.Fake<IPermissionService>());

        var result = await service.Create("page", new CommentCreateModel { Message = "Hello" }, user);

        result.Id.Should().Be("comment");
        A.CallTo(() => comments.Create(page, "Hello", user)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ShouldThrow_UserHasNotPermissionException_WhenNonAuthorNonAdminRemovesComment()
    {
        var comments = A.Fake<ICommentManager>();
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        var users = A.Fake<IUserManager>();
        var removedBy = new UserInfo { Email = "other@example.test" };
        A.CallTo(() => pages.GetById("page")).Returns(Task.FromResult(new Page { Id = "page", SpaceId = "space" }));
        A.CallTo(() => spaces.GetById("space")).Returns(Task.FromResult(new Space { Id = "space", Key = "SPACE" }));
        A.CallTo(() => comments.GetById("comment")).Returns(Task.FromResult(new Comment { Id = "comment", PageId = "page", Author = new UserInfo { Email = "author@example.test" } }));
        A.CallTo(() => users.GetByEmail(removedBy.Email)).Returns(Task.FromResult(new Mimisbrunnr.Users.User { Role = UserRole.Employee }));
        var service = new CommentService(comments, spaces, pages, users, A.Fake<IPermissionService>());

        await service.Invoking(x => x.Remove("page", "comment", removedBy))
            .Should().ThrowAsync<UserHasNotPermissionException>();

        A.CallTo(() => comments.Remove(A<Comment>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_DeleteComment_WhenAdminRemovesForeignComment()
    {
        var comments = A.Fake<ICommentManager>();
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        var users = A.Fake<IUserManager>();
        var comment = new Comment { Id = "comment", PageId = "page", Author = new UserInfo { Email = "author@example.test" } };
        A.CallTo(() => pages.GetById("page")).Returns(Task.FromResult(new Page { Id = "page", SpaceId = "space" }));
        A.CallTo(() => spaces.GetById("space")).Returns(Task.FromResult(new Space { Id = "space", Key = "SPACE" }));
        A.CallTo(() => comments.GetById("comment")).Returns(Task.FromResult(comment));
        A.CallTo(() => users.GetByEmail("admin@example.test")).Returns(Task.FromResult(new Mimisbrunnr.Users.User { Role = UserRole.Admin }));
        var service = new CommentService(comments, spaces, pages, users, A.Fake<IPermissionService>());

        await service.Remove("page", "comment", new UserInfo { Email = "admin@example.test" });

        A.CallTo(() => comments.Remove(comment)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_DeleteComment_WhenAuthorRemovesOwnComment()
    {
        var comments = A.Fake<ICommentManager>();
        var pages = A.Fake<IPageManager>();
        var spaces = A.Fake<ISpaceManager>();
        var users = A.Fake<IUserManager>();
        var comment = new Comment { Id = "comment", PageId = "page", Author = new UserInfo { Email = "author@example.test" } };
        A.CallTo(() => pages.GetById("page")).Returns(Task.FromResult(new Page { Id = "page", SpaceId = "space" }));
        A.CallTo(() => spaces.GetById("space")).Returns(Task.FromResult(new Space { Id = "space", Key = "SPACE" }));
        A.CallTo(() => comments.GetById("comment")).Returns(Task.FromResult(comment));
        A.CallTo(() => users.GetByEmail("author@example.test")).Returns(Task.FromResult(new Mimisbrunnr.Users.User { Role = UserRole.Employee }));
        var service = new CommentService(comments, spaces, pages, users, A.Fake<IPermissionService>());

        await service.Remove("page", "comment", new UserInfo { Email = "author@example.test" });

        A.CallTo(() => comments.Remove(comment)).MustHaveHappenedOnceExactly();
    }
}
