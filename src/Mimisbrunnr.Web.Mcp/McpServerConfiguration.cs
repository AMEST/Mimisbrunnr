namespace Mimisbrunnr.Web.Mcp;

/// <summary>
/// Configuration for the MCP server exposed by the application.
/// </summary>
public class McpServerConfiguration
{
    /// <summary>
    /// Human-readable server name reported to MCP clients.
    /// </summary>
    public string ServerName { get; set; } = "Mimisbrunnr Wiki";

    /// <summary>
    /// Server version reported to MCP clients.
    /// </summary>
    public string ServerVersion { get; set; } = "1.0.0";

    /// <summary>
    /// Maximum size (in bytes) of an attachment uploaded through the MCP tool.
    /// MCP messages carry base64 content, so this is intentionally lower than the REST limit.
    /// </summary>
    public long MaxAttachmentBytes { get; set; } = 10 * 1024 * 1024;
}
