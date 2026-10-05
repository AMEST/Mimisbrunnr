using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Web.Quickstart;

/// <summary>
/// Model with parameters for initializing the application
/// </summary>
public class QuickstartModel
{
    /// <summary>
    /// Title of the application
    /// </summary>
    [Required]
    [MaxLength(32)]
    public string Title { get; set; }

    /// <summary>
    /// Whether anonymous access is allowed
    /// </summary>
    [Required]
    public bool AllowAnonymous { get; set; }

    /// <summary>
    /// Whether the Swagger API documentation is enabled
    /// </summary>
    [Required]
    public bool SwaggerEnabled {get; set;}

    /// <summary>
    /// Whether the MCP (Model Context Protocol) server for agents is enabled
    /// </summary>
    public bool McpEnabled {get; set;}

    /// <summary>
    /// Whether raw HTML is allowed in page content
    /// </summary>
    [Required]
    public bool AllowHtml { get; set; }

    /// <summary>
    /// Whether a custom homepage is enabled
    /// </summary>
    public bool CustomHomepageEnabled { get; set; }
}