using System.Security.Claims;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Mimisbrunnr.Web.Mcp.Internal;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Tests.Mcp;

internal static class McpTestFactory
{
    public const string UserEmail = "tester@example.com";

    public static ToolContext CreateContext(string email = UserEmail)
    {
        var accessor = A.Fake<IHttpContextAccessor>();
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Email, email)], "test");
        A.CallTo(() => accessor.HttpContext)
            .Returns(new DefaultHttpContext { User = new ClaimsPrincipal(identity) });
        return new ToolContext(accessor);
    }

    public static ToolExecutor CreateExecutor(ToolContext context = null) =>
        new(context ?? CreateContext(), NullLogger<ToolExecutor>.Instance);

    public static UserInfo ExpectedUser() => new() { Email = UserEmail };
}
