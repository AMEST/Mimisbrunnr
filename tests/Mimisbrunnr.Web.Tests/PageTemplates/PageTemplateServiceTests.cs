using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.PageTemplates.Contracts;
using Mimisbrunnr.PageTemplates.Services;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.PageTemplates;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

namespace Mimisbrunnr.Web.Tests.PageTemplates;

public class PageTemplateServiceTests
{
    [Fact]
    public async Task ShouldThrow_UserHasNotPermissionException_WhenUserReadsAnotherUsersTemplate()
    {
        var manager = A.Fake<IPageTemplateManager>();
        A.CallTo(() => manager.GetById("template")).Returns(Task.FromResult(new PageTemplate { Id = "template", Type = TemplateType.User, OwnerEmail = "owner@example.test" }));
        var service = new PageTemplateService(manager, A.Fake<ISpaceManager>(), A.Fake<IPermissionService>(), A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>());

        await service.Invoking(x => x.GetById("template", new UserInfo { Email = "other@example.test" }))
            .Should().ThrowAsync<UserHasNotPermissionException>();
    }

    [Fact]
    public async Task Should_PersistCurrentUserAsOwner_WhenCreatingUserTemplate()
    {
        var manager = A.Fake<IPageTemplateManager>();
        A.CallTo(() => manager.Create(A<PageTemplate>._)).ReturnsLazily(call => Task.FromResult(call.GetArgument<PageTemplate>(0)));
        var service = new PageTemplateService(manager, A.Fake<ISpaceManager>(), A.Fake<IPermissionService>(), A.Fake<IUserManager>(), A.Fake<ITemplateRenderer>());
        var user = new UserInfo { Email = "user@example.test" };

        await service.Create(new Mimisbrunnr.Integration.PageTemplates.PageTemplateCreateModel { Name = "Template", Type = TemplateType.User }, user);

        A.CallTo(() => manager.Create(A<PageTemplate>.That.Matches(x => x.OwnerEmail == user.Email && x.Type == TemplateType.User))).MustHaveHappenedOnceExactly();
    }
}
