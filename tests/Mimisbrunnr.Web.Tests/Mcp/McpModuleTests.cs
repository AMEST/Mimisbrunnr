using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Mimisbrunnr.Json;
using Mimisbrunnr.Web.Mcp;
using ModelContextProtocol.Server;

namespace Mimisbrunnr.Web.Tests.Mcp;

public class McpModuleTests
{
    [Fact]
    public void Should_CreateTools_WhenRegisteredWithDefaultJsonOptions()
    {
        var services = new ServiceCollection();
        services.AddMcpServer()
            .WithHttpTransport()
            .WithToolsFromAssembly(typeof(McpModule).Assembly, JsonSerializerOptionsFactory.Default);
        using var provider = services.BuildServiceProvider();

        var tools = provider.GetServices<McpServerTool>().ToArray();

        tools.Should().NotBeEmpty();
        tools.Select(tool => tool.ProtocolTool.Name).Should().Contain("get_page");
        tools.Select(tool => tool.ProtocolTool.Name).Should().Contain("get_current_user");
        tools.Select(tool => tool.ProtocolTool.Name).Should().Contain(["search_pages", "search_spaces", "search_users"]);
    }
}
