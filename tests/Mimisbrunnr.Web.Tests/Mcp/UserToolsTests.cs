using System.Text.Json;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Mimisbrunnr.Integration.User;
using Mimisbrunnr.Web.Mcp.Internal;
using Mimisbrunnr.Web.Mcp.Tools;
using Mimisbrunnr.Web.User;
using Mimisbrunnr.Wiki.Contracts;
using ModelContextProtocol;

namespace Mimisbrunnr.Web.Tests.Mcp;

public class UserToolsTests
{
    [Fact]
    public async Task Should_ReturnCurrentUser_WhenGetCurrentUserInvoked()
    {
        var users = A.Fake<IUserService>();
        A.CallTo(() => users.GetCurrent(A<UserInfo>.That.Matches(user => user.Email == McpTestFactory.UserEmail)))
            .Returns(Task.FromResult(new UserViewModel
            {
                Email = McpTestFactory.UserEmail,
                Name = "Test User",
                AvatarUrl = "/avatar.png",
                IsAdmin = true,
                Enable = true
            }));
        var context = McpTestFactory.CreateContext();
        var tools = new UserTools(users, context, McpTestFactory.CreateExecutor(context));

        var json = await tools.GetCurrentUser();

        using var document = JsonDocument.Parse(json);
        var user = document.RootElement;
        user.GetProperty("email").GetString().Should().Be(McpTestFactory.UserEmail);
        user.GetProperty("name").GetString().Should().Be("Test User");
        user.GetProperty("avatarUrl").GetString().Should().Be("/avatar.png");
        user.GetProperty("isAdmin").GetBoolean().Should().BeTrue();
        user.GetProperty("enable").GetBoolean().Should().BeTrue();
        A.CallTo(() => users.GetCurrent(A<UserInfo>.That.Matches(info => info.Email == McpTestFactory.UserEmail)))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ShouldThrow_Forbidden_WhenRequestIsNotAuthenticated()
    {
        var users = A.Fake<IUserService>();
        var context = new ToolContext(new HttpContextAccessor { HttpContext = new DefaultHttpContext() });
        var tools = new UserTools(users, context, McpTestFactory.CreateExecutor(context));

        var exception = await Assert.ThrowsAsync<McpException>(() => tools.GetCurrentUser());

        exception.Message.Should().StartWith("forbidden:");
        A.CallTo(() => users.GetCurrent(A<UserInfo>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ShouldThrow_PrincipalNotFound_WhenCurrentUserDoesNotExist()
    {
        var users = A.Fake<IUserService>();
        A.CallTo(() => users.GetCurrent(A<UserInfo>._)).Returns(Task.FromResult<UserViewModel>(null));
        var context = McpTestFactory.CreateContext();
        var tools = new UserTools(users, context, McpTestFactory.CreateExecutor(context));

        var exception = await Assert.ThrowsAsync<McpException>(() => tools.GetCurrentUser());

        exception.Message.Should().StartWith("principal_not_found:");
    }
}
