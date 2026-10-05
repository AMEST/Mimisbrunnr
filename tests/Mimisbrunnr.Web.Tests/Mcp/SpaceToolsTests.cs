using System.Text.Json;
using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Web.Mcp.Tools;
using Mimisbrunnr.Web.Wiki;
using ModelContextProtocol;

namespace Mimisbrunnr.Web.Tests.Mcp;

public class SpaceToolsTests
{
    [Fact]
    public async Task Should_ReturnSerializedSpaces_WhenListSpacesInvoked()
    {
        var spaces = A.Fake<ISpaceService>();
        A.CallTo(() => spaces.GetAll(A<Mimisbrunnr.Wiki.Contracts.UserInfo>._, 10, 0))
            .Returns(Task.FromResult(new[] { new SpaceModel { Key = "TEAM", Name = "Team" } }));
        var tools = new SpaceTools(spaces, McpTestFactory.CreateContext(), McpTestFactory.CreateExecutor());

        var json = await tools.ListSpaces(10, 0);

        using var document = JsonDocument.Parse(json);
        document.RootElement[0].GetProperty("key").GetString().Should().Be("TEAM");
        A.CallTo(() => spaces.GetAll(A<Mimisbrunnr.Wiki.Contracts.UserInfo>._, 10, 0)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ReturnValidationFailed_WhenCreateSpaceKeyIsEmpty()
    {
        var spaces = A.Fake<ISpaceService>();
        var tools = new SpaceTools(spaces, McpTestFactory.CreateContext(), McpTestFactory.CreateExecutor());

        var exception = await Assert.ThrowsAsync<McpException>(() => tools.CreateSpace("", "Name", "Public"));

        exception.Message.Should().StartWith("validation_failed");
        A.CallTo(() => spaces.Create(A<SpaceCreateModel>._, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_ReturnInvalidArgument_WhenCreateSpaceTypeIsUnknown()
    {
        var spaces = A.Fake<ISpaceService>();
        var tools = new SpaceTools(spaces, McpTestFactory.CreateContext(), McpTestFactory.CreateExecutor());

        var exception = await Assert.ThrowsAsync<McpException>(() => tools.CreateSpace("TEAM", "Name", "Unknown"));

        exception.Message.Should().StartWith("invalid_argument");
    }

    [Fact]
    public async Task Should_AddPermission_WhenPermissionDoesNotExist()
    {
        var spaces = A.Fake<ISpaceService>();
        A.CallTo(() => spaces.GetSpacePermissions("TEAM", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult(Array.Empty<SpacePermissionModel>()));
        var tools = new SpaceTools(spaces, McpTestFactory.CreateContext(), McpTestFactory.CreateExecutor());

        await tools.SetSpacePermission("TEAM", "User", "user@example.com", canView: true);

        A.CallTo(() => spaces.AddPermission("TEAM", A<SpacePermissionModel>._, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => spaces.UpdatePermission("TEAM", A<SpacePermissionModel>._, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .MustNotHaveHappened();
    }

    [Theory]
    [InlineData("999")]
    [InlineData("-1")]
    [InlineData("Private, Public")]
    public async Task ShouldThrow_InvalidArgument_WhenCreateSpaceTypeIsUndefined(string type)
    {
        var spaces = A.Fake<ISpaceService>();
        var context = McpTestFactory.CreateContext();
        var tools = new SpaceTools(spaces, context, McpTestFactory.CreateExecutor(context));

        var exception = await Assert.ThrowsAsync<McpException>(() => tools.CreateSpace("TEAM", "Name", type));

        exception.Message.Should().StartWith("invalid_argument:");
        A.CallTo(() => spaces.Create(A<SpaceCreateModel>._, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .MustNotHaveHappened();
    }

    [Theory]
    [InlineData("Personal", SpaceTypeModel.Personal)]
    [InlineData("private", SpaceTypeModel.Private)]
    [InlineData("PUBLIC", SpaceTypeModel.Public)]
    public async Task Should_CreateSpace_WhenTypeIsDefined(string type, SpaceTypeModel expected)
    {
        var spaces = A.Fake<ISpaceService>();
        var context = McpTestFactory.CreateContext();
        var tools = new SpaceTools(spaces, context, McpTestFactory.CreateExecutor(context));

        await tools.CreateSpace("TEAM", "Name", type);

        A.CallTo(() => spaces.Create(A<SpaceCreateModel>.That.Matches(m => m.Type == expected), A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_UpdatePermission_WhenPermissionAlreadyExists()
    {
        var spaces = A.Fake<ISpaceService>();
        A.CallTo(() => spaces.GetSpacePermissions("TEAM", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult(new[]
            {
                new SpacePermissionModel { User = new Mimisbrunnr.Integration.User.UserModel { Email = "user@example.com" } }
            }));
        var tools = new SpaceTools(spaces, McpTestFactory.CreateContext(), McpTestFactory.CreateExecutor());

        await tools.SetSpacePermission("TEAM", "User", "user@example.com", canEdit: true);

        A.CallTo(() => spaces.UpdatePermission("TEAM", A<SpacePermissionModel>._, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => spaces.AddPermission("TEAM", A<SpacePermissionModel>._, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .MustNotHaveHappened();
    }
}
