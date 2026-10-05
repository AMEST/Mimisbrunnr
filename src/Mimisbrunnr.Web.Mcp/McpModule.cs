using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mimisbrunnr.Json;
using Mimisbrunnr.Web.Mcp.Internal;
using Skidbladnir.Modules;

namespace Mimisbrunnr.Web.Mcp;

/// <summary>
/// Registers the MCP (Model Context Protocol) server used by AI agents.
/// The server shares authentication and business services with the host API.
/// </summary>
public class McpModule : Module
{
    public override Type[] DependsModules => [ typeof(WebModule) ];

    public override void Configure(IServiceCollection services)
    {
        var options = Configuration.AppConfiguration.GetSection("Mcp").Get<McpServerConfiguration>()
                      ?? new McpServerConfiguration();

        services.AddSingleton(options);
        services.AddTransient<ToolContext>();
        services.AddTransient<ToolExecutor>();

        services.AddMcpServer(o =>
            {
                o.ServerInfo = new()
                {
                    Name = options.ServerName,
                    Version = options.ServerVersion
                };
            })
            .WithHttpTransport()
            .WithToolsFromAssembly(typeof(McpModule).Assembly, JsonSerializerOptionsFactory.Default);
    }
}
