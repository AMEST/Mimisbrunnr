using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Integration.Group;
using Mimisbrunnr.Integration.User;
using Mimisbrunnr.Web.Group;

namespace Mimisbrunnr.Web.Tests.Group;

public class GroupControllerTests
{
    [Fact]
    public async Task Should_ReturnOk_WhenGroupIsCreated()
    {
        var service = A.Fake<IGroupService>();
        var model = new GroupCreateModel { Name = "developers" };
        A.CallTo(() => service.Create(model, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult(new GroupModel()));
        var controller = new GroupController(service);

        var result = await controller.Create(model);

        result.Should().BeOfType<OkResult>();
        A.CallTo(() => service.Create(model, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ReturnGroups_WhenGettingAll()
    {
        var service = A.Fake<IGroupService>();
        A.CallTo(() => service.GetAll(null, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult<IEnumerable<GroupModel>>(new[] { new GroupModel { Name = "developers" } }));

        var result = await new GroupController(service).GetAll(null);

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeAssignableTo<IEnumerable<GroupModel>>();
    }

    [Fact]
    public async Task Should_ReturnGroup_WhenGettingByName()
    {
        var service = A.Fake<IGroupService>();
        A.CallTo(() => service.Get("developers", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult(new GroupModel { Name = "developers" }));

        var result = await new GroupController(service).Get("developers");

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeOfType<GroupModel>();
    }

    [Fact]
    public async Task Should_ReturnGroupUsers_WhenGettingUsers()
    {
        var service = A.Fake<IGroupService>();
        A.CallTo(() => service.GetUsers("developers", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult<IEnumerable<UserModel>>(new[] { new UserModel { Email = "u@example.test" } }));

        var result = await new GroupController(service).GetUsers("developers");

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Should_UpdateGroup_WhenCalled()
    {
        var service = A.Fake<IGroupService>();
        var model = new GroupUpdateModel { Description = "New" };

        var result = await new GroupController(service).Update("developers", model);

        result.Should().BeOfType<OkResult>();
        A.CallTo(() => service.Update("developers", model, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_RemoveGroup_WhenCalled()
    {
        var service = A.Fake<IGroupService>();

        var result = await new GroupController(service).Remove("developers");

        result.Should().BeOfType<OkResult>();
        A.CallTo(() => service.Remove("developers", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_AddUserToGroup_WhenCalled()
    {
        var service = A.Fake<IGroupService>();

        var result = await new GroupController(service).AddToGroup("developers", "u@example.test");

        result.Should().BeOfType<OkResult>();
        A.CallTo(() => service.AddUserToGroup("developers", A<Mimisbrunnr.Wiki.Contracts.UserInfo>.That.Matches(x => x.Email == "u@example.test"), A<Mimisbrunnr.Wiki.Contracts.UserInfo>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_RemoveUserFromGroup_WhenCalled()
    {
        var service = A.Fake<IGroupService>();

        var result = await new GroupController(service).RemoveFromGroup("developers", "u@example.test");

        result.Should().BeOfType<OkResult>();
        A.CallTo(() => service.RemoveUserFromGroup("developers", A<Mimisbrunnr.Wiki.Contracts.UserInfo>.That.Matches(x => x.Email == "u@example.test"), A<Mimisbrunnr.Wiki.Contracts.UserInfo>._)).MustHaveHappenedOnceExactly();
    }
}