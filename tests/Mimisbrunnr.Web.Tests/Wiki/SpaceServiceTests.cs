using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.PageTemplates.Services;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Web.Wiki;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

namespace Mimisbrunnr.Web.Tests.Wiki;

public class SpaceServiceTests
{
    [Fact]
    public async Task Should_UseCurrentUserIdentity_WhenCreatingPersonalSpace()
    {
        var spaces = A.Fake<ISpaceManager>();
        var user = new UserInfo { Email = "user@example.test", Name = "User" };
        A.CallTo(() => spaces.FindPersonalSpace(user)).Returns(Task.FromResult<Space>(null));
        A.CallTo(() => spaces.GetByKey(user.Email)).Returns(Task.FromResult<Space>(null));
        A.CallTo(() => spaces.Create(user.Email.ToUpper(), user.Name, A<string>._, SpaceType.Personal, user))
            .Returns(Task.FromResult(new Space { Key = user.Email.ToUpper(), Name = user.Name, Type = SpaceType.Personal }));
        var service = new SpaceService(A.Fake<IPermissionService>(), spaces, A.Fake<IUserManager>(), A.Fake<IUserGroupManager>(), A.Fake<IApplicationConfigurationManager>(), NullLogger<SpaceService>.Instance, A.Fake<IPageTemplateManager>());

        var result = await service.Create(new SpaceCreateModel { Key = "ignored", Name = "ignored", Type = SpaceTypeModel.Personal }, user);

        result.Key.Should().Be(user.Email.ToUpper());
        A.CallTo(() => spaces.Create(user.Email.ToUpper(), user.Name, A<string>._, SpaceType.Personal, user)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ShouldThrow_AnonymousNotAllowedException_WhenAnonymousUserCreatesSpace()
    {
        var service = new SpaceService(A.Fake<IPermissionService>(), A.Fake<ISpaceManager>(), A.Fake<IUserManager>(), A.Fake<IUserGroupManager>(), A.Fake<IApplicationConfigurationManager>(), NullLogger<SpaceService>.Instance, A.Fake<IPageTemplateManager>());

        await service.Invoking(x => x.Create(new SpaceCreateModel(), null)).Should().ThrowAsync<AnonymousNotAllowedException>();
    }
}
