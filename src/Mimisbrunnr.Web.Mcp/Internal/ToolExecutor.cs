using System.Diagnostics;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;

namespace Mimisbrunnr.Web.Mcp.Internal;

/// <summary>
/// Executes tool bodies with unified logging and error mapping.
/// </summary>
public sealed class ToolExecutor
{
    private readonly ToolContext _context;
    private readonly ILogger<ToolExecutor> _logger;

    public ToolExecutor(ToolContext context, ILogger<ToolExecutor> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string> Execute(string tool, Func<Task<string>> action)
    {
        var user = _context.TryGetUser()?.Email;
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await action();
            _logger.LogInformation("MCP tool {Tool} invoked by {User} completed in {ElapsedMs} ms",
                tool, user, stopwatch.ElapsedMilliseconds);
            return result;
        }
        catch (McpException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "MCP tool {Tool} invoked by {User} failed", tool, user);
            throw ToolErrors.Map(exception);
        }
    }
}
